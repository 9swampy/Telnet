namespace PrimS.Telnet
{
  using System;
  using System.Text;
  using System.Threading;
#if ASYNC
  using System.Threading.Tasks;
#endif

  /// <summary>
  /// Provides core functionality for interacting with the ByteStream.
  /// </summary>
  public partial class ByteStreamHandler
  {
    private readonly CancellationTokenSource internalCancellation;

    /// <summary>
    /// Initialises a new instance of the <see cref="ByteStreamHandler"/> class.
    /// </summary>
    /// <param name="byteStream">The byteStream to handle.</param>
    /// <param name="internalCancellation">A cancellation token.</param>
    public ByteStreamHandler(IByteStream byteStream, CancellationTokenSource internalCancellation)
    {
      this.byteStream = byteStream;
      this.internalCancellation = internalCancellation;
    }

    private bool IsCancellationRequested
    {
      get
      {
        return this.internalCancellation.Token.IsCancellationRequested;
      }
    }

#if ASYNC
    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <param name="timeout">The rolling timeout to wait for no further response from stream.</param>
    /// <returns>Any text read from the stream.</returns>
    public async Task<string> ReadAsync(TimeSpan timeout)
#else
    /// <summary>
    /// Reads from the stream.
    /// </summary>
    /// <param name="timeout">The rolling timeout to wait for no further response from stream.</param>
    /// <returns>Any text read from the stream.</returns>
    public string Read(TimeSpan timeout)
#endif
    {
      if (!this.byteStream.Connected || this.internalCancellation.Token.IsCancellationRequested)
      {
        return string.Empty;
      }

      var sb = new StringBuilder();
      this.byteStream.ReceiveTimeout = (int)timeout.TotalMilliseconds;
      var endInitialTimeout = DateTime.Now.Add(timeout);
      var rollingTimeout = ExtendRollingTimeout(timeout);
      do
      {
        if (this.RetrieveAndParseResponse(sb))
        {
          rollingTimeout = ExtendRollingTimeout(timeout);
        }
      }
      while (!this.IsCancellationRequested &&
#if ASYNC
await this.IsResponseAnticipated(IsInitialResponseReceived(sb), endInitialTimeout, rollingTimeout).ConfigureAwait(false));
#else
      this.IsResponseAnticipated(IsInitialResponseReceived(sb), endInitialTimeout, rollingTimeout));
#endif
      LogIfTimeoutExpired(rollingTimeout);
      return sb.ToString();
    }
    
    /// <summary>
    /// Add null check to cancel commands. Fail gracefully.
    /// </summary>
    protected void SendCancel()
    {
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
    }

    /// <summary>
    /// Add null check to cancel commands. Fail gracefully.
    /// </summary>
    protected void SendCancel()
    {
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
    }

    private static void LogIfTimeoutExpired(DateTime rollingTimeout)
    {
      if (IsRollingTimeoutExpired(rollingTimeout))
      {
        System.Diagnostics.Debug.WriteLine("RollingTimeout exceeded {0}", DateTime.Now.ToString("ss:fff"));
      }
    }
  }
}
