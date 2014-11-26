Telnet
======



I needed to issue simple commands to, and monitor, a router remotely and that's what I've been able to achieve with this 
little library. It's usage is simple, it doesn't do anything particularly fancy but if all you're wanting to do is interact
with a telnet server, issuing commands, parsing text responses and respond accordingly then hopefully having taken the time to wrap this up in a simple NuGet package will make your life easier!



Usage:
```C#
    [TestMethod]
    public async Task ReadmeExample()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (Client client = new Client(server.IPAddress.ToString(), server.Port, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
          (await client.TryLoginAsync("username", "password", TimeoutMs)).Should().Be(true);
          client.WriteLine("show statistic wan2");
          string s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(TimeoutMs));
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