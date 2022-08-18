namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Diagnostics;
  using System.Diagnostics.CodeAnalysis;
  using System.Threading;
  using System.Threading.Tasks;
  using FakeItEasy;
  using FluentAssertions;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [ExcludeFromCodeCoverage]
  [TestClass]
  public class WithFakeClient
  {
    [TestMethod]
    public async Task GivenByteStreamWillNeverRespondWhenTerminatedReadShouldWaitRoughlyOneTimeout()
    {
      var byteStream = A.Fake<IByteStream>();
      A.CallTo(() => byteStream.Connected).Returns(true);

      using (var sut = new Client(byteStream, default(CancellationToken), new TimeSpan(0, 0, 0, 0, 1)))
      {
        var start = DateTime.Now;
        var timeout = new TimeSpan(0, 0, 2);
        sut.MillisecondReadDelay = 1;
        await sut.TerminatedReadAsync(".", timeout, 1).ConfigureAwait(false);
        DateTime.Now.Subtract(start).Should().BeCloseTo(timeout, TimeSpan.FromMilliseconds(30));
      }
    }

    [TestMethod]
    public async Task ShouldWaitRoughlyOneMillisecondSpin()
    {
      var millisecondsSpin = 1500;
      bool hasResponded = false;
      var stopwatch = new Stopwatch();
      var byteStream = ArrangeByteStreamToRespondWithTerminationOnceAfterMillisecondSpin();

      using (var sut = new Client(byteStream, default(CancellationToken), new TimeSpan(0, 0, 0, 0, 1)))
      {
        stopwatch.Start();
        await sut.TerminatedReadAsync(".", new TimeSpan(0, 0, 0, 3), millisecondsSpin).ConfigureAwait(false);

        stopwatch.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(millisecondsSpin + 150), TimeSpan.FromMilliseconds(200));
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
  }
}
