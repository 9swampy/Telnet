namespace PrimS.Telnet.CiTests
{
  using System;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using FluentAssertions;
  using System.Diagnostics;

  [TestClass]
  public class WithInvalidServer
  {
    [TestMethod]
    public void ShouldNotConnect()
    {
      using (Client client = new Client("localhost", 11001, new System.Threading.CancellationToken(), TimeSpan.FromMilliseconds(100), ConnectionMode.OnDemand))
      {
        client.IsConnected.Should().Be(false);
      }
    }

    [TestMethod]
    public void ShouldNotThrow()
    {
      using (Client client = new Client("localhost", 11001, new System.Threading.CancellationToken(), TimeSpan.FromMilliseconds(100), ConnectionMode.OnDemand))
      {
        Action act = () => client.IsConnected.Should().Be(false);
        act.ShouldNotThrow();
      }
    }

    [TestMethod]
    public void ShouldNotWaitForConnectionAttemptToTimeout()
    {
      var timeout = TimeSpan.FromMilliseconds(100);
      Stopwatch sw = new Stopwatch();
      sw.Start();
      using (Client client = new Client("localhost", 11001, new System.Threading.CancellationToken(), timeout, ConnectionMode.OnDemand))
      {
        client.IsConnected.Should().Be(false);
        sw.Elapsed.Should().BeLessThan(timeout);
      }
    }
  }
}
