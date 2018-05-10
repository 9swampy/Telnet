namespace PrimS.Telnet
{
  [IsNotDeadCodeAttribute]
  internal enum Options
  {
    Echo = 1,
    SuppressGoAhead = 3,
    Status = 5,
    TimingMark = 6,
    TerminalType = 24,
    WindowSize = 31,
    TerminalSpeed = 32,
    RemoteFlowCcontrol = 33,
    LineMode = 34,
    OldEnvironment = 36,
    NewEnvironment = 39,
    CharacterSet = 42
  }
}
