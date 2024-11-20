namespace PrimS.Telnet
{
  using System;
  using System.Threading;
#if ASYNC
  using System.Threading.Tasks;
#endif

  /// <summary>
  /// Basic Telnet client.
  /// Terminal type and speed can be configured via static properties on the <see cref="Client"/> class.
  /// <see cref="Client"/>.IsWriteConsole can be used to configure whether to write output to the console; often useful for debugging purposes.
  /// </summary>
  public partial class Client
  {
    /// <summary>
    /// Prior to v0.9.0 LegacyLineFeed was the default. To be Rfc854 compliant you should prefer Rfc854LineFeed.
    /// </summary>
    public const string LegacyLineFeed = "\n";

    /// <summary>
    /// Post to v0.9.0 LegacyLineFeed has been retained as the default, but to be Rfc854 compliant you should prefer this.
    /// </summary>
    public const string Rfc854LineFeed = "\r\n";

    /// <summary>
    /// Due to https://github.com/9swampy/Telnet/issues/79 allow skip SkipProactiveOptionNegotiation.
    /// </summary>
    public static bool SkipProactiveOptionNegotiation { get; set; } = false;

    /// <summary>
    /// Initialises a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="hostname">The hostname.</param>
    /// <param name="port">The port.</param>
    /// <param name="token">The cancellation token.</param>
    [Obsolete("Prefer overload that accepts new TcpByteStream(hostname, port) and dispose it properly.")]
    public Client(string hostname, int port, CancellationToken token)
#pragma warning disable CA2000
      : this(new TcpByteStream(hostname, port), token)
#pragma warning restore CA2000
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="byteStream">The stream served by the host connected to.</param>
    /// <param name="token">The cancellation token.</param>
    public Client(IByteStream byteStream, CancellationToken token)
      : this(byteStream, new TimeSpan(0, 0, 30), token)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="byteStream">The stream served by the host connected to.</param>
    /// <param name="timeout">The timeout to wait for initial successful connection to <cref>byteStream</cref>. Other overloads default to new TimeSpan(0, 0, 30).</param>
    /// <param name="token">The cancellation token.</param>
    public Client(IByteStream byteStream, TimeSpan timeout, CancellationToken token)
      : this(byteStream, timeout, token, Array.Empty<(Commands Command, Options Option)>())
    { }

    /// <summary>
    /// Initialises a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="byteStream">The stream served by the host connected to.</param>
    /// <param name="timeout">The timeout to wait for initial successful connection to <cref>byteStream</cref>.</param>
    /// <param name="token">The cancellation token.</param>
    /// <param name="options">Additional options to send during negotiation.</param>
    public Client(IByteStream byteStream, TimeSpan timeout, CancellationToken token, (Commands Command, Options Option)[] options)
      : base(byteStream, token)
    {
      Guard.AgainstNullArgument(nameof(byteStream), byteStream);
      var timeoutEnd = DateTime.Now.Add(timeout);
      using (var are = new AutoResetEvent(false))
      {
        while (!ByteStream.Connected && timeoutEnd > DateTime.Now)
        {
          are.WaitOne(2);
        }
      }

      if (!ByteStream.Connected)
      {
        throw new InvalidOperationException("Unable to connect to the host.");
      }
      else
      {
#if ASYNC
        // https://stackoverflow.com/questions/70964917/optimising-an-asynchronous-call-in-a-constructor-using-joinabletaskfactory-run
        if (!SkipProactiveOptionNegotiation)
        {
          Task.Run(async () => await ProactiveOptionNegotiation().ConfigureAwait(false)).Wait();
        }

        foreach ((Commands command, Options option) in options)
        {
          Task.Run(async () => await NegotiateOption(command, option).ConfigureAwait(false)).Wait();
        }
#else
        if (!SkipProactiveOptionNegotiation)
        {
          ProactiveOptionNegotiation();
        }

        foreach (var option in options)
        {
          NegotiateOption(option.Command, option.Option);
        }
#endif
      }
    }

    /// <summary>
    /// Gets and sets a value indicating whether the <see cref="Client"/> should write responses received via <see cref="ByteStreamHandler"/>.Read to the Console.
    /// </summary>
    public static bool IsWriteConsole { get; set; } = false;

    /// <summary>
    /// Gets and sets the TerminalType to negotiate.
    /// </summary>
    public static string TerminalType { get; set; } = "vt100";

    /// <summary>
    /// Gets and sets the TerminalSpeed to negotiate.
    /// </summary>
    public static string TerminalSpeed { get; set; } = "19200,19200";

    internal static byte[] SuppressGoAheadBuffer
    {
      get
      {
        var supressGoAhead = new byte[3];
        supressGoAhead[0] = (byte)Commands.InterpretAsCommand;
        supressGoAhead[1] = (byte)Commands.Do;
        supressGoAhead[2] = (byte)Options.SuppressGoAhead;
        return supressGoAhead;
      }
    }

    /// <summary>
    /// Sending <see cref="Commands.Do"/> <see cref="Options.SuppressGoAhead"/> up front will get us to the logon prompt faster.
    /// </summary>
    private
#if ASYNC
      Task
#else
      void
#endif
      ProactiveOptionNegotiation()
    {
      var supressGoAhead = SuppressGoAheadBuffer;
#if ASYNC
      return ByteStream.WriteAsync(supressGoAhead, 0, supressGoAhead.Length, InternalCancellation.Token);
#else
      ByteStream.Write(supressGoAhead, 0, supressGoAhead.Length);
#endif
    }

    /// <summary>
    /// Negotiate Option specified.
    /// </summary>
    private
#if ASYNC
      Task
#else
      void
#endif
      NegotiateOption(Commands command, Options option)
    {
      var buffer = new byte[] { (byte)Commands.InterpretAsCommand, (byte)command, (byte)option };
#if ASYNC
      return ByteStream.WriteAsync(buffer, 0, buffer.Length, InternalCancellation.Token);
#else
      ByteStream.Write(buffer, 0, buffer.Length);
#endif
    }
  }
}
