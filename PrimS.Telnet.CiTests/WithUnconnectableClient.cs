namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  [ExcludeFromCodeCoverage]
  public class WithUnconnectableClient
  {
    [Fact]
    public void ShouldTimeoutOnCtor()
    {
      var byteStream = A.Fake<IByteStream>();
      A.CallTo(() => byteStream.Connected).Returns(false);
      Client
#if NET6_0_OR_GREATER
        ?
#endif
        sut = null;
      Action act = () => sut = new Client(byteStream, new TimeSpan(0, 0, 0, 0, 1), default);

      act.Should().Throw<InvalidOperationException>().WithMessage("Unable to connect to the host.");
      sut.Should().BeNull();
    }
  }
}
