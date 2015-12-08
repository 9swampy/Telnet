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

    public int Available
    {
      get
      {
        return this.client.Available;
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    public void Close()
    {
      this.client.Close();
    }

    public INetworkStream GetStream()
    {
      return new NetworkStream(this.client.GetStream());
    }

    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        this.client.Close();
      }
    }
  }
}