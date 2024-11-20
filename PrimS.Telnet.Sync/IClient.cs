namespace PrimS.Telnet
{
  using System;
  using System.Text.RegularExpressions;

  /// <summary>
  /// Basic Telnet client.
  /// </summary>
  public interface IClient : IBaseClient
  {
    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <returns>Any text read from the stream.</returns>
    string Read();

    /// <summary>
    /// Reads from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    string Read(TimeSpan timeout);

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="regex"/> is matched.
    /// </summary>
    /// <param name="regex">The regex to match.</param>
    /// <returns>Any text read from the stream.</returns>
    string TerminatedRead(Regex regex);

    /// <summary>
    /// Reads synchronously from the stream, terminating as soon as the <paramref name="regex"/> is located.
    /// </summary>
    /// <param name="regex">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    string TerminatedRead(Regex regex, TimeSpan timeout);

    /// <summary>
    /// Reads synchronously from the stream, terminating as soon as the <paramref name="regex"/> is matched.
    /// </summary>
    /// <param name="regex">The regex to match.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="millisecondSpin">The millisecond spin between each read from the stream.</param>
    /// <returns>Any text read from the stream.</returns>
    string TerminatedRead(Regex regex, TimeSpan timeout, int millisecondSpin);

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <returns>Any text read from the stream.</returns>
    string TerminatedRead(string terminator);

    /// <summary>
    /// Reads synchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    string TerminatedRead(string terminator, TimeSpan timeout);

    /// <summary>
    /// Reads synchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="millisecondSpin">The millisecond spin between each read from the stream.</param>
    /// <returns>Any text read from the stream.</returns>
    string TerminatedRead(string terminator, TimeSpan timeout, int millisecondSpin);

    /// <summary>
    /// Tries to login.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <param name="password">The password.</param>
    /// <param name="loginTimeOutMs">The login time out ms.</param>
    /// <param name="terminator">The prompt terminator to anticipate.</param>
    /// <param name="lineFeed">The line feed to use. Issue 38: According to RFC 854, CR+LF should be the default a client sends. For backward compatibility \n maintained.</param>
    /// <returns>True if successful.</returns>
    bool TryLogin(string userName, string password, int loginTimeOutMs, string terminator = ">", string lineFeed = Client.LegacyLineFeed);

    /// <summary>
    /// Writes the specified command to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    void Write(string command);

    /// <summary>
    /// Writes the line to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="lineFeed">The line feed to use. Issue 38: According to RFC 854, CR+LF should be the default a client sends. For backward compatibility \n maintained.</param>
    void WriteLine(string command, string lineFeed = Client.LegacyLineFeed);
  }
}
