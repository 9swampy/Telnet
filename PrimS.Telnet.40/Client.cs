namespace PrimS.Telnet
{
  using System;
  using System.Text.RegularExpressions;

  // Referencing https://support.microsoft.com/kb/231866?wa=wsignin1.0 and http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library got me started

  /// <summary>
  /// Basic Telnet client.
  /// </summary>
  public partial class Client : BaseClient
  {
    /// <summary>
    /// Tries to login.
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
        // NOP
      }

      return false;
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
      return this.Read(TimeSpan.FromMilliseconds(Client.DefaultTimeOutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <returns>Any text read from the stream.</returns>
    public string TerminatedRead(string terminator)
    {
      return this.TerminatedRead(terminator, TimeSpan.FromMilliseconds(Client.DefaultTimeOutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="regex"/> is matched.
    /// </summary>
    /// <param name="terminator">The regex to match.</param>
    /// <returns>Any text read from the stream.</returns>
    public string TerminatedRead(Regex regex)
    {
      return this.TerminatedRead(regex, TimeSpan.FromMilliseconds(Client.DefaultTimeOutMs));
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
    /// Reads synchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
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
      DateTime endTimeout = DateTime.Now.Add(timeout);
      string s = string.Empty;
      while (!Client.IsTerminatorLocated(terminator, s) && endTimeout >= DateTime.Now)
      {
        s += this.Read(TimeSpan.FromMilliseconds(1));
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
      DateTime endTimeout = DateTime.Now.Add(timeout);
      string s = string.Empty;
      while (!Client.IsRegexLocated(regex, s) && endTimeout >= DateTime.Now)
      {
        s += this.Read(TimeSpan.FromMilliseconds(1));
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