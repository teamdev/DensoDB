using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Core.CQRS
{
  public class Command
  {
    public long Execute(string databasename, byte[] command)
    {
      LogWriter.LogInformation("Received command", System.Diagnostics.EventLogEntryType.Information);
      var es = StoreManager.GetEventStore(databasename);
      return es.Enqueue(command);
    }
  }
}
