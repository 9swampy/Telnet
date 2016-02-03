namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Threading;
  using FakeItEasy;
  using FluentAssertions;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [ExcludeFromCodeCoverage]
  [TestClass]
  public class WithUnconnectableClient
  {
    [TestMethod]
    public void ShouldFailOnCtorWhenConnectionModeOnInitialiseByDefault()
    {
      IByteStream byteStream = A.Fake<IByteStream>();
      A.CallTo(() => byteStream.Connected).Returns(false);
      Client sut = null;
      Action act = () => sut = new Client(byteStream, default(CancellationToken), new TimeSpan(0, 0, 0, 0, 1));

      act.ShouldThrow<InvalidOperationException>().WithMessage("Unable to connect to the host.");
      sut.Should().BeNull();
    }

    [TestMethod]
    public void ShouldFailOnCtorWhenConnectionModeOnInitialise()
    {
      IByteStream byteStream = A.Fake<IByteStream>();
      A.CallTo(() => byteStream.Connected).Returns(false);
      Client sut = null;
      Action act = () => sut = new Client(byteStream, default(CancellationToken), new TimeSpan(0, 0, 0, 0, 1), ConnectionMode.OnInitialise);

      act.ShouldThrow<InvalidOperationException>().WithMessage("Unable to connect to the host.");
      sut.Should().BeNull();
    }

    [TestMethod]
    public void ShouldNotFailOnCtorWhenConnectionModeOnDemand()
    {
      IByteStream byteStream = A.Fake<IByteStream>();
      A.CallTo(() => byteStream.Connected).Returns(false);
      Client sut = null;
      Action act = () => sut = new Client(byteStream, default(CancellationToken), new TimeSpan(0, 0, 0, 0, 1), ConnectionMode.OnDemand);

      act.ShouldNotThrow();
      sut.Should().NotBeNull();
    }

    [TestMethod]
    public void ShouldTimeoutOnFirstRead()
    {
      IByteStream byteStream = A.Fake<IByteStream>();
      A.CallTo(() => byteStream.Connected).Returns(false);
      Client sut = new Client(byteStream, default(CancellationToken), new TimeSpan(0, 0, 0, 0, 1), ConnectionMode.OnDemand);

      Action act = () => sut.ReadAsync().Wait();

      act.ShouldThrow<InvalidOperationException>().WithMessage("Unable to connect to the host.");
    }

    [TestMethod]
    public void ShouldTimeoutOnFirstWrite()
    {
      IByteStream byteStream = A.Fake<IByteStream>();
      A.CallTo(() => byteStream.Connected).Returns(false);
      Client sut = new Client(byteStream, default(CancellationToken), new TimeSpan(0, 0, 0, 0, 1), ConnectionMode.OnDemand);

      Action act = () => sut.Write("A").Wait();

      act.ShouldThrow<InvalidOperationException>().WithMessage("Unable to connect to the host.");
    }
  }
}