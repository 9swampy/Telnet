namespace PrimS.Telnet
{
  using System;
  
  /// <summary>
  /// The base class for Clients.
  /// </summary>
  public abstract partial class BaseClient
  {
    /// <summary>
    /// Initialises a new instance of the <see cref="BaseClient"/> class.
    /// </summary>
    /// <param name="byteStream">The byte stream.</param>
    protected BaseClient(IByteStream byteStream)
    {
      this.byteStream = byteStream;
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        this.ByteStream.Close();
      }

      System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
      are.WaitOne(100);
    }
  }
}