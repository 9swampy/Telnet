namespace PrimS.Telnet
{
  using System;
  using System.Text.RegularExpressions;
  using System.Threading;
  using System.Threading.Tasks;

  // Referencing https://support.microsoft.com/kb/231866?wa=wsignin1.0 and http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library got me started

  /// <summary>
  /// Basic Telnet client.
  /// </summary>
  public class Client : BaseClient
  {
    /// <summary>
    /// Initialises a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="hostName">The hostname to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    /// <param name="token">The cancellation token.</param>
    public Client(string hostName, int port, CancellationToken token)
      : this(new TcpByteStream(hostName, port), token)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="byteStream">The stream served by the host connected to.</param>
    /// <param name="token">The cancellation token.</param>
    public Client(IByteStream byteStream, CancellationToken token)
      : this(byteStream, token, new TimeSpan(0, 0, 30))
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="byteStream">The stream served by the host connected to.</param>
    /// <param name="token">The cancellation token.</param>
    /// <param name="timeout">The timeout to wait for initial successful connection to <cref>byteStream</cref>.</param>
    public Client(IByteStream byteStream, CancellationToken token, TimeSpan timeout)
      : base(byteStream, token)
    {
#if NetStandard
      PrimS.Telnet.NetStandard.Guard.AgainstNullArgument(nameof(byteStream), byteStream);
#else
      PrimS.Telnet.Guard.AgainstNullArgument(nameof(byteStream), byteStream);
#endif

      var timeoutEnd = DateTime.Now.Add(timeout);
      var are = new AutoResetEvent(false);
      while (!ByteStream.Connected && timeoutEnd > DateTime.Now)
      {
        are.WaitOne(2);
      }

      if (!this.ByteStream.Connected)
      {
        throw new InvalidOperationException("Unable to connect to the host.");
      }
      else
      {
        this.ProactiveOptionNegotiation();
      }
    }

    /// <summary>
    /// Sending options up front will get us to the logon prompt faster.
    /// </summary>
    public void ProactiveOptionNegotiation()
    {
      // SEND DO SUPPRESS GO AHEAD
      byte[] supressGoAhead = new byte[3];
      supressGoAhead[0] = (byte)Commands.InterpretAsCommand;
      supressGoAhead[1] = (byte)Commands.Do;
      supressGoAhead[2] = (byte)Options.SuppressGoAhead;
#if ASYNC
      this.ByteStream.WriteAsync(supressGoAhead, 0, supressGoAhead.Length, this.InternalCancellation.Token);
#else
      this.byteStream.Write(supressGoAhead, 0, outBuffer.Length);
#endif
    }

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
      return this.TryLoginAsync(userName, password, loginTimeoutMs, ">", linefeed);
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
      var result = await this.TrySendUsernameAndPassword(userName, password, loginTimeoutMs, linefeed).ConfigureAwait(false);
      if (result)
      {
        result = await this.IsTerminatedWith(loginTimeoutMs, terminator).ConfigureAwait(false);
      }

      return result;
    }

    /// <summary>
    /// Writes the line to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="linefeed">The type of linefeed to use.</param>
    /// <returns>An awaitable Task.</returns>
    public async Task WriteLine(string command, string linefeed = "\n")
    {
      await this.Write(string.Format("{0}{1}", command, linefeed)).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes the specified command to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>An awaitable Task.</returns>
    public async Task Write(string command)
    {
      if (this.ByteStream.Connected && !this.InternalCancellation.Token.IsCancellationRequested)
      {
        await this.SendRateLimit.WaitAsync(this.InternalCancellation.Token).ConfigureAwait(false);
        await this.ByteStream.WriteAsync(command, this.InternalCancellation.Token).ConfigureAwait(false);
        this.SendRateLimit.Release();
      }
    }

    /// <summary>
    /// Writes the specified <paramref name="data"/> to the server.
    /// </summary>
    /// <param name="data">The byte array to send.</param>
    /// <returns>An awaitable Task.</returns>
    public async Task Write(byte[] data)
    {
      if (this.ByteStream.Connected && !this.InternalCancellation.Token.IsCancellationRequested)
      {
        await this.SendRateLimit.WaitAsync(this.InternalCancellation.Token).ConfigureAwait(false);
        await this.ByteStream.WriteAsync(data, 0, data.Length, this.InternalCancellation.Token).ConfigureAwait(false);
        this.SendRateLimit.Release();
      }
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <returns>Any text read from the stream.</returns>
    public async Task<string> TerminatedReadAsync(string terminator)
    {
      return await this.TerminatedReadAsync(terminator, TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs)).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public async Task<string> TerminatedReadAsync(string terminator, TimeSpan timeout)
    {
      return await this.TerminatedReadAsync(terminator, timeout, 1).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="regex"/> is located.
    /// </summary>
    /// <param name="regex">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public async Task<string> TerminatedReadAsync(Regex regex, TimeSpan timeout)
    {
      return await this.TerminatedReadAsync(regex, timeout, 1).ConfigureAwait(false);
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
      Func<string, bool> isTerminated = (x) => Client.IsTerminatorLocated(terminator, x);
      var s = await this.TerminatedReadAsync(isTerminated, timeout, millisecondSpin).ConfigureAwait(false);
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
      Func<string, bool> isTerminated = (x) => Client.IsRegexLocated(regex, x);
      var s = await this.TerminatedReadAsync(isTerminated, timeout, millisecondSpin).ConfigureAwait(false);
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
    public async Task<string> ReadAsync()
    {
      return await this.ReadAsync(TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs)).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public async Task<string> ReadAsync(TimeSpan timeout)
    {
      var handler = new ByteStreamHandler(this.ByteStream, this.InternalCancellation);
      return await handler.ReadAsync(timeout).ConfigureAwait(false);
    }

    private async Task<bool> TrySendUsernameAndPassword(string userName, string password, int loginTimeoutMs, string linefeed)
    {
      var result = await this.TryAwaitTerminatorThenSend(userName, loginTimeoutMs, linefeed).ConfigureAwait(false);
      if (result)
      {
        result = await this.TryAwaitTerminatorThenSend(password, loginTimeoutMs, linefeed).ConfigureAwait(false);
      }

      return result;
    }

    private async Task<bool> TryAwaitTerminatorThenSend(string value, int loginTimeoutMs, string linefeed)
    {
      var isTerminated = await this.IsTerminatedWith(loginTimeoutMs, ":").ConfigureAwait(false);
      if (isTerminated)
      {
        await this.WriteLine(value, linefeed).ConfigureAwait(false);
      }

      return isTerminated;
    }

    private async Task<string> TerminatedReadAsync(Func<string, bool> isTerminated, TimeSpan timeout, int millisecondSpin)
    {
      var endTimeout = DateTime.Now.Add(timeout);
      var s = string.Empty;
      while (!isTerminated(s) && endTimeout >= DateTime.Now)
      {
        var read = await this.ReadAsync(TimeSpan.FromMilliseconds(millisecondSpin)).ConfigureAwait(false);
        Console.Write(read);
        s += read;
      }

      return s;
    }

    private async Task<bool> IsTerminatedWith(int loginTimeoutMs, string terminator)
    {
      return (await this.TerminatedReadAsync(terminator, TimeSpan.FromMilliseconds(loginTimeoutMs), 1).ConfigureAwait(false)).TrimEnd().EndsWith(terminator);
    }
  }
}
