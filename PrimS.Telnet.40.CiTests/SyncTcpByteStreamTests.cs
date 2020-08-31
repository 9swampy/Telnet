namespace PrimS.Telnet.CiTests
{
  using System;
  using FakeItEasy;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class SyncTcpByteStreamTests
  {
    [TestMethod]
    public void GivenAFakedSocketACallToWriteShouldBeRelayed()
    {
      var writtenString = Guid.NewGuid().ToString();
      var socket = A.Fake<ISocket>();
      var stream = A.Fake<INetworkStream>();
      A.CallTo(() => socket.GetStream()).Returns(stream);
      var sut = new TcpByteStream(socket);

      sut.Write(writtenString);

      A.CallTo(() => stream.Write(A<byte[]>.Ignored, 0, writtenString.Length)).MustHaveHappened();
    }
  }
}
