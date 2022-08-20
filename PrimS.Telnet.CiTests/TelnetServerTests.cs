#if NetStandard || NET6_0_OR_GREATER
namespace PrimS.Telnet.CiTests
#else
namespace PrimS.Telnet.Sync.CiTests
#endif
{
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using FluentAssertions;

  [TestClass]
  public class TelnetServerTests
  {
    [TestMethod]
    public void TelnetServerShouldTerminateAndReleaseDebuggingContext()
    {
      TelnetServer server;
      using (server = new TelnetServer())
      {
      }
      server.IsListening.Should().BeFalse();
    }
  }
}
