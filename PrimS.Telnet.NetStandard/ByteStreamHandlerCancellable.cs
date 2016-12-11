﻿namespace PrimS.Telnet
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
      DateTime endInitialTimeout = DateTime.Now.Add(timeout);
      DateTime rollingTimeout = ExtendRollingTimeout(timeout);
      do
      {
        if (this.RetrieveAndParseResponse(sb))
        {
          rollingTimeout = ExtendRollingTimeout(timeout);
        }
      }
      while (!this.IsCancellationRequested &&
#if ASYNC
 await
#endif
 this.IsResponseAnticipated(IsInitialResponseReceived(sb), endInitialTimeout, rollingTimeout));
      this.LogIfTimeoutExpired(rollingTimeout);
      return sb.ToString();
    }

    private void LogIfTimeoutExpired(DateTime rollingTimeout)
    {
      if (IsRollingTimeoutExpired(rollingTimeout))
      {
        System.Diagnostics.Debug.WriteLine("RollingTimeout exceeded {0}", DateTime.Now.ToString("ss:fff"));
      }
    }
}
}
