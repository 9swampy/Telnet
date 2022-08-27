namespace PrimS.Telnet
{
  using System;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;

  public interface IClient : IBaseClient
  {
    Task<string> ReadAsync();

    Task<string> ReadAsync(TimeSpan timeout);

    Task<string> TerminatedReadAsync(Regex regex, TimeSpan timeout);

    Task<string> TerminatedReadAsync(Regex regex, TimeSpan timeout, int millisecondSpin);

    Task<string> TerminatedReadAsync(string terminator);

    Task<string> TerminatedReadAsync(string terminator, TimeSpan timeout);

    Task<string> TerminatedReadAsync(string terminator, TimeSpan timeout, int millisecondSpin);

    Task<bool> TryLoginAsync(string userName, string password, int loginTimeoutMs, string lineFeed = Client.LegacyLineFeed);

    Task<bool> TryLoginAsync(string userName, string password, int loginTimeoutMs, string terminator, string lineFeed = Client.LegacyLineFeed);

    Task WriteAsync(byte[] data);

    Task WriteAsync(string command);

    Task WriteLineAsync(string command, string lineFeed = Client.LegacyLineFeed);
  }
}
