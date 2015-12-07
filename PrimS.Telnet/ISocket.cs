namespace PrimS.Telnet
{
  using System;
  using System.Linq;
  using System.Net.Sockets;

  public interface ISocket
  {
    NetworkStream GetStream();

    bool Connected { get; }

    void Close();

    int Available { get; }

    int ReceiveTimeout { get; set; }
  }
}
