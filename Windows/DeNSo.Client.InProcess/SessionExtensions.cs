using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo;


public static class SessionExtensions
{
  public static void Wait(this EventCommandStatus status)
  {
    status._eventSession.WaitForNonStaleDataAt(status.Value);
  }

  public static void Wait(this EventCommandStatus status, TimeSpan timeout)
  {
    status._eventSession.WaitForNonStaleDataAt(status.Value, timeout);
  }

  public static void Wait(this EventCommandStatus status, int timeout)
  {
    status._eventSession.WaitForNonStaleDataAt(status.Value, timeout);
  }
}

