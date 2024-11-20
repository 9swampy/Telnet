namespace PrimS.Telnet
{
  using System;

  // Referencing https://support.microsoft.com/kb/231866?wa=wsignin1.0 and http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library got me started

  /// <summary>
  /// Basic Telnet client.
  /// </summary>
  public partial class Client
  {
    /// <inheritdoc/>
    public string Read(TimeSpan timeout)
    {
      var handler = new ByteStreamHandler(ByteStream, InternalCancellation, MillisecondReadDelay);
      return handler.Read(timeout);
    }
  }
}
