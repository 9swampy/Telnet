namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using FakeItEasy;
  using FluentAssertions;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class ByteStreamHandlerTests
  {
    [TestMethod]
    public async Task UnconnectedByteStreamShouldReturnEmptyResponse()
    {
      ByteStreamHandler sut = new ByteStreamHandler(A.Fake<IByteStream>(), new CancellationTokenSource());
      
      (await sut.ReadAsync(new TimeSpan())).Should().Be(string.Empty);
    }
  }
}
