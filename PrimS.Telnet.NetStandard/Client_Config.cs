namespace PrimS.Telnet
{
  using System;
  using System.Threading;
#if ASYNC
  using System.Threading.Tasks;
  using Microsoft.VisualStudio.Threading;
#endif

  /// <summary>
  /// Basic Telnet client.
  /// Terminal type and speed can be configured via static properties on the <see cref="Client"/> class.
  /// <see cref="Client"/>.IsWriteConsole can be used to configure whether to write output to the console; often useful for debugging purposes.
  /// </summary>
  public partial class Client
  {
#if ASYNC
    private static readonly JoinableTaskContext joinableTaskContext = new();
#endif

    /// <summary>
    /// Initialises a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="hostname">The hostname.</param>
    /// <param name="port">The port.</param>
    /// <param name="token">The cancellation token.</param>
    public Client(string hostname, int port, CancellationToken token)
      : this(new TcpByteStream(hostname, port), token)
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
    /// <param name="timeout">The timeout to wait for initial successful connection to <cref>byteStream</cref>.</param>
    /// <param name="token">The cancellation token.</param>
    public Client(IByteStream byteStream, TimeSpan timeout, CancellationToken token)
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
#pragma warning disable VSTHRD104 // Offer async methods
        // https://stackoverflow.com/questions/70964917/optimising-an-asynchronous-call-in-a-constructor-using-joinabletaskfactory-run
        joinableTaskContext.Factory.Run(async () => await ProactiveOptionNegotiation().ConfigureAwait(false));
#pragma warning restore VSTHRD104 // Offer async methods
#else
        ProactiveOptionNegotiation();
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
      var supressGoAhead = new byte[3];
      supressGoAhead[0] = (byte)Commands.InterpretAsCommand;
      supressGoAhead[1] = (byte)Commands.Do;
      supressGoAhead[2] = (byte)Options.SuppressGoAhead;
#if ASYNC
      return ByteStream.WriteAsync(supressGoAhead, 0, supressGoAhead.Length, InternalCancellation.Token);
#else
      ByteStream.Write(supressGoAhead, 0, supressGoAhead.Length);
#endif
    }
  }
}
