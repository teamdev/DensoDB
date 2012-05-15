using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Core.CQRS;
using DeNSo.Meta.BSon;
using DeNSo.Meta;
using System.Linq.Expressions;
using System.Threading;
using DeNSo.Core.Struct;

namespace DeNSo.Core
{
  public delegate void StoreUpdatedHandler(long executedcommandsn);

  public class Session : DeNSo.Meta.ISession, IDisposable
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
          if (_waitingfor <= sn)
            _waiting.Set();
        };
      });
      waitingThread.IsBackground = true;
      waitingThread.Start();
    }

    public void WaitForNonStaleDataAt(long eventcommandnumber)
    {
      if (_lastexecutedcommand >= eventcommandnumber) return;
      _waitingfor = eventcommandnumber;
      _waiting.WaitOne();
      _waiting.Reset();
      _waitingfor = 0;

    }
    public void WaitForNonStaleDataAt(long eventcommandnumber, TimeSpan timeout)
    {
      if (_lastexecutedcommand >= eventcommandnumber) return;
      _waitingfor = eventcommandnumber;
      _waiting.WaitOne(timeout);
      _waiting.Reset();
      _waitingfor = 0;
    }
    public void WaitForNonStaleDataAt(long eventcommandnumber, int timeout)
    {
      if (_lastexecutedcommand >= eventcommandnumber) return;
      _waitingfor = eventcommandnumber;

      _waiting.WaitOne(timeout);
      _waiting.Reset();
      _waitingfor = 0;
    }

    public EventCommandStatus Set<T>(T entity) where T : class
    {
      var cmd = new { _action = "set", _value = entity, _collection = typeof(T).Name };
      return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
    }
    public EventCommandStatus Set<T>(string collection, T entity) where T : class
    {
      var cmd = new { _action = "set", _collection = collection, _value = entity };
      return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
    }

    public EventCommandStatus Execute<T>(T command) where T : class
    {
      return EventCommandStatus.Create(_command.Execute(DataBase, command.ToBSon().Serialize()), this);
    }

    public EventCommandStatus Delete<T>(T entity) where T : class
    {
      var cmd = new { _action = "delete", _value = entity, _collection = typeof(T).Name };
      return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
    }
    public EventCommandStatus Delete<T>(string collection, T entity)
    {
      var cmd = new { _action = "delete", _collection = collection, _value = entity };
      return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
    }

    public EventCommandStatus Flush<T>() where T : class
    {
      var cmd = new { _action = "flush", _collection = typeof(T).Name };
      return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
    }
    public EventCommandStatus Flush(string collection)
    {
      var cmd = new { _action = "flush", _collection = collection };
      return EventCommandStatus.Create(_command.Execute(DataBase, cmd.ToBSon().Serialize()), this);
    }

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
    public IEnumerable<BSonDoc> Get(string collection, Expression<Func<BSonDoc, bool>> filter = null)
    {
      return _query.Get(DataBase, collection, filter != null ? filter.Compile() : null);
    }

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
