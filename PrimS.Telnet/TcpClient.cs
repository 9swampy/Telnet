namespace PrimS.Telnet
{
  using System;
  using System.Linq;

  public class TcpClient : ISocket, IDisposable
  {
    private readonly System.Net.Sockets.TcpClient client;

    public TcpClient(string hostname, int port)
    {
      this.client = new System.Net.Sockets.TcpClient(hostname, port);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        this.client.Close();
      }
    }

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

    public bool Connected
    {
      get
      {
        return this.client.Connected;
      }
    }

    public void Close()
    {
      this.client.Close();
    }

    public int Available
    {
      get
      {
        return this.client.Available;
      }
    }

    public INetworkStream GetStream()
    {
      return new NetworkStream(this.client.GetStream());
    }
  }
}
