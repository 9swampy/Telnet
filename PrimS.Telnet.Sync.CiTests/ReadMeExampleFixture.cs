namespace PrimS.Telnet.Sync.CiTests
{
  using FluentAssertions;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using System;
  using System.Text.RegularExpressions;

  [TestClass]
  public class ReadMeExampleFixture
  {
    private const int TimeoutMs = 5000;

    [TestMethod]
    public void ReadMeExample()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          Client.IsWriteConsole = false;
          client.TryLogin("username", "password", TimeoutMs).Should().Be(true);
          client.WriteLine("show statistic wan2");
          string s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(TimeoutMs));
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
          Regex regEx = new Regex("(?!WAN2 total TX: )([0-9.]*)(?! GB ,RX: )([0-9.]*)(?= GB)");
          regEx.IsMatch(s).Should().Be(true);
          MatchCollection matches = regEx.Matches(s);
          decimal tx = decimal.Parse(matches[0].Value);
          decimal rx = decimal.Parse(matches[1].Value);
          (tx + rx).Should().BeGreaterThan(0);
          (tx + rx).Should().BeLessThan(50);
        }
      }
    }
  }
}
