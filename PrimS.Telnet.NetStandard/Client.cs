namespace PrimS.Telnet
{
  using System;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;

  // Referencing https://support.microsoft.com/kb/231866?wa=wsignin1.0 and http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library got me started

  public partial class Client : BaseClient, IClient
  {
    /// <summary>
    /// Tries to login asynchronously, passing in a default LineTerminator of ">".
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <param name="password">The password.</param>
    /// <param name="loginTimeoutMs">The login time out ms.</param>
    /// <param name="linefeed">The line feed to use. Issue 38: According to RFC 854, CR+LF should be the default a client sends. For backward compatibility \n maintained.</param>
    /// <returns>True if successful.</returns>
    public Task<bool> TryLoginAsync(string userName, string password, int loginTimeoutMs, string linefeed = "\n")
    {
      return TryLoginAsync(userName, password, loginTimeoutMs, ">", linefeed);
    }

    /// <summary>
    /// Tries to login asynchronously.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <param name="password">The password.</param>
    /// <param name="loginTimeoutMs">The login time out ms.</param>
    /// <param name="terminator">The terminator.</param>
    /// <param name="linefeed">The line feed to use. Issue 38: According to RFC 854, CR+LF should be the default a client sends. For backward compatibility \n maintained.</param>
    /// <returns>True if successful.</returns>
    public async Task<bool> TryLoginAsync(string userName, string password, int loginTimeoutMs, string terminator, string linefeed = "\n")
    {
      var result = await TrySendUsernameAndPasswordAsync(userName, password, loginTimeoutMs, linefeed).ConfigureAwait(false);
      if (result)
      {
        result = await IsTerminatedWithAsync(loginTimeoutMs, terminator).ConfigureAwait(false);
      }

      return result;
    }

    /// <summary>
    /// Writes the line to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="linefeed">The type of linefeed to use.</param>
    /// <returns>An awaitable Task.</returns>
    public Task WriteLineAsync(string command, string linefeed = "\n")
    {
      return WriteAsync(string.Format("{0}{1}", command, linefeed));
    }

    /// <summary>
    /// Writes the specified command to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>An awaitable Task.</returns>
    public async Task WriteAsync(string command)
    {
      if (ByteStream.Connected && !InternalCancellation.Token.IsCancellationRequested)
      {
        await SendRateLimit.WaitAsync(InternalCancellation.Token).ConfigureAwait(false);
        await ByteStream.WriteAsync(command, InternalCancellation.Token).ConfigureAwait(false);
        SendRateLimit.Release();
      }
    }

    /// <summary>
    /// Writes the specified <paramref name="data"/> to the server.
    /// </summary>
    /// <param name="data">The byte array to send.</param>
    /// <returns>An awaitable Task.</returns>
    public async Task WriteAsync(byte[] data)
    {
      if (ByteStream.Connected && !InternalCancellation.Token.IsCancellationRequested)
      {
        await SendRateLimit.WaitAsync(InternalCancellation.Token).ConfigureAwait(false);
        await ByteStream.WriteAsync(data, 0, data.Length, InternalCancellation.Token).ConfigureAwait(false);
        SendRateLimit.Release();
      }
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <returns>Any text read from the stream.</returns>
    public Task<string> TerminatedReadAsync(string terminator)
    {
      return TerminatedReadAsync(terminator, TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public Task<string> TerminatedReadAsync(string terminator, TimeSpan timeout)
    {
      return TerminatedReadAsync(terminator, timeout, 1);
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="regex"/> is located.
    /// </summary>
    /// <param name="regex">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public Task<string> TerminatedReadAsync(Regex regex, TimeSpan timeout)
    {
      return TerminatedReadAsync(regex, timeout, 1);
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="millisecondSpin">The millisecond spin between each read from the stream.</param>
    /// <returns>Any text read from the stream.</returns>
    public async Task<string> TerminatedReadAsync(string terminator, TimeSpan timeout, int millisecondSpin)
    {
      bool isTerminated(string x) => Client.IsTerminatorLocated(terminator, x);
      var s = await TerminatedReadAsync(isTerminated, timeout, millisecondSpin).ConfigureAwait(false);
      if (!isTerminated(s))
      {
        System.Diagnostics.Debug.WriteLine("Failed to terminate '{0}' with '{1}'", s, terminator);
      }

      return s;
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="regex"/> is matched.
    /// </summary>
    /// <param name="regex">The regex to match.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="millisecondSpin">The millisecond spin between each read from the stream.</param>
    /// <returns>Any text read from the stream.</returns>
    public async Task<string> TerminatedReadAsync(Regex regex, TimeSpan timeout, int millisecondSpin)
    {
      bool isTerminated(string x) => Client.IsRegexLocated(regex, x);
      var s = await TerminatedReadAsync(isTerminated, timeout, millisecondSpin).ConfigureAwait(false);
      if (!isTerminated(s))
      {
        System.Diagnostics.Debug.WriteLine(string.Format("Failed to match '{0}' with '{1}'", s, regex.ToString()));
      }

      return s;
    }

    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <returns>Any text read from the stream.</returns>
    public Task<string> ReadAsync()
    {
      return ReadAsync(TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public Task<string> ReadAsync(TimeSpan timeout)
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
      var handler = new ByteStreamHandler(ByteStream, InternalCancellation, MillisecondReadDelay);
#pragma warning restore CA2000 // Dispose objects before losing scope
      return handler.ReadAsync(timeout);
    }

    private async Task<bool> TrySendUsernameAndPasswordAsync(string userName, string password, int loginTimeoutMs, string linefeed)
    {
      var result = await TryAwaitTerminatorThenSendAsync(userName, loginTimeoutMs, linefeed).ConfigureAwait(false);
      if (result)
      {
        result = await TryAwaitTerminatorThenSendAsync(password, loginTimeoutMs, linefeed).ConfigureAwait(false);
      }

      return result;
    }

    private async Task<bool> TryAwaitTerminatorThenSendAsync(string value, int loginTimeoutMs, string linefeed)
    {
      var isTerminated = await IsTerminatedWithAsync(loginTimeoutMs, ":").ConfigureAwait(false);
      if (isTerminated)
      {
        await WriteLineAsync(value, linefeed).ConfigureAwait(false);
      }

      return isTerminated;
    }

    private async Task<string> TerminatedReadAsync(Func<string, bool> isTerminated, TimeSpan timeout, int millisecondSpin)
    {
      var endTimeout = DateTime.Now.Add(timeout);
      var s = string.Empty;
      while (!isTerminated(s) && endTimeout >= DateTime.Now)
      {
        var read = await ReadAsync(TimeSpan.FromMilliseconds(millisecondSpin)).ConfigureAwait(false);
        s += read;
      }

      return s;
    }

    private async Task<bool> IsTerminatedWithAsync(int loginTimeoutMs, string terminator)
    {
      return (await TerminatedReadAsync(terminator, TimeSpan.FromMilliseconds(loginTimeoutMs), 1).ConfigureAwait(false)).TrimEnd().EndsWith(terminator);
    }
  }
}
