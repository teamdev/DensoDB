using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.CQRS;
using DeNSo;
using System.Linq.Expressions;
using System.Threading;
using DeNSo.Struct;
using DeNSo.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DeNSo.Linq;

namespace DeNSo
{
  public delegate void StoreUpdatedHandler(long executedcommandsn);

  public class Session : DeNSo.ISession, IDisposable
  {
    private Command _command = new Command();
    private Query _query = new Query();

    private ManualResetEvent _waiting = new ManualResetEvent(false);
    private long _waitingfor = 0;
    private long _lastexecutedcommand = 0;

    public static string DefaultDataBase { get; set; }
    public static Session New { get { return new Session() { DataBase = DefaultDataBase ?? string.Empty }; } }

    public string DataBase { get; set; }
    public static event StoreUpdatedHandler StoreUpdatedHandler;

    internal static void RaiseStoreUpdated(long commandnumber)
    {
      if (StoreUpdatedHandler != null)
        StoreUpdatedHandler(commandnumber);
    }

    private Session()
    {
      StoreManager.Start();
      //RegisterWaitEventAsync();
    }

    public void Dispose()
    {
    }

    private void RegisterWaitEventAsync()
    {
      Thread waitingThread = new Thread((ThreadStart)delegate
      {
        Session.StoreUpdatedHandler += (sn) =>
        {
          _lastexecutedcommand = sn;
          //if (_waitingfor <= sn)
          _waiting.Set();
        };
      });
      waitingThread.IsBackground = true;
      waitingThread.Start();
    }

    #region Wait Methods
    public void WaitForNonStaleDataAt(long eventcommandnumber)
    {
      //if (_lastexecutedcommand >= eventcommandnumber) return;
      _waitingfor = eventcommandnumber;
      while (_lastexecutedcommand < eventcommandnumber)
      {
        _waiting.WaitOne(200);
        _waiting.Reset();
      }
      _waitingfor = 0;
    }
    public bool WaitForNonStaleDataAt(long eventcommandnumber, TimeSpan timeout)
    {
      //if (_lastexecutedcommand >= eventcommandnumber) return;
      _waitingfor = eventcommandnumber;
      _waiting.WaitOne(timeout);
      if (_lastexecutedcommand < eventcommandnumber) return false;
      _waiting.Reset();
      _waitingfor = 0;
      return true;
    }
    public bool WaitForNonStaleDataAt(long eventcommandnumber, int timeout)
    {
      _waitingfor = eventcommandnumber;

      _waiting.WaitOne(timeout);
      if (_lastexecutedcommand < eventcommandnumber) return false;
      _waiting.Reset();
      _waitingfor = 0;
      return true;
    }
    #endregion

    #region Set Methods
    public EventCommandStatus Set<T>(T entity) where T : class
    {
      var cmd = new { _action = DensoBuiltinCommands.Set, _value = entity, _collection = typeof(T).Name };
      return EventCommandStatus.Create(_command.Execute(DataBase, JsonConvert.SerializeObject(cmd)), this);
    }
    public EventCommandStatus Set<T>(string collection, T entity) where T : class
    {
      var cmd = new { _action = DensoBuiltinCommands.Set, _collection = collection, _value = entity };
      return EventCommandStatus.Create(_command.Execute(DataBase, JsonConvert.SerializeObject(cmd)), this);
    }
    public EventCommandStatus SetAll<T>(IEnumerable<T> entity) where T : class
    {
      EventCommandStatus result = new EventCommandStatus();
      foreach (var item in entity)
      {
        result = Set<T>(item);
      }
      return result;
      //var cmd = new { _action = DensoBuiltinCommands.SetMany, _collection = typeof(T).Name, _value = entity };
      //return EventCommandStatus.Create(_command.Execute(DataBase, JsonConvert.SerializeObject(cmd)), this);
    }
    public EventCommandStatus SetAll<T>(string collection, IEnumerable<T> entity) where T : class
    {
      EventCommandStatus result = new EventCommandStatus();
      foreach (var item in entity)
      {
        result = Set<T>(collection, item);
      }
      return result;
      //var cmd = new { _action = DensoBuiltinCommands.SetMany, _collection = collection, _value = entity };
      //return EventCommandStatus.Create(_command.Execute(DataBase, JsonConvert.SerializeObject(cmd)), this);
    }
    #endregion

    public EventCommandStatus Execute<T>(T command) where T : class
    {
      return EventCommandStatus.Create(_command.Execute(DataBase, JsonConvert.SerializeObject(command)), this);
    }

