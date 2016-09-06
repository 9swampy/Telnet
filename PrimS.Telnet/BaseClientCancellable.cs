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
    protected readonly SemaphoreSlim SendRateLimit;

    /// <summary>
    /// The internal cancellation token.
    /// </summary>
    protected readonly CancellationTokenSource InternalCancellation;

    /// <summary>
    /// Initialises a new instance of the <see cref="BaseClient"/> class.
    /// </summary>
    /// <param name="byteStream">The byte stream.</param>
    /// <param name="token">The token.</param>
    protected BaseClient(IByteStream byteStream, CancellationToken token)
    {
      this.ByteStream = byteStream;
      this.SendRateLimit = new SemaphoreSlim(1);
      this.InternalCancellation = new CancellationTokenSource();
      token.Register(() => this.InternalCancellation.Cancel());
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        this.ByteStream.Close();
        this.SendRateLimit.Dispose();
        this.InternalCancellation.Dispose();
      }

      System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
      are.WaitOne(100);
    }
  }
}