using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using DeNSo.Core.Struct;
using DeNSo.Meta;
using DeNSo.Meta.BSon;

namespace DeNSo.Core
{
  // Changes list. 
  // 2012-04-23 -> converted storekey from int to byte[] to store multiple types value. 

  [Serializable]
  public class ObjectStore : IObjectStore
  {
    //public byte[] CurrentId { get { return currentIdFunction(); } }
    public long LastEventSN { get; internal set; }
    public int ChangesFromLastSave { get; set; }

    public ObjectStoreKeyType KeyType { get; set; }

    #region Private Fields and function for storeid

    //private int _storeintuid = 0;
    //private Guid _storeGuid = Guid.Empty;

    private byte[] newIdFunction() { return Guid.NewGuid().ToByteArray(); }
    //private Func<byte[]> currentIdFunction;

    #endregion

    #region Private fields

    internal volatile List<Dictionary<byte[], byte[]>> _primarystore = new List<Dictionary<byte[], byte[]>>();
    #endregion

    //#region Constructor

    //internal ObjectStore()
    //  : this(ObjectStoreKeyType.Integer)
    //{ }

    //internal ObjectStore(ObjectStoreKeyType keytype)
    //{

    //  KeyType = keytype;

    //  switch (KeyType)
    //  {
    //    case ObjectStoreKeyType.Integer:
    //      newIdFunction = () => ++_storeintuid;
    //      currentIdFunction = () => _storeintuid;
    //      break;
    //    case ObjectStoreKeyType.LongInteger:
    //      newIdFunction = () => ++_storelonguid;
    //      currentIdFunction = () => _storelonguid;
    //      break;
    //    case ObjectStoreKeyType.GlobalUniqueIdentifier:
    //      newIdFunction = () => { _storeGuid = Guid.NewGuid(); return _storeGuid; };
    //      currentIdFunction = () => _storeGuid;
    //      break;
    //    default:
    //      break;
    //  }

    //}
    //#endregion

    #region Public methods for storing objects

    public void Set(BSonDoc entity)
    {
      ChangesFromLastSave++;
      //var lastid = currentIdFunction();
      var uid = GetEntityUI(entity);

      if (dContains(uid))
      {
        dUpdate(uid, entity);
        return;
      }
      dSet(uid, entity);
    }

    public void Remove(BSonDoc entity)
    {
      //var lastid = currentIdFunction();
      var uid = GetEntityUI(entity);
      if (dContains(uid))
        if (dRemove(uid))
          ChangesFromLastSave++;
    }

    public void Flush()
    {
      foreach (var d in _primarystore)
      {
        d.Clear();
        //foreach (var k in d.Keys)
        //{
        //  d.Clear();
        //}
      }
    }

    #endregion

    #region private entities methods

    private byte[] GetEntityUI(BSonDoc entity)
    {
      if (entity.HasProperty(Consts.DensoIDKeyName) && entity[Consts.DensoIDKeyName] != null)
        return (byte[])entity[Consts.DensoIDKeyName];

      entity[Consts.DensoIDKeyName] = newIdFunction();
      return (byte[])entity[Consts.DensoIDKeyName];
    }

    #endregion

    #region private DeNSo Dictionaries manipulations methods

    private BSonDoc dGet(byte[] key)
    {
      foreach (var d in _primarystore)
      {
        if (d.ContainsKey(key))
          return d[key].Deserialize();
      }
      return null;
    }

    private void dSet(byte[] key, BSonDoc doc)
    {
      lock (_primarystore)
      {
        if (!dUpdate(key, doc))
          dInsert(key, doc);
      }
    }

    private void dInsert(byte[] key, BSonDoc doc)
    {
      //doc["@ts#"

      Dictionary<byte[], byte[]> freedictionary = null;
      foreach (var d in _primarystore)
        if (d.Count < Configuration.DictionarySplitSize)
        {
          freedictionary = d; break;
        }

      if (freedictionary == null)
      {
        freedictionary = new Dictionary<byte[], byte[]>();
        _primarystore.Add(freedictionary);
      }

      if (!freedictionary.ContainsKey(key))
        freedictionary.Add(key, doc.Serialize());
    }

    private bool dUpdate(byte[] key, BSonDoc doc)
    {
      lock (_primarystore)
        foreach (var d in _primarystore)
          if (d.ContainsKey(key))
          {
            d[key] = doc.Serialize();
            return true;
          }
      return false;
    }

    private bool dContains(byte[] key)
    {
      foreach (var d in _primarystore)
      {
        if (d.ContainsKey(key))
          return true;
      }
      return false;
    }

    private bool dRemove(byte[] key)
    {
      lock (_primarystore)
      {
        Dictionary<byte[], byte[]> realdictionary = null;
        foreach (var d in _primarystore)
          if (d.ContainsKey(key))
          {
            realdictionary = d; break;
          }

        if (realdictionary != null)
        {
          realdictionary.Remove(key);
          return true;
        }
      }
      return false;
    }

    #endregion

    internal void dInsert(byte[] key, byte[] data)
    {
      Dictionary<byte[], byte[]> freedictionary = null;
      foreach (var d in _primarystore)
        if (d.Count < Configuration.DictionarySplitSize)
        {
          freedictionary = d; break;
        }

      if (freedictionary == null)
      {
        freedictionary = new Dictionary<byte[], byte[]>();
        _primarystore.Add(freedictionary);
      }

      if (!freedictionary.ContainsKey(key))
        freedictionary.Add(key, data);
    }

    #region Public query methods

    public IEnumerable<BSonDoc> Where(Func<BSonDoc, bool> filter)
    {
      if (filter != null)
        foreach (var d in _primarystore)
        {
          foreach (var v in d.Values)
          {
            var bdoc = v.Deserialize();
            if (bdoc != null && filter(bdoc))
              yield return bdoc;
          }
        }

      yield break;
    }

    public IEnumerable<BSonDoc> GetAll()
    {
      foreach (var d in _primarystore)
      {
        foreach (var v in d.Values)
        {
          var bdoc = v.Deserialize();
          if (bdoc != null)
            yield return bdoc;
        }
      }

      yield break;
    }

    public BSonDoc GetById(byte[] key)
    {
      return dGet(key);
    }

    public int Count()
    {
      var result = 0;
      foreach (var d in _primarystore)
        result += d.Count();

      return result;
    }

    public int Count(Func<BSonDoc, bool> filter)
    {
      int count = 0;
      if (filter != null)
        foreach (var d in _primarystore)
        {
          foreach (var v in d.Values)
          {
            var bdoc = v.Deserialize();
            if (bdoc != null && filter(bdoc))
              count++;
          }
        }

      return count;
    }
    #endregion
  }
}
