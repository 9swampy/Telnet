namespace PrimS.Telnet
{
  using System;
#if ASYNC
  using System.Threading.Tasks;
#endif

  /// <summary>
  /// Contract of core functionality required to interact with a ByteStream.
  /// </summary>
  public interface IByteStreamHandler
  {
#if ASYNC
    /// <summary>
    /// Reads for up to the specified timeout.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>A task representing the asynchronous read action.</returns>
    Task<string> ReadAsync(TimeSpan timeout);
#else
    /// <summary>
    /// Reads for up to the specified timeout.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    string Read(TimeSpan timeout);
#endif
  }
}