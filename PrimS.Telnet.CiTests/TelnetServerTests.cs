namespace PrimS.Telnet.CiTests
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
