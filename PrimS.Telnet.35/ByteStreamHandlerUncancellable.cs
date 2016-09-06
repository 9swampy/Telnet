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
  public partial class ByteStreamHandler
  {
    /// <summary>
    /// Initialises a new instance of the <see cref="ByteStreamHandler"/> class.
    /// </summary>
    /// <param name="byteStream">The byteStream to handle.</param>
    public ByteStreamHandler(IByteStream byteStream)
    {
      this.byteStream = byteStream;
    }
#if ASYNC
    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public async Task<string> ReadAsync(TimeSpan timeout)
#else
    /// <summary>
    /// Reads from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public string Read(TimeSpan timeout)
#endif
    {
      if (!this.byteStream.Connected)
      {
        return string.Empty;
      }

      StringBuilder sb = new StringBuilder();
      this.byteStream.ReceiveTimeout = (int)timeout.TotalMilliseconds;
      DateTime endInitialTimeout = DateTime.Now.Add(timeout);
      DateTime rollingTimeout = ExtendRollingTimeout(timeout);
      do
      {
        if (this.RetrieveAndParseResponse(sb))
        {
          rollingTimeout = ExtendRollingTimeout(timeout);
        }
      }
      while (
#if ASYNC
 await
#endif
this.IsResponseAnticipated(IsInitialResponseReceived(sb), endInitialTimeout, rollingTimeout));
      if (IsRollingTimeoutExpired(rollingTimeout))
      {
        System.Diagnostics.Debug.Print("RollingTimeout exceeded {0}", DateTime.Now.ToString("ss:fff"));
      }

      return sb.ToString();
    }
  }
}
