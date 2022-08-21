#if NetStandard || NET6_0_OR_GREATER
namespace PrimS.Telnet.CiTests
#else
namespace PrimS.Telnet.Sync.CiTests
#endif
{
  using System;
  using System.Diagnostics;
  using System.Diagnostics.CodeAnalysis;
  using System.Threading;
#if ASYNC
  using System.Threading.Tasks;
#endif
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  [ExcludeFromCodeCoverage]
  public class ByteStreamHandlerTests
  {
    [Fact]
#if ASYNC
    public async Task UnconnectedByteStreamShouldReturnEmptyResponse()
#else
    public void UnconnectedByteStreamShouldReturnEmptyResponse()
#endif
    {
      using var sut = new ByteStreamHandler(A.Fake<IByteStream>());
#if ASYNC
      (await sut.ReadAsync(new TimeSpan()).ConfigureAwait(false)).Should().Be(string.Empty);
#else
      sut.Read(new TimeSpan()).Should().Be(string.Empty);
#endif
    }

    [Fact]
#if ASYNC
    public async Task ByteStreamShouldReturnEmptyResponse()
#else
    public void ByteStreamShouldReturnEmptyResponse()
#endif
    {
      var socket = A.Fake<ISocket>();
      using (var networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        var isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using var tcpByteStream = new TcpByteStream(socket);
        A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(-1);
        tcpByteStream.Connected.Should().BeTrue();
        using var sut = new ByteStreamHandler(tcpByteStream);

#if ASYNC
        var response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
#else
        var response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif

        response.Should().BeEmpty();
      }
    }

    [Fact]
#if ASYNC
    public async Task ByteStreamShouldReturnCharA()
#else
    public void ByteStreamShouldReturnCharA()
#endif
    {
      var socket = A.Fake<ISocket>();
      using (var networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        var isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using var tcpByteStream = new TcpByteStream(socket);
        A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(65);
        tcpByteStream.Connected.Should().BeTrue();
        using var sut = new ByteStreamHandler(tcpByteStream);

#if ASYNC
        var response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
#else
        var response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif

        response.Should().Be("A");
      }
    }

    [Fact]
#if ASYNC
    public async Task ByteStreamShouldReturnEscapedIac()
#else
    public void ByteStreamShouldReturnEscapedIac()
#endif
    {
      var socket = A.Fake<ISocket>();
      using (var networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        var isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using var tcpByteStream = new TcpByteStream(socket);
        A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, (int)Commands.InterpretAsCommand });
        tcpByteStream.Connected.Should().BeTrue();
        using var sut = new ByteStreamHandler(tcpByteStream);

#if ASYNC
        var response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
#else
        var response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif
        response.Should().Be("255");
      }
    }

    [Fact]
#if ASYNC
    public async Task ByteStreamShouldReturnEmpty()
#else
    public void ByteStreamShouldReturnEmpty()
#endif
    {
      var socket = A.Fake<ISocket>();
      using (var networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        var isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using var tcpByteStream = new TcpByteStream(socket);
        A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, -1 });
        tcpByteStream.Connected.Should().BeTrue();
        using var sut = new ByteStreamHandler(tcpByteStream);

#if ASYNC
        var response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
#else
        var response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif

        response.Should().BeEmpty();
      }
    }

    [Fact]
#if ASYNC
    public async Task WhenIacDoSgaByteStreamShouldReturnEmptyAndReplyIacWill()
#else
    public void WhenIacDoSgaByteStreamShouldReturnEmptyAndReplyIacWill()
#endif
    {
      var socket = A.Fake<ISocket>();
      using (var networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        var isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using var tcpByteStream = new TcpByteStream(socket);
        A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, (int)Commands.Do, (int)Options.SuppressGoAhead });
        tcpByteStream.Connected.Should().BeTrue();
        using var sut = new ByteStreamHandler(tcpByteStream);

#if ASYNC
        var response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
        A.CallTo(() => networkStream.WriteAsync(A<byte[]>.Ignored, 0, 3, A<CancellationToken>.Ignored))
                            .WhenArgumentsMatch(o => o[0] is byte[] param && param[0] == (byte)Commands.InterpretAsCommand && param[1] == (byte)Commands.Will && param[2] == 3)
                            .MustHaveHappened();
#else
        var response = sut.Read(TimeSpan.FromMilliseconds(10));
        A.CallTo(() => networkStream.Write(A<byte[]>.Ignored, 0, 3))
                            .WhenArgumentsMatch(o => o[0] is byte[] param && param[0] == (byte)Commands.InterpretAsCommand && param[1] == (byte)Commands.Will && param[2] == 3)
                            .MustHaveHappened();
#endif

        response.Should().BeEmpty();
      }
    }

    [Fact]
#if ASYNC
    public async Task WhenIacDo1ByteStreamShouldReturnEmptyAndReplyIacWont()
