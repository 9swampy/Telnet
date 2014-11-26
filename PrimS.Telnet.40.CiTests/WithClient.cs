using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PrimS.Telnet.CiTests
{
  [TestClass]
  public class WithClient
  {
    private const int TimeoutMs = 500;

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
    public void ShouldTerminateWithASemiColon()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          client.TerminatedRead(":", TimeSpan.FromMilliseconds(TimeoutMs)).Should().EndWith(":");
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
          string s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(TimeoutMs));
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
          string s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(TimeoutMs));
          s.Should().Contain("Account:");
          client.WriteLine("username");
          s = client.TerminatedRead("Password:", TimeSpan.FromMilliseconds(TimeoutMs));
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
          string s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(TimeoutMs));
          client.WriteLine("username");
          s = client.TerminatedRead("Password:", TimeSpan.FromMilliseconds(TimeoutMs));
          client.WriteLine("password");
          s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(TimeoutMs));
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
          (client.TryLogin("username", "password", TimeoutMs)).Should().Be(true);
          client.WriteLine("show statistic wan2");
          string s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(TimeoutMs));
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
          (client.TryLogin("username", "password", TimeoutMs)).Should().Be(true);
        }
      }
    }
  }
}
