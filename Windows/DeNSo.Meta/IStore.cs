using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DeNSo
{
  public interface IStore
  {
    string DataBaseName { get; }
    IObjectStore GetCollection(string collection);
  }

  public interface IObjectStore
  {
    IEnumerable<string> Where(Func<JObject, bool> filter);
    void Set(string key, JObject entity);
    void Remove(string key);
    void Flush();
    string GetById(string key);

    float IncoerenceIndexRatio();
    void Reindex();
  }
}
