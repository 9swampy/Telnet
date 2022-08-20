#if NetStandard || NET6_0_OR_GREATER
namespace PrimS.Telnet.CiTests
#else
namespace PrimS.Telnet.Sync.CiTests
#endif
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Linq;
  using System.Net;
  using System.Net.Sockets;
  using System.Text;

  [ExcludeFromCodeCoverage]
  public abstract class TelnetServerBase : System.Net.Sockets.Socket
  {
    private readonly System.Threading.Thread t;

    private readonly string expectedLineFeedTerminator;

    protected TelnetServerBase(string expectedLineFeedTerminator)
      : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    {
      IsListening = true;
      this.expectedLineFeedTerminator = expectedLineFeedTerminator;

      var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
      IPAddress = ipHostInfo.AddressList.First(o => o.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
      Port = 11000;

      t = new System.Threading.Thread(new System.Threading.ThreadStart(SpinListen));
      t.Start();
    }

    protected override void Dispose(bool disposing)
    {
      IsListening = false;
      base.Dispose(disposing);
      System.Threading.Thread.Sleep(10);
    }

    public bool IsListening { get; private set; }

    public void StopListening()
    {
      IsListening = false;
    }

    //private static string data = null;
    private readonly TimeSpan spinWait = TimeSpan.FromMilliseconds(10);

    public IPAddress IPAddress { get; private set; }

    public int Port { get; private set; }

    private void SpinListen()
    {
#pragma warning disable CA1031 // Do not catch general exception types
      try
      {
        // Start listening for connections.
        var localEndPoint = new IPEndPoint(IPAddress, Port);
        Bind(localEndPoint);
        Listen(10);

        while (IsListening)
        {
          Console.WriteLine("Waiting for a connection...");
          var handler = Accept();

          Console.WriteLine("Connection made, respond with Account: prompt");
          handler.Send(Encoding.ASCII.GetBytes("Account:"));

          WaitFor(handler, $"username{expectedLineFeedTerminator}");

          Console.WriteLine("Account entered, respond with Password: prompt");
          handler.Send(Encoding.ASCII.GetBytes("Password:"));

          WaitFor(handler, $"password{expectedLineFeedTerminator}");

          Console.WriteLine("Password entered, respond with Command> prompt");
          handler.Send(Encoding.ASCII.GetBytes("Command >"));

          WaitFor(handler, $"show statistic wan2{expectedLineFeedTerminator}");

          Console.WriteLine("Command entered, respond with WAN2 terminated reply");
          handler.Send(Encoding.ASCII.GetBytes("show statistic wan2\n\r WAN1 total TX: 0 Bytes ,RX: 0 Bytes \n\r WAN2 total TX: 6.3 GB ,RX: 6.9 GB \n\r WAN3 total TX: 0 Bytes ,RX: 0 Bytes \n\r WAN4 total TX: 0 Bytes ,RX: 0 Bytes \n\r WAN5 total TX: 0 Bytes ,RX: 0 Bytes \n\r>"));

          //handler.Send(new byte[] { (byte)PrimS.Telnet.Commands.InterpretAsCommand, (byte)PrimS.Telnet.Commands.Do });

          while (IsListening)
          {
            System.Threading.Thread.Sleep(100);
          }
          handler.Shutdown(SocketShutdown.Both);
          handler.Close();
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private void WaitFor(Socket handler, string awaitedResponse)
    {
      var data = string.Empty;
      while (true)
      {
        data += ReceiveResponse(handler);
        if (IsResponseReceived(data, awaitedResponse))
        {
          break;
        }
      }
    }

    private static string ReceiveResponse(Socket handler)
    {
      var bytes = new byte[1024];
      var bytesRec = handler.Receive(bytes);
      return Encoding.ASCII.GetString(bytes, 0, bytesRec).Trim((char)255);
    }

    private bool IsResponseReceived(string currentResponse, string responseAwaited)
    {
#if (NetStandard || NET6_0_OR_GREATER) && !Net48
      if (currentResponse.Contains(responseAwaited, StringComparison.InvariantCulture))
#else
      if (currentResponse.Contains(responseAwaited))
#endif
      {
        System.Diagnostics.Debug.Print("{0} response received", responseAwaited);
        Console.WriteLine("{0} response received", responseAwaited);
        return true;
      }
      else
      {
        System.Diagnostics.Debug.Print("Waiting for {1} response, received {0}", currentResponse, responseAwaited);
        Console.WriteLine("Waiting for {1} response, received {0}", currentResponse, responseAwaited);
        System.Threading.Thread.Sleep(spinWait);
        return false;
      }
    }
  }
}
