namespace PrimS.Telnet
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using LiteGuard;

  //Referencing https://support.microsoft.com/kb/231866?wa=wsignin1.0 and http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library got me started

  /// <summary>
  /// Basic Telnet client
  /// </summary>
  public class Client : BaseClient
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="hostname">The hostname to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    /// <param name="token">The cancellation token.</param>
    public Client(string hostname, int port, CancellationToken token)
      : this(new TcpByteStream(hostname, port), token)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="byteStream">The stream served by the host connected to.</param>
    /// <param name="token">The cancellation token.</param>
    /// /// <param name="timeout">The timeout to wait for initial successful connection to <cref>byteStream</cref>.</param>
    public Client(IByteStream byteStream, CancellationToken token)
      : this(byteStream, token, new TimeSpan(0, 0, 30))
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="byteStream">The stream served by the host connected to.</param>
    /// <param name="token">The cancellation token.</param>
    /// /// <param name="timeout">The timeout to wait for initial successful connection to <cref>byteStream</cref>.</param>
    public Client(IByteStream byteStream, CancellationToken token, TimeSpan timeout)
      : base(byteStream, token)
    {
      Guard.AgainstNullArgument("byteStream", byteStream);

      DateTime timeoutEnd = DateTime.Now.Add(timeout);
      while (!this.byteStream.Connected && timeoutEnd > DateTime.Now)
      {
        System.Threading.Thread.Sleep(2);
      }
      if (!this.byteStream.Connected)
      {
        throw new InvalidOperationException("Unable to connect to the host.");
      }
    }

    /// <summary>
    /// Tries to login asynchronously.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="loginTimeOutMs">The login time out ms.</param>
    /// <returns>True if successful.</returns>
    public async Task<bool> TryLoginAsync(string username, string password, int loginTimeOutMs)
    {
      try
      {
        if (await this.IsTerminatedWith(loginTimeOutMs, ":"))
        {
          this.WriteLine(username);
          if (await this.IsTerminatedWith(loginTimeOutMs, ":"))
          {
            this.WriteLine(password);
          }
          return await this.IsTerminatedWith(loginTimeOutMs, ">");
        }
      }
      catch (Exception)
      {
        //NOP
      }
      return false;
    }

    private async Task<bool> IsTerminatedWith(int loginTimeOutMs, string terminator)
    {
      return (await this.TerminatedReadAsync(terminator, TimeSpan.FromMilliseconds(loginTimeOutMs), 1)).TrimEnd().EndsWith(terminator);
    }

    /// <summary>
    /// Writes the line to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    public async void WriteLine(string command)
    {
      await this.Write(string.Format("{0}\n", command));
    }

    /// <summary>
    /// Writes the specified command to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns></returns>
    public async Task Write(string command)
    {
      if (this.byteStream.Connected && !this.internalCancellation.Token.IsCancellationRequested)
      {
        await this.sendRateLimit.WaitAsync(this.internalCancellation.Token);
        await this.byteStream.WriteAsync(command, this.internalCancellation.Token);
        this.sendRateLimit.Release();
      }
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <see cref="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <returns></returns>
    public async Task<string> TerminatedReadAsync(string terminator)
    {
      return await this.TerminatedReadAsync(terminator, TimeSpan.FromMilliseconds(DefaultTimeOutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <see cref="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns></returns>
    public async Task<string> TerminatedReadAsync(string terminator, TimeSpan timeout)
    {
      return await this.TerminatedReadAsync(terminator, timeout, 1);
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <see cref="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="millisecondSpin">The millisecond spin between each read from the stream.</param>
    /// <returns></returns>
    public async Task<string> TerminatedReadAsync(string terminator, TimeSpan timeout, int millisecondSpin)
    {
      DateTime endTimeout = DateTime.Now.Add(timeout);
      string s = string.Empty;
      while (!IsTerminatorLocated(terminator, s) && endTimeout >= DateTime.Now)
      {
        s += await this.ReadAsync(TimeSpan.FromMilliseconds(millisecondSpin));
      }
      if (!IsTerminatorLocated(terminator, s))
      {
        System.Diagnostics.Debug.Print("Failed to terminate '{0}' with '{1}'", s, terminator);
      }
      return s;
    }

    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <returns>Any content retrieved.</returns>
    public async Task<string> ReadAsync()
    {
      return await this.ReadAsync(TimeSpan.FromMilliseconds(DefaultTimeOutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns></returns>
    public async Task<string> ReadAsync(TimeSpan timeout)
    {
      ByteStreamHandler handler = new ByteStreamHandler(this.byteStream, this.internalCancellation);
      return await handler.ReadAsync(timeout);
    }
  }
}
