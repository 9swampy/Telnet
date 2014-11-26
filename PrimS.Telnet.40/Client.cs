namespace PrimS.Telnet
{
  using System;
  using System.Net.Sockets;
  using System.Text;
  using System.Threading;

  //Referencing https://support.microsoft.com/kb/231866?wa=wsignin1.0 and http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library got me started

  /// <summary>
  /// Basic Telnet client
  /// </summary>
  public class Client : IDisposable
  {
    private readonly TcpClient tcpSocket;

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
      this.tcpSocket = new TcpClient(hostname, port);
      System.Threading.Thread.Sleep(20);

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
      if (this.tcpSocket.Connected && !this.internalCancellation.Token.IsCancellationRequested)
      {
        this.sendRateLimit.Wait(this.internalCancellation.Token);
        byte[] buf = System.Text.ASCIIEncoding.ASCII.GetBytes(command.Replace("\0xFF", "\0xFF\0xFF"));
        this.tcpSocket.GetStream().Write(buf, 0, buf.Length);
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
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns></returns>
    public string Read(TimeSpan timeout)
    {
      DateTime endInitialTimeout = DateTime.Now.Add(timeout);
      if (!this.tcpSocket.Connected || this.internalCancellation.Token.IsCancellationRequested)
      {
        return string.Empty;
      }
      StringBuilder sb = new StringBuilder();
      this.tcpSocket.ReceiveTimeout = (int)timeout.TotalMilliseconds;
      DateTime rollingTimeout = ExtendRollingTimeout(timeout);
      do
      {
        if (this.ParseResponse(sb))
        {
          rollingTimeout = ExtendRollingTimeout(timeout);
        }
      }
      while (!this.internalCancellation.Token.IsCancellationRequested && (this.IsResponsePending || IsWaitForInitialResponse(endInitialTimeout, sb) || IsWaitForIncrementalResponse(rollingTimeout)));
      return sb.ToString();
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
      while (!s.TrimEnd().EndsWith(terminator) && endTimeout >= DateTime.Now)
      {
        s += this.Read(TimeSpan.FromMilliseconds(1));
      }
      return s;
    }

    private static DateTime ExtendRollingTimeout(TimeSpan timeout)
    {
      return DateTime.Now.Add(TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / 100));
    }

    private static bool IsWaitForIncrementalResponse(DateTime rollingTimeout)
    {
      bool result = DateTime.Now < rollingTimeout;
      
      System.Threading.Thread.Sleep(1);
      return result;
    }

    private static bool IsWaitForInitialResponse(DateTime endInitialTimeout, StringBuilder sb)
    {
      return (sb.Length == 0 && DateTime.Now < endInitialTimeout);
    }

    private bool IsResponsePending
    {
      get
      {
        return this.tcpSocket.Available > 0;
      }
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
        return this.tcpSocket.Connected;
      }
    }

    private bool ParseResponse(StringBuilder sb)
    {
      if (this.IsResponsePending)
      {
        int input = this.tcpSocket.GetStream().ReadByte();
        switch (input)
        {
          case -1:
            break;
          case (int)Commands.InterpretAsCommand:
            // interpret as command
            int inputVerb = this.tcpSocket.GetStream().ReadByte();
            if (inputVerb == -1)
            {
              break;
            }
            switch (inputVerb)
            {
              case (int)Commands.InterpretAsCommand:
                //literal IAC = 255 escaped, so append char 255 to string
                sb.Append(inputVerb);
                break;
              case (int)Commands.Do:
              case (int)Commands.Dont:
              case (int)Commands.Will:
              case (int)Commands.Wont:
                // reply to all commands with "WONT", unless it is SGA (suppress go ahead)
                int inputOption = this.tcpSocket.GetStream().ReadByte();
                if (inputOption != -1)
                {
                  this.tcpSocket.GetStream().WriteByte((byte)Commands.InterpretAsCommand);
                  if (inputOption == (int)Options.SuppressGoAhead)
                  {
                    this.tcpSocket.GetStream().WriteByte(inputVerb == (int)Commands.Do ? (byte)Commands.Will : (byte)Commands.Do);
                  }
                  else
                  {
                    this.tcpSocket.GetStream().WriteByte(inputVerb == (int)Commands.Do ? (byte)Commands.Wont : (byte)Commands.Dont);
                  }
                  this.tcpSocket.GetStream().WriteByte((byte)inputOption);
                }
                break;
              default:
                break;
            }
            break;
          default:
            sb.Append((char)input);
            break;
        }

        return true;
      }

      return false;
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
        this.tcpSocket.Close();
        this.sendRateLimit.Dispose();
        this.internalCancellation.Dispose();
      }
      System.Threading.Thread.Sleep(100);
    }
  }
}