namespace PrimS.Telnet.CiTests
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
      this.IsListening = true;
      this.expectedLineFeedTerminator = expectedLineFeedTerminator;

      var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
      this.IPAddress = ipHostInfo.AddressList.First(o => o.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
      this.Port = 11000;

      this.t = new System.Threading.Thread(new System.Threading.ThreadStart(this.SpinListen));
      this.t.Start();
    }

    protected override void Dispose(bool disposing)
    {
      this.IsListening = false;
      base.Dispose(disposing);
      System.Threading.Thread.Sleep(10);
    }

    public bool IsListening { get; private set; }

    public void StopListening()
    {
      this.IsListening = false;
    }

    private static string data = null;
    private readonly TimeSpan spinWait = TimeSpan.FromMilliseconds(10);

    public IPAddress IPAddress { get; private set; }

    public int Port { get; private set; }

    private void SpinListen()
    {
      try
      {
        // Start listening for connections.
        var localEndPoint = new IPEndPoint(this.IPAddress, this.Port);
        this.Bind(localEndPoint);
        this.Listen(10);

        while (this.IsListening)
        {
          Console.WriteLine("Waiting for a connection...");
          var handler = this.Accept();
          data = null;

          Console.WriteLine("Connection made, respond with Account: prompt");
          handler.Send(Encoding.ASCII.GetBytes("Account:"));

          this.WaitFor(handler, $"username{this.expectedLineFeedTerminator}");

          Console.WriteLine("Account entered, respond with Password: prompt");
          handler.Send(Encoding.ASCII.GetBytes("Password:"));

          this.WaitFor(handler, $"password{this.expectedLineFeedTerminator}");

          Console.WriteLine("Password entered, respond with Command> prompt");
          handler.Send(Encoding.ASCII.GetBytes("Command >"));

          this.WaitFor(handler, $"show statistic wan2{this.expectedLineFeedTerminator}");

          Console.WriteLine("Command entered, respond with WAN2 terminated reply");
          handler.Send(Encoding.ASCII.GetBytes("show statistic wan2\n\r WAN1 total TX: 0 Bytes ,RX: 0 Bytes \n\r WAN2 total TX: 6.3 GB ,RX: 6.9 GB \n\r WAN3 total TX: 0 Bytes ,RX: 0 Bytes \n\r WAN4 total TX: 0 Bytes ,RX: 0 Bytes \n\r WAN5 total TX: 0 Bytes ,RX: 0 Bytes \n\r>"));

          //handler.Send(new byte[] { (byte)PrimS.Telnet.Commands.InterpretAsCommand, (byte)PrimS.Telnet.Commands.Do });

          while (this.IsListening)
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
    }

    private void WaitFor(Socket handler, string awaitedResponse)
    {
      data = string.Empty;
      while (true)
      {
        ReceiveResponse(handler);
        if (this.IsResponseReceived(data, awaitedResponse))
        {
          break;
        }
      }
    }

    private static void ReceiveResponse(Socket handler)
    {
      var bytes = new byte[1024];
      var bytesRec = handler.Receive(bytes);
      data += Encoding.ASCII.GetString(bytes, 0, bytesRec).Trim((char)255);
    }

    private bool IsResponseReceived(string currentResponse, string responseAwaited)
    {
#if NetStandard
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
        System.Threading.Thread.Sleep(this.spinWait);
        return false;
      }
    }
  }
}
