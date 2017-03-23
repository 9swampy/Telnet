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

    private readonly HashSet<Options> acknowledgingOptions;

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
          default:
            sb.Append((char)input);
            break;
        }

        return true;
      }

      return false;
    }

    private void ReplyToCommand(int inputVerb)
    {
      // reply to all commands with "WONT", unless it is SGA (suppress go ahead)
      int inputOption = this.byteStream.ReadByte();
      if (inputOption != -1)
      {
        var reply = new byte[3];
        reply[0] = (byte)Commands.InterpretAsCommand;
        if (this.acknowledgingOptions.Contains((Options)inputOption))
        {
          reply[1] = inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do;
        }
        else
        {
          reply[1] = inputVerb == (int)Commands.Do ? (byte)Commands.Wont : (byte)Commands.Dont;
        }
        reply[2] = (byte)inputOption;

#if ASYNC
        // TODO WriteAsync() ???
#else
        this.byteStream.Write(reply, 0, reply.Length);
#endif
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