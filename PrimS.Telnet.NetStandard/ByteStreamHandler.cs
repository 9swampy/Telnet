namespace PrimS.Telnet
{
  using System;
  using System.Collections.Generic;
  using System.Text;
#if ASYNC
  using System.Threading.Tasks;
#endif

  /// <summary>
  /// Provides core functionality for interacting with the ByteStream.
  /// </summary>
  public partial class ByteStreamHandler : IByteStreamHandler
  {
    private readonly IByteStream byteStream;

    private bool IsResponsePending
    {
      get
      {
        return byteStream.Available > 0;
      }
    }

    internal int MillisecondReadDelay { get; set; } = 16;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        byteStream.Dispose();
        if (isCancellationTokenOwned)
        {
          internalCancellation.Dispose();
        }
#if ASYNC
        SendCancel();
#endif
      }
    }

    private static DateTime ExtendRollingTimeout(TimeSpan timeout)
    {
      return DateTime.Now.Add(TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / 100));
    }

    private static bool IsWaitForInitialResponse(DateTime endInitialTimeout, bool isInitialResponseReceived)
    {
      return !isInitialResponseReceived && DateTime.Now < endInitialTimeout;
    }

    private static bool IsTimeoutExpired(DateTime timeout)
    {
      return DateTime.Now >= timeout;
    }

    private static bool IsInitialResponseReceived(StringBuilder sb)
    {
      return sb.Length > 0;
    }

#if ASYNC
    private async Task<bool> IsWaitForIncrementalResponse(DateTime rollingTimeout)
#else
    private bool IsWaitForIncrementalResponse(DateTime rollingTimeout)
#endif
    {
      var result = DateTime.Now < rollingTimeout;
#if ASYNC
      await Task.Delay(MillisecondReadDelay, internalCancellation.Token).ConfigureAwait(false);
#else
      System.Threading.Thread.Sleep(MillisecondReadDelay);
#endif
      return result;
    }

    /// <summary>
    /// Separate TELNET commands from text. Handle non-printable characters.
    /// </summary>
    /// <param name="sb">The incoming message.</param>
    /// <returns>True if response is pending.</returns>
    private
#if ASYNC
      async Task<bool>
#else
      bool
#endif
      RetrieveAndParseResponse(StringBuilder sb)
    {
      if (IsResponsePending)
      {
        var input = byteStream.ReadByte();
        switch (input)
        {
          case -1:
            break;
          case (int)Commands.InterpretAsCommand:
            var inputVerb = byteStream.ReadByte();
            if (inputVerb == -1)
            {
              // do nothing
            }
            else if (inputVerb == 255)
            {
              // literal IAC = 255 escaped, so append char 255 to string
              sb.Append(inputVerb);
            }
            else
            {
#if ASYNC
              await
#endif
              PreprocessorAsyncAdapter.ExecuteWithConfigureAwait(() => InterpretNextAsCommand(inputVerb));
            }

            break;
          case 1: // Start of Heading
            sb.Append("\n \n");
            break;
          case 2: // Start of Text
            sb.Append("\t");
            break;
          case 3: // End of Text or "break" CTRL+C
            sb.Append("^C");
            System.Diagnostics.Debug.WriteLine("^C");
            break;
          case 4: // End of Transmission
            sb.Append("^D");
            break;
          case 5: // Enquiry
#if ASYNC
            await byteStream.WriteByteAsync(6, internalCancellation.Token); // Send ACK
#else
            byteStream.WriteByte(6); // Send ACK
#endif
            break;
          case 6: // Acknowledge
            // We got an ACK
            break;
          case 7: // Bell character
            Console.Beep();
            break;
          case 8: // Backspace
            // We could delete a character from sb, or just swallow the char here.
            break;
          case 11: // Vertical TAB
          case 12: // Form Feed
            sb.Append(Environment.NewLine);
            break;
          case 21:
            sb.Append("NAK: Retransmit last message.");
            System.Diagnostics.Debug.WriteLine("ERROR NAK: Retransmit last message.");
            break;
          case 31: // Unit Separator
            sb.Append(",");
            break;
          default:
            sb.Append((char)input);
            break;
        }

        return true;
      }

      return false;
    }

    /// <summary>
    /// We received a TELNET command. Handle it.
    /// </summary>
    /// <param name="inputVerb">The command we received.</param>
