namespace PrimS.Telnet
{
  using System;
#if ASYNC
  using System.Threading.Tasks;
  using System.Threading;
#endif

  public class NetworkStream : INetworkStream
  {
    private readonly System.Net.Sockets.NetworkStream stream;

    public NetworkStream(System.Net.Sockets.NetworkStream stream)
    {
      this.stream = stream;
    }

    public int ReadByte()
    {
      return this.stream.ReadByte();
    }

    public void WriteByte(byte value)
    {
      this.stream.WriteByte(value);
    }

#if ASYNC
    public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      return this.stream.WriteAsync(buffer, offset, count, cancellationToken);
    }
#else
    public void Write(byte[] buffer, int offset, int size)
    {
      this.stream.Write(buffer, offset, size);
    }
#endif
  }
}