namespace PrimS.Telnet
{
  using System;
  using System.Text.RegularExpressions;
  using System.Threading;
  using System.Threading.Tasks;
  using LiteGuard;

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
      Guard.AgainstNullArgument("byteStream", byteStream);

      DateTime timeoutEnd = DateTime.Now.Add(timeout);
      AutoResetEvent are = new AutoResetEvent(false);
      while (!this.ByteStream.Connected && timeoutEnd > DateTime.Now)
      {
        are.WaitOne(2);
      }

      if (!this.ByteStream.Connected)
      {
        throw new InvalidOperationException("Unable to connect to the host.");
      }
    }

    /// <summary>
    /// Tries to login asynchronously.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="loginTimeoutMs">The login time out ms.</param>
    /// <returns>True if successful.</returns>
    public async Task<bool> TryLoginAsync(string username, string password, int loginTimeoutMs)
    {
      bool result = await this.TrySendUsernameAndPassword(username, password, loginTimeoutMs);
      if (result)
      {
        result = await this.IsTerminatedWith(loginTimeoutMs, ">");
      }

      return result;
    }

    /// <summary>
    /// Writes the line to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>An awaitable Task.</returns>
    public async Task WriteLine(string command)
    {
      await this.Write(string.Format("{0}\n", command));
    }

    /// <summary>
    /// Writes the specified command to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>Any text read from the stream.</returns>
    public async Task Write(string command)
    {
      if (this.ByteStream.Connected && !this.InternalCancellation.Token.IsCancellationRequested)
      {
        await this.SendRateLimit.WaitAsync(this.InternalCancellation.Token);
        await this.ByteStream.WriteAsync(command, this.InternalCancellation.Token);
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
      return await this.TerminatedReadAsync(terminator, TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public async Task<string> TerminatedReadAsync(string terminator, TimeSpan timeout)
    {
      return await this.TerminatedReadAsync(terminator, timeout, 1);
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="regex">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public async Task<string> TerminatedReadAsync(Regex regex, TimeSpan timeout)
    {
      return await this.TerminatedReadAsync(regex, timeout, 1);
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
      string s = await this.TerminatedReadAsync(isTerminated, timeout, millisecondSpin);
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
      string s = await this.TerminatedReadAsync(isTerminated, timeout, millisecondSpin);
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
      return await this.ReadAsync(TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public async Task<string> ReadAsync(TimeSpan timeout)
    {
      ByteStreamHandler handler = new ByteStreamHandler(this.ByteStream, this.InternalCancellation);
      return await handler.ReadAsync(timeout);
    }

    private async Task<bool> TrySendUsernameAndPassword(string username, string password, int loginTimeoutMs)
    {
      bool result = await this.TryAwaitTerminatorThenSend(username, loginTimeoutMs);
      if (result)
      {
        result = await this.TryAwaitTerminatorThenSend(password, loginTimeoutMs);
      }

      return result;
    }

    private async Task<bool> TryAwaitTerminatorThenSend(string value, int loginTimeoutMs)
    {
      bool isTerminated = await this.IsTerminatedWith(loginTimeoutMs, ":");
      if (isTerminated)
      {
        await this.WriteLine(value);
      }

      return isTerminated;
    }

    private async Task<string> TerminatedReadAsync(Func<string, bool> isTerminated, TimeSpan timeout, int millisecondSpin)
    {
      DateTime endTimeout = DateTime.Now.Add(timeout);
      string s = string.Empty;
      while (!isTerminated(s) && endTimeout >= DateTime.Now)
      {
        s += await this.ReadAsync(TimeSpan.FromMilliseconds(millisecondSpin));
      }

      return s;
    }

    private async Task<bool> IsTerminatedWith(int loginTimeoutMs, string terminator)
    {
      return (await this.TerminatedReadAsync(terminator, TimeSpan.FromMilliseconds(loginTimeoutMs), 1)).TrimEnd().EndsWith(terminator);
    }
  }
}