namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Text.RegularExpressions;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using FluentAssertions;

  [TestClass]
  public class WithClient
  {
    private const int timeoutMs = 500;

    [TestMethod]
    public void ShouldConnect()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public void ShouldTerminateWithAColon()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          Assert.IsTrue(client.TerminatedRead(":", TimeSpan.FromMilliseconds(timeoutMs)).EndsWith(":"));
        }
      }
    }

    [TestMethod]
    public void ShouldRegexMatchColonTerminated()
    {
      var regex = new Regex(".*(:)$");
      regex.IsMatch("blah:").Should().BeTrue();
    }

    [TestMethod]
    public void ShouldRegexMatchTwoLines()
    {
      var content = string.Format("blah:{0}blah", Environment.NewLine);
      var regex = new Regex("^.*$", RegexOptions.Multiline);
      regex.IsMatch(content).Should().BeTrue();
      regex.Matches(content).Count.Should().Be(2);
    }

    [TestMethod]
    public void ShouldRegexMatchColonTerminatedFirstMultilineSlashRSlashNTerminated()
    {
      var content = string.Format("blah:{0}blah", "\r\n");
      var regex = new Regex(".*:\r?$", RegexOptions.Multiline);
      regex.IsMatch(content).Should().BeTrue();
      regex.Matches(content).Count.Should().Be(1);
    }

    [TestMethod]
    public void ShouldRegexMatchColonTerminatedFirstMultilineNewLineTerminated()
    {
      var content = string.Format("blah:{0}blah", Environment.NewLine);
      var regex = new Regex(".*:\r?$", RegexOptions.Multiline);
      regex.IsMatch(content).Should().BeTrue();
      regex.Matches(content).Count.Should().Be(1);
    }

    [TestMethod]
    public void ShouldRegexMatchColonTerminatedFirstMultilineSlashNTerminated()
    {
      var content = string.Format("blah:{0}blah", "\n");
      var regex = new Regex(".*:$", RegexOptions.Multiline);
      regex.IsMatch(content).Should().BeTrue();
      regex.Matches(content).Count.Should().Be(1);
    }

    [TestMethod]
    public void ShouldRegexMatchColonTerminatedLastMultiline()
    {
      var regex = new Regex(".*(:)$", RegexOptions.Multiline);
      regex.IsMatch(string.Format("blah{0}blah:", Environment.NewLine)).Should().BeTrue();
    }

    [TestMethod, Timeout(2000)]
    public void ShouldRegexMatchWithAColon()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          var regex = new Regex(".*:\r?$", RegexOptions.Multiline);
          Assert.IsTrue(client.TerminatedRead(regex, TimeSpan.FromMilliseconds(timeoutMs)).EndsWith(":"));
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public void ShouldBePromptingForAccount()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          var s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(timeoutMs));
          Assert.IsTrue(s.Contains("Account:"));
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public void ShouldRegexMatchPromptingForAccount()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          var regex = new Regex(".*Account:\r?$", RegexOptions.Multiline);
          var s = client.TerminatedRead(regex, TimeSpan.FromMilliseconds(timeoutMs));
          Assert.IsTrue(s.Contains("Account:"));
        }
      }
    }

    [TestMethod, Timeout(2000)]
    public void ShouldBePromptingForPassword()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          var s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(timeoutMs));
          Assert.IsTrue(s.Contains("Account:"));
          client.WriteLine("username");
          s = client.TerminatedRead("Password:", TimeSpan.FromMilliseconds(timeoutMs));
        }
      }
    }

    [TestMethod, Timeout(3000)]
    public void ShouldPromptForInput()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
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
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          Assert.AreEqual(client.TryLogin("username", "password", timeoutMs), true);
          client.WriteLine("show statistic wan2");
          var s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(timeoutMs));
          Assert.IsTrue(s.Contains(">"));
          Assert.IsTrue(s.Contains("WAN2"));
        }
      }
    }

    [TestMethod, Timeout(5000)]
    public void ShouldRespondWithWan2InfoCrLf()
    {
      using (var server = new TelnetServerRFC854())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          Assert.AreEqual(client.TryLogin("username", "password", timeoutMs, linefeed: "\r\n"), true);
          client.WriteLine("show statistic wan2", "\r\n");
          var s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(timeoutMs));
          Assert.IsTrue(s.Contains(">"));
          Assert.IsTrue(s.Contains("WAN2"));
        }
      }
    }

    [TestMethod, Timeout(5000)]
    public void ShouldLogin()
    {
      using (var server = new TelnetServer())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          Assert.AreEqual((client.TryLogin("username", "password", timeoutMs)), true);
        }
      }
    }

    [TestMethod, Timeout(5000)]
    public void ShouldLoginCrLf()
    {
      using (var server = new TelnetServerRFC854())
      {
        using (var client = new Client(server.IPAddress.ToString(), server.Port))
        {
          Assert.AreEqual(client.IsConnected, true);
          Assert.AreEqual((client.TryLogin("username", "password", timeoutMs, linefeed: "\r\n")), true);
        }
      }
    }
  }
}
