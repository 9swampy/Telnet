namespace PrimS.Telnet
{
  using System;
  using System.Linq;

  /// <summary>
  /// A TcpClient to connect to the specified socket.
  /// </summary>
  public class TcpClient : ISocket, IDisposable
  {
    private readonly System.Net.Sockets.TcpClient client;

    /// <summary>
    /// Initialises a new instance of the <see cref="TcpClient"/> class.
    /// </summary>
    /// <param name="interfaceIP">The IP of the network interface to connect through.</param>
    /// <param name="hostName">The host name.</param>
    /// <param name="port">The port.</param>
    public TcpClient(System.Net.IPAddress interfaceIP, string hostName, int port)
    {
      // Create local end point to bind to adapter
      System.Net.IPEndPoint localEndPoint = new System.Net.IPEndPoint(interfaceIP, 0);
      this.client = new System.Net.Sockets.TcpClient(localEndPoint);

#if NET451
      this.client.Connect(hostName, port);
#else
      this.client = new System.Net.Sockets.TcpClient();
      // .NetStandard does not include a synchronous constructor or Connect method.
      // This will normally not be connected by the time the constructor returns,
      // it is the responsibility of the caller to ensure that they wait for the
      // connection to complete or fail, using this.Connected.
      // The PrimS.Telnet.Client constructor does this.
      // Adding something awaitable on this class to connect or wait for connection
      // would break backward compatibility and require a lot of refactoring.
      // This will do for now.
      var nowait = this.client.ConnectAsync(hostName, port);
#endif
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="TcpClient"/> class.
    /// </summary>
    /// <param name="hostName">The host name.</param>
    /// <param name="port">The port.</param>
    public TcpClient(string hostName, int port)
    {
#if NET451
      this.client = new System.Net.Sockets.TcpClient(hostName, port);
#else
      this.client = new System.Net.Sockets.TcpClient();
      // .NetStandard does not include a synchronous constructor or Connect method.
      // This will normally not be connected by the time the constructor returns,
      // it is the responsibility of the caller to ensure that they wait for the
      // connection to complete or fail, using this.Connected.
      // The PrimS.Telnet.Client constructor does this.
      // Adding something awaitable on this class to connect or wait for connection
      // would break backward compatibility and require a lot of refactoring.
      // This will do for now.
      var nowait = this.client.ConnectAsync(hostName, port);
#endif
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
        return this.client.ReceiveTimeout;
      }

      set
      {
        this.client.ReceiveTimeout = value;
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
        return this.client.Connected;
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
        return this.client.Available;
      }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Closes this instance.
    /// </summary>
    public void Close()
    {
#if NET451
      this.client.Close();
#else
      this.client.Dispose();
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
      return new NetworkStream(this.client.GetStream());
    }

    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
#if NET451
        this.client.Close();
#else
        this.client.Dispose();
#endif
      }
    }
  }
}
