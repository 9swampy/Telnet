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
      var socket = A.Fake<ISocket>();
      TcpByteStream sut = null;
      Action act = () => sut = new TcpByteStream(socket);
      act.Should().NotThrow();
      sut.Should().NotBeNull();
      sut.Dispose();
    }

    [TestMethod]
    public void GivenAFakedSocketACallToWriteByteShouldBeRelayed()
    {
      var writtenByte = new byte();
      var socket = A.Fake<ISocket>();
      var stream = A.Fake<INetworkStream>();
      A.CallTo(() => socket.GetStream()).Returns(stream);
      var sut = new TcpByteStream(socket);
      
      sut.WriteByte(writtenByte);

      A.CallTo(() => stream.WriteByte(writtenByte)).MustHaveHappened();
    }

    [TestMethod]
    public void GivenAFakedSocketACallToReadByteShouldBeRelayed()
    {
      var socket = A.Fake<ISocket>();
      var stream = A.Fake<INetworkStream>();
      A.CallTo(() => socket.GetStream()).Returns(stream);
      var sut = new TcpByteStream(socket);

      sut.ReadByte();

      A.CallTo(() => stream.ReadByte()).MustHaveHappened();
    }
  }
}
