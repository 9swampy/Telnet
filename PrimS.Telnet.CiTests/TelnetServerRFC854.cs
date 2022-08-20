#if NetStandard
namespace PrimS.Telnet.CiTests
#else
namespace PrimS.Telnet.Sync.CiTests
#endif
{
  public class TelnetServerRFC854 : TelnetServerBase
  {
    public TelnetServerRFC854()
      : base("\r\n")
    {
      
    }
  }
}
