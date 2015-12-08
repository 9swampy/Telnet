namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Threading;
  using FakeItEasy;
  using FluentAssertions;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [ExcludeFromCodeCoverage]
  [TestClass]
  public class WithUnconnectableClient
  {
    [TestMethod]
    public void ShouldTimeoutOnCtor()
    {
      IByteStream byteStream = A.Fake<IByteStream>();
      Client sut;
      Action act = () => sut = new Client(byteStream, default(CancellationToken), new TimeSpan(0, 0, 0, 0, 1));

      act.ShouldThrow<InvalidOperationException>().WithMessage("Unable to connect to the host.");
    }
  }
}