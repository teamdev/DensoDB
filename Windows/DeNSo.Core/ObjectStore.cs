using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using DeNSo.Struct;
using DeNSo;

using Newtonsoft.Json.Linq;
using DeNSo.Core.Filters;

namespace DeNSo
{
  // Changes list. 
  // 2012-04-23 -> converted storekey from int to byte[] to store multiple types value. 

#if WINDOWS
  [Serializable]
#endif
  public class ObjectStore : IObjectStore
  {
    private int _indexpossibleincoerences = 0;
    private volatile BloomFilter<string> _bloomfilter = new BloomFilter<string>(Configuration.DictionarySplitSize * 2);

    internal volatile List<Dictionary<string, string>> _primarystore = new List<Dictionary<string, string>>();

    public int ChangesFromLastSave { get; set; }
    public long LastEventSN { get; internal set; }

    public int Count()
    {
      var result = 0;
      foreach (var d in _primarystore)
        lock (d)
          result += d.Count;

      return result;
    }

    public int Count(Func<JObject, bool> filter)
    {
      int count = 0;
      if (filter != null)
        foreach (var d in _primarystore)
        {
          lock (d)
            foreach (var v in d.Values)
            {
              if (!string.IsNullOrEmpty(v) && filter(JObject.Parse(v)))
                count++;
            }
        }

      return count;
    }

    public IEnumerable<string> GetAll()
    {
      foreach (var d in _primarystore)
      {
        lock (d)
          foreach (var v in d.Values)
          {
            if (!string.IsNullOrEmpty(v))
              yield return v;
          }
      }

      yield break;
    }

    public string GetById(string key)
    {
      return InternalDictionaryGet(key);
    }

    public void Flush()
    {
      foreach (var d in _primarystore)
      {
        lock (d)
          d.Clear();
      }
    }

    public float IncoerenceIndexRatio()
    {
      return (((float)Math.Max(this.Count() - _bloomfilter.Size, 0) + (float)_indexpossibleincoerences) / (float)this.Count()) * 100;
    }

    public void Remove(string key)
    {
      if (InternalDictionaryContains(key))
        if (InternalDictionaryRemove(key))
          ChangesFromLastSave++;
    }

    public void Reindex()
    {
      lock (_bloomfilter)
      {
        var newsize = this.Count() + Configuration.DictionarySplitSize * 2;
        var newbloom = new BloomFilter<string>(newsize);
        foreach (var d in _primarystore)
          foreach (var k in d.Keys)
            newbloom.Add(k);

        _bloomfilter = newbloom;
        _indexpossibleincoerences = 0;
      }
    }

    public void Set(string key, JObject document)
    {
      ChangesFromLastSave++;

      if (string.IsNullOrEmpty(key))
      {
        key = GetEntityUI(document);
      }

      if (InternalDictionaryContains(key))
      {
        InternalDictionaryUpdate(key, document.ToString());
        return;
      }
      InternalDictionarySet(key, document.ToString());
    }

    public IEnumerable<string> Where(Func<JObject, bool> filter)
    {
      if (filter != null)
        foreach (var d in _primarystore)
        {
          lock (d)
            foreach (var v in d.Values)
            {
              if (!string.IsNullOrEmpty(v) && filter(JObject.Parse(v)))
                yield return v;
            }
        }

      yield break;
    }

    private string GetEntityUI(JObject document)
    {
      var r = document.Property(DocumentMetadata.IdPropertyName);
      var newkey = ((string)r ?? Guid.NewGuid().ToString());
      document[DocumentMetadata.IdPropertyName] = newkey;
      return newkey;
    }

    private string InternalDictionaryGet(string key)
    {
      if (BloomFilterCheck(key))
        foreach (var d in _primarystore)
        {
          if (d.ContainsKey(key))
            return d[key];
        }
      return null;
    }

    internal void InternalDictionaryInsert(string key, string doc)
    {
      Dictionary<string, string> freedictionary = null;
      foreach (var d in _primarystore)
        lock (d)
          if (d.Count < Configuration.DictionarySplitSize)
          {
            freedictionary = d; break;
          }

      if (freedictionary == null)
      {
        freedictionary = new Dictionary<string, string>();
        _primarystore.Add(freedictionary);
      }

      lock (freedictionary)
        if (!freedictionary.ContainsKey(key))
          freedictionary.Add(key, doc);

      BloomFilterAdd(key);
    }

    private void InternalDictionarySet(string key, string doc)
    {
      lock (_primarystore)
      {
        if (!InternalDictionaryUpdate(key, doc))
        {
          InternalDictionaryInsert(key, doc);
        }
      }
    }

    private bool InternalDictionaryUpdate(string key, string doc)
    {
      if (BloomFilterCheck(key))
        lock (_primarystore)
          foreach (var d in _primarystore)
            lock (d)
              if (d.ContainsKey(key))
              {
                d[key] = doc;
                return true;
              }
      return false;
    }

    private bool InternalDictionaryContains(string key)
    {
      if (!string.IsNullOrEmpty(key))
        if (BloomFilterCheck(key))
          foreach (var d in _primarystore)
          {
            lock (d)
              if (d.ContainsKey(key))
                return true;
          }
      return false;
    }

    private bool InternalDictionaryRemove(string key)
    {
      if (BloomFilterCheck(key))
        lock (_primarystore)
        {
          Dictionary<string, string> realdictionary = null;
          foreach (var d in _primarystore)
            lock (d)
              if (d.ContainsKey(key))
              {
                realdictionary = d; break;
              }

          lock (realdictionary)
            if (realdictionary != null)
            {
              realdictionary.Remove(key);
              _indexpossibleincoerences++;
              return true;
            }
        }
      return false;
    }

    private bool BloomFilterCheck(string key)
    {
      lock (_bloomfilter)
        return _bloomfilter.Contains(key);
    }

    private void BloomFilterAdd(string key)
    {
      lock (_bloomfilter)
        _bloomfilter.Add(key);
    }

  }
}
