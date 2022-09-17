namespace PrimS.Telnet
{
  using System;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;

  /// <summary>
  /// Telnet Client behaviour.
  /// </summary>
  public interface IClient : IBaseClient
  {
    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <returns>Any text read from the stream.</returns>
    Task<string> ReadAsync();

    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    Task<string> ReadAsync(TimeSpan timeout);

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="regex"/> is located.
    /// </summary>
    /// <param name="regex">The regex to match.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    Task<string> TerminatedReadAsync(Regex regex, TimeSpan timeout);

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="regex"/> is matched.
    /// </summary>
    /// <param name="regex">The regex to match.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="millisecondSpin">The millisecond spin between each read from the stream.</param>
    /// <returns>Any text read from the stream.</returns>
    Task<string> TerminatedReadAsync(Regex regex, TimeSpan timeout, int millisecondSpin);

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <returns>Any text read from the stream.</returns>
    Task<string> TerminatedReadAsync(string terminator);

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    Task<string> TerminatedReadAsync(string terminator, TimeSpan timeout);

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="millisecondSpin">The millisecond spin between each read from the stream.</param>
    /// <returns>Any text read from the stream.</returns>
    Task<string> TerminatedReadAsync(string terminator, TimeSpan timeout, int millisecondSpin);

    /// <summary>
    /// Syntactic sugar; tries to login asynchronously, passing in a default LineTerminator of ">".
    /// Anticipates a terminator (TerminatedRead); responds with username (WriteLine).
    /// Anticipates another terminator (TerminatedRead); responds with password (WriteLine).
    /// This is just a proxy for common Telnet behavour, but of course it relies on the Server implementing the expected behaviour.
    /// If the server you're connecting to does anything different, just use custom TerminatedReads followed by WriteLines.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <param name="password">The password.</param>
    /// <param name="loginTimeoutMs">The login timeout ms.</param>
    /// <param name="lineFeed">The line feed to use. Issue 38: According to RFC 854, CR+LF should be the default a client sends. For backward compatibility \n maintained.</param>
    /// <returns>True if successful.</returns>
    Task<bool> TryLoginAsync(string userName, string password, int loginTimeoutMs, string lineFeed = Client.LegacyLineFeed);

    /// <summary>
    /// Syntactic sugar; tries to login asynchronously. 
    /// Anticipates a terminator (TerminatedRead); responds with username (WriteLine).
    /// Anticipates another terminator (TerminatedRead); responds with password (WriteLine).
    /// This is just a proxy for common Telnet behavour, but of course it relies on the Server implementing the expected behaviour.
    /// If the server you're connecting to does anything different, just use custom TerminatedReads followed by WriteLines.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <param name="password">The password.</param>
    /// <param name="loginTimeoutMs">The login timeout ms.</param>
    /// <param name="terminator">The prompt terminator to anticipate.</param>
    /// <param name="lineFeed">The line feed to use. Issue 38: According to RFC 854, CR+LF should be the default a client sends. For backward compatibility \n maintained.</param>
    /// <returns>True if successful.</returns>
    Task<bool> TryLoginAsync(string userName, string password, int loginTimeoutMs, string terminator, string lineFeed = Client.LegacyLineFeed);

    /// <summary>
    /// Writes the specified <paramref name="data"/> to the server.
    /// </summary>
    /// <param name="data">The byte array to send.</param>
    /// <returns>An awaitable Task.</returns>
    Task WriteAsync(byte[] data);

    /// <summary>
    /// Writes the specified <paramref name="command"/> to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>An awaitable Task.</returns>
    Task WriteAsync(string command);

    /// <summary>
    /// Writes the specified <paramref name="command"/> to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>An awaitable Task.</returns>
    public Task WriteLineAsync(string command);

    /// <summary>
    /// Writes the specified <paramref name="command"/> to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="lineFeed">The type of lineFeed to use. For legacy reasons the default "\n" is supplied, but to be RFC854 compliant "\r\n" should be supplied.</param>
    /// <returns>An awaitable Task.</returns>
    Task WriteLineAsync(string command, string lineFeed = Client.LegacyLineFeed);

    /// <summary>
    /// Writes the specified <paramref name="command"/> to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>An awaitable Task.</returns>
    Task WriteLineRfc854Async(string command);
  }
}
