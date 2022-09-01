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
  public class WithFakeClient
  {
    [Fact]
    public
#if ASYNC
      async Task
#else
      void
#endif
      GivenByteStreamWillNeverRespondWhenTerminatedReadShouldWaitRoughlyOneTimeout()
    {
#if NCRUNCH
      var millisecondTolerance = 30;
#else
      var millisecondTolerance = 250;
#endif
      var byteStream = A.Fake<IByteStream>();
      A.CallTo(() => byteStream.Connected).Returns(true);

      using (var sut = new Client(byteStream, new TimeSpan(0, 0, 0, 0, 1), default))
      {
        var start = DateTime.Now;
        var timeout = new TimeSpan(0, 0, 3);
        sut.MillisecondReadDelay = 1;
#if ASYNC
        await sut.TerminatedReadAsync(".", timeout, 1).ConfigureAwait(false);
#else
        sut.TerminatedRead(".", timeout, 1);
#endif
        DateTime.Now.Subtract(start).Should().BeCloseTo(timeout.Add(TimeSpan.FromMilliseconds(millisecondTolerance)), TimeSpan.FromMilliseconds(millisecondTolerance));
      }
    }

    [Fact]
    public
#if ASYNC
      async Task
#else
      void
#endif
      ShouldWaitRoughlyOneMillisecondSpin()
    {
      var millisecondsSpin = 1500;
      var millisecondTolerance = 300;
      var hasResponded = false;
      var stopwatch = new Stopwatch();
      var byteStream = ArrangeByteStreamToRespondWithTerminationOnceAfterMillisecondSpin();

      using (var sut = new Client(byteStream, new TimeSpan(0, 0, 0, 0, 1), default))
      {
        stopwatch.Start();
#if ASYNC
        await sut.TerminatedReadAsync(".", new TimeSpan(0, 0, 0, 3), millisecondsSpin).ConfigureAwait(false);
#else
        sut.TerminatedRead(".", new TimeSpan(0, 0, 0, 3), millisecondsSpin);
#endif

        stopwatch.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(millisecondsSpin + millisecondTolerance), TimeSpan.FromMilliseconds(millisecondTolerance));
      }

      IByteStream ArrangeByteStreamToRespondWithTerminationOnceAfterMillisecondSpin()
      {
        byteStream = A.Fake<IByteStream>();
        A.CallTo(() => byteStream.Connected).Returns(true);
        A.CallTo(() => byteStream.Available).ReturnsLazily(() =>
        {
          if (stopwatch.ElapsedMilliseconds >= millisecondsSpin && !hasResponded)
          {
            return 1;
          }

          return 0;
        });
        A.CallTo(() => byteStream.ReadByte()).ReturnsLazily(() =>
        {
          if (stopwatch.ElapsedMilliseconds >= millisecondsSpin && !hasResponded)
          {
            hasResponded = true;
          }

          return '.';
        });
        return byteStream;
      }
    }

    [Fact]
#if ASYNC
    public async Task ClientShouldReturnUponCancellation()
#else
    public void ClientShouldReturnUponCancellation()
#endif
    {
      var byteStream = A.Fake<IByteStream>();
      A.CallTo(() => byteStream.Connected).Returns(true);
      using (var cancellationToken = new CancellationTokenSource())
      {
        var stopwatch = new Stopwatch();
        using (var sut = new Client(byteStream, new TimeSpan(0, 0, 0, 0, 1), default))
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
