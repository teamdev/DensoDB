using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.CQRS;
using DeNSo.BSon;
using DeNSo;
using System.Linq.Expressions;
using System.Threading;
using DeNSo.Struct;
using DeNSo.Exceptions;

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
      RegisterWaitEventAsync();
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
      return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
    }
    public EventCommandStatus Set<T>(string collection, T entity) where T : class
    {
      var cmd = new { _action = DensoBuiltinCommands.Set, _collection = collection, _value = entity };
      return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
    }
    public EventCommandStatus SetAll<T>(IEnumerable<T> entity) where T : class
    {
      var cmd = new { _action = DensoBuiltinCommands.SetMany, _collection = typeof(T).Name, _value = entity };
      return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
    }
    public EventCommandStatus SetAll<T>(string collection, IEnumerable<T> entity) where T : class
    {
      var cmd = new { _action = DensoBuiltinCommands.SetMany, _collection = collection, _value = entity };
      return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
    }
    #endregion

    public EventCommandStatus Execute<T>(T command) where T : class
    {
      return EventCommandStatus.Create(_command.Execute(DataBase, command.ToBSon().Serialize()), this);
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
        return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
      }
      throw new DocumentWithoutIdException();
    }
    public EventCommandStatus DeleteAll<T>(IEnumerable<T> entities) where T : class
    {
      return DeleteAll<T>(typeof(T).Name, entities);
    }
    public EventCommandStatus DeleteAll<T>(string collection, IEnumerable<T> entities) where T : class
    {
      var enttype = typeof(T);
      List<object> itemsid = new List<object>();
      foreach (var item in entities)
      {
        var pi = enttype.GetProperty(DocumentMetadata.IdPropertyName);
        if (pi != null)
        {
          itemsid.Add(pi.GetValue(item, null));
        }
      }

      if (itemsid.Count() > 0)
      {
        var cmd = new { _action = DensoBuiltinCommands.Delete, _value = itemsid, _collection = typeof(T).Name };
        return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
      }

      return EventCommandStatus.Create(-1, this);
    }
    #endregion

    #region Flush methods
    public EventCommandStatus Flush<T>() where T : class
    {
      var cmd = new { _action = DensoBuiltinCommands.CollectionFlush, _collection = typeof(T).Name };
      return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
    }
    public EventCommandStatus Flush(string collection)
    {
      var cmd = new { _action = DensoBuiltinCommands.CollectionFlush, _collection = collection };
      return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
    }
    #endregion

    #region Get Methods
    public IEnumerable<T> Get<T>(Expression<Func<T, bool>> filter = null) where T : class, new()
    {
      return this.Get(typeof(T).Name, filter);
    }
    public IEnumerable<T> Get<T>(string collection, Expression<Func<T, bool>> filter = null) where T : class, new()
    {
      Generic2BsonLambdaConverter visitor = new Generic2BsonLambdaConverter();
      var expr = visitor.Visit(filter) as Expression<Func<BSonDoc, bool>>;
      return Get(collection, expr).Select(doc => doc.FromBSon<T>()).AsEnumerable();
    }

    public IEnumerable<T> Get<T>(Expression<Func<BSonDoc, bool>> filter = null) where T : class, new()
    {
      return Get(typeof(T).Name, filter).Select(doc => doc.FromBSon<T>()).AsEnumerable();
    }

    public IEnumerable<BSonDoc> Get(string collection, Expression<Func<BSonDoc, bool>> filter = null)
    {
      return _query.Get(DataBase, collection, filter != null ? filter.Compile() : null);
    }
    #endregion

    #region Count Methods
    public int Count<T>()
    {
      return Count(typeof(T).Name);
    }
    public int Count(string collection)
    {
      return _query.Count(DataBase, collection);
    }
    public int Count(string collection, Expression<Func<BSonDoc, bool>> filter)
    {
      return _query.Count(DataBase, collection, filter.Compile());
    }
    public int Count<T>(Expression<Func<T, bool>> filter) where T : class, new()
    {
      Generic2BsonLambdaConverter visitor = new Generic2BsonLambdaConverter();
      var expr = visitor.Visit(filter) as Expression<Func<BSonDoc, bool>>;
      return Count(typeof(T).Name, expr);
    }
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
