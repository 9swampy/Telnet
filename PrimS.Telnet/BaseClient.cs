namespace PrimS.Telnet
{
  using System;
  using System.Threading;

  /// <summary>
  /// The base class for Clients.
  /// </summary>
  public abstract partial class BaseClient : IDisposable
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
    /// <param name="s">The content to search for the <paramref name="terminator"/>.</param>
    /// <returns>True if the terminator is located, otherwise false.</returns>
    protected static bool IsTerminatorLocated(string terminator, string s)
    {
      return s.TrimEnd().EndsWith(terminator);
    }
  }
}