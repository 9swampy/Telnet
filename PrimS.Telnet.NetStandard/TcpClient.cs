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
      System.Net.IPEndPoint localEndPoint = new System.Net.IPEndPoint(interfaceIP, port);

      // Use socket initially to bind to end point
      System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,
                                                                       System.Net.Sockets.SocketType.Stream,
                                                                       System.Net.Sockets.ProtocolType.Tcp);

      // Bind socket to endpoint
      socket.Bind(localEndPoint);

#if NET451
      // Connect to the host
      socket.Connect(hostName, port);
#else
      // ConnectAsync implementation
      // (Not tested) throw exception for time being
      throw new NotImplementedException("This feature has not been fully implemented for the framework you are using." +
                                        "An implementation exists, but it has not been tested and is therefore disabled by this exception." +
                                        "See source code for more.");  

      System.Net.Sockets.SocketAsyncEventArgs e = new System.Net.Sockets.SocketAsyncEventArgs();

      var addresses = System.Net.Dns.GetHostAddresses(hostName);
      if (addresses.Length == 0)
      {
          throw new ArgumentException(
              "Unable to retrieve address from specified host name.", 
              "hostName"
          );
      }
      else if (addresses.Length > 1)
      {
          throw new ArgumentException(
              "There is more than one IP address to the specified host.", 
              "hostName"
          );
      }

      e.RemoteEndPoint = System.Net.IPEndPoint(addresses[0], port); // Port gets validated here.
      e.UserToken = socket;

      // .NetStandard does not include a synchronous constructor or Connect method.
      // This will normally not be connected by the time the constructor returns,
      // it is the responsibility of the caller to ensure that they wait for the
      // connection to complete or fail, using this.Connected.
      // The PrimS.Telnet.Client constructor does this.
      // Adding something awaitable on this class to connect or wait for connection
      // would break backward compatibility and require a lot of refactoring.
      // This will do for now.
      var nowait = socket.ConnectAsync(e);
#endif

      // Add bound and connected socket to TcpClient
      this.client = new System.Net.Sockets.TcpClient
      {
        Client = socket
      };
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
