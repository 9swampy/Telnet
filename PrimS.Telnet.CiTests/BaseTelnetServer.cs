namespace PrimS.Telnet.CiTests
{
  using System;
  using System.Linq;
  using System.Net;
  using System.Net.Sockets;
  using System.Text;

  public abstract class BaseTelnetServer : System.Net.Sockets.Socket
  {
    private readonly System.Threading.Thread t;

    protected readonly TimeSpan spinWait = TimeSpan.FromMilliseconds(10);

    public BaseTelnetServer(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
      : base(addressFamily, socketType, protocolType)
    {
      this.IsListening = true;

      IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
      this.IPAddress = ipHostInfo.AddressList.First(o => o.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
      this.Port = 11000;

      this.t = new System.Threading.Thread(new System.Threading.ThreadStart(this.SpinListen));
      this.t.Start();
    }

    protected string Data = null;

    public bool IsListening { get; private set; }

    public IPAddress IPAddress { get; private set; }

    public int Port { get; private set; }

    /// <summary>
    /// Provide a spinning implementation to anticipate and respond to incoming commands that will be run on a background thread.
    /// </summary>
    protected abstract void SpinListen();

    protected override void Dispose(bool disposing)
    {
      this.IsListening = false;
      base.Dispose(disposing);
      System.Threading.Thread.Sleep(10);
    }

    public void StopListening()
    {
      this.IsListening = false;
    }

    protected void ReceiveResponse(Socket handler)
    {
      byte[] bytes = new byte[1024];
      int bytesRec = handler.Receive(bytes);
      this.Data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
    }
  }
}