using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Threading.Tasks;

namespace PrimS.Telnet.CiTest
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
    public async Task ShouldTerminateWithASemiColon()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          string s = await client.TerminatedReadAsync(":");
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
          string s = await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(10000));
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
          string s = await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(1000));
          s.Should().Contain("Account:");
          client.WriteLine("username");
          s = await client.TerminatedReadAsync("Password:", TimeSpan.FromMilliseconds(1000));
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
          string s = await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(1000));
          client.WriteLine("username");
          s = await client.TerminatedReadAsync("Password:", TimeSpan.FromMilliseconds(1000));
          client.WriteLine("Hearth5t0ne");
          s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(1000));
        }
      }
    }

    [TestMethod, Timeout(3000)]
    public async Task ShouldRespondWithWan2Info()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          string s = await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(1000));
          s.Should().Contain("Account:");
          client.WriteLine("username");
          s = await client.TerminatedReadAsync("Password:", TimeSpan.FromMilliseconds(1000));
          s.Should().Contain("Password:");
          client.WriteLine("password");
          s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(1000));
          s.Should().Contain(">");
          client.WriteLine("show statistic wan2");
          s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(1000));
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
          (await client.TryLoginAsync("username", "password", 1000)).Should().Be(true);
        }
      }
    }
  }
}
