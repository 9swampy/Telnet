namespace PrimS.Telnet
{
  using System;
#if ASYNC
  using System.Threading.Tasks;
#endif

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
    {
#if NetStandard || NET6_0_OR_GREATER
      client = new System.Net.Sockets.TcpClient();
      // .NetStandard does not include a synchronous constructor or Connect method.
      // This will normally not be connected by the time the constructor returns,
      // it is the responsibility of the caller to ensure that they wait for the
      // connection to complete or fail, using this.Connected.
      // The PrimS.Telnet.Client constructor does this.
      // Adding something awaitable on this class to connect or wait for connection
      // would break backward compatibility and require a lot of refactoring.
      // This will do for now.
      // https://stackoverflow.com/questions/70964917/optimising-an-asynchronous-call-in-a-constructor-using-joinabletaskfactory-run
      Task.Run(async () => await client.ConnectAsync(hostName, port).ConfigureAwait(false)).Wait();
#else
      client = new System.Net.Sockets.TcpClient(hostName, port);
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
  }
}
