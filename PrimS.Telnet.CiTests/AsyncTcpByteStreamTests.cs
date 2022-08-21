namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Threading;
  using System.Threading.Tasks;
  using FakeItEasy;
  using Xunit;

  [ExcludeFromCodeCoverage]
  public class AsyncTcpByteStreamTests
  {
    [Fact]
    public async Task GivenAFakedSocketACallToWriteShouldBeRelayed()
    {
      var writtenString = Guid.NewGuid().ToString();
      var socket = A.Fake<ISocket>();
      var stream = A.Fake<INetworkStream>();
      A.CallTo(() => socket.GetStream()).Returns(stream);
      using (var sut = new TcpByteStream(socket))
      {
        var cancellationToken = new CancellationToken();
        await sut.WriteAsync(writtenString, cancellationToken).ConfigureAwait(false);

        A.CallTo(() => stream.WriteAsync(A<byte[]>.Ignored, 0, writtenString.Length, cancellationToken)).MustHaveHappened();
      }
    }
  }
}
