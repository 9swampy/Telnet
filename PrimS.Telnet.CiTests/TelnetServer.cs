namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Linq;
  using System.Net;
  using System.Net.Sockets;
  using System.Text;

  [ExcludeFromCodeCoverage]
  public class TelnetServer : TelnetServerBase
  {
    public TelnetServer()
      : base("\n")
    { }
  }
}