    #region Delete methods
    public EventCommandStatus Delete<T>(T entity) where T : class
    {
      return Delete<T>(typeof(T).Name, entity);
    }
    public EventCommandStatus Delete<T>(string collection, T entity)
    {
      var enttype = typeof(T);
      var pi = enttype.GetProperty(DocumentMetadata.IdPropertyName);
      if (pi != null)
      {
        var cmd = new { _action = DensoBuiltinCommands.Delete, _id = pi.GetValue(entity, null), _collection = collection };
        return EventCommandStatus.Create(_command.Execute(DataBase, JsonConvert.SerializeObject(cmd)), this);
      }
      throw new DocumentWithoutIdException();
    }
    public EventCommandStatus DeleteAll<T>(IEnumerable<T> entities) where T : class
    {
      return DeleteAll<T>(typeof(T).Name, entities);
    }
    public EventCommandStatus DeleteAll<T>(string collection, IEnumerable<T> entities) where T : class
    {
      EventCommandStatus cs = new EventCommandStatus();
      foreach (var item in entities)
      {
        cs = Delete(collection, item);
      }
      return cs;
    }
    #endregion

    #region Flush methods
    public EventCommandStatus Flush<T>() where T : class
    {
      var cmd = new { _action = DensoBuiltinCommands.CollectionFlush, _collection = typeof(T).Name };
      return EventCommandStatus.Create(_command.Execute(DataBase, JsonConvert.SerializeObject(cmd)), this);
    }
    public EventCommandStatus Flush(string collection)
    {
      var cmd = new { _action = DensoBuiltinCommands.CollectionFlush, _collection = collection };
      return EventCommandStatus.Create(_command.Execute(DataBase, JsonConvert.SerializeObject(cmd)), this);
    }
    #endregion

    #region Get Methods
    //public IEnumerable<T> Get<T>(Expression<Func<T, bool>> filter = null) where T : class, new()
    //{
    //  return this.Get(typeof(T).Name, filter);
    //}
    //public IEnumerable<T> Get<T>(string collection, Expression<Func<T, bool>> filter = null) where T : class, new()
    //{
    //  Generic2BsonLambdaConverter visitor = new Generic2BsonLambdaConverter();
    //  var expr = visitor.Visit(filter) as Expression<Func<BSonDoc, bool>>;
    //  return Get(collection, expr != null ? expr.Compile() : null).Select(doc => doc.FromBSon<T>()).AsEnumerable();
    //}
    public IEnumerable<T> Get<T>() where T : class, new()
    {
      return GetJSon(typeof(T).Name).Select(doc => JsonConvert.DeserializeObject<T>(doc)).AsEnumerable();
    }

    public IEnumerable<T> Get<T>(Expression<Func<T, bool>> filter = null) where T : class, new()
    {
      var qt = new QueryTranslator<T>();
      var tfilter = qt.Translate(filter) as Expression<Func<JObject, bool>>;
      return GetJSon(typeof(T).Name, tfilter).Select(doc => JsonConvert.DeserializeObject<T>(doc)).AsEnumerable();
    }

    public IEnumerable<T> Get<T>(string collection, Expression<Func<T, bool>> filter = null) where T : class, new()
    {
      var qt = new QueryTranslator<T>();
      var tfilter = qt.Translate(filter) as Expression<Func<JObject, bool>>;
      return GetJSon(collection, tfilter).Select(doc => JsonConvert.DeserializeObject<T>(doc)).AsEnumerable();
    }

    public IEnumerable<string> GetJSon<T>(Expression<Func<JObject, bool>> filter = null) where T : class, new()
    {
      return GetJSon(typeof(T).Name, filter).AsEnumerable();
    }

    public IEnumerable<string> GetJSon(string collection, Expression<Func<JObject, bool>> filter = null)
    {

      return _query.Get(DataBase, collection, filter != null ? filter.Compile() : (Func<JObject, bool>)null);
    }

    public T GetById<T>(string id) where T : class, new()
    {
      var result = _query.Get(DataBase, typeof(T).Name, id);
      if (result != null)
        return JsonConvert.DeserializeObject<T>(result);
      return default(T);
    }

    public string GetById(string collection, string id)
    {
      return _query.Get(DataBase, collection, id);
    }
    #endregion

    #region Count Methods
    public int Count<T>() where T : class, new()
    {
      return Count(typeof(T).Name);
    }
    public int Count<T>(Expression<Func<T, bool>> filter) where T : class, new()
    {
      var qt = new QueryTranslator<T>();
      var tfilter = qt.Translate(filter) as Expression<Func<JObject, bool>>;
      return Count(typeof(T).Name, tfilter);
    }

    public int Count(string collection)
    {
      return _query.Count(DataBase, collection);
    }
    public int Count(string collection, Expression<Func<JObject, bool>> filter)
    {
      return _query.Count(DataBase, collection, filter.Compile());
    }
    //public int Count<T>(Expression<Func<T, bool>> filter) where T : class, new()
    //{
    //  Generic2BsonLambdaConverter visitor = new Generic2BsonLambdaConverter();
    //  var expr = visitor.Visit(filter) as Expression<Func<JObject, bool>>;
    //  return Count(typeof(T).Name, expr);
    //}
    #endregion

    public static void ShutDown()
    {
      StoreManager.ShutDown();
    }
    public static void Start()
    {
      StoreManager.Start();
    }
  }
}
