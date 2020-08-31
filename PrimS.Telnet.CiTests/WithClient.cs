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
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public async Task ShouldTerminateWithAColon()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          var s = await client.TerminatedReadAsync(":", TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
          s.Should().EndWith(":");
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public async Task ShouldBePromptingForAccount()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          var s = await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
          s.Should().Contain("Account:");
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public async Task ShouldBePromptingForPassword()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          var s = await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
          s.Should().Contain("Account:");
          await client.WriteLine("username").ConfigureAwait(false);
          s = await client.TerminatedReadAsync("Password:", TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
        }
      }
    }

    [TestMethod, Timeout(3000)]
    public async Task ShouldPromptForInput()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
          await client.WriteLine("username").ConfigureAwait(false);
          await client.TerminatedReadAsync("Password:", TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
          await client.WriteLine("password").ConfigureAwait(false);
          await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
        }
      }
    }

    [TestMethod, Timeout(5000)]
    public async Task ShouldRespondWithWan2Info()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", 1500).ConfigureAwait(false)).Should().Be(true);
          await client.WriteLine("show statistic wan2").ConfigureAwait(false);
          var s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
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
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", 1500, linefeed: "\r\n").ConfigureAwait(false)).Should().Be(true);
          await client.WriteLine("show statistic wan2", linefeed: "\r\n").ConfigureAwait(false);
          var s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }

    [TestMethod, Timeout(5000)]
    public async Task ShouldRespondWithWan2InfoRegexTerminated()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", 1500).ConfigureAwait(false)).Should().Be(true);
          await client.WriteLine("show statistic wan2").ConfigureAwait(false);
          var s = await client.TerminatedReadAsync(new Regex(".*>$"), TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }

    [TestMethod, Timeout(3000)]
    public async Task ShouldLogin()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", timeoutMs).ConfigureAwait(false)).Should().Be(true);
        }
      }
    }


    [TestMethod, Timeout(5000)]
    public async Task ShouldLoginCrLf()
    {
      using (var server = new TelnetServerRFC854())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", timeoutMs, linefeed: "\r\n").ConfigureAwait(false)).Should().Be(true);
        }
      }
    }

    [TestMethod]
    public async Task ReadmeExample()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", timeoutMs).ConfigureAwait(false)).Should().Be(true);
          await client.WriteLine("show statistic wan2").ConfigureAwait(false);
          var s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
          var regEx = new System.Text.RegularExpressions.Regex("(?!WAN2 total TX: )([0-9.]*)(?! GB ,RX: )([0-9.]*)(?= GB)");
          regEx.IsMatch(s).Should().Be(true);
          var matches = regEx.Matches(s);
          var tx = decimal.Parse(matches[0].Value);
          var rx = decimal.Parse(matches[1].Value);
          (tx + rx).Should().BeLessThan(50);
        }
      }
    }
  }
}
