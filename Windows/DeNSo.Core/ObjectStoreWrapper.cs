using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo;

namespace DeNSo
{
  public class ObjectStoreWrapper: IStore
  {
    private string _databasename;
    public string DataBaseName { get { return _databasename; } }

    internal ObjectStoreWrapper(string databasename)
    {
      _databasename = databasename;
    }

    public IObjectStore GetCollection(string collection)
    {
      return StoreManager.GetObjectStore(_databasename, collection);
    }
  }
}
