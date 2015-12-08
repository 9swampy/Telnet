namespace PrimS.Telnet.CiTests
{
  using System;
#if ASYNC
  using System.Threading;
#endif
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using FluentAssertions;
  using FakeItEasy;
  
  [TestClass]
  public class ExtendedTcpByteStreamTests
  {
    [TestMethod]
    public void ShouldConstructWithAFakedSocket()
    {
      ISocket socket = A.Fake<ISocket>();
      TcpByteStream sut = null;
      Action act = () => sut = new TcpByteStream(socket);
      act.ShouldNotThrow();
      sut.Should().NotBeNull();
      sut.Dispose();
    }

    [TestMethod]
    public void GivenAFakedSocketACallToWriteByteShouldBeRelayed()
    {
      byte writtenByte = new byte();
      ISocket socket = A.Fake<ISocket>();
      INetworkStream stream = A.Fake<INetworkStream>();
      A.CallTo(() => socket.GetStream()).Returns(stream);
      TcpByteStream sut = new TcpByteStream(socket);
      
      sut.WriteByte(writtenByte);

      A.CallTo(() => stream.WriteByte(writtenByte)).MustHaveHappened();
    }

    [TestMethod]
    public void GivenAFakedSocketACallToReadByteShouldBeRelayed()
    {
      ISocket socket = A.Fake<ISocket>();
      INetworkStream stream = A.Fake<INetworkStream>();
      A.CallTo(() => socket.GetStream()).Returns(stream);
      TcpByteStream sut = new TcpByteStream(socket);

      sut.ReadByte();

      A.CallTo(() => stream.ReadByte()).MustHaveHappened();
    }
  }
}
