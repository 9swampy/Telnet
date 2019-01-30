namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Linq;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using FluentAssertions;
  using System.Threading.Tasks;
  using System.Text.RegularExpressions;
  using System.Diagnostics.CodeAnalysis;

  [ExcludeFromCodeCoverage]
  [TestClass]
  public class WithClient
  {
    private const int timeoutMs = 100;

    [TestMethod]
    public void ShouldConnect()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public async Task ShouldTerminateWithAColon()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          string s = await client.TerminatedReadAsync(":", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().EndWith(":");
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public async Task ShouldBePromptingForAccount()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          string s = await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain("Account:");
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public async Task ShouldBePromptingForPassword()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          string s = await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain("Account:");
          await client.WriteLine("username");
          s = await client.TerminatedReadAsync("Password:", TimeSpan.FromMilliseconds(timeoutMs));
        }
      }
    }

    [TestMethod, Timeout(3000)]
    public async Task ShouldPromptForInput()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(timeoutMs));
          await client.WriteLine("username");
          await client.TerminatedReadAsync("Password:", TimeSpan.FromMilliseconds(timeoutMs));
          await client.WriteLine("password");
          await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(timeoutMs));
        }
      }
    }

    [TestMethod, Timeout(5000)]
    public async Task ShouldRespondWithWan2Info()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", 1500)).Should().Be(true);
          await client.WriteLine("show statistic wan2");
          string s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }

    [TestMethod, Timeout(5000)]
    public async Task ShouldRespondWithWan2InfoCrLf()
    {
      using (var server = new TelnetServerRFC854())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", 1500, lineFeed: "\r\n")).Should().Be(true);
          await client.WriteLine("show statistic wan2", linefeed: "\r\n");
          string s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }

    [TestMethod, Timeout(5000)]
    public async Task ShouldRespondWithWan2InfoRegexTerminated()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", 1500)).Should().Be(true);
          await client.WriteLine("show statistic wan2");
          string s = await client.TerminatedReadAsync(new Regex(".*>$"), TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }

    [TestMethod, Timeout(3000)]
    public async Task ShouldLogin()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", timeoutMs)).Should().Be(true);
        }
      }
    }


    [TestMethod, Timeout(5000)]
    public async Task ShouldLoginCrLf()
    {
      using (var server = new TelnetServerRFC854())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", timeoutMs, lineFeed: "\r\n")).Should().Be(true);
        }
      }
    }

    [TestMethod]
    public async Task ReadmeExample()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", timeoutMs)).Should().Be(true);
          await client.WriteLine("show statistic wan2");
          string s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
          System.Text.RegularExpressions.Regex regEx = new System.Text.RegularExpressions.Regex("(?!WAN2 total TX: )([0-9.]*)(?! GB ,RX: )([0-9.]*)(?= GB)");
          regEx.IsMatch(s).Should().Be(true);
          MatchCollection matches = regEx.Matches(s);
          decimal tx = decimal.Parse(matches[0].Value);
          decimal rx = decimal.Parse(matches[1].Value);
          (tx + rx).Should().BeLessThan(50);
        }
      }
    }
  }
}
