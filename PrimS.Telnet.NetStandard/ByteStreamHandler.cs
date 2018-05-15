using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PrimS.Telnet
{
  using System;
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
      bool result = DateTime.Now < rollingTimeout;
#if ASYNC
      await Task.Delay(16, this.internalCancellation.Token);
#else
      System.Threading.Thread.Sleep(16);
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
        int input = this.byteStream.ReadByte();
        switch (input)
        {
          case -1:
            break;
          case (int)Commands.InterpretAsCommand:
            int inputVerb = this.byteStream.ReadByte();
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
    /// We received a TELNET command. Handle it.
    /// </summary>
    /// <param name="inputVerb">The command we received.</param>
    private void InterpretNextAsCommand(int inputVerb)
    {
      System.Diagnostics.Debug.Write(Enum.GetName(typeof(Commands), inputVerb));
      switch (inputVerb)
      {
        case (int)Commands.InterruptProcess:
          System.Diagnostics.Debug.WriteLine("Interrupt Process (IP) received.");
#if ASYNC
          this.SendCancel();        
#endif
          break;
        case (int)Commands.Dont:
        case (int)Commands.Wont:
          // We should ignore Don't\Won't because that is the default state.
          // Only reply on state change. This helps avoid loops.
          // See RFC1143: https://tools.ietf.org/html/rfc1143
          break;
        case (int)Commands.Do:
        case (int)Commands.Will: 
          this.ReplyToCommand(inputVerb);
          break;
        case (int)Commands.Subnegotiation:
          this.PerformNegotiation();
          break;
        default:
          break;
      }
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
            string clientTerminalType = "vt100"; // This could be an environment variable
            this.SendNegotiation(inputOption, clientTerminalType);
            break;
          case (int)Options.TerminalSpeed:
            string clientTerminalSpeed = "19200,19200"; // This could be an environment variable
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
        byte[] outBuffer = new byte[3];
        outBuffer[0] = (byte)Commands.InterpretAsCommand;
        outBuffer[1] = (byte)Commands.Wont;
        outBuffer[2] = (byte)inputOption;
#if ASYNC
        this.byteStream.WriteAsync(outBuffer, 0, outBuffer.Length, this.internalCancellation.Token);
#else
        this.byteStream.Write(outBuffer, 0, outBuffer.Length);
#endif
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
      byte[] myResponse = Encoding.ASCII.GetBytes(optionMessage);   
      List<byte> outBuffer = new List<byte>();
      outBuffer.Add((byte)Commands.InterpretAsCommand);
      outBuffer.Add((byte)Commands.Subnegotiation);
      outBuffer.Add((byte)inputOption);
      outBuffer.Add(0);  // "IS"
      outBuffer.AddRange(myResponse);
      outBuffer.Add((byte)Commands.InterpretAsCommand);
      outBuffer.Add((byte)Commands.SubnegotiationEnd);
#if ASYNC
      this.byteStream.WriteAsync(outBuffer.ToArray(), 0, outBuffer.Count, this.internalCancellation.Token);
#else
      this.byteStream.Write(outBuffer.ToArray(), 0, outBuffer.Count);
#endif
    }

    /// <summary>
    /// Send TELNET command response to the server.
    /// </summary>
    /// <param name="inputVerb">The TELNET command we received.</param>
    private void ReplyToCommand(int inputVerb)
    {
      // reply to all commands with "WONT\DONT", unless it is SGA (suppress go ahead), Terminal Type, or Terminal Speed.
      int inputOption = this.byteStream.ReadByte();
      if (inputOption != -1)
      {
        System.Diagnostics.Debug.WriteLine(Enum.GetName(typeof(Options), inputOption));
        byte[] outBuffer = new byte[3];
        outBuffer[0] = (byte)Commands.InterpretAsCommand;
        switch (inputOption)
        {
          case (int)Options.SuppressGoAhead:
            outBuffer[1] = inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do;
            break;
          case (int)Options.TerminalType:
            outBuffer[1] = inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do;
            break;
          case (int)Options.TerminalSpeed:
            outBuffer[1] = inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do;
            break;
          case (int)Options.WindowSize:
            outBuffer[1] = inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do;
            break;
          default:
            outBuffer[1] = inputVerb == (int)Commands.Do ? (byte)Commands.Wont : (byte)Commands.Dont;
            break;
        }
        
        outBuffer[2] = (byte)inputOption;
#if ASYNC
        this.byteStream.WriteAsync(outBuffer, 0, outBuffer.Length, this.internalCancellation.Token);
#else
        this.byteStream.Write(outBuffer, 0, outBuffer.Length);
#endif

        if (inputOption == (int)Options.WindowSize)
        {  // NAWS needs to be sent immediately because the server doesn't request subnegotiation.
          string clientNAWS = ((char)132 + (char)0 + (char)24).ToString(); // This could be an environment variable
          this.SendNegotiation(inputOption, clientNAWS);
        }
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
 await
#endif
 this.IsWaitForIncrementalResponse(rollingTimeout);
    }
  }
}
