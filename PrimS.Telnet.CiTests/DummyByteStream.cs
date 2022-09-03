namespace PrimS.Telnet.Sync.CiTests
{
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using System.Text;
  using System.Threading;
#if ASYNC
  using System.Threading.Tasks;
#endif

  public sealed class DummyByteStream : IByteStream
  {
    private readonly Queue<byte> buffer = new Queue<byte>();
    private bool isErrored = false;
    private readonly Queue<WriteHandlerBase> handlers;

    /// <summary>
    /// Supplies the lineFeed character expected: default = "\n")
    /// </summary>
    public DummyByteStream()
      : this(Client.LegacyLineFeed)
    { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lineFeed">The lineFeed character expected: default = "\n")</param>
    public DummyByteStream(string lineFeed)
    {
      Connected = true;

      handlers = new Queue<WriteHandlerBase>();
      handlers.Enqueue(BuildNegotiationHandler());
      handlers.Enqueue(BuildUsernameHandler());
      handlers.Enqueue(BuildPasswordHandler());
      handlers.Enqueue(BuildGetStatisticsHandler());
      Console.WriteLine("Waiting for a connection...");
      LineFeed = lineFeed;
    }

    private WriteHandler BuildGetStatisticsHandler()
    {
      return new WriteHandler(
                o => ByteStringConverter.ToString(o) == $"show statistic wan2{this.LineFeed}",
                () =>
                {
                  Console.WriteLine("Command entered, respond with WAN2 terminated reply");
                  Encoding.ASCII.GetBytes("show statistic wan2\n\r WAN1 total TX: 0 Bytes ,RX: 0 Bytes \n\r WAN2 total TX: 6.3 GB ,RX: 6.9 GB \n\r WAN3 total TX: 0 Bytes ,RX: 0 Bytes \n\r WAN4 total TX: 0 Bytes ,RX: 0 Bytes \n\r WAN5 total TX: 0 Bytes ,RX: 0 Bytes \n\r>").ToList().ForEach(o => this.buffer.Enqueue(o));
                  return;
                });
    }

    private WriteHandler BuildPasswordHandler()
    {
      return new WriteHandler(
                o => ByteStringConverter.ToString(o) == $"password{this.LineFeed}",
                () =>
                {
                  Console.WriteLine("Password entered, respond with Command> prompt");
                  Encoding.ASCII.GetBytes("Command >").ToList().ForEach(o => this.buffer.Enqueue(o));
                  return;
                });
    }

    private WriteHandler BuildUsernameHandler()
    {
      return new WriteHandler(
              o => ByteStringConverter.ToString(o) == $"username{this.LineFeed}",
              () =>
              {
                Console.WriteLine("Account entered, respond with Password: prompt");
                Encoding.ASCII.GetBytes("Password:").ToList().ForEach(o => this.buffer.Enqueue(o));
                return;
              });
    }

    private WriteHandler BuildNegotiationHandler()
    {
      return new WriteHandler(
                bytes => Enumerable.SequenceEqual(bytes, Client.SuppressGoAheadBuffer),
                () =>
                {
                  Connected = true;
                  Console.WriteLine("Connection made, respond with Account: prompt");
                  Encoding.ASCII.GetBytes("Account:").ToList().ForEach(o => this.buffer.Enqueue(o));
                  return;
                });
    }

    public int Available => buffer.Count;

    public bool Connected { get; set; } = false;

    public int ReceiveTimeout { get; set; } = 0;

    public string LineFeed { get; }

    public void Close()
    {
      buffer.Clear();
      Connected = false;
    }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    {
      this.Close();
    }

    public int ReadByte()
    {
      return this.buffer.Dequeue();
    }

    public
#if ASYNC
      Task WriteAsync
#else
      void Write
#endif
      (byte[] buffer, int offset, int count
#if ASYNC
      , CancellationToken cancellationToken
#endif
      )
    {
      if (isErrored)
        return
#if ASYNC
          Task.CompletedTask
#endif
          ;

      var bufferEnum = buffer.Skip(offset).Take(count);
      var bufferString = ByteStringConverter.ToString(buffer, offset, count);

      if (handlers.Peek().Check(buffer))
      {
        handlers.Dequeue().Handle();
      }
      else
      {
        isErrored = true;
        Connected = false;
        throw new NotImplementedException();
      }

#if ASYNC
      return Task.CompletedTask;
#endif
    }

    public
#if ASYNC
      async Task WriteAsync
#else
      void Write
#endif
      (string value
#if ASYNC
      , CancellationToken cancellationToken
#endif
      )
    {
      var buffer = ByteStringConverter.ConvertStringToByteArray(value);
#if ASYNC
      await WriteAsync
#else
      Write
#endif
        (buffer, 0, buffer.Length
#if ASYNC
      , cancellationToken
#endif
        );
    }

    public
#if ASYNC
      async Task WriteByteAsync
#else
      void WriteByte
#endif
      (byte value
#if ASYNC
      , CancellationToken cancellationToken
#endif
      )
    {
#if ASYNC
      await WriteAsync
#else
      Write
#endif
        (new byte[] { value }, 0, 1
#if ASYNC
      , cancellationToken
#endif
        );
    }
  }
}
