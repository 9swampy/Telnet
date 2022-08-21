#if NetStandard || NET6_0_OR_GREATER
namespace PrimS.Telnet.CiTests
#else
namespace PrimS.Telnet.Sync.CiTests
#endif
{
  using Xunit;
  using FluentAssertions;

  public class DummyTelnetServerTests
  {
    [Fact]
    public void TelnetServerShouldTerminateAndReleaseDebuggingContext()
    {
      DummyTelnetServer server;
      using (server = new DummyTelnetServer())
      {
      }
      server.IsListening.Should().BeFalse();
    }
  }
}
