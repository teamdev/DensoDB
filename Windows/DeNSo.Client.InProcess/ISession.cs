using System;
namespace DeNSo
{
  public interface ISession
  {
    int Count(string collection);
    int Count(string collection, System.Linq.Expressions.Expression<Func<Newtonsoft.Json.Linq.JObject, bool>> filter);
    int Count<T>() where T : class, new();
    int Count<T>(System.Linq.Expressions.Expression<Func<T, bool>> filter) where T : class, new();

    string DataBase { get; set; }

    EventCommandStatus Delete<T>(string collection, T entity);
    EventCommandStatus Delete<T>(T entity) where T : class;
    EventCommandStatus DeleteAll<T>(System.Collections.Generic.IEnumerable<T> entities) where T : class;
    EventCommandStatus DeleteAll<T>(string collection, System.Collections.Generic.IEnumerable<T> entities) where T : class;

    EventCommandStatus Execute<T>(T command) where T : class;

    EventCommandStatus Flush(string collection);
    EventCommandStatus Flush<T>() where T : class;

    string GetById(string collection, string id);
    T GetById<T>(string id) where T : class, new();

    System.Collections.Generic.IEnumerable<T> Get<T>() where T : class, new();
    System.Collections.Generic.IEnumerable<T> Get<T>(System.Linq.Expressions.Expression<Func<T, bool>> entityfilter = null) where T : class, new();
    System.Collections.Generic.IEnumerable<T> Get<T>(string collection, System.Linq.Expressions.Expression<Func<T, bool>> entityfilter = null) where T : class, new();

    System.Collections.Generic.IEnumerable<string> GetJSon(string collection, System.Linq.Expressions.Expression<Func<Newtonsoft.Json.Linq.JObject, bool>> jsonfilter = null);
    System.Collections.Generic.IEnumerable<string> GetJSon<T>(System.Linq.Expressions.Expression<Func<Newtonsoft.Json.Linq.JObject, bool>> jsonfilter = null) where T : class, new();

    EventCommandStatus Set<T>(string collection, T entity) where T : class;
    EventCommandStatus Set<T>(T entity) where T : class;
    EventCommandStatus SetAll<T>(System.Collections.Generic.IEnumerable<T> entity) where T : class;
    EventCommandStatus SetAll<T>(string collection, System.Collections.Generic.IEnumerable<T> entity) where T : class;

    void WaitForNonStaleDataAt(long eventcommandnumber);
    bool WaitForNonStaleDataAt(long eventcommandnumber, TimeSpan timeout);
    bool WaitForNonStaleDataAt(long eventcommandnumber, int timeout);
  }
}
