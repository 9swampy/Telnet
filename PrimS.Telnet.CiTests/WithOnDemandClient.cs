namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Threading.Tasks;
  using FluentAssertions;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class WithOnDemandClient
  {
    private const int TimeoutMs = 100;

    [TestMethod]
    public void ShouldNotConnect()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken(), ConnectionMode.OnDemand))
        {
          client.IsConnected.Should().Be(false);
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public async Task ShouldTerminateWithAColon()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken(), ConnectionMode.OnDemand))
        {
          string s = await client.TerminatedReadAsync(":", TimeSpan.FromMilliseconds(TimeoutMs));
          s.Should().EndWith(":");
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public async Task ShouldHaveConnectedOnDemand()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken(), ConnectionMode.OnDemand))
        {
          await client.TerminatedReadAsync(":", TimeSpan.FromMilliseconds(TimeoutMs));
          client.IsConnected.Should().Be(true);
        }
      }
    }
  }
}