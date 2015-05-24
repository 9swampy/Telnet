namespace PrimS.Telnet
{
  using System;
  using System.Threading;

  /// <summary>
  /// The base class for Clients.
  /// </summary>
  public abstract class BaseClient : IDisposable
  {
    /// <summary>
    /// The default time out ms.
    /// </summary>
    protected const int DefaultTimeOutMs = 100;

    /// <summary>
    /// The byte stream.
    /// </summary>
    protected readonly IByteStream ByteStream;

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
    /// Gets a value indicating whether this instance is connected.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is connected; otherwise, <c>false</c>.
    /// </value>
    public bool IsConnected
    {
      get
      {
        return this.ByteStream.Connected;
      }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      try
      {
        this.Dispose(true);
        GC.SuppressFinalize(this);
      }
      catch (Exception)
      {
        // NOP
      }
    }

    /// <summary>
    /// Determines whether the specified terminator has been located.
    /// </summary>
    /// <param name="terminator">The terminator to search for.</param>
    /// <param name="s">The content to search for the <see cref="terminator"/>.</param>
    /// <returns>True if the terminator is located, otherwise false.</returns>
    protected static bool IsTerminatorLocated(string terminator, string s)
    {
      return s.TrimEnd().EndsWith(terminator);
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

      System.Threading.Thread.Sleep(100);
    }
  }
}