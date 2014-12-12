using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PrimS.Telnet
{
  public class TcpByteStream : IByteStream
  {
    private readonly TcpClient tcpSocket;

    public TcpByteStream(TcpClient tcpClient)
    {
      this.tcpSocket = tcpClient;
    }
    
    public int ReadByte()
    {
      return this.tcpSocket.GetStream().ReadByte();
    }

    public void WriteByte(byte value)
    {
      this.tcpSocket.GetStream().WriteByte(value);
    }

    public int Available
    {
      get
      {
        return this.tcpSocket.Available;
      }
    }

    public bool Connected
    {
      get
      {
        return this.tcpSocket.Connected;
      }
    }

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

    public Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
    {
      return this.tcpSocket.GetStream().WriteAsync(buffer, offset, count, cancellationToken);
    }
  }
}