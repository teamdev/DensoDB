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

    #region Private Fields and function for storeid
    //private long _storelonguid = 0;
    private int _storeintuid = 0;
    //private StoreGuid _storeGuid = new StoreGuid();

    private Func<object> newIdFunction;
    private Func<object> currentIdFunction;
    #endregion

    #region Private fields
    internal volatile List<Dictionary<int, byte[]>> _primarystore = new List<Dictionary<int, byte[]>>();
    #endregion

    #region Constructor
    internal ObjectStore()
    {
      //if (ptype == typeof(int))
      //{
      newIdFunction = () => ++_storeintuid;
      currentIdFunction = () => _storeintuid;
      //}

      //if (ptype == typeof(long))
      //{
      //newIdFunction = () => ++_storelonguid;
      //currentIdFunction = () => _storelonguid;
      //}

      //if (ptype == typeof(Guid))
      //{
      //_storeGuid.init();
      //newIdFunction = () => _storeGuid.Increment();
      //currentIdFunction = () => _storeGuid.GetUid();
      //}

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
      //if (_primarystore.ContainsKey(uid))
      //{
      //  _primarystore[uid] = entity;
      //  return;
      //}
      //_primarystore.Add(uid, entity);
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
      if (entity.HasProperty("_id") && entity["_id"] != null)
        return (int)entity["_id"];

      entity["_id"] = newIdFunction();
      return (int)entity["_id"];
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
