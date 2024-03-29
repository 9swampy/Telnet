[![Build, Publish, Tag](https://github.com/9swampy/Telnet/actions/workflows/build.yml/badge.svg)](https://github.com/9swampy/Telnet/actions/workflows/build.yml)
[![][nuget-img]][nuget]

[nuget]:     https://badge.fury.io/nu/telnet
[nuget-img]: https://badge.fury.io/nu/telnet.svg

# Telnet
We needed to issue simple commands to (and monitor) a router remotely and that's what we've been able to achieve with this 
little library. It's usage is simple, it doesn't do anything particularly fancy but if all you're wanting to do is interact
with a telnet server by issuing commands, parsing text responses and having the opportunity to respond in code accordingly
then hopefully having taken the time to wrap this up in a simple NuGet package will make your life easier!

FYI I'm happy to take merge requests if they meet incumbent standards, don't break anything and accomodate the code file
sharing so all versions work (and likely receive the same change). I'm no TCPIP/Telnet expert but if I can help out with
bugs or feature requests I'll do what I can, outside my day job ofc!


## Release Notes
v0.9 introduced a significant bump to Net versions and some potentially breaking code changes. Changes to your consuming
code should be minimal, but be prepared to test thoroughly. Please raise an Issue if you encounter any problems; in part
it'll help others with the same problems and we'll see what we can do to accomodate your use cases.

v0.10 introduced an unintended dependency that's removed as of 0.11.2.

## Most common functions in C# NuGet
Depending on which version of this library you use, you may or may not have access to asynchronous functionality. Wherever possible we would recommend using the async overloads.
 - Client - ```var client = new Client(\<ip string>, \<port#>, new CancellationToken()));```
    + Needed to setup the connection 
 - TryLogin - ```bool result = await client.TryLoginAsync(\<username>, \<password>, \<Timout ms>,  \<lineFeed default='\\n'>)```
    + Most Telnet connections are required to login; this function handles that and lets you know if it was successful. Please note it is just a wrapper for Read and Write calls to cover the most common scenario. If it doesn't work, roll your own TerminatedReads and Writes to service your specific UseCase.
      - Try login uses a 'terminator' (the character after which you start writing commands) to detect if the login was successful (default is ```>```).         The terminator character is found after login and you can begin entering commands, for example, ```> configure``` - '>' is the terminator.  
        If the terminator/linestarter is different to '>', you will need to specify it as a extra parameter. ```client.TryLoginAsync(\<username>, \<password>, \<Timout ms>, \<terminator>, \<lineFeed default='\\n'>)```  
 - TerminatedRead - ```string s = await client.TerminatedReadAsync(\<value>, \<Timeout ms>)```
    + Will read until it sees the specified value (usually just use the terminator/```>``` as the value to get started)
 - WriteLine - ```await client.WriteLineAsync(\<value>, \<Timeout ms>```
    + Will write the value to the session

## Debugging telnet connections
Wireshark is a good way to go about checking what is being sent back and forth - you can see the transmissions in plain text. 
Filter recording entries by 'telnet'.

## Usage Examples:
.NET Fiddle https://dotnetfiddle.net/S9Ii6n

### Asynchronous C#
```csharp
    namespace PrimS.Telnet.CiTests
    {
      using System;
      using FluentAssertions;
      using System.Threading;
      using System.Threading.Tasks;
      using System.Text.RegularExpressions;
      using Xunit;

      public class ReadMeExampleFixture
      {
        public const string Pattern =
          "(?:WAN2 total TX: )([0-9.]*) " +
          "((?:[KMG]B)|(?:Bytes))" +
          "(?:[, ]*RX: )([0-9.]*) " +
          "((?:[KMG]B)|(?:Bytes))";
        private const int TimeoutMs = 5000;

        [Fact]
        public async Task ReadMeExample()
        {
          using (var server = new DummyTelnetServer())
          {
            using (var client = new Client(
              server.IPAddress.ToString(),
              server.Port,
              new CancellationToken()))
            {
              client.IsConnected.Should().Be(true);
              Client.IsWriteConsole = false;
              (await client.TryLoginAsync(
                "username",
                "password",
                TimeoutMs)).Should().Be(true);
              await client.WriteLineAsync("show statistic wan2");
              string s = await client.TerminatedReadAsync(
                ">",
                TimeSpan.FromMilliseconds(TimeoutMs));
              s.Should().Contain(">");
              s.Should().Contain("WAN2");
              var regEx = new Regex(Pattern);
              regEx.IsMatch(s).Should().Be(true);
              MatchCollection matches = regEx.Matches(s);
              matches.Count.Should().Be(1);
              matches[0].Captures.Count.Should().Be(1);
              matches[0].Groups.Count.Should().Be(5);
              matches[0].Groups[0].Value.Should().Be(
                "WAN2 total TX: 6.3 GB ,RX: 6.9 GB");
              matches[0].Groups[1].Value.Should().Be("6.3");
              matches[0].Groups[2].Value.Should().Be("GB");
              matches[0].Groups[3].Value.Should().Be("6.9");
              matches[0].Groups[4].Value.Should().Be("GB");
            }
          }
        }
      }
    }
```
### Synchronous C#
```csharp
    namespace PrimS.Telnet.CiTests
    {
      using FluentAssertions;
      using Xunit;
      using System;
      using System.Text.RegularExpressions;

      [TestClass]
      public class ReadMeExampleFixture
      {
        public const string Pattern =
          "(?:WAN2 total TX: )([0-9.]*) " +
          "((?:[KMG]B)|(?:Bytes))" +
          "(?:[, ]*RX: )([0-9.]*) " +
          "((?:[KMG]B)|(?:Bytes))";
        private const int TimeoutMs = 5000;

        [Fact]
        public void ReadMeExample()
        {
          using (var server = new DummyTelnetServer())
          {
            using (var client = new Client(
              server.IPAddress.ToString(),
              server.Port,
              new CancellationToken()))
            {
              client.IsConnected.Should().Be(true);
              Client.IsWriteConsole = false;
              client.TryLogin(
                "username",
                "password",
                TimeoutMs).Should().Be(true);
              client.WriteLine("show statistic wan2");
              string s = client.TerminatedRead(
                ">",
                TimeSpan.FromMilliseconds(TimeoutMs));
              s.Should().Contain(">");
              s.Should().Contain("WAN2");
              var regEx = new Regex(Pattern);
              regEx.IsMatch(s).Should().Be(true);
              MatchCollection matches = regEx.Matches(s);
              matches.Count.Should().Be(1);
              matches[0].Captures.Count.Should().Be(1);
              matches[0].Groups.Count.Should().Be(5);
              matches[0].Groups[0].Value.Should().Be(
                "WAN2 total TX: 6.3 GB ,RX: 6.9 GB");
              matches[0].Groups[1].Value.Should().Be("6.3");
              matches[0].Groups[2].Value.Should().Be("GB");
              matches[0].Groups[3].Value.Should().Be("6.9");
              matches[0].Groups[4].Value.Should().Be("GB");
            }
          }
        }
      }
    }
```

### Visual Basic
```vbnet
    Private Async Function RunRemoteScript(
      commandLine As String) As Task(Of Boolean)
        Using telnet = New Client(
          "HostName",
          23,
          _cancellationSource.Token)
            If Not telnet.IsConnected Then Return False
            Dim loggedOn = Await telnet.TryLoginAsync(
              "username",
              "password",
              SocketTimeout, "#"))
            If Not loggedOn Then Return False
            Await telnet.WriteLine(commandLine)
            Dim serverResponse = Await telnet.TerminatedReadAsync(
              "#",
              TimeSpan.FromMilliseconds(SocketTimeout))
            Debug.Print(serverResponse)
            Await telnet.WriteLine("exit")
            Dim logoutMessage = Await telnet.ReadAsync(
              New TimeSpan(100))
            Debug.Print(logoutMessage)
        End Using
        Return True  ' If we got this far; celebrate
    End Function
