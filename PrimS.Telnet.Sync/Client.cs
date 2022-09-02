namespace PrimS.Telnet
{
  using System;
  using System.Text.RegularExpressions;

  // Referencing https://support.microsoft.com/kb/231866?wa=wsignin1.0 and http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library got me started
  // Rfc854 Spec https://tools.ietf.org/html/rfc854
  // Rfc1123 Telnet End-Of-Line Convention https://www.freesoft.org/CIE/RFC/1123/31.htm

  public partial class Client : BaseClient, IClient
  {
    /// <summary>
    /// Tries to login.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <param name="password">The password.</param>
    /// <param name="loginTimeOutMs">The login time out ms.</param>
    /// <param name="terminator">The prompt terminator to anticipate.</param>
    /// <param name="lineFeed">The line feed to use. Issue 38: According to RFC 854, CR+LF should be the default a client sends. For backward compatibility \n maintained.</param>
    /// <returns>True if successful.</returns>
    public bool TryLogin(string userName, string password, int loginTimeOutMs, string terminator = ">", string lineFeed = LegacyLineFeed)
    {
      try
      {
        if (IsTerminatedWith(loginTimeOutMs, ":"))
        {
          WriteLine(userName, lineFeed);
          if (IsTerminatedWith(loginTimeOutMs, ":"))
          {
            WriteLine(password, lineFeed);
          }

          return IsTerminatedWith(loginTimeOutMs, terminator);
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
    public void WriteLine(string command)
    {
      Write(string.Format("{0}{1}", command, LegacyLineFeed));
    }

    /// <summary>
    /// Writes the line to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    public void WriteLineRfc854(string command)
    {
      Write(string.Format("{0}{1}", command, Rfc854LineFeed));
    }

    /// <summary>
    /// Writes the line to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="lineFeed">The line feed to use. Issue 38: According to RFC 854, CR+LF should be the default a client sends. For backward compatibility \n maintained.</param>
    public void WriteLine(string command, string lineFeed = LegacyLineFeed)
    {
      Write(string.Format("{0}{1}", command, lineFeed));
    }

    /// <summary>
    /// Writes the specified command to the server.
    /// </summary>
    /// <param name="command">The command.</param>
    public void Write(string command)
    {
      if (ByteStream.Connected)
      {
        ByteStream.Write(command);
      }
    }

    /// <summary>
    /// Reads asynchronously from the stream.
    /// </summary>
    /// <returns>Any text read from the stream.</returns>
    public string Read()
    {
      return Read(TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <returns>Any text read from the stream.</returns>
    public string TerminatedRead(string terminator)
    {
      return TerminatedRead(terminator, TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs));
    }

    /// <summary>
    /// Reads asynchronously from the stream, terminating as soon as the <paramref name="regex"/> is matched.
    /// </summary>
    /// <param name="regex">The regex to match.</param>
    /// <returns>Any text read from the stream.</returns>
    public string TerminatedRead(Regex regex)
    {
      return TerminatedRead(regex, TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs));
    }

    /// <summary>
    /// Reads synchronously from the stream, terminating as soon as the <paramref name="terminator"/> is located.
    /// </summary>
    /// <param name="terminator">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public string TerminatedRead(string terminator, TimeSpan timeout)
    {
      return TerminatedRead(terminator, timeout, 1);
    }

    /// <summary>
    /// Reads synchronously from the stream, terminating as soon as the <paramref name="regex"/> is located.
    /// </summary>
    /// <param name="regex">The terminator.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>Any text read from the stream.</returns>
    public string TerminatedRead(Regex regex, TimeSpan timeout)
    {
      return TerminatedRead(regex, timeout, 1);
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
        s += Read(TimeSpan.FromMilliseconds(millisecondSpin));
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
        s += Read(TimeSpan.FromMilliseconds(millisecondSpin));
      }

      if (!Client.IsRegexLocated(regex, s))
      {
        System.Diagnostics.Debug.Print(string.Format("Failed to match '{0}' with '{1}'", s, regex.ToString()));
      }

      return s;
    }

    private bool IsTerminatedWith(int loginTimeOutMs, string terminator)
    {
      return TerminatedRead(terminator, TimeSpan.FromMilliseconds(loginTimeOutMs), 1).TrimEnd().EndsWith(terminator);
    }
  }
}
