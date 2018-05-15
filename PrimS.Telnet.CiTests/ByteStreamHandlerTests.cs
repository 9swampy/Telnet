using System.ComponentModel;

namespace PrimS.Telnet.CiTests
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
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [ExcludeFromCodeCoverage]
  [TestClass]
  public class ByteStreamHandlerTests
  {
    [TestMethod]
#if ASYNC
    public async Task UnconnectedByteStreamShouldReturnEmptyResponse()
#else
    public void UnconnectedByteStreamShouldReturnEmptyResponse()
#endif
    {
      ByteStreamHandler sut = new ByteStreamHandler(A.Fake<IByteStream>(), new CancellationTokenSource());
#if ASYNC
      (await sut.ReadAsync(new TimeSpan())).Should().Be(string.Empty);
#else
      sut.Read(new TimeSpan()).Should().Be(string.Empty);
#endif
    }

    [TestMethod]
#if ASYNC
    public async Task ByteStreamShouldReturnEmptyResponse()
#else
    public void ByteStreamShouldReturnEmptyResponse()
#endif
    {
      ISocket socket = A.Fake<ISocket>();
      using (INetworkStream networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        bool isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        TcpByteStream tcpByteStream = new TcpByteStream(socket);
        A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(-1);
        tcpByteStream.Connected.Should().BeTrue();
        ByteStreamHandler sut = new ByteStreamHandler(tcpByteStream, new CancellationTokenSource());

#if ASYNC
      string response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10));
#else
        string response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif

        response.Should().BeEmpty();
      }
    }

    [TestMethod]
#if ASYNC
    public async Task ByteStreamShouldReturnCharA()
#else
    public void ByteStreamShouldReturnCharA()
#endif
    {
      ISocket socket = A.Fake<ISocket>();
      using (INetworkStream networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        bool isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        TcpByteStream tcpByteStream = new TcpByteStream(socket);
        A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(65);
        tcpByteStream.Connected.Should().BeTrue();
        ByteStreamHandler sut = new ByteStreamHandler(tcpByteStream, new CancellationTokenSource());

#if ASYNC
      string response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10));
#else
        string response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif

        response.Should().Be("A");
      }
    }

    [TestMethod]
#if ASYNC
    public async Task ByteStreamShouldReturnEscapedIac()
#else
    public void ByteStreamShouldReturnEscapedIac()
#endif
    {
      ISocket socket = A.Fake<ISocket>();
      using (INetworkStream networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        bool isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        TcpByteStream tcpByteStream = new TcpByteStream(socket);
        A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, (int)Commands.InterpretAsCommand });
        tcpByteStream.Connected.Should().BeTrue();
        ByteStreamHandler sut = new ByteStreamHandler(tcpByteStream, new CancellationTokenSource());

#if ASYNC
      string response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10));
#else
        string response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif
        response.Should().Be("255");
      }
    }

    [TestMethod]
#if ASYNC
    public async Task ByteStreamShouldReturnEmpty()
#else
    public void ByteStreamShouldReturnEmpty()
#endif
    {
      ISocket socket = A.Fake<ISocket>();
      using (INetworkStream networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        bool isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        TcpByteStream tcpByteStream = new TcpByteStream(socket);
        A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, -1 });
        tcpByteStream.Connected.Should().BeTrue();
        ByteStreamHandler sut = new ByteStreamHandler(tcpByteStream, new CancellationTokenSource());

#if ASYNC
      string response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10));
#else
        string response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif

        response.Should().BeEmpty();
      }
    }

    [TestMethod]
#if ASYNC
    public async Task WhenIacDoSgaByteStreamShouldReturnEmptyAndReplyIacWill()
#else
    public void WhenIacDoSgaByteStreamShouldReturnEmptyAndReplyIacWill()
#endif
    {
      ISocket socket = A.Fake<ISocket>();
      using (INetworkStream networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        bool isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        TcpByteStream tcpByteStream = new TcpByteStream(socket);
        A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, (int)Commands.Do, (int)Options.SuppressGoAhead });
        tcpByteStream.Connected.Should().BeTrue();
        ByteStreamHandler sut = new ByteStreamHandler(tcpByteStream, new CancellationTokenSource());

#if ASYNC
        string response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10));
        // A.CallTo(() => networkStream.WriteAsync(A<byte[]>.That.Contains((char)255), 0, 3, new CancellationToken())).MustHaveHappened();
        A.CallTo(networkStream).Where(x => x.Method.Name == "WriteAsync").MustHaveHappened();
#else
        string response = sut.Read(TimeSpan.FromMilliseconds(10));
        A.CallTo(networkStream).Where(x => x.Method.Name == "Write").MustHaveHappened();
       // A.CallTo(() => networkStream.Write(A<byte[]>.That.Contains((char)255), 0, 3)).MustHaveHappened();
#endif
        
        response.Should().BeEmpty();
      }
    }

    [TestMethod]
#if ASYNC
    public async Task WhenIacDo1ByteStreamShouldReturnEmptyAndReplyIacWont()
#else
    public void WhenIacDo1ByteStreamShouldReturnEmptyAndReplyIacWont()
