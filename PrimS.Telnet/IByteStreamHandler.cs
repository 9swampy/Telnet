namespace PrimS.Telnet
{
  using System;
#if ASYNC
  using System.Threading.Tasks;
#endif

  public interface IByteStreamHandler
  {
#if ASYNC
    Task<string> ReadAsync(TimeSpan timeout);
#else
    string Read(TimeSpan timeout);
#endif
  }
}