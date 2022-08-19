namespace PrimS.Telnet.CiTests
{
  using System;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using FluentAssertions;

  [TestClass]
  public class TcpByteStreamTests
  {
    [TestMethod]
    public void TcpByteStreamShouldTerminateAndReleaseDebuggingContext()
    {
      using (var server = new TelnetServer())
      {
        using (var sut = new TcpByteStream(server.IPAddress.ToString(), server.Port))
        {
          sut.Should().NotBeNull();
        }
      }
    }

    [TestMethod]
    public void TcpByteStreamShouldConstruct()
    {
      using (var server = new TelnetServer())
      {
        TcpByteStream sut = null;
        Action act = () => sut = new TcpByteStream(server.IPAddress.ToString(), server.Port);

        act.Should().NotThrow();
        sut.Dispose();
      }
    }

    [TestMethod]
    public void ReceiveTimeoutShouldBeDefaultTo0()
    {
      using (var server = new TelnetServer())
      {
        using (var sut = new TcpByteStream(server.IPAddress.ToString(), server.Port))
        {
          sut.ReceiveTimeout.Should().Be(0);
        }
      }
    }

    [TestMethod]
    public void WriteByteShouldNotThrow()
    {
      var writtenByte = new byte();
      using (var server = new TelnetServer())
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
