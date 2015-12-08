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
      string writtenString = Guid.NewGuid().ToString();
      ISocket socket = A.Fake<ISocket>();
      INetworkStream stream = A.Fake<INetworkStream>();
      A.CallTo(() => socket.GetStream()).Returns(stream);
      TcpByteStream sut = new TcpByteStream(socket);

      sut.Write(writtenString);

      A.CallTo(() => stream.Write(A<byte[]>.Ignored, 0, writtenString.Length)).MustHaveHappened();
    }
  }
}
