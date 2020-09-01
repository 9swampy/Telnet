namespace PrimS.Telnet.CiTests
{
  using FluentAssertions;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using System;
  using System.Diagnostics.CodeAnalysis;

  [ExcludeFromCodeCoverage]
  [TestClass]
  public class WithClient
  {
    private const int timeoutMs = 500;

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
    public void ShouldTerminateWithAColon()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          client.TerminatedRead(":", TimeSpan.FromMilliseconds(timeoutMs)).Should().EndWith(":");
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public void ShouldBePromptingForAccount()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          string s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain("Account:");
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public void ShouldBePromptingForPassword()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          string s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain("Account:");
          client.WriteLine("username");
          s = client.TerminatedRead("Password:", TimeSpan.FromMilliseconds(timeoutMs));
        }
      }
    }

    [TestMethod, Timeout(3000)]
    public void ShouldPromptForInput()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
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

    [TestMethod, Timeout(5000)]
    public void ShouldRespondWithWan2Info()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (client.TryLogin("username", "password", timeoutMs)).Should().Be(true);
          client.WriteLine("show statistic wan2");
          string s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }

    [TestMethod, Timeout(5000)]
    public void ShouldRespondWithWan2InfoCrLf()
    {
      using (var server = new TelnetServerRFC854())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (client.TryLogin("username", "password", timeoutMs, linefeed: "\r\n")).Should().Be(true);
          client.WriteLine("show statistic wan2", linefeed: "\r\n");
          string s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(timeoutMs));
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }

    [TestMethod, Timeout(5000)]
    public void ShouldLogin()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          client.TryLogin("username", "password", timeoutMs).Should().Be(true);
        }
      }
    }

    [TestMethod, Timeout(5000)]
    public void ShouldLoginCrLf()
    {
      using (var server = new TelnetServerRFC854())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          client.TryLogin("username", "password", timeoutMs, linefeed: "\r\n").Should().Be(true);
        }
      }
    }
  }
}
