namespace PrimS.Telnet.CiTests
{
  using System;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using FluentAssertions;

  [TestClass]
  public class WithDelayedConnectionTelnetServer
  {
    [TestMethod]
    public void ShouldNotDelayClientConstruction()
    {
      using (DelayedConnectionTelnetServer server = new DelayedConnectionTelnetServer())
      {
        DateTime timeout = DateTime.Now.Add(server.ConnectionDelay);
        using (new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken(), TimeSpan.FromMilliseconds(server.ConnectionDelay.TotalMilliseconds * 2), ConnectionMode.OnDemand))
        {
          DateTime.Now.Should().BeBefore(timeout, "construction of the delayed server should not block the thread");
        }
      }
    }

    [TestMethod]
    public void ShouldDelayClientConstruction()
    {
      using (DelayedConnectionTelnetServer server = new DelayedConnectionTelnetServer())
      {
        DateTime timeout = DateTime.Now.Add(server.ConnectionDelay);
        using (new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken(), TimeSpan.FromMilliseconds(server.ConnectionDelay.TotalMilliseconds * 2), ConnectionMode.OnInitialise))
        {
          DateTime.Now.Should().BeAfter(timeout, "construction of the delayed server should block the thread");
        }
      }
    }
  }
}