namespace PrimS.Telnet.CiTest
{
  using System;
  using System.Linq;
  using System.Net;
  using System.Net.Sockets;
  using System.Text;
  
  public class TelnetServer : System.Net.Sockets.Socket
  {
    private readonly System.Threading.Thread t;

    public TelnetServer()
      : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    {
      this.IsListening = true;

      IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
      this.IPAddress = ipHostInfo.AddressList.First(o => o.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
      this.Port = 11000;

      t = new System.Threading.Thread(new System.Threading.ThreadStart(this.SpinListen));
      t.Start();
    }

    protected override void Dispose(bool disposing)
    {
      this.IsListening = false;
      base.Dispose(disposing);
      System.Threading.Thread.Sleep(10);
    }

    public bool IsListening
    {
      get;
      private set;
    }

    public void StopListening()
    {
      this.IsListening = false;
    }

    // Incoming data from the client.
    public static string data = null;

    public IPAddress IPAddress { get; private set; }

    public int Port { get; private set; }

    private void SpinListen()
    {
      try
      {
        // Start listening for connections.
        IPEndPoint localEndPoint = new IPEndPoint(this.IPAddress, this.Port);
        this.Bind(localEndPoint);
        this.Listen(10);

        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        while (IsListening)
        {
          Console.WriteLine("Waiting for a connection...");
          // Program is suspended while waiting for an incoming connection.
          Socket handler = this.Accept();
          data = null;

          System.Diagnostics.Debug.Print("Connection made, respond with Account: prompt");
          handler.Send(Encoding.ASCII.GetBytes("Account:"));

          // Wait for username
          while (true)
          {
            bytes = new byte[1024];
            int bytesRec = handler.Receive(bytes);
            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            if (data == "username\n")
            {
              System.Diagnostics.Debug.Print("Username response received");
              break;
            }
          }

          System.Diagnostics.Debug.Print("Account entered, respond with Password: prompt");
          handler.Send(Encoding.ASCII.GetBytes("Password:"));

          // Wait for username
          data = string.Empty;
          while (true)
          {
            bytes = new byte[1024];
            int bytesRec = handler.Receive(bytes);
            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            if (data == "password\n")
            {
              System.Diagnostics.Debug.Print("Password response received");
              break;
            }
          }

          System.Diagnostics.Debug.Print("Password entered, respond with Command> prompt");

          handler.Send(Encoding.ASCII.GetBytes("Command>"));

          // Wait for username
          data = string.Empty;
          while (true)
          {
            bytes = new byte[1024];
            int bytesRec = handler.Receive(bytes);
            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            if (data == "show statistic wan2\n")
            {
              break;
            }
          }

          System.Diagnostics.Debug.Print("Command entered, respond with WAN2 terminated reply");
          handler.Send(Encoding.ASCII.GetBytes("Data for WAN2 >"));

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
  }
}
