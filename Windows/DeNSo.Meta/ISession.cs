using System;
namespace DeNSo
{
  public interface ISession
  {
    int Count(string collection);
    int Count(string collection, System.Linq.Expressions.Expression<Func<DeNSo.BSon.BSonDoc, bool>> filter);
    int Count<T>();
    int Count<T>(System.Linq.Expressions.Expression<Func<T, bool>> filter) where T : class, new();
    string DataBase { get; set; }
    EventCommandStatus Delete<T>(string collection, T entity);
    EventCommandStatus Delete<T>(T entity) where T : class;
    EventCommandStatus Execute<T>(T command) where T : class;
    System.Collections.Generic.IEnumerable<DeNSo.BSon.BSonDoc> Get(string collection, Func<DeNSo.BSon.BSonDoc, bool> filter = null);
    System.Collections.Generic.IEnumerable<T> Get<T>(System.Linq.Expressions.Expression<Func<T, bool>> filter = null) where T : class, new();
    System.Collections.Generic.IEnumerable<T> Get<T>(Func<DeNSo.BSon.BSonDoc, bool> bsonfilter = null, Func<T, bool> entityfilter = null) where T : class, new();
    System.Collections.Generic.IEnumerable<T> Get<T>(string collection, System.Linq.Expressions.Expression<Func<T, bool>> filter = null) where T : class, new();
    EventCommandStatus Set<T>(string collection, T entity) where T : class;
    EventCommandStatus Set<T>(T entity) where T : class;

    void WaitForNonStaleDataAt(long eventcommandnumber);
    bool WaitForNonStaleDataAt(long eventcommandnumber, TimeSpan timeout);
    bool WaitForNonStaleDataAt(long eventcommandnumber, int timeout);
  }
}
