namespace PrimS.Telnet
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  public interface IByteStream
  {
    /// <summary>
    /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
    /// </summary>
    /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
    /// <exception cref="System.NotSupportedException">The stream does not support reading.</exception>
    /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
    int ReadByte();  

    /// <summary>
    /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
    /// </summary>
    /// <param name="value">The byte to write to the stream.</param>
    /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
    /// <exception cref="System.NotSupportedException">The stream does not support writing, or the stream is already closed.</exception>
    /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
    void WriteByte(byte value);

    /// <summary>
    /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
    /// </summary>
    /// <param name="buffer">The buffer to write data from.</param>
    /// <param name="offset">The zero-based byte offset in buffer from which to begin copying bytes to the stream.</param>
    /// <param name="count">The maximum number of bytes to write.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="System.ArgumentNullException">buffer is null.</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">offset or count is negative.</exception>
    /// <exception cref="System.ArgumentException">The sum of offset and count is larger than the buffer length.</exception>
    /// <exception cref="System.NotSupportedException">The stream does not support writing.</exception>
    /// <exception cref="System.ObjectDisposedException">The stream has been disposed.</exception>
    /// <exception cref="System.InvalidOperationException">The stream is currently in use by a previous write operation.</exception>
    Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the amount of data that has been received from the network and is available to be read.
    /// </summary>
    /// <value>
    /// The number of bytes of data received from the network and available to be read.
    /// </value>
    /// <exception cref="System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information.</exception>
    /// <exception cref="System.ObjectDisposedException">The System.Net.Sockets.Socket has been closed.</exception>
    int Available { get; }
    
    /// <summary>
    /// Gets a value indicating whether this <see cref="IByteStream"/> is connected.
    /// </summary>
    /// <value>
    ///   <c>True</c> if connected; otherwise, <c>false</c>.
    /// </value>
    bool Connected { get; }
    
    /// <summary>
    /// Gets or sets the amount of time this <see cref="IByteStream"/> will wait to receive data once a read operation is initiated.
    /// </summary>
    /// <value>
    /// The time-out value of the connection in milliseconds. The default value is 0.
    /// </value>
    int ReceiveTimeout { get; set; }
  }
}