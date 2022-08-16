namespace PrimS.Telnet.CiTests
{
  using System.Diagnostics.CodeAnalysis;

  [ExcludeFromCodeCoverage]
  public class TelnetServer : TelnetServerBase
  {
    public TelnetServer()
      : base("\n")
    { }
  }
}
