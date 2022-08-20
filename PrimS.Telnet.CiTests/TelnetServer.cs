#if NetStandard
namespace PrimS.Telnet.CiTests
#else
namespace PrimS.Telnet.Sync.CiTests
#endif
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
