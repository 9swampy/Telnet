namespace PrimS.Telnet.CiTests
{
  using System;
  using FluentAssertions;
  using System.Threading.Tasks;
  using System.Text.RegularExpressions;
  using Xunit;

  public class ReadMeExampleFixture
  {
    public const string Pattern = "(?:WAN2 total TX: )([0-9.]*) ((?:[KMG]B)|(?:Bytes))(?:[, ]*RX: )([0-9.]*) ((?:[KMG]B)|(?:Bytes))";
    private const int TimeoutMs = 5000;

    [Fact]
    public async Task ReadMeExample()
    {
      using (var server = new DummyTelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          Client.IsWriteConsole = false;
          (await client.TryLoginAsync("username", "password", TimeoutMs)).Should().Be(true);
          await client.WriteLineAsync("show statistic wan2");
          string s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(TimeoutMs));
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
          var regEx = new Regex(Pattern);
          regEx.IsMatch(s).Should().Be(true);
          MatchCollection matches = regEx.Matches(s);
          matches.Count.Should().Be(1);
          matches[0].Captures.Count.Should().Be(1);
          matches[0].Groups.Count.Should().Be(5);
          matches[0].Groups[0].Value.Should().Be("WAN2 total TX: 6.3 GB ,RX: 6.9 GB");
          matches[0].Groups[1].Value.Should().Be("6.3");
          matches[0].Groups[2].Value.Should().Be("GB");
          matches[0].Groups[3].Value.Should().Be("6.9");
          matches[0].Groups[4].Value.Should().Be("GB");
          var total = (ConvertToGigabytes(matches[0].Groups[1].Value, matches[0].Groups[2].Value) + ConvertToGigabytes(matches[0].Groups[3].Value, matches[0].Groups[4].Value));
          total.Should().Be(13.2M);
        }
      }
    }

    [Theory]
    [InlineData("WAN2 total TX: 2 MB, RX: 9 MB", 2, "MB", 9, "MB", 0.011)]
    [InlineData(" WAN2 total TX: 252 KB, RX: 673 KB", 252, "KB", 673, "KB", 0.001)]
    [InlineData("WAN2 total TX: 213 Bytes, RX: 0 Bytes", 213, "Bytes", 0, "Bytes", 0)]
    [InlineData("WAN2 total TX: 550 GB ,RX: 1 MB", 550, "GB", 1, "MB", 550.001)]
    public void GivenStatisticsWhenRegexThenExpected(string text, int tx, string txUnit, int rx, string rxUnit, decimal total)
    {
      Regex regEx = new Regex(Pattern);
      MatchCollection matches = regEx.Matches(text);
      regEx.IsMatch(text).Should().BeTrue();
      matches.Count.Should().Be(1);
      matches[0].Captures.Count.Should().Be(1);
      matches[0].Groups.Count.Should().Be(5);
      matches[0].Groups[1].Value.Should().Be(tx.ToString());
      matches[0].Groups[2].Value.Should().Be(txUnit);
      matches[0].Groups[3].Value.Should().Be(rx.ToString());
      matches[0].Groups[4].Value.Should().Be(rxUnit);
      (ConvertToGigabytes(matches[0].Groups[1].Value, txUnit) + ConvertToGigabytes(matches[0].Groups[3].Value, rxUnit)).Should().Be(total);
    }

    private static decimal ConvertToGigabytes(string value, string unit)
    {
      var result = decimal.Parse(value);
      switch (unit)
      {
        case "KB":
          result *= 1024;
          break;
        case "MB":
          result *= 1024 * 1024;
          break;
        case "GB":
          result *= 1024 * 1024 * 1024;
          break;
        case "Bytes":
        default:
          result = 1;
          break;
      }

      return Math.Round(result /= 1024 * 1024 * 1024, 3);
    }
  }
}
