using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using DeNSo.Core.CQRS;

namespace DeNSo
{
  [ServiceContract()]
  public class dbservice
  {
    private Query _qry = new Query();
    private Command _cmd = new Command();

    public int Count(string database, string collection)
    {
      return _qry.Count(database, collection);
    }

    public int Count(string database, string collection, string filter)
    {
      return _qry.Count(database, collection, null);
    }

    public IEnumerable<string> Collections(string database)
    {
      return _qry.Collections(database);
    }

    public IEnumerable<byte[]> Get(string database, string collection, string filter)
    {
      return _qry.Get(database, collection, null).Select(r => r.GetBytes()).AsEnumerable();
    }

    public long Execute(string database, byte[] command)
    {
      return _cmd.Execute(database, command);
    }

  }
}