#if ASYNC
    private Task InterpretNextAsCommand(int inputVerb)
#else
    private void InterpretNextAsCommand(int inputVerb)
#endif
    {
      System.Diagnostics.Debug.Write(Enum.GetName(typeof(Commands), inputVerb));
      switch (inputVerb)
      {
        case (int)Commands.InterruptProcess:
          System.Diagnostics.Debug.WriteLine("Interrupt Process (IP) received.");
#if ASYNC
          SendCancel();
          return Task.CompletedTask;
#else
          break;
#endif
        case (int)Commands.Dont:
        case (int)Commands.Wont:
          // We should ignore Don't\Won't because that is the default state.
          // Only reply on state change. This helps avoid loops.
          // See RFC1143: https://tools.ietf.org/html/rfc1143
#if ASYNC
          return Task.CompletedTask;
#else
          break;
#endif
        case (int)Commands.Do:
        case (int)Commands.Will:
#if ASYNC
          return
#endif
          PreprocessorAsyncAdapter.Execute(() => ReplyToCommand(inputVerb));
#if !ASYNC
          break;
#endif
        case (int)Commands.Subnegotiation:
#if ASYNC
          return
#endif
          PreprocessorAsyncAdapter.Execute(() => PerformNegotiation());
#if !ASYNC
          break;
#endif
        default:
#if ASYNC
          return Task.CompletedTask;
#else
          break;
#endif
      }
    }

    /// <summary>
    /// We received a request to perform sub negotiation on a TELNET option.
    /// <see cref="Client.TerminalType"/> and <see cref="Client.TerminalSpeed"/> can be configured via static properties on the <see cref="Client"/> class.
    /// </summary>
#if ASYNC
    private async Task PerformNegotiation()
#else
    private void PerformNegotiation()
#endif
    {
      var inputOption = byteStream.ReadByte();
      var subCommand = byteStream.ReadByte();

      // ISSUE: We should loop here until IAC-SE but what is the exit condition if that never comes?
      var shouldIAC = byteStream.ReadByte();
      var shouldSE = byteStream.ReadByte();
      if (subCommand == 1 && // Sub-negotiation SEND command.
          shouldIAC == (int)Commands.InterpretAsCommand &&
          shouldSE == (int)Commands.SubnegotiationEnd)
      {
        switch (inputOption)
        {
          case (int)Options.TerminalType:
#if ASYNC
            await
#endif
            PreprocessorAsyncAdapter.ExecuteWithConfigureAwait(() => SendNegotiation(inputOption, Client.TerminalType));
            break;
          case (int)Options.TerminalSpeed:
#if ASYNC
            await
#endif
            PreprocessorAsyncAdapter.ExecuteWithConfigureAwait(() => SendNegotiation(inputOption, Client.TerminalSpeed));
            break;
          default:
            // We don't handle other sub negotiation options yet.
            System.Diagnostics.Debug.WriteLine("Request to negotiate: " + Enum.GetName(typeof(Options), inputOption));
            break;
        }
      }
      else
      {
        // If we get lost just send WONT to end the negotiation
        var outBuffer = new byte[3];
        outBuffer[0] = (byte)Commands.InterpretAsCommand;
        outBuffer[1] = (byte)Commands.Wont;
        outBuffer[2] = (byte)inputOption;
#if ASYNC
        await byteStream.WriteAsync(outBuffer, 0, outBuffer.Length, internalCancellation.Token).ConfigureAwait(false);
#else
        byteStream.Write(outBuffer, 0, outBuffer.Length);
#endif
      }
    }

    /// <summary>
    /// Send the sub negotiation response to the server.
    /// </summary>
    /// <param name="inputOption">The option we are negotiating.</param>
    /// <param name="optionMessage">The setting for <paramref name="inputOption"/>.</param>
    private
