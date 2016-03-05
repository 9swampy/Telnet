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

    /// <summary>
    /// Gets a value indicating whether this instance has unhandled streamed content.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is response pending; otherwise, <c>false</c>.
    /// </value>
    private bool IsResponsePending
    {
      get
      {
        return this.byteStream.Available > 0;
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

    /// <summary>
    /// Determines if we're still waiting for an initial response to be received.
    /// </summary>
    /// <param name="endInitialTimeout">The initial timeout.</param>
    /// <param name="sb">The stringBuilder collecting the stream.</param>
    /// <returns>True if no response has been received, otherwise false if any response received or we timed out waiting for a response.</returns>
    private static bool IsWaitForInitialResponse(DateTime endInitialTimeout, StringBuilder sb)
    {
      return sb.Length == 0 && DateTime.Now < endInitialTimeout;
    }

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
                AppendChar(sb, inputVerb);
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
          case (int)Commands.NoOperation:
            break;
          default:
            AppendChar(sb, input);
            break;
        }

        return true;
      }

      return false;
    }

    private static void AppendChar(StringBuilder sb, int inputVerb)
    {
      char c = (char)inputVerb;
      sb.Append(c);
    }

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
  }
}