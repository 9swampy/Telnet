using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrimS.Telnet
{
  public class ByteStreamHandler : IByteStreamHandler
  {
    private readonly CancellationTokenSource internalCancellation;
    private readonly IByteStream byteStream;

    public ByteStreamHandler(IByteStream byteStream, CancellationTokenSource internalCancellation)
    {
      this.byteStream = byteStream;
      this.internalCancellation = internalCancellation;
    }

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

    private static async Task<bool> IsWaitForIncrementalResponse(DateTime rollingTimeout)
    {
      bool result = DateTime.Now < rollingTimeout;
      await Task.Delay(1);
      return result;
    }

    private static bool IsWaitForInitialResponse(DateTime endInitialTimeout, StringBuilder sb)
    {
      return (sb.Length == 0 && DateTime.Now < endInitialTimeout);
    }

    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns></returns>
    public async Task<string> ReadAsync(TimeSpan timeout)
    {
      if (!this.byteStream.Connected || this.internalCancellation.Token.IsCancellationRequested)
      {
        return string.Empty;
      }
      StringBuilder sb = new StringBuilder();
      this.byteStream.ReceiveTimeout = (int)timeout.TotalMilliseconds;
      DateTime endInitialTimeout = DateTime.Now.Add(timeout);
      DateTime rollingTimeout = ExtendRollingTimeout(timeout);
      do
      {
        if (this.ParseResponse(sb))
        {
          rollingTimeout = ExtendRollingTimeout(timeout);
        }
      }
      while (!this.internalCancellation.Token.IsCancellationRequested && (this.IsResponsePending || IsWaitForInitialResponse(endInitialTimeout, sb) || await IsWaitForIncrementalResponse(rollingTimeout)));
      if (DateTime.Now >= rollingTimeout)
      {
        System.Diagnostics.Debug.Print("RollingTimeout exceeded {0}", DateTime.Now.ToString("ss:fff"));
      }
      return sb.ToString();
    }

    private bool ParseResponse(StringBuilder sb)
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
                //literal IAC = 255 escaped, so append char 255 to string
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