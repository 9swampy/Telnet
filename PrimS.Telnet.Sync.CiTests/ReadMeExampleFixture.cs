namespace PrimS.Telnet.Sync.CiTests
{
  using FluentAssertions;
#if NCRUNCH
  using Xunit;
#endif
  using System;
  using System.Threading;
  using System.Text.RegularExpressions;

  public class ReadMeExampleFixture
  {
    public const string Pattern =
      "(?:WAN2 total TX: )([0-9.]*) " +
      "((?:[KMG]B)|(?:Bytes))" +
      "(?:[, ]*RX: )([0-9.]*) " +
      "((?:[KMG]B)|(?:Bytes))";
    private const int TimeoutMs = 5000;

#if NCRUNCH
    [Fact]
#endif
    public void ReadMeExample()
    {
      using (var server = new DummyTelnetServer())
      {
        using (var client = new Client(
          server.IPAddress.ToString(),
          server.Port,
          new CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          Client.IsWriteConsole = false;
          client.TryLogin(
            "username",
            "password",
            TimeoutMs).Should().Be(true);
          client.WriteLine("show statistic wan2");
          string s = client.TerminatedRead(
            ">",
            TimeSpan.FromMilliseconds(TimeoutMs));
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
          var regEx = new Regex(Pattern);
          regEx.IsMatch(s).Should().Be(true);
          MatchCollection matches = regEx.Matches(s);
          matches.Count.Should().Be(1);
          matches[0].Captures.Count.Should().Be(1);
          matches[0].Groups.Count.Should().Be(5);
          matches[0].Groups[0].Value.Should().Be(
            "WAN2 total TX: 6.3 GB ,RX: 6.9 GB");
          matches[0].Groups[1].Value.Should().Be("6.3");
          matches[0].Groups[2].Value.Should().Be("GB");
          matches[0].Groups[3].Value.Should().Be("6.9");
          matches[0].Groups[4].Value.Should().Be("GB");
        }
      }
    }
  }
}
