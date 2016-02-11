namespace PrimS.Telnet
{
  using System;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;

  internal interface IClient
  {
    /// <summary>
    /// Connects this instance.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="System.InvalidOperationException">Unable to connect to the host.</exception>
    Task ConnectAsync();

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
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
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
    /// Tries to login asynchronously.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="loginTimeOutMs">The login time out in milliseconds.</param>
    /// <returns>True if successful.</returns>
    Task<bool> TryLoginAsync(string username, string password, int loginTimeOutMs);

    /// <summary>
    /// Writes the specified command to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>Any text read from the stream.</returns>
    Task Write(string command);

    /// <summary>
    /// Writes the line to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>An awaitable Task.</returns>
    Task WriteLine(string command);
  }
}