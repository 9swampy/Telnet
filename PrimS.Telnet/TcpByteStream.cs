using System;
using System.Net.Sockets;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace PrimS.Telnet
{
  public class TcpByteStream : IByteStream
  {
    private readonly TcpClient tcpSocket;

    public TcpByteStream(string hostname, int port)
    {
      this.tcpSocket = new TcpClient(hostname, port);
#if ASYNC
#else
      System.Threading.Thread.Sleep(20);
#endif
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

#if ASYNC
    public Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
    {
      return this.tcpSocket.GetStream().WriteAsync(buffer, offset, count, cancellationToken);
    }
#else
    public void Write(byte[] buffer, int offset, int count)
    {
      this.tcpSocket.GetStream().Write(buffer, offset, count);
    }
#endif

#if ASYNC
    public Task WriteAsync(string command, System.Threading.CancellationToken cancellationToken)
    {
      byte[] buffer = ConvertStringToByteArray(command);
      return this.tcpSocket.GetStream().WriteAsync(buffer, 0, buffer.Length, cancellationToken);
    }
#else
    public void Write(string command)
    {        
      byte[] buffer = ConvertStringToByteArray(command);
      this.tcpSocket.GetStream().Write(buffer, 0, buffer.Length);
    }
#endif

    private static byte[] ConvertStringToByteArray(string command)
    {
      byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(command.Replace("\0xFF", "\0xFF\0xFF"));
      return buffer;
    }

    public void Close()
    {
      this.tcpSocket.Close();
    }
  }
}