namespace PrimS.Telnet.Sync.CiTests
{
  using System;

  internal class WriteHandler : WriteHandlerBase
  {
    private readonly Action handler;

    public WriteHandler(Func<byte[], bool> check, Action handler)
    {
      Check = check;
      this.handler = handler;
    }

    public override Func<byte[], bool> Check { get; }

    public override void Handle()
    {
      handler();
    }
  }
}
