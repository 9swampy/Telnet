namespace PrimS.Telnet
{
  using System;
#if ASYNC
  using System.Threading;
  using System.Threading.Tasks;
#endif

  /// <summary>
  /// An implementation of a network stream to read from and write to.
  /// </summary>
  public class NetworkStream : INetworkStream
  {
    private readonly System.Net.Sockets.NetworkStream stream;

    /// <summary>
    /// Initialises a new instance of the <see cref="NetworkStream" /> class.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public NetworkStream(System.Net.Sockets.NetworkStream stream)
    {
      this.stream = stream;
    }

    /// <summary>
    /// Reads the next byte.
    /// </summary>
    /// <returns>
    /// The next byte read.
    /// </returns>
    public int ReadByte()
    {
      return stream.ReadByte();
    }

#if ASYNC
    /// <inheritdoc/>
    public Task WriteByteAsync(byte value, CancellationToken cancellationToken)
    {
      return stream.WriteAsync(new byte[] { value }, 0, 1, cancellationToken);
    }
#else
    /// <inheritdoc/>
    public void WriteByte(byte value)
    {
      stream.WriteByte(value);
    }
#endif

#if ASYNC
    /// <inheritdoc/>
    public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      return stream.WriteAsync(buffer, offset, count, cancellationToken);
    }
#else
    /// <inheritdoc/>
    public void Write(byte[] buffer, int offset, int size)
    {
      stream.Write(buffer, offset, size);
    }
#endif

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        stream.Dispose();
      }
    }
  }
}
