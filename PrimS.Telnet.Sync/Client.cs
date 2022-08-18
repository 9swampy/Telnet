namespace PrimS.Telnet
{
  using System;
  using System.Text.RegularExpressions;

  // Referencing https://support.microsoft.com/kb/231866?wa=wsignin1.0 and http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library got me started

  /// <inheritdoc />
  /// <summary>
  /// Basic Telnet client.
  /// </summary>
  public partial class Client : BaseClient
  {
    /// <summary>
    /// Gets and sets a value indicating whether the Client should write responses received by TerminatedRead out to the Console.
    /// </summary>
    public static bool IsWriteConsole { get; set; } = false;

    /// <summary>
    /// Tries to login.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <param name="password">The password.</param>
    /// <param name="loginTimeOutMs">The login time out ms.</param>
    /// <param name="terminator">The terminator.</param>
    /// <param name="linefeed">The line feed to use. Issue 38: According to RFC 854, CR+LF should be the default a client sends. For backward compatibility \n maintained.</param>
    /// <returns>True if successful.</returns>
    public bool TryLogin(string userName, string password, int loginTimeOutMs, string terminator = ">", string linefeed = "\n")
    {
      try
      {
        if (this.IsTerminatedWith(loginTimeOutMs, ":"))
        {
          this.WriteLine(userName, linefeed);
          if (this.IsTerminatedWith(loginTimeOutMs, ":"))
          {
            this.WriteLine(password, linefeed);
          }

          return this.IsTerminatedWith(loginTimeOutMs, terminator);
        }
      }
      catch (Exception ex)
      {
        // NOP
        System.Diagnostics.Debug.Print(ex.Message);
      }

      return false;
    }

    /// <summary>
    /// Writes the line to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="linefeed">The line feed to use. Issue 38: According to RFC 854, CR+LF should be the default a client sends. For backward compatibility \n maintained.</param>
    public void WriteLine(string command, string linefeed = "\n")
    {
      this.Write(string.Format("{0}{1}", command, linefeed));
    }

    /// <summary>
    /// Writes the specified command to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    public void Write(string command)
    {
      if (this.ByteStream.Connected)
      {
        this.ByteStream.Write(command);
      }
    }

    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <returns>Any text read from the stream.</returns>
    public string Read()
    {
      return this.Read(TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <returns>Any text read from the stream.</returns>
    public string TerminatedRead(string terminator)
    {
      return this.TerminatedRead(terminator, TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="regex"/> is matched.
    /// </summary>
    /// <param name="regex">The regex to match.</param>
    /// <returns>Any text read from the stream.</returns>
    public string TerminatedRead(Regex regex)
    {
      return this.TerminatedRead(regex, TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs));
    }

    /// <summary>
    /// Reads synchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public string TerminatedRead(string terminator, TimeSpan timeout)
    {
      return this.TerminatedRead(terminator, timeout, 1);
    }

    /// <summary>
    /// Reads synchronously from the stream, terminating as soon as the <paramref name="regex"/> is located.
    /// </summary>
    /// <param name="regex">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public string TerminatedRead(Regex regex, TimeSpan timeout)
    {
      return this.TerminatedRead(regex, timeout, 1);
    }

    /// <summary>
    /// Reads synchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="millisecondSpin">The millisecond spin between each read from the stream.</param>
    /// <returns>Any text read from the stream.</returns>
    public string TerminatedRead(string terminator, TimeSpan timeout, int millisecondSpin)
    {
      var endTimeout = DateTime.Now.Add(timeout);
      var s = string.Empty;
      while (!Client.IsTerminatorLocated(terminator, s) && endTimeout >= DateTime.Now)
      {
        var read = this.Read(TimeSpan.FromMilliseconds(millisecondSpin));
        if (IsWriteConsole)
        {
          Console.Write(read);
        }

        s += read;
        s += this.Read(TimeSpan.FromMilliseconds(millisecondSpin));
      }

      if (!Client.IsTerminatorLocated(terminator, s))
      {
        System.Diagnostics.Debug.Print(string.Format("Failed to terminate '{0}' with '{1}'", s, terminator));
      }

      return s;
    }

    /// <summary>
    /// Reads synchronously from the stream, terminating as soon as the <paramref name="regex"/> is matched.
    /// </summary>
    /// <param name="regex">The regex to match.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <param name="millisecondSpin">The millisecond spin between each read from the stream.</param>
    /// <returns>Any text read from the stream.</returns>
    public string TerminatedRead(Regex regex, TimeSpan timeout, int millisecondSpin)
    {
      var endTimeout = DateTime.Now.Add(timeout);
      var s = string.Empty;
      while (!Client.IsRegexLocated(regex, s) && endTimeout >= DateTime.Now)
      {
        s += this.Read(TimeSpan.FromMilliseconds(millisecondSpin));
      }

      if (!Client.IsRegexLocated(regex, s))
      {
        System.Diagnostics.Debug.Print(string.Format("Failed to match '{0}' with '{1}'", s, regex.ToString()));
      }

      return s;
    }

    private bool IsTerminatedWith(int loginTimeOutMs, string terminator)
    {
      return this.TerminatedRead(terminator, TimeSpan.FromMilliseconds(loginTimeOutMs), 1).TrimEnd().EndsWith(terminator);
    }
  }
}
