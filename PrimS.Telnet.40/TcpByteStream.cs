using System;
using System.Net.Sockets;

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

    public void Write(byte[] buffer, int offset, int count)
    {
      this.tcpSocket.GetStream().Write(buffer, offset, count);
    }
  }
}