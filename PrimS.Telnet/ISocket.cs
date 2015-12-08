namespace PrimS.Telnet
{
  using System;
  using System.Linq;

  public interface ISocket
  {
    INetworkStream GetStream();

    bool Connected { get; }

    void Close();

    int Available { get; }

    int ReceiveTimeout { get; set; }
  }
}
