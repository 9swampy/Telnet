﻿namespace PrimS.Telnet.CiTests
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
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      int millisecondsSpin = 150;
      await sut.TerminatedReadAsync(".", new TimeSpan(0, 0, 0, 0, 10), millisecondsSpin);
      stopwatch.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(millisecondsSpin), 70);
    }
  }
}