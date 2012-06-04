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
      var es = StoreManager.GetEventStore(databasename);
      return es.Enqueue(command);
    }
  }
}
