using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PrimS.Telnet.CiTests")]
[assembly: InternalsVisibleTo("PrimS.Telnet.48.CiTests")]
[assembly: InternalsVisibleTo("PrimS.Telnet.NetStandard.CiTests")]

namespace PrimS.Telnet
{
  using System;
#if ASYNC
  using System.Threading.Tasks;
#endif

  /// <summary>
  /// A ByteStream acting over a TCP channel.
  /// </summary>
  public class TcpByteStream : IByteStream
  {
    private readonly ISocket socket;
    private readonly bool isSocketOwned;

    /// <summary>
    /// Initialises a new instance of the <see cref="TcpByteStream" /> class.
    /// </summary>
    /// <param name="hostName">The host name.</param>
    /// <param name="port">The port.</param>
    public TcpByteStream(string hostName, int port)
      : this(new PrimS.Telnet.TcpClient(hostName, port))
    {
      isSocketOwned = true;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="TcpByteStream" /> class.
    /// </summary>
    /// <param name="tcpSocket">The TCP socket.</param>
    public TcpByteStream(ISocket tcpSocket)
    {
      socket = tcpSocket;
#if !ASYNC
      var are = new System.Threading.AutoResetEvent(false);
      are.WaitOne(20);
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
        return socket.Available;
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
        return socket.Connected;
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
        return socket.ReceiveTimeout;
      }

      set
      {
        socket.ReceiveTimeout = value;
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
      return socket.GetStream().ReadByte();
    }

#if ASYNC
    /// <summary>
    /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
    /// </summary>
    /// <param name="value">The byte to write to the stream.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
    public async Task WriteByteAsync(byte value, System.Threading.CancellationToken cancellationToken)
    {
      await socket.GetStream().WriteByteAsync(value, cancellationToken).ConfigureAwait(false);
#else
    /// <summary>
    /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
    /// </summary>
    /// <param name="value">The byte to write to the stream.</param>
    public void WriteByte(byte value)
    {
      socket.GetStream().WriteByte(value);
#endif
      System.Diagnostics.Debug.WriteLine("SENT: " + (char)value);
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
      var result = socket.GetStream().WriteAsync(buffer, offset, count, cancellationToken);
      System.Diagnostics.Debug.WriteLine("SENT: " + System.Text.Encoding.UTF8.GetString(buffer));
      return result;
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
      socket.GetStream().Write(buffer, offset, count);
      System.Diagnostics.Debug.WriteLine("SENT: " + System.Text.Encoding.UTF8.GetString(buffer));
    }
#endif

#if ASYNC
    /// <summary>
    /// Asynchronously writes the specified value to the stream.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous action.</returns>
    public Task WriteAsync(string value, System.Threading.CancellationToken cancellationToken)
    {
      var buffer = ByteStringConverter.ConvertStringToByteArray(value);
      return socket.GetStream().WriteAsync(buffer, 0, buffer.Length, cancellationToken);
    }
#else    
    /// <summary>
    /// Writes the specified command to the stream.
    /// </summary>
    /// <param name="value">The command.</param>
    public void Write(string value)
    {
      var buffer = ByteStringConverter.ConvertStringToByteArray(value);
      socket.GetStream().Write(buffer, 0, buffer.Length);
    }
#endif

    /// <summary>
    /// Disposes the instance and requests that the underlying connection be closed.
    /// </summary>
    public void Close()
    {
      socket.Close();
    }

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
    /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        Close();
        if (isSocketOwned)
        {
          socket.Dispose();
        }
      }
    }
  }
}