#if ASYNC
      Task
#else
      void
#endif
    SendNegotiation(int inputOption, string optionMessage)
    {
      System.Diagnostics.Debug.WriteLine("Sending: " + Enum.GetName(typeof(Options), inputOption) + " Setting: " + optionMessage);
      var outBuffer = BuildSendNegotiationOutBuffer(inputOption, optionMessage);
#if ASYNC
      return byteStream.WriteAsync(outBuffer.ToArray(), 0, outBuffer.Count, internalCancellation.Token);
#else
      byteStream.Write(outBuffer.ToArray(), 0, outBuffer.Count);
#endif
    }

    private static List<byte> BuildSendNegotiationOutBuffer(int inputOption, string optionMessage)
    {
      var outBuffer = new List<byte>();
      outBuffer.Add((byte)Commands.InterpretAsCommand);
      outBuffer.Add((byte)Commands.Subnegotiation);
      outBuffer.Add((byte)inputOption);
      outBuffer.Add(0);  // "IS"
      outBuffer.AddRange(Encoding.ASCII.GetBytes(optionMessage));
      outBuffer.Add((byte)Commands.InterpretAsCommand);
      outBuffer.Add((byte)Commands.SubnegotiationEnd);
      return outBuffer;
    }

    /// <summary>
    /// Send TELNET command response to the server.
    /// Replies to all commands with <see cref="Commands.Wont"/>||<see cref="Commands.Dont"/> unless it is <see cref="Options.SuppressGoAhead"/>, <see cref="Options.TerminalType"/>, or <see cref="Options.TerminalSpeed"/>.
    /// </summary>
    /// <param name="inputVerb">The TELNET command we received.</param>
    private
#if ASYNC
      async Task
#else
      void
#endif
      ReplyToCommand(int inputVerb)
    {
      var inputOption = byteStream.ReadByte();
      if (IsCommand(inputOption))
      {
        System.Diagnostics.Debug.WriteLine(Enum.GetName(typeof(Options), inputOption));
        var outBuffer = new byte[3];
        outBuffer[0] = (byte)Commands.InterpretAsCommand;
        outBuffer[1] = inputOption switch
        {
          (int)Options.SuppressGoAhead => inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do,
          (int)Options.TerminalType => inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do,
          (int)Options.TerminalSpeed => inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do,
          (int)Options.WindowSize => inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do,
          _ => inputVerb == (int)Commands.Do ? (byte)Commands.Wont : (byte)Commands.Dont,
        };

        outBuffer[2] = (byte)inputOption;
#if ASYNC
        await byteStream.WriteAsync(outBuffer, 0, outBuffer.Length, internalCancellation.Token).ConfigureAwait(false);
#else
        byteStream.Write(outBuffer, 0, outBuffer.Length);
#endif

        if (inputOption == (int)Options.WindowSize)
        {  // NAWS needs to be sent immediately because the server doesn't request subnegotiation.
          var clientNAWS = ((char)132 + (char)0 + (char)24).ToString(); // This could be an environment variable
#if ASYNC
          await
#endif
          SendNegotiation(inputOption, clientNAWS);
        }
      }
    }

    private static bool IsCommand(int inputOption)
    {
      return inputOption != -1;
    }

#if ASYNC
    private async Task<bool> IsResponseAnticipated(bool isInitialResponseReceived, DateTime endInitialTimeout, DateTime rollingTimeout)
#else
    private bool IsResponseAnticipated(bool isInitialResponseReceived, DateTime endInitialTimeout, DateTime rollingTimeout)
#endif
    {
      return IsResponsePending || IsWaitForInitialResponse(endInitialTimeout, isInitialResponseReceived) ||
#if ASYNC
      await IsWaitForIncrementalResponse(rollingTimeout).ConfigureAwait(false);
#else
      IsWaitForIncrementalResponse(rollingTimeout);
#endif
    }
  }
}