#else
    public void WhenIacDo1ByteStreamShouldReturnEmptyAndReplyIacWont()
#endif
    {
      var socket = A.Fake<ISocket>();
      using (var networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        var isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using (var tcpByteStream = new TcpByteStream(socket))
        {
          A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, (int)Commands.Do, 1 });
          tcpByteStream.Connected.Should().BeTrue();
          using (var sut = new ByteStreamHandler(tcpByteStream))
          {
#if ASYNC
            var response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
            A.CallTo(() => networkStream.WriteAsync(A<byte[]>.Ignored, 0, 3, A<CancellationToken>.Ignored))
                .WhenArgumentsMatch(o => o[0] is byte[] param && param[0] == (byte)Commands.InterpretAsCommand && param[1] == (byte)Commands.Wont && param[2] == 1)
                .MustHaveHappened();
#else
            var response = sut.Read(TimeSpan.FromMilliseconds(10));
            A.CallTo(() => networkStream.Write(A<byte[]>.Ignored, 0, 3))
                .WhenArgumentsMatch(o => o[0] is byte[] param && param[0] == (byte)Commands.InterpretAsCommand && param[1] == (byte)Commands.Wont && param[2] == 1)
                .MustHaveHappened();
#endif

            response.Should().BeEmpty();
          }
        }
      }
    }

    [Fact]
#if ASYNC
    public async Task WhenIac2ByteStreamShouldReturnEmptyAndNotReply()
#else
    public void WhenIac2ByteStreamShouldReturnEmptyAndNotReply()
#endif
    {
      var socket = A.Fake<ISocket>();
      using (var networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        var isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using (var tcpByteStream = new TcpByteStream(socket))
        {
          A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, 2 });
          tcpByteStream.Connected.Should().BeTrue();
          using (var sut = new ByteStreamHandler(tcpByteStream))
          {
#if ASYNC
            var response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
#else
            var response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif

            response.Should().BeEmpty();
            A.CallTo(() => networkStream.WriteByte(A<byte>.Ignored)).MustNotHaveHappened();
          }
        }
      }
    }

    [Fact]
#if ASYNC
    public async Task WhenIacDont1ByteStreamShouldReturnEmptyAndNotReply()
#else
    public void WhenIacDont1ByteStreamShouldReturnEmptyAndNotReply()
#endif
    {
      var socket = A.Fake<ISocket>();
      using (var networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        var isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using (var tcpByteStream = new TcpByteStream(socket))
        {
          A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, (int)Commands.Dont, 1 });
          tcpByteStream.Connected.Should().BeTrue();
          using (var sut = new ByteStreamHandler(tcpByteStream))
          {
#if ASYNC
            var response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
#else
            var response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif

            response.Should().BeEmpty();
            A.CallTo(() => networkStream.WriteByte(A<byte>.Ignored)).MustNotHaveHappened();
          }
        }
      }
    }

    [Fact]
#if ASYNC
    public async Task WhenIacDontSgaByteStreamShouldReturnEmptyAndNotReply()
#else
    public void WhenIacDontSgaByteStreamShouldReturnEmptyAndNotReply()
#endif
    {
      var socket = A.Fake<ISocket>();
      using (var networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        var isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using (var tcpByteStream = new TcpByteStream(socket))
        {
          A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, (int)Commands.Dont, (int)Options.SuppressGoAhead });
          tcpByteStream.Connected.Should().BeTrue();
          using (var sut = new ByteStreamHandler(tcpByteStream))
          {
#if ASYNC
            var response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
#else
            var response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif
            response.Should().BeEmpty();
            A.CallTo(() => networkStream.WriteByte(A<byte>.Ignored)).MustNotHaveHappened();
          }
        }
      }
    }

    [Fact]
#if ASYNC
    public async Task ByteStreamShouldReturnUponCancellation()
#else
    public void ByteStreamShouldReturnUponCancellation()
#endif
    {
      var socket = A.Fake<ISocket>();
      using (var networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        A.CallTo(() => socket.Available).Returns(1);
        using (var tcpByteStream = new TcpByteStream(socket))
        {
          A.CallTo(() => networkStream.ReadByte()).Returns(142);
          tcpByteStream.Connected.Should().BeTrue();
          using (var cancellationToken = new CancellationTokenSource())
          {
            var stopwatch = new Stopwatch();
            using (var sut = new ByteStreamHandler(tcpByteStream, cancellationToken))
            {

#if ASYNC
              cancellationToken.CancelAfter(100);
              await sut.ReadAsync(TimeSpan.FromMilliseconds(1000)).ConfigureAwait(false);
#else
              var t = new Thread(new ThreadStart(() =>
              {
                using var are = new AutoResetEvent(false);
                are.WaitOne(100);
                cancellationToken.Cancel();
              }));
              t.Start();
              sut.Read(TimeSpan.FromMilliseconds(1000));
#endif

              stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
            }
          }
        }
      }
    }
  }
}
