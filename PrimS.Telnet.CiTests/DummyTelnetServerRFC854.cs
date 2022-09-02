#if NetStandard || NET6_0_OR_GREATER
namespace PrimS.Telnet.CiTests
#else
namespace PrimS.Telnet.Sync.CiTests
#endif
{
  public class DummyTelnetServerRFC854 : DummyTelnetServerBase
  {
    public DummyTelnetServerRFC854()
      : base(Client.Rfc854LineFeed)
    {

    }
  }
}
