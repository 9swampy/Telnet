namespace PrimS.Telnet
{
  using System;
  using System.Text.RegularExpressions;

  public interface IClient : IBaseClient
  {
    string Read();

    string Read(TimeSpan timeout);

    string TerminatedRead(Regex regex);

    string TerminatedRead(Regex regex, TimeSpan timeout);

    string TerminatedRead(Regex regex, TimeSpan timeout, int millisecondSpin);

    string TerminatedRead(string terminator);

    string TerminatedRead(string terminator, TimeSpan timeout);

    string TerminatedRead(string terminator, TimeSpan timeout, int millisecondSpin);

    bool TryLogin(string userName, string password, int loginTimeOutMs, string terminator = ">", string linefeed = "\n");

    void Write(string command);

    void WriteLine(string command, string linefeed = "\n");
  }
}
