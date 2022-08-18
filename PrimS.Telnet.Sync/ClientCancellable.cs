namespace PrimS.Telnet
{
  using System;
  using System.Threading;

  // Referencing https://support.microsoft.com/kb/231866?wa=wsignin1.0 and http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library got me started

  /// <summary>
  /// Basic Telnet client.
  /// </summary>
  public partial class Client
  {
    /// <summary>
    /// Initialises a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="hostname">The hostname.</param>
    /// <param name="port">The port.</param>
    /// <param name="token">The cancellation token.</param>
    public Client(string hostname, int port, CancellationToken token)
      : base(new TcpByteStream(hostname, port), token)
    {
    }

    /// <summary>
    /// Reads from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public string Read(TimeSpan timeout)
    {
      var handler = new ByteStreamHandler(this.ByteStream, this.InternalCancellation, this.MillisecondReadDelay);
      return handler.Read(timeout);
    }
  }
}
