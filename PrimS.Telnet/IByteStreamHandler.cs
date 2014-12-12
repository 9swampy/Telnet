namespace PrimS.Telnet
{
  using System;
  using System.Threading.Tasks;

  public interface IByteStreamHandler
  {
    Task<string> ReadAsync(TimeSpan timeout);
  }
}