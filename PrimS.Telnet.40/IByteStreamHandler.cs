namespace PrimS.Telnet
{
  using System;

  public interface IByteStreamHandler
  {
    string Read(TimeSpan timeout);
  }
}