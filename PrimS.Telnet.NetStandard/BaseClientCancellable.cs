namespace PrimS.Telnet
{
  using System;
  using System.Threading;

  /// <summary>
  /// The base class for Clients.
  /// </summary>
  public abstract partial class BaseClient
  {
    /// <summary>
    /// The send rate limit.
    /// </summary>
    private readonly SemaphoreSlim sendRateLimit;

    /// <summary>
    /// The internal cancellation token.
    /// </summary>
    private readonly CancellationTokenSource internalCancellation;

    /// <summary>
    /// Initialises a new instance of the <see cref="BaseClient"/> class.
    /// </summary>
    /// <param name="byteStream">The byte stream.</param>
    /// <param name="token">The token.</param>
    protected BaseClient(IByteStream byteStream, CancellationToken token)
    {
      this.byteStream = byteStream;
      sendRateLimit = new SemaphoreSlim(1);
      internalCancellation = new CancellationTokenSource();
      token.Register(() =>
      {
        SendCancel();
      });
    }

    /// <summary>
    /// Gets the send rate limit.
    /// </summary>
    protected SemaphoreSlim SendRateLimit
    {
      get
      {
        return sendRateLimit;
      }
    }

    /// <summary>
    /// Gets the internal cancellation token.
    /// </summary>
    protected CancellationTokenSource InternalCancellation
    {
      get
      {
        return internalCancellation;
      }
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

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        ByteStream.Close();
        sendRateLimit.Dispose();
        if (!internalCancellation.IsCancellationRequested)
        {
          SendCancel();
        }

        internalCancellation.Dispose();
      }

      using (var are = new AutoResetEvent(false))
      {
        are.WaitOne(100);
      }
    }
  }
}
