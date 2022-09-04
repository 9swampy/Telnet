namespace PrimS.Telnet
{
  using System;

  /// <summary>
  /// A TcpClient to connect to the specified socket.
  /// </summary>
  public class TcpClient : ISocket
  {
    private readonly System.Net.Sockets.TcpClient client;

    /// <summary>
    /// Initialises a new instance of the <see cref="TcpClient"/> class.
    /// </summary>
    /// <param name="hostName">The host name.</param>
    /// <param name="port">The port.</param>
    public TcpClient(string hostName, int port)
      : this(GetConnectedClient(hostName, port))
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="TcpClient"/> class.
    /// </summary>
    /// <param name="client">The <see cref="System.Net.Sockets.TcpClient"/> instance to wrap.</param>
    public TcpClient(System.Net.Sockets.TcpClient client)
    {
      this.client = client;
    }

    /// <summary>
    /// Gets or sets the receive timeout.
    /// </summary>
    /// <value>
    /// The receive timeout.
    /// </value>
    public int ReceiveTimeout
    {
      get
      {
        return client.ReceiveTimeout;
      }

      set
      {
        client.ReceiveTimeout = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="ISocket" /> is connected.
    /// </summary>
    /// <value>
    ///   <c>true</c> if connected; otherwise, <c>false</c>.
    /// </value>
    public bool Connected
    {
      get
      {
        return client.Connected;
      }
    }

    /// <summary>
    /// Gets the available bytes to be read.
    /// </summary>
    /// <value>
    /// The available bytes to be read.
    /// </value>
    public int Available
    {
      get
      {
        return client.Available;
      }
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
    /// Closes this instance.
    /// </summary>
    public void Close()
    {
#if NET462
      client.Close();
#else
      client.Dispose();
#endif
    }

    /// <summary>
    /// Gets the stream.
    /// </summary>
    /// <returns>
    /// Network stream socket connected to.
    /// </returns>
    public INetworkStream GetStream()
    {
      return new NetworkStream(client.GetStream());
    }

    /// <inheritdoc/>
    protected virtual void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
#if NET461
        this.client.Close();
#else
        client.Dispose();
#endif
      }
    }

    private static System.Net.Sockets.TcpClient GetConnectedClient(string hostName, int port)
    {
      var client = new System.Net.Sockets.TcpClient();
      client.Connect(hostName, port);
      return client;
    }
  }
}
