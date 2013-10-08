using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json.Linq;

namespace DeNSo.CQRS
{
  public class Query
  {
    public string Get(string database, string collection, string objectid)
    {
      if (!string.IsNullOrEmpty(objectid))
        return StoreManager.GetObjectStore(database, collection).GetById(objectid);
      return null;
    }

    public IEnumerable<string> Get(string database, string collection, Func<JObject, bool> filter)
    {
      if (filter != null)
        return StoreManager.GetObjectStore(database, collection).Where(filter);
      return StoreManager.GetObjectStore(database, collection).GetAll();
    }

    public int Count(string database, string collection)
    {
      return StoreManager.GetObjectStore(database, collection).Count();
    }

    public int Count(string database, string collection, Func<JObject, bool> filter)
    {
      return StoreManager.GetObjectStore(database, collection).Count(filter);
    }

    public IEnumerable<string> Collections(string database)
    {
      return StoreManager.GetCollections(database);
    }
  }
}
