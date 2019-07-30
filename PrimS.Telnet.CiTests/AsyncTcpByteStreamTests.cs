namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Threading;
  using System.Threading.Tasks;
  using FakeItEasy;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [ExcludeFromCodeCoverage]
  [TestClass]
  public class AsyncTcpByteStreamTests
  {
    [TestMethod]
    public async Task GivenAFakedSocketACallToWriteShouldBeRelayed()
    {
      string writtenString = Guid.NewGuid().ToString();
      ISocket socket = A.Fake<ISocket>();
      INetworkStream stream = A.Fake<INetworkStream>();
      A.CallTo(() => socket.GetStream()).Returns(stream);
      TcpByteStream sut = new TcpByteStream(socket);

      CancellationToken cancellationToken = new CancellationToken();
      await sut.WriteAsync(writtenString, cancellationToken).ConfigureAwait(false);

      A.CallTo(() => stream.WriteAsync(A<byte[]>.Ignored, 0, writtenString.Length, cancellationToken)).MustHaveHappened();
    }
  }
}
