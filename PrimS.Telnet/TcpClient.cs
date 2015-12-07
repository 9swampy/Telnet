namespace PrimS.Telnet
{
  using System;
  using System.Linq;
  using System.Net.Sockets;

  public class TcpClient : ISocket
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

    public NetworkStream GetStream()
    {
      return this.client.GetStream();
    }
  }
}
