namespace PrimS.Telnet
{
  using System;

#if ASYNC
  using System.Threading;
  using System.Threading.Tasks;
#endif

  /// <summary>
  /// A network stream that can be read and written to.
  /// </summary>
  public interface INetworkStream : IDisposable
  {
    /// <summary>
    /// Reads the next byte.
    /// </summary>
    /// <returns>The next byte read.</returns>
    int ReadByte();

    /// <summary>
    /// Writes the byte.
    /// </summary>
    /// <param name="value">The value to write.</param>
    void WriteByte(byte value);
    
#if ASYNC
    /// <summary>
    /// Writes the specified buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="count">The count.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>An awaitable task.</returns>
    Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
#else
    /// <summary>
    /// Writes the specified buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="size">The size.</param>
    void Write(byte[] buffer, int offset, int size);
#endif
  }
}