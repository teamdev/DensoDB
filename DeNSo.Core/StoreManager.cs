using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Meta;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DeNSo.Core
{
  internal static class StoreManager
  {
    internal static ManualResetEvent ShutDownEvent = new ManualResetEvent(false);
    private static bool _started = false;
    internal static bool ShuttingDown = false;
    private static Thread _saveDBThread = null;

    private static Dictionary<string, Dictionary<string, IObjectStore>> _stores =
              new Dictionary<string, Dictionary<string, IObjectStore>>();

    private static Dictionary<string, EventStore> _eventStore =
              new Dictionary<string, EventStore>();

    static StoreManager()
    {

    }

    public static string[] GetCollections(string databasename)
    {
      if (_stores.ContainsKey(databasename))
        return _stores[databasename].Keys.ToArray();
      return null;
    }

    public static EventStore GetEventStore(string databasename)
    {
      if (!_eventStore.ContainsKey(databasename))
        _eventStore.Add(databasename, new EventStore(databasename, 0));

      return _eventStore[databasename];
    }

    public static ObjectStore GetObjectStore(string databasename, string collection)
    {
      if (!_stores.ContainsKey(databasename))
        _stores.Add(databasename, new Dictionary<string, IObjectStore>());

      if (!_stores[databasename].ContainsKey(collection))
        _stores[databasename].Add(collection, LoadCollection(databasename, collection));

      return _stores[databasename][collection] as ObjectStore;
    }

    public static void Start()
    {
      if (!_started)
      {
        ShuttingDown = false;
        ShutDownEvent.Reset();

        foreach (var db in GetDatabases())
        {
          OpenDataBase(db);
        }

        _saveDBThread = new Thread(new ThreadStart(SaveDBThreadMethod));
        _saveDBThread.Start();
        _started = true;
      }
    }

    public static void ShutDown()
    {
      Journaling.RaiseCloseEvent();
      ShuttingDown = true;
      ShutDownEvent.Set();
      if (_saveDBThread != null)
        _saveDBThread.Join(new TimeSpan(0, 5, 0));

      // remove all Event Store
      _eventStore.Clear();
      _stores.Clear();

      _started = false;
    }

    public static string[] Databases
    {
      get { return _stores.Keys.ToArray(); }
    }

    private static string[] GetDatabases()
    {
      List<string> result = new List<string>();
      if (Directory.Exists(Configuration.BasePath))
      {
        foreach (var dir in Directory.GetDirectories(Configuration.BasePath))
        {
          var dinfo = new DirectoryInfo(dir);
          if (dinfo.Exists)
            result.Add(dinfo.Name);
        }
      }
      return result.ToArray();
    }

    private static void OpenDataBase(string databasename)
    {
      if (!_eventStore.ContainsKey(databasename))
      {
        var filename = Path.Combine(Configuration.BasePath, databasename, "denso.trn");

        long eventcommandsn = 0;
        if (File.Exists(filename))
          using (var fs = File.OpenRead(Path.Combine(Configuration.BasePath, databasename, "denso.trn")))
            if (fs.Length > 0)
              using (var br = new BinaryReader(fs))
                eventcommandsn = br.ReadInt64();

        _eventStore.Add(databasename, new EventStore(databasename, eventcommandsn));
      }
    }

    private static void SaveDBThreadMethod()
    {
      while (!ShutDownEvent.WaitOne(2))
      {
        ShutDownEvent.WaitOne(Configuration.SaveInterval);
        foreach (var db in _stores.Keys)
        {
          SaveDataBase(db);
          ShutDownEvent.WaitOne(Configuration.DBCheckTimeSpan);
        }
      }
      ShutDownEvent.Reset();
    }

    internal static void SaveDataBase(string databasename)
    {
      var collections = GetCollections(databasename);
      foreach (var coll in collections)
        SaveCollection(databasename, coll);

      var es = GetEventStore(databasename);
      using (var fs = File.Create(Path.Combine(Configuration.BasePath, databasename, "denso.trn")))
      using (var bw = new BinaryWriter(fs))
        bw.Write(es.LastExecutedCommandSN);

      es.ShrinkEventStore();
    }

    internal static void SaveCollection(string database, string collection)
    {
      var store = GetObjectStore(database, collection);
      if (store.ChangesFromLastSave > 0)
      {
        var fullpath = Path.Combine(Configuration.BasePath, database, collection + ".coll");

        if (!Directory.Exists(Path.GetDirectoryName(fullpath)))
          Directory.CreateDirectory(Path.GetDirectoryName(fullpath));

        using (var file = File.Open(fullpath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
        using (var writer = new BinaryWriter(file))
        {
          foreach (var dstore in store._primarystore)
            foreach (var item in dstore)
            {
              writer.Write((int)item.Value.Length); // Data Lenght
              writer.Write((byte)item.Key.Length);
              writer.Write(item.Key); // Database _id
              writer.Write(item.Value); // Data
            }
        }
      }
    }

    internal static ObjectStore LoadCollection(string database, string collection)
    {
      var store = new ObjectStore();
      var fullpath = Path.Combine(Configuration.BasePath, database, collection + ".coll");
      if (File.Exists(fullpath))
        using (var fs = File.Open(fullpath, FileMode.Open, FileAccess.Read, FileShare.None))
        using (var br = new BinaryReader(fs))
          while (fs.Position < fs.Length)
          {
            var len = br.ReadInt32();
            var klen = br.ReadByte();
            var id = br.ReadBytes(klen);
            var data = br.ReadBytes(len);

            store.dInsert(id, data);
          }
      return store;
    }
  }
}
