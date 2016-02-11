namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Net;
  using System.Net.Sockets;
  using System.Text;

  public class DelayedConnectionTelnetServer : BaseTelnetServer
  {
    internal const string StillSpinningResponse = "Still spinning...";
    internal TimeSpan ConnectionDelay = TimeSpan.FromMilliseconds(100);

    public DelayedConnectionTelnetServer()
      : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    {
    }

    protected override void SpinListen()
    {
      System.Threading.Thread.Sleep(Convert.ToInt32(ConnectionDelay.TotalMilliseconds));

      // Start listening for connections.
      IPEndPoint localEndPoint = new IPEndPoint(this.IPAddress, this.Port);
      this.Bind(localEndPoint);
      this.Listen(10);

      while (this.IsListening)
      {
        try
        {
          Console.WriteLine("Waiting for a connection...");
          Socket handler = this.Accept();
          Data = null;

          this.WaitForAnyInput(handler);

          Console.WriteLine("Connection made");
          handler.Send(Encoding.ASCII.GetBytes(DelayedConnectionTelnetServer.StillSpinningResponse));

          while (this.IsListening)
          {
            System.Threading.Thread.Sleep(100);
          }

          handler.Shutdown(SocketShutdown.Both);
          handler.Close();
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }
    }

    private void WaitForAnyInput(Socket handler)
    {
      this.Data = string.Empty;
      while (this.IsListening)
      {
        this.ReceiveResponse(handler);
        if (this.IsAnyResponseReceived(this.Data))
        {
          break;
        }
      }
    }

    private bool IsAnyResponseReceived(string currentResponse)
    {
      if (!string.IsNullOrWhiteSpace(currentResponse))
      {
        System.Diagnostics.Debug.Print("{0} response received", currentResponse);
        Console.WriteLine("{0} response received", currentResponse);
        return true;
      }
      else
      {
        System.Diagnostics.Debug.Print("Waiting for any response...", currentResponse);
        Console.WriteLine("Waiting for any response...", currentResponse);
        System.Threading.Thread.Sleep(this.spinWait);
        return false;
      }
    }
  }
}