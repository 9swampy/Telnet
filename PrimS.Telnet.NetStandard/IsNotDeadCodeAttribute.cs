namespace PrimS.Telnet
{
  using System;

  /// <summary>
  /// Suppress IsDeadCode warning.
  /// </summary>
  [AttributeUsage(AttributeTargets.Enum)]
  public sealed class IsNotDeadCodeAttribute : Attribute
  {
  }
}
