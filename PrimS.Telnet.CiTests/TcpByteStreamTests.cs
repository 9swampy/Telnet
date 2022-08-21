#if NetStandard || NET6_0_OR_GREATER
namespace PrimS.Telnet.CiTests
#else
namespace PrimS.Telnet.Sync.CiTests
#endif
{
  using System;
  using Xunit;
  using FluentAssertions;

  public class TcpByteStreamTests
  {
    [Fact]
    public void TcpByteStreamShouldTerminateAndReleaseDebuggingContext()
    {
      using (var server = new DummyTelnetServer())
      {
        using (var sut = new TcpByteStream(server.IPAddress.ToString(), server.Port))
        {
          sut.Should().NotBeNull();
        }
      }
    }

    [Fact]
    public void TcpByteStreamShouldConstruct()
    {
      using (var server = new DummyTelnetServer())
      {
        Action act = () =>
        {
          using var sut = new TcpByteStream(server.IPAddress.ToString(), server.Port);
        };

        act.Should().NotThrow();
      }
    }

    [Fact]
    public void ReceiveTimeoutShouldBeDefaultTo0()
    {
      using (var server = new DummyTelnetServer())
      {
        using (var sut = new TcpByteStream(server.IPAddress.ToString(), server.Port))
        {
          sut.ReceiveTimeout.Should().Be(0);
        }
      }
    }

    [Fact]
    public void WriteByteShouldNotThrow()
    {
      var writtenByte = new byte();
      using (var server = new DummyTelnetServer())
      {
        using (var sut = new TcpByteStream(server.IPAddress.ToString(), server.Port))
        {
          sut.Connected.Should().BeTrue();
          Action act = () => sut.WriteByte(writtenByte);
          act.Should().NotThrow();
        }
      }
    }
  }
}
