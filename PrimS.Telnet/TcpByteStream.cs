namespace PrimS.Telnet
{
  using System;
  using System.Net.Sockets;
#if ASYNC
  using System.Threading.Tasks;
#endif

  /// <summary>
  /// A ByteStream acting over a TCP channel.
  /// </summary>
  public class TcpByteStream : IByteStream
  {
    private readonly TcpClient tcpSocket;

    /// <summary>
    /// Initialises a new instance of the <see cref="TcpByteStream"/> class.
    /// </summary>
    /// <param name="hostname">The hostname.</param>
    /// <param name="port">The port.</param>
    public TcpByteStream(string hostname, int port)
    {
      this.tcpSocket = new TcpClient(hostname, port);
#if ASYNC
#else
      System.Threading.Thread.Sleep(20);
#endif
    }

    /// <summary>
    /// Gets the amount of data that has been received from the network and is available to be read.
    /// </summary>
    /// <value>
    /// The number of bytes of data received from the network and available to be read.
    /// </value>
    public int Available
    {
      get
      {
        return this.tcpSocket.Available;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="IByteStream" /> is connected.
    /// </summary>
    /// <value>
    ///   <c>True</c> if connected; otherwise, <c>false</c>.
    /// </value>
    public bool Connected
    {
      get
      {
        return this.tcpSocket.Connected;
      }
    }

    /// <summary>
    /// Gets or sets the amount of time this <see cref="IByteStream" /> will wait to receive data once a read operation is initiated.
    /// </summary>
    /// <value>
    /// The time-out value of the connection in milliseconds. The default value is 0.
    /// </value>
    public int ReceiveTimeout
    {
      get
      {
        return this.tcpSocket.ReceiveTimeout;
      }

      set
      {
        this.tcpSocket.ReceiveTimeout = value;
      }
    }

    /// <summary>
    /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
    /// </summary>
    /// <returns>
    /// The unsigned byte cast to an integer, or -1 if at the end of the stream.
    /// </returns>
    public int ReadByte()
    {
      return this.tcpSocket.GetStream().ReadByte();
    }

    /// <summary>
    /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
    /// </summary>
    /// <param name="value">The byte to write to the stream.</param>
    public void WriteByte(byte value)
    {
      this.tcpSocket.GetStream().WriteByte(value);
    }

#if ASYNC
    /// <summary>
    /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
    /// </summary>
    /// <param name="buffer">The buffer to write data from.</param>
    /// <param name="offset">The zero-based byte offset in buffer from which to begin copying bytes to the stream.</param>
    /// <param name="count">The maximum number of bytes to write.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
    /// <returns>
    /// A task that represents the asynchronous write operation.
    /// </returns>
    public Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
    {
      return this.tcpSocket.GetStream().WriteAsync(buffer, offset, count, cancellationToken);
    }
#else    
    /// <summary>
    /// Writes the specified buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="count">The count.</param>
    public void Write(byte[] buffer, int offset, int count)
    {
      this.tcpSocket.GetStream().Write(buffer, offset, count);
    }
#endif

#if ASYNC
    /// <summary>
    /// Asynchronously writes the specified command to the stream.
    /// </summary>
    /// <param name="command">The command.</param> 
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous action.</returns>
    public Task WriteAsync(string command, System.Threading.CancellationToken cancellationToken)
    {
      byte[] buffer = ConvertStringToByteArray(command);
      return this.tcpSocket.GetStream().WriteAsync(buffer, 0, buffer.Length, cancellationToken);
    }
#else    
    /// <summary>
    /// Writes the specified command to the stream.
    /// </summary>
    /// <param name="command">The command.</param>
    public void Write(string command)
    {        
      byte[] buffer = ConvertStringToByteArray(command);
      this.tcpSocket.GetStream().Write(buffer, 0, buffer.Length);
    }
#endif

    /// <summary>
    /// Disposes the instance and requests that the underlying connection be closed.
    /// </summary>
    public void Close()
    {
      this.tcpSocket.Close();
    }

    private static byte[] ConvertStringToByteArray(string command)
    {
      byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(command.Replace("\0xFF", "\0xFF\0xFF"));
      return buffer;
    }
  }
}