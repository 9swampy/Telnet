#if NetStandard || NET6_0_OR_GREATER
namespace PrimS.Telnet.CiTests
#else
namespace PrimS.Telnet.Sync.CiTests
#endif
{
  using System.Diagnostics.CodeAnalysis;

  [ExcludeFromCodeCoverage]
  public class DummyTelnetServer : DummyTelnetServerBase
  {
    public DummyTelnetServer()
      : base(Client.LegacyLineFeed)
    { }
  }
}
