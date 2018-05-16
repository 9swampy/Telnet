[![][build-img]][build]
[![][nuget-img]][nuget]

[build]:     https://ci.appveyor.com/project/9swampy/telnet
[build-img]: https://ci.appveyor.com/api/projects/status/a85v943mb52y0gii?svg=true

[nuget]:     https://badge.fury.io/nu/telnet
[nuget-img]: https://badge.fury.io/nu/telnet.svg

Telnet
======



I needed to issue simple commands to (and monitor) a router remotely and that's what I've been able to achieve with this 
little library. It's usage is simple, it doesn't do anything particularly fancy but if all you're wanting to do is interact
with a telnet server by issuing commands, parsing text responses and having the opportunity to respond in code accordingly then hopefully having taken the time to wrap this up in a simple NuGet package will make your life easier!

FWIW I'm happy to take merge requests and if I can help out with bugs or feature requests I'll do what I can, outside my day job ofc!

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
          Regex regEx = new Regex("(?!WAN2 total TX: )([0-9.]*)(?! GB ,RX: )([0-9.]*)(?= GB)");
          regEx.IsMatch(s).Should().Be(true);
          MatchCollection matches = regEx.Matches(s);
          decimal tx = decimal.Parse(matches[0].Value);
          decimal rx = decimal.Parse(matches[1].Value);
          (tx + rx).Should().BeLessThan(50);
        }
      }
    }

```VB.NET
    Private Async Function RunRemoteScript(commandLine As String) As Task(Of Boolean)
        Using telnet = New Client("HostName", 23, _cancellationSource.Token)
            If Not telnet.IsConnected Then Return False
            Dim loggedOn = Await telnet.TryLoginAsync("username", "password", SocketTimeout, "#"))
            If Not loggedOn Then Return False
            Await telnet.WriteLine(commandLine)
            Dim serverResponse = Await telnet.TerminatedReadAsync("#", TimeSpan.FromMilliseconds(SocketTimeout))
            Debug.Print(serverResponse)
            Await telnet.WriteLine("exit")
            Dim logoutMessage = Await telnet.ReadAsync(New TimeSpan(100))
            Debug.Print(logoutMessage)
        End Using
        Return True  ' If we got this far; celebrate
    End Function
