namespace PrimS.Telnet
{
  using System;
  using System.Linq;

  public interface ISocket
  {
    bool Connected { get; }

    int Available { get; }

    int ReceiveTimeout { get; set; }

    INetworkStream GetStream();

    void Close();
  }
}