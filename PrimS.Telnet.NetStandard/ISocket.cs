namespace PrimS.Telnet
{
  /// <summary>
  /// A socket to connect to.
  /// </summary>
  public interface ISocket
  {
    /// <summary>
    /// Gets a value indicating whether this <see cref="ISocket" /> is connected.
    /// </summary>
    /// <value>
    /// <c>true</c> if connected; otherwise, <c>false</c>.
    /// </value>
    bool Connected { get; }

    /// <summary>
    /// Gets the available bytes to be read.
    /// </summary>
    /// <value>
    /// The available bytes to be read.
    /// </value>
    int Available { get; }

    /// <summary>
    /// Gets or sets the receive timeout.
    /// </summary>
    /// <value>
    /// The receive timeout.
    /// </value>
    int ReceiveTimeout { get; set; }

    /// <summary>
    /// Gets the stream.
    /// </summary>
    /// <returns>Network stream socket connected to.</returns>
    INetworkStream GetStream();

    /// <summary>
    /// Closes this instance.
    /// </summary>
    void Close();
  }
}