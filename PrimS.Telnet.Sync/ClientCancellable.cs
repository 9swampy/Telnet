namespace PrimS.Telnet
{
  using System;

  // Referencing https://support.microsoft.com/kb/231866?wa=wsignin1.0 and http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library got me started

  /// <summary>
  /// Basic Telnet client.
  /// </summary>
  public partial class Client
  {
    /// <summary>
    /// Reads from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public string Read(TimeSpan timeout)
    {
      var handler = new ByteStreamHandler(ByteStream, InternalCancellation, MillisecondReadDelay);
      return handler.Read(timeout);
    }
  }
}
