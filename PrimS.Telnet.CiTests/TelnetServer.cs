namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Linq;
  using System.Net;
  using System.Net.Sockets;
  using System.Text;

  [ExcludeFromCodeCoverage]
  public class TelnetServer : BaseTelnetServer
  {
    public TelnetServer()
      : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    {}  
    
    protected override void SpinListen()
    {
      try
      {
        // Start listening for connections.
        IPEndPoint localEndPoint = new IPEndPoint(this.IPAddress, this.Port);
        this.Bind(localEndPoint);
        this.Listen(10);

        while (this.IsListening)
        {
          Console.WriteLine("Waiting for a connection...");
          Socket handler = this.Accept();
          this.Data = null;

          Console.WriteLine("Connection made, respond with Account: prompt");
          handler.Send(Encoding.ASCII.GetBytes("Account:"));

          this.WaitFor(handler, "username\n");

          Console.WriteLine("Account entered, respond with Password: prompt");
          handler.Send(Encoding.ASCII.GetBytes("Password:"));

          this.WaitFor(handler, "password\n");

          Console.WriteLine("Password entered, respond with Command> prompt");
          handler.Send(Encoding.ASCII.GetBytes("Command >"));

          this.WaitFor(handler, "show statistic wan2\n");

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
      this.Data = string.Empty;
      while (this.IsListening)
      {
        this.ReceiveResponse(handler);
        if (this.IsResponseReceived(Data, awaitedResponse))
        {
          break;
        }
      }
    }
    
    private bool IsResponseReceived(string currentResponse, string responseAwaited)
    {
      if (currentResponse.Replace(Encoding.ASCII.GetString(new byte[] { (byte)Commands.NoOperation }), string.Empty) == responseAwaited)
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