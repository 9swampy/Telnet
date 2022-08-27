namespace PrimS.Telnet.Sync.CiTests
{
  using FluentAssertions;
  using Xunit;
  using System;
  using System.Text.RegularExpressions;
  using System.Threading;
#if ASYNC
  using System.Threading.Tasks;
#endif

  public class WithDummyByteStream
  {
    private const int timeoutMs = 500;

    [Fact]
    public void ShouldConnect()
    {
      using (var stream = new DummyByteStream())
      {
        using (var client = new Client(stream, new CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
        }
      }
    }

    [Fact(Timeout = 2000)]
    public
#if ASYNC
      async Task
#else
      void
#endif
      ShouldTerminateWithAColon()
    {
      using (var stream = new DummyByteStream())
      {
        using (var client = new Client(stream, new CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
#if ASYNC
          (await client.TerminatedReadAsync(":", TimeSpan.FromMilliseconds(timeoutMs)))
#else
          client.TerminatedRead(":", TimeSpan.FromMilliseconds(timeoutMs))
#endif
            .Should().EndWith(":");
        }
      }
    }

    [Fact(Timeout = 2000)]
    public
#if ASYNC
      async Task
#else
      void
#endif
      ShouldBePromptingForAccount()
    {
      using (var stream = new DummyByteStream())
      {
        using (var client = new Client(stream, new CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
#if ASYNC
          var s = await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(timeoutMs));
#else
          var s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(timeoutMs));
#endif

          s.Should().Contain("Account:");
        }
      }
    }

    [Fact(Timeout = 2000)]
    public
#if ASYNC
      async Task
#else
      void
#endif
       ShouldBePromptingForPassword()
    {
      using (var stream = new DummyByteStream())
      {
        using (var client = new Client(stream, new CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
#if ASYNC
          var s = await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(timeoutMs));
#else
          var s = client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(timeoutMs));
#endif
          s.Should().Contain("Account:");
#if ASYNC
          await client.WriteLineAsync("username");
          s = await client.TerminatedReadAsync("Password:", TimeSpan.FromMilliseconds(timeoutMs));
#else
          client.WriteLine("username");
          s = client.TerminatedRead("Password:", TimeSpan.FromMilliseconds(timeoutMs));
#endif
        }
      }
    }

    [Fact(Timeout = 3000)]
    public
#if ASYNC
      async Task
#else
      void
#endif
       ShouldPromptForInput()
    {
      using (var stream = new DummyByteStream())
      {
        using (var client = new Client(stream, new CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
#if ASYNC
          await client.TerminatedReadAsync("Account:", TimeSpan.FromMilliseconds(timeoutMs));
          await client.WriteLineAsync("username");
          await client.TerminatedReadAsync("Password:", TimeSpan.FromMilliseconds(timeoutMs));
          await client.WriteLineAsync("password");
          await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(timeoutMs));
#else
          client.TerminatedRead("Account:", TimeSpan.FromMilliseconds(timeoutMs));
          client.WriteLine("username");
          client.TerminatedRead("Password:", TimeSpan.FromMilliseconds(timeoutMs));
          client.WriteLine("password");
          client.TerminatedRead(">", TimeSpan.FromMilliseconds(timeoutMs));
#endif
        }
      }
    }

    [Fact(Timeout = 5000)]
    public
#if ASYNC
      async Task
#else
      void
#endif
       ShouldRespondWithWan2Info()
    {
      using (var stream = new DummyByteStream())
      {
        using (var client = new Client(stream, new CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
#if ASYNC
          (await client.TryLoginAsync("username", "password", timeoutMs)).Should().Be(true);
          await client.WriteLineAsync("show statistic wan2");
          var s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(timeoutMs));
#else
          (client.TryLogin("username", "password", timeoutMs)).Should().Be(true);
          client.WriteLine("show statistic wan2");
          var s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(timeoutMs));
#endif
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }

    [Fact(Timeout = 5000)]
    public
#if ASYNC
      async Task
#else
      void
#endif
       ShouldRespondWithWan2InfoRfc854()
    {
      using (var stream = new DummyByteStream(Client.Rfc854LineFeed))
      {
        using (var client = new Client(stream, new CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
#if ASYNC
          (await (client.TryLoginAsync("username", "password", timeoutMs, lineFeed: Client.Rfc854LineFeed))).Should().Be(true);
          await client.WriteLineRfc854Async("show statistic wan2");
          var s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(timeoutMs));
#else
          (client.TryLogin("username", "password", timeoutMs, lineFeed: Client.Rfc854LineFeed)).Should().Be(true);
          client.WriteLineRfc854("show statistic wan2");
          var s = client.TerminatedRead(">", TimeSpan.FromMilliseconds(timeoutMs));
#endif
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }

    [Fact(Timeout = 5000)]
    public
#if ASYNC
      async Task
#else
      void
#endif
       ShouldLogin()
    {
      using (var stream = new DummyByteStream())
      {
        using (var client = new Client(stream, new CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
#if ASYNC
          (await client.TryLoginAsync("username", "password", timeoutMs)).Should().Be(true);
#else
          client.TryLogin("username", "password", timeoutMs).Should().Be(true);
#endif
        }
      }
    }

    [Fact(Timeout = 5000)]
    public
#if ASYNC
      async Task
#else
      void
#endif
       ShouldLoginCrLf()
    {
      using (var stream = new DummyByteStream(Client.Rfc854LineFeed))
      {
        using (var client = new Client(stream, new CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
#if ASYNC
          (await client.TryLoginAsync("username", "password", timeoutMs, lineFeed: Client.Rfc854LineFeed)).Should().Be(true);
#else
          client.TryLogin("username", "password", timeoutMs, lineFeed: Client.Rfc854LineFeed).Should().Be(true);
#endif
        }
      }
    }


    [Fact]
    public
#if ASYNC
      async Task
#else
      void
#endif
      ShouldRespondWithWan2InfoRegexTerminated()
    {
      using (var stream = new DummyByteStream())
      {
        using (var client = new Client(stream, new CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
#if ASYNC
          (await client.TryLoginAsync("username", "password", 1500).ConfigureAwait(false)).Should().Be(true);
          await client.WriteLineAsync("show statistic wan2").ConfigureAwait(false);
          var s = await client.TerminatedReadAsync(new Regex(".*>$"), TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
#else
          client.TryLogin("username", "password", 1500).Should().Be(true);
          client.WriteLine("show statistic wan2");
          var s = client.TerminatedRead(new Regex(".*>$"), TimeSpan.FromMilliseconds(timeoutMs));
#endif
          s.Should().Contain(">");
          s.Should().Contain("WAN2");
        }
      }
    }
  }
}
