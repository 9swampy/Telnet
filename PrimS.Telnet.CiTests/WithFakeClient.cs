namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using FakeItEasy;
  using FluentAssertions;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class WithFakeClient
  {
    private static Client sut;
    private static IByteStream byteStream;

    [ClassInitialize]
    public static void ClassInitialize(TestContext ctx)
    {
      byteStream = A.Fake<IByteStream>();
      A.CallTo(() => byteStream.Connected).Returns(true);

      sut = new Client(byteStream, default(CancellationToken), new TimeSpan(0, 0, 0, 0, 1));
    }

    [TestMethod]
    public void ShouldInitialise()
    {
      sut.Should().NotBeNull();
    }

    [TestMethod]
    public async Task ShouldWaitRoughlyOneTimeout()
    {
      DateTime start = DateTime.Now;
      TimeSpan timeout = new TimeSpan(0, 0, 1);
      await sut.TerminatedReadAsync(".", timeout, 1);
      DateTime.Now.Subtract(start).Should().BeCloseTo(timeout, 35);
    }

    [TestMethod]
    public async Task ShouldWaitRoughlyOneMillisecondSpin()
    {
      DateTime start = DateTime.Now;
      int millisecondsTimeout = 50;
      TimeSpan timeout = TimeSpan.FromMilliseconds(millisecondsTimeout);
      await sut.TerminatedReadAsync(".", new TimeSpan(0, 0, 0, 0, 1), millisecondsTimeout);
      DateTime.Now.Subtract(start).Should().BeCloseTo(timeout, 35);
    }
  }
}