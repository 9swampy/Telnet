namespace PrimS.Telnet
{
  using System;
  using System.Net.Sockets;
  using System.Threading;

  //Referencing https://support.microsoft.com/kb/231866?wa=wsignin1.0 and http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library got me started

  /// <summary>
  /// Basic Telnet client
  /// </summary>
  public class Client : IDisposable
  {
    private readonly IByteStream byteStream;

    private readonly SemaphoreSlim sendRateLimit;
    private readonly CancellationTokenSource internalCancellation;

    private const int DefaultTimeOutMs = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="hostname">The hostname.</param>
    /// <param name="port">The port.</param>
    /// <param name="token">The token.</param>
    public Client(string hostname, int port, CancellationToken token)
    {
      this.byteStream = new TcpByteStream(hostname, port);

      this.sendRateLimit = new SemaphoreSlim(1);
      this.internalCancellation = new CancellationTokenSource();
      token.Register(() => this.internalCancellation.Cancel());
    }

    /// <summary>
    /// Tries to login asynchronously.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="loginTimeOutMs">The login time out ms.</param>
    /// <returns>True if successful.</returns>
    public bool TryLogin(string username, string password, int loginTimeOutMs)
    {
      try
      {
        if (this.IsTerminatedWith(loginTimeOutMs, ":"))
        {
          this.WriteLine(username);
          if (this.IsTerminatedWith(loginTimeOutMs, ":"))
          {
            this.WriteLine(password);
          }
          return this.IsTerminatedWith(loginTimeOutMs, ">");
        }
      }
      catch (Exception)
      {
        //NOP
      }
      return false;
    }

    private bool IsTerminatedWith(int loginTimeOutMs, string terminator)
    {
      return (this.TerminatedRead(terminator, TimeSpan.FromMilliseconds(loginTimeOutMs), 1)).TrimEnd().EndsWith(terminator);
    }

    /// <summary>
    /// Writes the line to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    public void WriteLine(string command)
    {
      this.Write(string.Format("{0}\n", command));
    }

    /// <summary>
    /// Writes the specified command to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns></returns>
    public void Write(string command)
    {
      if (this.byteStream.Connected && !this.internalCancellation.Token.IsCancellationRequested)
      {
        this.sendRateLimit.Wait(this.internalCancellation.Token);
        byte[] buf = System.Text.ASCIIEncoding.ASCII.GetBytes(command.Replace("\0xFF", "\0xFF\0xFF"));
        this.byteStream.Write(buf, 0, buf.Length);
        this.sendRateLimit.Release();
      }
    }

    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <returns>Any content retrieved.</returns>
    public string Read()
    {
      return this.Read(TimeSpan.FromMilliseconds(DefaultTimeOutMs));
    }

    /// <summary>
    /// Reads from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns></returns>
    public string Read(TimeSpan timeout)
    {
      ByteStreamHandler handler = new ByteStreamHandler(this.byteStream, this.internalCancellation);
      return handler.Read(timeout);
    }

    private static bool IsTerminatorLocated(string terminator, string s)
    {
      return s.TrimEnd().EndsWith(terminator);
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <see cref="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <returns></returns>
    public string TerminatedRead(string terminator)
    {
      return this.TerminatedRead(terminator, TimeSpan.FromMilliseconds(DefaultTimeOutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <see cref="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns></returns>
    public string TerminatedRead(string terminator, TimeSpan timeout)
    {
      return this.TerminatedRead(terminator, timeout, 1);
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <see cref="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="millisecondSpin">The millisecond spin between each read from the stream.</param>
    /// <returns></returns>
    public string TerminatedRead(string terminator, TimeSpan timeout, int millisecondSpin)
    {
      DateTime endTimeout = DateTime.Now.Add(timeout);
      string s = string.Empty;
      while (!IsTerminatorLocated(terminator, s) && endTimeout >= DateTime.Now)
      {
        s += this.Read(TimeSpan.FromMilliseconds(1));
      }
      if (!IsTerminatorLocated(terminator, s))
      {
        System.Diagnostics.Debug.Print("Failed to terminate '{0}' with '{1)'", s, terminator);
      }
      return s;
    }
    
    /// <summary>
    /// Gets a value indicating whether this instance is connected.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is connected; otherwise, <c>false</c>.
    /// </value>
    public bool IsConnected
    {
      get
      {
        return this.byteStream.Connected;
      }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      try
      {
        this.Dispose(true);
        GC.SuppressFinalize(this);
      }
      catch (Exception)
      {
        //NOP
      }
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        this.byteStream.Close();
        this.sendRateLimit.Dispose();
        this.internalCancellation.Dispose();
      }
      System.Threading.Thread.Sleep(100);
    }
  }
}