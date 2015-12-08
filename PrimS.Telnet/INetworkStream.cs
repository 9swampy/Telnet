namespace PrimS.Telnet
{
  using System;

#if ASYNC
  using System.Threading.Tasks;
  using System.Threading;
#endif

  public interface INetworkStream
  {
    int ReadByte();

    void WriteByte(byte value);
    
#if ASYNC
    Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
#else
    void Write(byte[] buffer, int offset, int size);
#endif
  }
}