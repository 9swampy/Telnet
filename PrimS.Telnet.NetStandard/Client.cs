namespace PrimS.Telnet
{
  using System;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;

  // Referencing https://support.microsoft.com/kb/231866?wa=wsignin1.0 and http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library got me started

  public partial class Client : BaseClient, IClient
  {
    /// <inheritdoc/>
    public Task<bool> TryLoginAsync(string userName, string password, int loginTimeoutMs, string lineFeed = LegacyLineFeed)
    {
      return TryLoginAsync(userName, password, loginTimeoutMs, ">", lineFeed);
    }

    /// <inheritdoc/>
    public async Task<bool> TryLoginAsync(string userName, string password, int loginTimeoutMs, string terminator, string lineFeed = LegacyLineFeed)
    {
      var result = await TrySendUsernameAndPasswordAsync(userName, password, loginTimeoutMs, lineFeed).ConfigureAwait(false);
      if (result)
      {
        result = await IsTerminatedWithAsync(loginTimeoutMs, terminator).ConfigureAwait(false);
      }

      return result;
    }

    /// <inheritdoc/>
    public Task WriteLineAsync(string command)
    {
      return WriteAsync(string.Format("{0}{1}", command, LegacyLineFeed));
    }

    /// <inheritdoc/>
    public Task WriteLineRfc854Async(string command)
    {
      return WriteAsync(string.Format("{0}{1}", command, Rfc854LineFeed));
    }

    /// <inheritdoc/>
    public Task WriteLineAsync(string command, string lineFeed = LegacyLineFeed)
    {
      return WriteAsync(string.Format("{0}{1}", command, lineFeed));
    }

    /// <inheritdoc/>
    public async Task WriteAsync(string command)
    {
      if (ByteStream.Connected && !InternalCancellation.Token.IsCancellationRequested)
      {
        await SendRateLimit.WaitAsync(InternalCancellation.Token).ConfigureAwait(false);
        await ByteStream.WriteAsync(command, InternalCancellation.Token).ConfigureAwait(false);
        SendRateLimit.Release();
      }
    }

    /// <inheritdoc/>
    public async Task WriteAsync(byte[] data)
    {
      if (ByteStream.Connected && !InternalCancellation.Token.IsCancellationRequested)
      {
        await SendRateLimit.WaitAsync(InternalCancellation.Token).ConfigureAwait(false);
        await ByteStream.WriteAsync(data, 0, data.Length, InternalCancellation.Token).ConfigureAwait(false);
        SendRateLimit.Release();
      }
    }

    /// <inheritdoc/>
    public Task<string> TerminatedReadAsync(string terminator)
    {
      return TerminatedReadAsync(terminator, TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs));
    }

    /// <inheritdoc/>
    public Task<string> TerminatedReadAsync(string terminator, TimeSpan timeout)
    {
      return TerminatedReadAsync(terminator, timeout, 1);
    }

    /// <inheritdoc/>
    public Task<string> TerminatedReadAsync(Regex regex, TimeSpan timeout)
    {
      return TerminatedReadAsync(regex, timeout, 1);
    }

    /// <inheritdoc/>
    public async Task<string> TerminatedReadAsync(string terminator, TimeSpan timeout, int millisecondSpin)
    {
      bool isTerminated(string x) => Client.IsTerminatorLocated(terminator, x);
      var s = await TerminatedReadAsync(isTerminated, timeout, millisecondSpin).ConfigureAwait(false);
      if (!isTerminated(s))
      {
        System.Diagnostics.Debug.WriteLine("Failed to terminate '{0}' with '{1}'", s, terminator);
      }

      return s;
    }

    /// <inheritdoc/>
    public async Task<string> TerminatedReadAsync(Regex regex, TimeSpan timeout, int millisecondSpin)
    {
      bool isTerminated(string x) => Client.IsRegexLocated(regex, x);
      var s = await TerminatedReadAsync(isTerminated, timeout, millisecondSpin).ConfigureAwait(false);
      if (!isTerminated(s))
      {
        System.Diagnostics.Debug.WriteLine(string.Format("Failed to match '{0}' with '{1}'", s, regex.ToString()));
      }

      return s;
    }

    /// <inheritdoc/>
    public Task<string> ReadAsync()
    {
      return ReadAsync(TimeSpan.FromMilliseconds(Client.DefaultTimeoutMs));
    }

    /// <inheritdoc/>
    public Task<string> ReadAsync(TimeSpan timeout)
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
      var handler = new ByteStreamHandler(ByteStream, InternalCancellation, MillisecondReadDelay);
#pragma warning restore CA2000 // Dispose objects before losing scope
      return handler.ReadAsync(timeout);
    }

    private async Task<bool> TrySendUsernameAndPasswordAsync(string userName, string password, int loginTimeoutMs, string lineFeed)
    {
      var result = await TryAwaitTerminatorThenSendAsync(userName, loginTimeoutMs, lineFeed).ConfigureAwait(false);
      if (result)
      {
        result = await TryAwaitTerminatorThenSendAsync(password, loginTimeoutMs, lineFeed).ConfigureAwait(false);
      }

      return result;
    }

    private async Task<bool> TryAwaitTerminatorThenSendAsync(string value, int loginTimeoutMs, string lineFeed)
    {
      var isTerminated = await IsTerminatedWithAsync(loginTimeoutMs, ":").ConfigureAwait(false);
      if (isTerminated)
      {
        await WriteLineAsync(value, lineFeed).ConfigureAwait(false);
      }

      return isTerminated;
    }

    private async Task<string> TerminatedReadAsync(Func<string, bool> isTerminated, TimeSpan timeout, int millisecondSpin)
    {
      var endTimeout = DateTime.Now.Add(timeout);
      var s = string.Empty;
      while (!isTerminated(s) && endTimeout >= DateTime.Now)
      {
        var read = await ReadAsync(TimeSpan.FromMilliseconds(millisecondSpin)).ConfigureAwait(false);
        s += read;
      }

      return s;
    }

    private async Task<bool> IsTerminatedWithAsync(int loginTimeoutMs, string terminator)
    {
      return (await TerminatedReadAsync(terminator, TimeSpan.FromMilliseconds(loginTimeoutMs), 1).ConfigureAwait(false)).TrimEnd().EndsWith(terminator);
    }
  }
}
