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
      return this.stream.ReadByte();
    }

    /// <summary>
    /// Writes the byte.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteByte(byte value)
    {
      this.stream.WriteByte(value);
    }

#if ASYNC    
    /// <summary>
    /// Writes the specified buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="count">The count.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>
    /// An awaitable task.
    /// </returns>
    public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      return this.stream.WriteAsync(buffer, offset, count, cancellationToken);
    }
#else
    /// <summary>
    /// Writes the specified buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="size">The size.</param>
    public void Write(byte[] buffer, int offset, int size)
    {
      this.stream.Write(buffer, offset, size);
    }
#endif

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      this.Dispose(true);
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
        this.stream.Dispose();
      }
    }
  }
}