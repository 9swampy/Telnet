namespace PrimS.Telnet
{
  using System;
  using System.Threading;

  public abstract class BaseClient : IDisposable
  {
    protected readonly IByteStream byteStream;

    protected readonly SemaphoreSlim sendRateLimit;
    protected readonly CancellationTokenSource internalCancellation;

    protected const int DefaultTimeOutMs = 100;

    protected BaseClient(IByteStream byteStream, CancellationToken token)
    {
      this.byteStream = byteStream;
      this.sendRateLimit = new SemaphoreSlim(1);
      this.internalCancellation = new CancellationTokenSource();
      token.Register(() => this.internalCancellation.Cancel());
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
        return this.byteStream.Connected;
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
        //NOP
      }
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        this.byteStream.Close();
        this.sendRateLimit.Dispose();
        this.internalCancellation.Dispose();
      }
      System.Threading.Thread.Sleep(100);
    }

    protected static bool IsTerminatorLocated(string terminator, string s)
    {
      return s.TrimEnd().EndsWith(terminator);
    }
  }
}