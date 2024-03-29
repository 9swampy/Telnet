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
    private readonly bool isCancellationTokenOwned;
    private readonly CancellationTokenSource internalCancellation;

    /// <summary>
    /// Initialises a new instance of the <see cref="ByteStreamHandler"/> class.
    /// </summary>
    /// <param name="byteStream">The byteStream to handle.</param>
    public ByteStreamHandler(IByteStream byteStream)
      : this(byteStream, new CancellationTokenSource(), Client.DefaultMillisecondReadDelay)
    {
      isCancellationTokenOwned = true;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="ByteStreamHandler"/> class.
    /// </summary>
    /// <param name="byteStream">The byteStream to handle.</param>
    /// <param name="internalCancellation">A cancellation token.</param>
    public ByteStreamHandler(IByteStream byteStream, CancellationTokenSource internalCancellation)
      : this(byteStream, internalCancellation, Client.DefaultMillisecondReadDelay)
    { }

    /// <summary>
    /// Initialises a new instance of the <see cref="ByteStreamHandler"/> class.
    /// </summary>
    /// <param name="byteStream">The byteStream to handle.</param>
    /// <param name="internalCancellation">A cancellation token.</param>
    /// <param name="millisecondReadDelay">Time to delay between reads from the stream.</param>
    public ByteStreamHandler(IByteStream byteStream, CancellationTokenSource internalCancellation, int millisecondReadDelay)
    {
      this.byteStream = byteStream;
      this.internalCancellation = internalCancellation;
      MillisecondReadDelay = millisecondReadDelay;
    }

    private bool IsCancellationRequested
    {
      get
      {
        return internalCancellation.Token.IsCancellationRequested;
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
      if (!byteStream.Connected || internalCancellation.Token.IsCancellationRequested)
      {
        return string.Empty;
      }

      var sb = new StringBuilder();
      byteStream.ReceiveTimeout = (int)timeout.TotalMilliseconds;
      var endInitialTimeout = DateTime.Now.Add(timeout);
      var rollingTimeout = ExtendRollingTimeout(timeout);
      do
      {
        if (
#if ASYNC
         await
#endif
         PreprocessorAsyncAdapter.ExecuteWithConfigureAwait<bool>(() => RetrieveAndParseResponse(sb)))
        {
          rollingTimeout = ExtendRollingTimeout(timeout);
        }
      }
      while (!IsCancellationRequested &&
#if ASYNC
      await IsResponseAnticipated(IsInitialResponseReceived(sb), endInitialTimeout, rollingTimeout).ConfigureAwait(false));
#else
      IsResponseAnticipated(IsInitialResponseReceived(sb), endInitialTimeout, rollingTimeout));
#endif
      LogIfTimeoutExpired(endInitialTimeout);

      var read = sb.ToString();
      if (Client.IsWriteConsole)
      {
        Console.Write(read);
      }

      return read;
    }

    /// <summary>
    /// Add null check to cancel commands. Fail gracefully.
    /// </summary>
    protected void SendCancel()
    {
#pragma warning disable CA1031 // Do not catch general exception types
      try
      {
        internalCancellation?.Cancel();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private static void LogIfTimeoutExpired(DateTime timeout)
    {
      if (IsTimeoutExpired(timeout))
      {
        System.Diagnostics.Debug.WriteLine("Timeout exceeded {0}", DateTime.Now.ToString("ss:fff"));
      }
    }
  }
}