#endif
    {
      ISocket socket = A.Fake<ISocket>();
      using (INetworkStream networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        bool isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using (TcpByteStream tcpByteStream = new TcpByteStream(socket))
        {
          A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, (int)Commands.Do, 1 });
          tcpByteStream.Connected.Should().BeTrue();
          using (ByteStreamHandler sut = new ByteStreamHandler(tcpByteStream, new CancellationTokenSource()))
          {
#if ASYNC
            string response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10));
            A.CallTo(networkStream).Where(x => x.Method.Name == "WriteAsync").MustHaveHappened();
#else
            string response = sut.Read(TimeSpan.FromMilliseconds(10));
            A.CallTo(networkStream).Where(x => x.Method.Name == "Write").MustHaveHappened();
#endif

            response.Should().BeEmpty();
          }
        }
      }
    }

    [TestMethod]
#if ASYNC
    public async Task WhenIac2ByteStreamShouldReturnEmptyAndNotReply()
#else
    public void WhenIac2ByteStreamShouldReturnEmptyAndNotReply()
#endif
    {
      ISocket socket = A.Fake<ISocket>();
      using (INetworkStream networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        bool isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using (TcpByteStream tcpByteStream = new TcpByteStream(socket))
        {
          A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, 2 });
          tcpByteStream.Connected.Should().BeTrue();
          using (ByteStreamHandler sut = new ByteStreamHandler(tcpByteStream, new CancellationTokenSource()))
          {
#if ASYNC
      string response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10));
#else
            string response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif

            response.Should().BeEmpty();
            A.CallTo(() => networkStream.WriteByte(A<byte>.Ignored)).MustNotHaveHappened();
          }
        }
      }
    }

        [TestMethod]
#if ASYNC
    public async Task WhenIacDont1ByteStreamShouldReturnEmptyAndNotReply()
#else
    public void WhenIacDont1ByteStreamShouldReturnEmptyAndNotReply()
#endif
    {
      ISocket socket = A.Fake<ISocket>();
      using (INetworkStream networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        bool isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using (TcpByteStream tcpByteStream = new TcpByteStream(socket))
        {
          A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, (int)Commands.Dont, 1 });
          tcpByteStream.Connected.Should().BeTrue();
          using (ByteStreamHandler sut = new ByteStreamHandler(tcpByteStream, new CancellationTokenSource()))
          {
#if ASYNC
      string response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10));
#else
            string response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif

            response.Should().BeEmpty();
            A.CallTo(() => networkStream.WriteByte(A<byte>.Ignored)).MustNotHaveHappened();
          }
        }
      }
    }

    [TestMethod]
#if ASYNC
    public async Task WhenIacDontSgaByteStreamShouldReturnEmptyAndNotReply()
#else
    public void WhenIacDontSgaByteStreamShouldReturnEmptyAndNotReply()
#endif
    {
      ISocket socket = A.Fake<ISocket>();
      using (INetworkStream networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        bool isFirst = true;
        A.CallTo(() => socket.Available).ReturnsLazily(() =>
        {
          if (isFirst)
          {
            isFirst = false;
            return 1;
          }
          return 0;
        });
        using (TcpByteStream tcpByteStream = new TcpByteStream(socket))
        {
          A.CallTo(() => networkStream.ReadByte()).ReturnsNextFromSequence(new int[] { (int)Commands.InterpretAsCommand, (int)Commands.Dont, (int)Options.SuppressGoAhead });
          tcpByteStream.Connected.Should().BeTrue();
          using (ByteStreamHandler sut = new ByteStreamHandler(tcpByteStream, new CancellationTokenSource()))
          {
#if ASYNC
      string response = await sut.ReadAsync(TimeSpan.FromMilliseconds(10));
#else
            string response = sut.Read(TimeSpan.FromMilliseconds(10));
#endif

            response.Should().BeEmpty();
            A.CallTo(() => networkStream.WriteByte(A<byte>.Ignored)).MustNotHaveHappened();
          }
        }
      }
    }

    [TestMethod]
#if ASYNC
    public async Task ByteStreamShouldReturnUponCancellation()
#else
    public void ByteStreamShouldReturnUponCancellation()
#endif
    {
      ISocket socket = A.Fake<ISocket>();
      using (INetworkStream networkStream = A.Fake<INetworkStream>())
      {
        A.CallTo(() => socket.GetStream()).Returns(networkStream);
        A.CallTo(() => socket.Connected).Returns(true);
        A.CallTo(() => socket.Available).Returns(1);
        using (TcpByteStream tcpByteStream = new TcpByteStream(socket))
        {
          A.CallTo(() => networkStream.ReadByte()).Returns(142);
          tcpByteStream.Connected.Should().BeTrue();
          CancellationTokenSource cancellationToken = new CancellationTokenSource();

          Stopwatch stopwatch = new Stopwatch();
          using (ByteStreamHandler sut = new ByteStreamHandler(tcpByteStream, cancellationToken))
          {

#if ASYNC
      cancellationToken.CancelAfter(100);
      await sut.ReadAsync(TimeSpan.FromMilliseconds(1000));
#else
            Thread t = new Thread(new ThreadStart(() =>
            {
              AutoResetEvent are = new AutoResetEvent(false);
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
