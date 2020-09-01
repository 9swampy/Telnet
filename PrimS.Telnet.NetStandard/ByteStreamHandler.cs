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
        return this.byteStream.Available > 0;
      }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      this.Dispose(true);
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
        this.byteStream.Dispose();
#if ASYNC
        this.SendCancel();
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

    private static bool IsRollingTimeoutExpired(DateTime rollingTimeout)
    {
      return DateTime.Now >= rollingTimeout;
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
      await Task.Delay(1, this.internalCancellation.Token).ConfigureAwait(false);
#else
      System.Threading.Thread.Sleep(1);
#endif
      return result;
    }

    /// <summary>
    /// Separate TELNET commands from text. Handle non-printable characters.
    /// </summary>
    /// <param name="sb">The incoming message.</param>
    /// <returns>True if response is pending.</returns>
    private bool RetrieveAndParseResponse(StringBuilder sb)
    {
      if (this.IsResponsePending)
      {
        var input = this.byteStream.ReadByte();
        switch (input)
        {
          case -1:
            break;
          case (int)Commands.InterpretAsCommand:
            // interpret as command
            var inputVerb = this.byteStream.ReadByte();
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
              this.InterpretNextAsCommand(inputVerb);
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
            this.byteStream.WriteByte((byte)6); // Send ACK
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
    /// We received a request to perform sub negotiation on a TELNET option.
    /// </summary>
    private void PerformNegotiation()
    {
      int inputOption = this.byteStream.ReadByte();
      int subCommand = this.byteStream.ReadByte();

      // ISSUE: We should loop here until IAC-SE but what is the exit condition if that never comes?
      int shouldIAC = this.byteStream.ReadByte();
      int shouldSE = this.byteStream.ReadByte();
      if (subCommand == 1 && // Sub-negotiation SEND command.
          shouldIAC == (int)Commands.InterpretAsCommand &&
          shouldSE == (int)Commands.SubnegotiationEnd)
      {
        switch (inputOption)
        {
          case (int)Options.TerminalType:
            string clientTerminalType = "VT100"; // This could be an environment variable in the client.
            this.SendNegotiation(inputOption, clientTerminalType);
            break;
          case (int)Options.TerminalSpeed:
            string clientTerminalSpeed = "38400,38400";
            this.SendNegotiation(inputOption, clientTerminalSpeed);
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
        this.byteStream.WriteByte((byte)Commands.InterpretAsCommand);
        this.byteStream.WriteByte((byte)Commands.Wont);
        this.byteStream.WriteByte((byte)inputOption);
      }
    }

    /// <summary>
    /// Send the sub negotiation response to the server.
    /// </summary>
    /// <param name="inputOption">The option we are negotiating.</param>
    /// <param name="optionMessage">The setting for that option.</param>
    private void SendNegotiation(int inputOption, string optionMessage)
    {    
        System.Diagnostics.Debug.WriteLine("Sending: " + Enum.GetName(typeof(Options), inputOption) + " Setting: " + optionMessage);
        this.byteStream.WriteByte((byte)Commands.InterpretAsCommand);
        this.byteStream.WriteByte((byte)Commands.Subnegotiation);
        this.byteStream.WriteByte((byte)inputOption);
        this.byteStream.WriteByte(0);  // Sub-negotiation IS command.
        byte[] myTerminalType = Encoding.ASCII.GetBytes(optionMessage); 
        foreach (byte msgPart in myTerminalType)
        {
          this.byteStream.WriteByte(msgPart);
        }

        this.byteStream.WriteByte((byte)Commands.InterpretAsCommand);
        this.byteStream.WriteByte((byte)Commands.SubnegotiationEnd);
    }

    /// <summary>
    /// Send TELNET command response to the server.
    /// </summary>
    /// <param name="inputVerb">The TELNET command we received.</param>
    private void ReplyToCommand(int inputVerb)
    {
      // reply to all commands with "WONT\DONT", unless it is SGA (suppress go ahead), Terminal Type, or Terminal Speed
      int inputOption = this.byteStream.ReadByte();
      if (inputOption != -1)
      {
        System.Diagnostics.Debug.WriteLine(Enum.GetName(typeof(Options), inputOption));
        this.byteStream.WriteByte((byte)Commands.InterpretAsCommand);
        switch (inputOption)
        {
          case (int)Options.SuppressGoAhead:
            this.byteStream.WriteByte(inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do);
            break;
          case (int)Options.TerminalType:
            this.byteStream.WriteByte(inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do);
            break;
          case (int)Options.TerminalSpeed:
            this.byteStream.WriteByte(inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do);
            break;
          default:
            this.byteStream.WriteByte(inputVerb == (int)Commands.Do ? (byte)Commands.Wont : (byte)Commands.Dont);
            break;
        }

        this.byteStream.WriteByte((byte)inputOption);
      }
    }

#if ASYNC
    private async Task<bool> IsResponseAnticipated(bool isInitialResponseReceived, DateTime endInitialTimeout, DateTime rollingTimeout)
#else
    private bool IsResponseAnticipated(bool isInitialResponseReceived, DateTime endInitialTimeout, DateTime rollingTimeout)
#endif
    {
      return this.IsResponsePending || IsWaitForInitialResponse(endInitialTimeout, isInitialResponseReceived) ||
#if ASYNC
await this.IsWaitForIncrementalResponse(rollingTimeout).ConfigureAwait(false);
#else
      this.IsWaitForIncrementalResponse(rollingTimeout);
#endif
    }
  }
}
