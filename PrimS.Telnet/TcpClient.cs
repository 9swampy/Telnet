namespace PrimS.Telnet
{
  using System;
  using System.Linq;

  /// <summary>
  /// A TcpClient to connect to the specified socket.
  /// </summary>
  public class TcpClient : ISocket, IDisposable
  {
    private readonly string hostname;

    private readonly int port;

    private System.Net.Sockets.TcpClient client;

    /// <summary>
    /// Initialises a new instance of the <see cref="TcpClient"/> class.
    /// </summary>
    /// <param name="hostname">The hostname.</param>
    /// <param name="port">The port.</param>
    public TcpClient(string hostname, int port)
    {
      this.hostname = hostname;
      this.port = port;
    }

    private System.Net.Sockets.TcpClient Client
    {
      get
      {
        if (this.client == null)
        {
          this.client = new System.Net.Sockets.TcpClient();
        }
        return this.client;
      }
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
        return this.Client.ReceiveTimeout;
      }

      set
      {
        this.Client.ReceiveTimeout = value;
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
        return this.Client.Connected;
      }
    }

    /// <summary>
    /// Connects this instance to the specified port on the specified host.
    /// </summary>
    public void Connect()
    {
      this.Client.Connect(this.hostname, this.port);
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
        return this.Client.Available;
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
      this.Client.Close();
    }

    /// <summary>
    /// Gets the stream.
    /// </summary>
    /// <returns>
    /// Network stream socket connected to.
    /// </returns>
    public INetworkStream GetStream()
    {
      if (!this.Client.Connected)
      {
        this.Connect();
      }

      return new NetworkStream(this.Client.GetStream());
    }

    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        this.Client.Close();
      }
    }
  }
}