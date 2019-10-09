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
      bool result = DateTime.Now < rollingTimeout;
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
        int input = this.byteStream.ReadByte();
        switch (input)
        {
          case -1:
            break;
          case (int)Commands.InterpretAsCommand:
            // interpret as command
            int inputVerb = this.byteStream.ReadByte();
            if (inputVerb == -1)
            {
              break;
            }

            switch (inputVerb)
            {
              case (int)Commands.InterpretAsCommand:
                // literal IAC = 255 escaped, so append char 255 to string
                sb.Append(inputVerb);
                break;
              case (int)Commands.Do:
              case (int)Commands.Dont:
              case (int)Commands.Will:
              case (int)Commands.Wont:
                this.ReplyToCommand(inputVerb);
                break;
              default:
                break;
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
    /// Send TELNET command response to the server.
    /// </summary>
    /// <param name="inputVerb">The TELNET command we received.</param>
    private void ReplyToCommand(int inputVerb)
    {
      // reply to all commands with "WONT", unless it is SGA (suppress go ahead)
      int inputOption = this.byteStream.ReadByte();
      if (inputOption != -1)
      {
        this.byteStream.WriteByte((byte)Commands.InterpretAsCommand);
        if (inputOption == (int)Options.SuppressGoAhead)
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
await this.IsWaitForIncrementalResponse(rollingTimeout).ConfigureAwait(false);
#else
      this.IsWaitForIncrementalResponse(rollingTimeout);
#endif

    }
  }
}
