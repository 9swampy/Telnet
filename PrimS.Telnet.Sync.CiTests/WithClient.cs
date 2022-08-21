namespace PrimS.Telnet.Sync.CiTests
{
  using FluentAssertions;
  using Xunit;
  using System;
  using System.Diagnostics.CodeAnalysis;

  [ExcludeFromCodeCoverage]
  public class WithClient
  {
    private const int timeoutMs = 500;

    [Fact]
    public void ShouldConnect()
    {
      using (var server = new DummyTelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
        }
      }
    }

    [Fact(Timeout = 2000)]
    public void ShouldTerminateWithAColon()
    {
      using (var server = new DummyTelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          client.TerminatedRead(":", TimeSpan.FromMilliseconds(timeoutMs)).Should().EndWith(":");
        }
      }
    }

    [Fact(Timeout = 2000)]
    public void ShouldBePromptingForAccount()
    {
      using (var server = new DummyTelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          var s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain("Account:");
        }
      }
    }

    [Fact(Timeout = 2000)]
    public void ShouldBePromptingForPassword()
    {
      using (var server = new DummyTelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          var s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain("Account:");
          client.WriteLine("username");
          s = client.TerminatedRead("Password:", TimeSpan.FromMilliseconds(timeoutMs));
        }
      }
    }

    [Fact(Timeout = 3000)]
    public void ShouldPromptForInput()
    {
      using (var server = new DummyTelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(timeoutMs));
          client.WriteLine("username");
          client.TerminatedRead("Password:", TimeSpan.FromMilliseconds(timeoutMs));
          client.WriteLine("password");
          client.TerminatedRead(">", TimeSpan.FromMilliseconds(timeoutMs));
        }
      }
    }

    [Fact(Timeout = 5000)]
    public void ShouldRespondWithWan2Info()
    {
      using (var server = new DummyTelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (client.TryLogin("username", "password", timeoutMs)).Should().Be(true);
          client.WriteLine("show statistic wan2");
          var s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }

    [Fact(Timeout = 5000)]
    public void ShouldRespondWithWan2InfoCrLf()
    {
      using (var server = new DummyTelnetServerRFC854())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (client.TryLogin("username", "password", timeoutMs, linefeed: "\r\n")).Should().Be(true);
          client.WriteLine("show statistic wan2", linefeed: "\r\n");
          var s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }

    [Fact(Timeout = 5000)]
    public void ShouldLogin()
    {
      using (var server = new DummyTelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          client.TryLogin("username", "password", timeoutMs).Should().Be(true);
        }
      }
    }

    [Fact(Timeout = 5000)]
    public void ShouldLoginCrLf()
    {
      using (var server = new DummyTelnetServerRFC854())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          client.TryLogin("username", "password", timeoutMs, linefeed: "\r\n").Should().Be(true);
        }
      }
    }
  }
}
