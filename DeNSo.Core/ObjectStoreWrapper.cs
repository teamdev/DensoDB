using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Meta;

namespace DeNSo.Core
{
  public class ObjectStoreWrapper: IStore
  {
    private string _databasename;

    internal ObjectStoreWrapper(string databasename)
    {
      _databasename = databasename;
    }

    public IObjectStore GetStore(string collection)
    {
      return StoreManager.GetObjectStore(_databasename, collection);
    }
  }
}
