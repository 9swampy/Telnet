namespace PrimS.Telnet
{
  using System;

  /// <summary>
  /// Base client behaviour.
  /// </summary>
  public interface IBaseClient : IDisposable
  {
    /// <summary>
    /// Gets a value indicating whether this instance is connected.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is connected; otherwise, <c>false</c>.
    /// </value>
    bool IsConnected { get; }

    /// <summary>
    /// The read delay ms.
    /// </summary>
    int MillisecondReadDelay { get; set; }
  }
}
