namespace PrimS.Telnet.Sync.CiTests
{
  using FluentAssertions;
  using Xunit;
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using System.Text;
  using System.Text.RegularExpressions;
#if ASYNC
  using System.Threading.Tasks;
  using System.Threading;
#endif

  public class WithDummyByteStream
  {
    private const int timeoutMs = 500;

    [Fact]
    public void ShouldConnect()
    {
      using (var stream = new DummyByteStream())
      {
        using (var client = new Client(stream, new System.Threading.CancellationToken()))
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
        using (var client = new Client(stream, new System.Threading.CancellationToken()))
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
        using (var client = new Client(stream, new System.Threading.CancellationToken()))
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
        using (var client = new Client(stream, new System.Threading.CancellationToken()))
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
        using (var client = new Client(stream, new System.Threading.CancellationToken()))
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
        using (var client = new Client(stream, new System.Threading.CancellationToken()))
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
       ShouldRespondWithWan2InfoCrLf()
    {
      using (var stream = new DummyByteStream("\r\n"))
      {
        using (var client = new Client(stream, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
#if ASYNC
          (await (client.TryLoginAsync("username", "password", timeoutMs, linefeed: "\r\n"))).Should().Be(true);
          await client.WriteLineAsync("show statistic wan2", linefeed: "\r\n");
          var s = await client.TerminatedReadAsync(">", TimeSpan.FromMilliseconds(timeoutMs));
#else
          (client.TryLogin("username", "password", timeoutMs, linefeed: "\r\n")).Should().Be(true);
          client.WriteLine("show statistic wan2", linefeed: "\r\n");
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
        using (var client = new Client(stream, new System.Threading.CancellationToken()))
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
      using (var stream = new DummyByteStream("\r\n"))
      {
        using (var client = new Client(stream, new System.Threading.CancellationToken()))
        {
          client.IsConnected.Should().Be(true);
#if ASYNC
          (await client.TryLoginAsync("username", "password", timeoutMs, linefeed: "\r\n")).Should().Be(true);
#else
          client.TryLogin("username", "password", timeoutMs, linefeed: "\r\n").Should().Be(true);
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
        using (var client = new Client(stream, new System.Threading.CancellationToken()))
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

  public class DummyByteStream : IByteStream
  {
    private readonly Queue<byte> buffer = new Queue<byte>();
    private bool isErrored = false;
    private readonly Queue<WriteHandlerBase> handlers;

    /// <summary>
    /// Supplies the linefeed character expected: default = "\n")
    /// </summary>
    public DummyByteStream()
      : this("\n")
    { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="linefeed">The linefeed character expected: default = "\n")</param>
    public DummyByteStream(string linefeed)
    {
      Connected = true;

      handlers = new Queue<WriteHandlerBase>();
      handlers.Enqueue(BuildNegotiationHandler());
      handlers.Enqueue(BuildUsernameHandler());
      handlers.Enqueue(BuildPasswordHandler());
      handlers.Enqueue(BuildGetStatisticsHandler());
      Console.WriteLine("Waiting for a connection...");
      Linefeed = linefeed;
    }

    private WriteHandler BuildGetStatisticsHandler()
    {
      return new WriteHandler(
                o => ByteStringConverter.ToString(o) == $"show statistic wan2{this.Linefeed}",
                () =>
                {
                  Console.WriteLine("Command entered, respond with WAN2 terminated reply");
                  Encoding.ASCII.GetBytes("show statistic wan2\n\r WAN1 total TX: 0 Bytes ,RX: 0 Bytes \n\r WAN2 total TX: 6.3 GB ,RX: 6.9 GB \n\r WAN3 total TX: 0 Bytes ,RX: 0 Bytes \n\r WAN4 total TX: 0 Bytes ,RX: 0 Bytes \n\r WAN5 total TX: 0 Bytes ,RX: 0 Bytes \n\r>").ToList().ForEach(o => this.buffer.Enqueue(o));
                  return;
                });
    }

    private WriteHandler BuildPasswordHandler()
    {
      return new WriteHandler(
                o => ByteStringConverter.ToString(o) == $"password{this.Linefeed}",
                () =>
                {
                  Console.WriteLine("Password entered, respond with Command> prompt");
                  Encoding.ASCII.GetBytes("Command >").ToList().ForEach(o => this.buffer.Enqueue(o));
                  return;
                });
    }

    private WriteHandler BuildUsernameHandler()
    {
      return new WriteHandler(
              o => ByteStringConverter.ToString(o) == $"username{this.Linefeed}",
              () =>
              {
                Console.WriteLine("Account entered, respond with Password: prompt");
                Encoding.ASCII.GetBytes("Password:").ToList().ForEach(o => this.buffer.Enqueue(o));
                return;
              });
    }

    private WriteHandler BuildNegotiationHandler()
    {
      return new WriteHandler(
                bytes => Enumerable.SequenceEqual(bytes, Client.SuppressGoAheadBuffer),
                () =>
                {
                  Connected = true;
                  Console.WriteLine("Connection made, respond with Account: prompt");
                  Encoding.ASCII.GetBytes("Account:").ToList().ForEach(o => this.buffer.Enqueue(o));
                  return;
                });
    }

    public int Available => buffer.Count;

    public bool Connected { get; set; } = false;

    public int ReceiveTimeout { get; set; } = 0;

    public string Linefeed { get; }

    public void Close()
    {
      buffer.Clear();
      Connected = false;
    }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    {
      this.Close();
    }

    public int ReadByte()
    {
      return this.buffer.Dequeue();
    }

    public
#if ASYNC
      Task WriteAsync
#else
      void Write
#endif
      (byte[] buffer, int offset, int count
#if ASYNC
      , CancellationToken cancellationToken
#endif
      )
    {
      if (isErrored)
        return
#if ASYNC
          Task.CompletedTask
#endif
          ;

      var bufferEnum = buffer.Skip(offset).Take(count);
      var bufferString = ByteStringConverter.ToString(buffer, offset, count);

      if (handlers.Peek().Check(buffer))
      {
        handlers.Dequeue().Handle();
      }
      else
      {
        isErrored = true;
        Connected = false;
        throw new NotImplementedException();
      }

#if ASYNC
      return Task.CompletedTask;
#endif
    }

    public
#if ASYNC
      async Task WriteAsync
#else
      void Write
#endif
      (string value
#if ASYNC
      , CancellationToken cancellationToken
#endif
      )
    {
      var buffer = ByteStringConverter.ConvertStringToByteArray(value);
#if ASYNC
      await WriteAsync
#else
      Write
#endif
        (buffer, 0, buffer.Length
#if ASYNC
      , cancellationToken
#endif
        );
    }

    public
#if ASYNC
      async Task WriteByteAsync
#else
      void WriteByte
#endif
      (byte value
#if ASYNC
      , CancellationToken cancellationToken
#endif
      )
    {
#if ASYNC
      await WriteAsync
#else
      Write
#endif
        (new byte[] { value }, 0, 1
#if ASYNC
      , cancellationToken
#endif
        );
    }
  }

  internal class WriteHandler : WriteHandlerBase
  {
    private readonly Action handler;

    public WriteHandler(Func<byte[], bool> check, Action handler)
    {
      Check = check;
      this.handler = handler;
    }

    public override Func<byte[], bool> Check { get; }

    public override void Handle()
    {
      handler();
    }
  }

  internal abstract class WriteHandlerBase
  {
    public abstract Func<byte[], bool> Check { get; }

    public abstract void Handle();
  }
}
