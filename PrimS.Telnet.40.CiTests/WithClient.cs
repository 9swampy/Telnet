using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PrimS.Telnet.CiTests
{
  [TestClass]
  public class WithClient
  {
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
          string s = client.TerminatedRead(":");
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
          string s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(10000));
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
          string s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(1000));
          s.Should().Contain("Account:");
          client.WriteLine("username");
          s = client.TerminatedRead("Password:", TimeSpan.FromMilliseconds(1000));
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
          string s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(1000));
          client.WriteLine("username");
          s = client.TerminatedRead("Password:", TimeSpan.FromMilliseconds(1000));
          client.WriteLine("Hearth5t0ne");
          s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(1000));
        }
      }
    }

    [TestMethod, Timeout(3000)]
    public void ShouldRespondWithWan2Info()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          string s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(1000));
          s.Should().Contain("Account:");
          client.WriteLine("username");
          s = client.TerminatedRead("Password:", TimeSpan.FromMilliseconds(1000));
          s.Should().Contain("Password:");
          client.WriteLine("password");
          s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(1000));
          s.Should().Contain(">");
          client.WriteLine("show statistic wan2");
          s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(1000));
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }

    [TestMethod, Timeout(3000)]
    public void ShouldLogin()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (client.TryLogin("username", "password", 1000)).Should().Be(true);
        }
      }
    }
  }
}
