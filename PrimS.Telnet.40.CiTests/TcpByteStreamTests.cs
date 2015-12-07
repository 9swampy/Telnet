namespace PrimS.Telnet.CiTests
{
  using System;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using FluentAssertions;

  [TestClass]
  public class TcpByteStreamTests
  {
    [TestMethod]
    public void TelnetServerShouldTerminateAndReleaseDebuggingContext()
    {
      TelnetServer server;
      using (server = new TelnetServer())
      {
      }
      server.IsListening.Should().BeFalse();
    }

    [TestMethod]
    public void TcpByteStreamShouldTerminateAndReleaseDebuggingContext()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (TcpByteStream sut = new TcpByteStream(server.IPAddress.ToString(), server.Port))
        {
          sut.Should().NotBeNull();
        }
      }
    }

    [TestMethod]
    public void TcpByteStreamShouldConstruct()
    {
      using (TelnetServer server = new TelnetServer())
      {
        TcpByteStream sut = null;
        Action act = () => sut = new TcpByteStream(server.IPAddress.ToString(), server.Port);

        act.ShouldNotThrow();
        sut.Dispose();
      }
    }
    
    [TestMethod]
    public void ReceiveTimeoutShouldBeDefaultTo0()
    {
      using (TelnetServer server = new TelnetServer())
      {
        using (TcpByteStream sut = new TcpByteStream(server.IPAddress.ToString(), server.Port))
        {
          sut.ReceiveTimeout.Should().Be(0);
        }
      }
    }

    [TestMethod]
    public void WriteByteShouldNotThrow()
    {
      byte writtenByte = new byte();
      using (TelnetServer server = new TelnetServer())
      {
        using (TcpByteStream sut = new TcpByteStream(server.IPAddress.ToString(), server.Port))
        {
          Action act = () => sut.WriteByte(writtenByte);
          act.ShouldNotThrow();
        }
      }
    }
  }
}
