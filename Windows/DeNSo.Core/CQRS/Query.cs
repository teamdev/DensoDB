using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.BSon;

namespace DeNSo.CQRS
{
  public class Query
  {
    public IEnumerable<BSonDoc> Get(string database, string collection, Func<BSonDoc, bool> filter)
    {
      if (filter != null)
        return StoreManager.GetObjectStore(database, collection).Where(filter);
      return StoreManager.GetObjectStore(database, collection).GetAll();
    }

    public int Count(string database, string collection)
    {
      return StoreManager.GetObjectStore(database, collection).Count();
    }

    public int Count(string database, string collection, Func<BSonDoc, bool> filter)
    {
      return StoreManager.GetObjectStore(database, collection).Count(filter);
    }

    public IEnumerable<string> Collections(string database)
    {
      return StoreManager.GetCollections(database);
    }
  }
}
