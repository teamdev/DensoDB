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
  [Serializable]
  public class ObjectStore : IObjectStore
  {
    public object CurrentId { get { return currentIdFunction(); } }
    public long LastEventSN { get; internal set; }
    public int ChangesFromLastSave { get; set; }

    public ObjectStoreKeyType KeyType { get; set; }

    #region Private Fields and function for storeid

    private long _storelonguid = 0;
    private int _storeintuid = 0;
    private Guid _storeGuid = Guid.Empty;

    private Func<object> newIdFunction;
    private Func<object> currentIdFunction;

    #endregion

    #region Private fields
    internal volatile List<Dictionary<int, byte[]>> _primarystore = new List<Dictionary<int, byte[]>>();
    #endregion

    #region Constructor

    internal ObjectStore()
      : this(ObjectStoreKeyType.Integer)
    { }

    internal ObjectStore(ObjectStoreKeyType keytype)
    {

      KeyType = keytype;

      switch (KeyType)
      {
        case ObjectStoreKeyType.Integer:
          newIdFunction = () => ++_storeintuid;
          currentIdFunction = () => _storeintuid;
          break;
        case ObjectStoreKeyType.LongInteger:
          newIdFunction = () => ++_storelonguid;
          currentIdFunction = () => _storelonguid;
          break;
        case ObjectStoreKeyType.GlobalUniqueIdentifier:
          newIdFunction = () => { _storeGuid = Guid.NewGuid(); return _storeGuid; };
          currentIdFunction = () => _storeGuid;
          break;
        default:
          break;
      }

    }
    #endregion

    #region Public methods for storing objects

    public void Set(BSonDoc entity)
    {
      ChangesFromLastSave++;
      var lastid = currentIdFunction();
      var uid = GetEntityUI(entity);
      if (uid.CompareTo(lastid) <= 0)
      {
        dUpdate(uid, entity);
        return;
      }
      dSet(uid, entity);
    }

    public void Remove(BSonDoc entity)
    {
      var lastid = currentIdFunction();
      var uid = GetEntityUI(entity);
      if (uid.CompareTo(lastid) <= 0)
        if (dRemove(uid))
          ChangesFromLastSave++;
    }

    #endregion

    #region private entities methods

    private int GetEntityUI(BSonDoc entity)
    {
      if (entity.HasProperty(Configuration.DensoIDKeyName) && entity[Configuration.DensoIDKeyName] != null)
        return (int)entity[Configuration.DensoIDKeyName];

      entity[Configuration.DensoIDKeyName] = newIdFunction();
      return (int)entity[Configuration.DensoIDKeyName];
    }

    #endregion

    #region private DeNSo Dictionaries manipulations methods

    private BSonDoc dGet(int key)
    {
      foreach (var d in _primarystore)
      {
        if (d.ContainsKey(key))
          return d[key].Deserialize();
      }
      return null;
    }

    private void dSet(int key, BSonDoc doc)
    {
      lock (_primarystore)
      {
        if (!dUpdate(key, doc))
          dInsert(key, doc);
      }
    }

    private void dInsert(int key, BSonDoc doc)
    {
      //doc["@ts#"

      Dictionary<int, byte[]> freedictionary = null;
      foreach (var d in _primarystore)
        if (d.Count < Configuration.DictionarySplitSize)
        {
          freedictionary = d; break;
        }

      if (freedictionary == null)
      {
        freedictionary = new Dictionary<int, byte[]>();
        _primarystore.Add(freedictionary);
      }

      if (!freedictionary.ContainsKey(key))
        freedictionary.Add(key, doc.Serialize());
    }

    private bool dUpdate(int key, BSonDoc doc)
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

    private bool dContains(int key)
    {
      foreach (var d in _primarystore)
      {
        if (d.ContainsKey(key))
          return true;
      }
      return false;
    }

    private bool dRemove(int key)
    {
      lock (_primarystore)
      {
        Dictionary<int, byte[]> realdictionary = null;
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

    internal void dInsert(int key, byte[] data)
    {
      Dictionary<int, byte[]> freedictionary = null;
      foreach (var d in _primarystore)
        if (d.Count < Configuration.DictionarySplitSize)
        {
          freedictionary = d; break;
        }

      if (freedictionary == null)
      {
        freedictionary = new Dictionary<int, byte[]>();
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

    public BSonDoc GetById(int key)
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
