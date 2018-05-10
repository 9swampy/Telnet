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
      }
    }

    private static DateTime ExtendRollingTimeout(TimeSpan timeout)
    {
      return DateTime.Now.Add(TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / 100));
    }

#if ASYNC
    private static async Task<bool> IsWaitForIncrementalResponse(DateTime rollingTimeout)
#else
    private static bool IsWaitForIncrementalResponse(DateTime rollingTimeout)
#endif
    {
      bool result = DateTime.Now < rollingTimeout;
#if ASYNC
      await Task.Delay(1);
#else
      System.Threading.Thread.Sleep(1);
#endif
      return result;
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

    private bool RetrieveAndParseResponse(StringBuilder sb)
    {
      if (this.IsResponsePending)
      {
        int input = this.byteStream.ReadByte();
        System.Diagnostics.Debug.WriteLine(input);
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
          default:
            sb.Append((char)input);
            break;
        }

        return true;
      }

      return false;
    }

    private void InterpretNextAsCommand(int inputVerb)
    {
      System.Diagnostics.Debug.WriteLine(inputVerb);
      switch (inputVerb)
      {
        case (int)Commands.InterruptProcess:
          System.Diagnostics.Debug.WriteLine("Interrupt Process (IP) recieved.");
#if ASYNC
          try
          {
            if (this.internalCancellation != null)
            {
              this.internalCancellation.Cancel();
            }
          }
          catch (Exception ex)
          {
            System.Diagnostics.Debug.WriteLine(ex.Message);
          }            
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

    private void PerformNegotiation()
    {
      int inputOption = this.byteStream.ReadByte();
      int subCommand = this.byteStream.ReadByte();

      // ISSUE: We should loop here until IAC-SE but what is the exit condition if that never comes?
      int shouldIAC = this.byteStream.ReadByte();
      int shouldSE = this.byteStream.ReadByte();
      if (inputOption == (int)Options.TerminalType)
      {
        if (subCommand == 1 && // Sub-negotiation SEND command.
            shouldIAC == (int)Commands.InterpretAsCommand && 
            shouldSE == (int)Commands.SubnegotiationEnd)
        {
          this.byteStream.WriteByte((byte)Commands.InterpretAsCommand);
          this.byteStream.WriteByte((byte)Commands.Subnegotiation);
          this.byteStream.WriteByte((byte)Options.TerminalType);
          this.byteStream.WriteByte(0);  // Sub-negotiation IS command.
          byte[] myTerminalType = Encoding.ASCII.GetBytes("VT100"); // This could be a variable we keep somewhere...
          foreach (byte msgPart in myTerminalType)
          {
            this.byteStream.WriteByte(msgPart);
          }

          this.byteStream.WriteByte((byte)Commands.InterpretAsCommand);
          this.byteStream.WriteByte((byte)Commands.SubnegotiationEnd);
        }
        else
        {
          // If we get lost just send WONT to end the negotiation
          this.byteStream.WriteByte((byte)Commands.InterpretAsCommand);
          this.byteStream.WriteByte((byte)Commands.Wont);
          this.byteStream.WriteByte((byte)inputOption);
        }
      }
      else
      {
        // We don't handle other subnegotiation options yet.
        System.Diagnostics.Debug.WriteLine("Request to negotiate: " + Enum.GetName(typeof(Options), inputOption));
      }
    }

    private void ReplyToCommand(int inputVerb)
    {
      // reply to all commands with "WONT", unless it is SGA (suppress go ahead) or Terminal Type
      int inputOption = this.byteStream.ReadByte();
      if (inputOption != -1)
      {
        System.Diagnostics.Debug.WriteLine(inputOption);
        this.byteStream.WriteByte((byte)Commands.InterpretAsCommand);
        if (inputOption == (int)Options.SuppressGoAhead)
        {
          this.byteStream.WriteByte(inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do);
        }
        else if (inputOption == (int)Options.TerminalType)
        {
          this.byteStream.WriteByte(inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do);
        }
        else
        {
          this.byteStream.WriteByte(inputVerb == (int)Commands.Do ? (byte)Commands.Wont : (byte)Commands.Dont);
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
 await
#endif
 IsWaitForIncrementalResponse(rollingTimeout);
    }
  }
}
