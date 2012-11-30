using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Studio.Meta.Entities;
using DeNSo.Core;
using System.Reflection;

namespace DeNSo.Studio.Meta
{
  public static class ConfigurationStore
  {
    public static IEnumerable<ConfiguredStore> GetDensoDatabases()
    {
      return Session.New.Get<ConfiguredStore>();
    }

    public static void SaveStoreConfiguration(ConfiguredStore store)
    {
      Session.New.Set(store);
    }

    static ConfigurationStore()
    {
      Session.DefaultDataBase = "StudioConfig";
      DeNSo.Core.Configuration.BasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      DeNSo.Core.Configuration.EnableJournaling = false;
      DeNSo.Core.Configuration.EnableOperationsLog = false;
      Session.Start();
    }
  }
}
