using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        using (Client client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public void ShouldTerminateWithAColon()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          Assert.IsTrue(client.TerminatedRead(":", TimeSpan.FromMilliseconds(TimeoutMs)).EndsWith(":"));
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public void ShouldBePromptingForAccount()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          string s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(TimeoutMs));
          Assert.IsTrue(s.Contains("Account:"));
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public void ShouldBePromptingForPassword()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          string s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(TimeoutMs));
          Assert.IsTrue(s.Contains("Account:"));
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
        using (Client client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
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
        using (Client client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          Assert.AreEqual(client.TryLogin("username", "password", TimeoutMs), true);
          client.WriteLine("show statistic wan2");
          string s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(TimeoutMs));
          Assert.IsTrue(s.Contains(">"));
          Assert.IsTrue(s.Contains("WAN2"));
        }
      }
    }

    [TestMethod, Timeout(5000)]
    public void ShouldLogin()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          Assert.AreEqual((client.TryLogin("username", "password", TimeoutMs)), true);
        }
      }
    }
  }
}
