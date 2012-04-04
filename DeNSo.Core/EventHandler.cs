using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using DeNSo.Meta.BSon;
using System.Diagnostics;
using DeNSo.Meta;

namespace DeNSo.Core
{
  public static class EventHandlerManager
  {
    private static Dictionary<string, Dictionary<string, Action<ObjectStoreWrapper, BSonDoc>>> _methods =
               new Dictionary<string, Dictionary<string, Action<ObjectStoreWrapper, BSonDoc>>>();

    private static List<Action<IStore, BSonDoc>> _globaleventhandlers = new List<Action<IStore, BSonDoc>>();

    public static void RegisterGlobalEventHandler(Action<IStore, BSonDoc> eventhandler)
    {
      _globaleventhandlers.Add(eventhandler);
    }

    public static void UnRegisterGlobalEventHandler(Action<IStore, BSonDoc> eventhandler)
    {
      _globaleventhandlers.Remove(eventhandler);
    }

    internal static void ExecuteEvent(string database, BSonDoc command)
    {
      var store = new ObjectStoreWrapper(database);

      foreach (var ge in _globaleventhandlers)
      {
        try
        {
          if (ge != null) ge(store, command);
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
        }
      }

      Action<ObjectStoreWrapper, BSonDoc> method = FindEventHandler(command);
      try
      {
        if (method != null)
          method(store, command);
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
      }
    }

    private static Action<ObjectStoreWrapper, BSonDoc> FindEventHandler(BSonDoc command)
    {
      var type = string.Empty;
      var action = string.Empty;
      Action<ObjectStoreWrapper, BSonDoc> mi = null;

      ExtractInfoFromCommand(command, ref type, ref action);
      mi = CheckMethodCache(type, action);

      if (mi != null) return mi;

      mi = ReflectMethodInfo(type, action);

      if (mi != null && !_methods[type].ContainsKey(action))
        _methods[type].Add(action, mi);

      return mi;
    }

    private static Action<ObjectStoreWrapper, BSonDoc> ReflectMethodInfo(string type, string action)
    {
      MethodInfo mi = null;
      if (!string.IsNullOrEmpty(type))
      {
        var tt = Type.GetType(type, false);
        if (tt != null)
        {
          mi = tt.GetMethod(action, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
        }
      }

      if (mi == null)
      {
        var evnthandlertype = typeof(DefaultCommandHandler);
        mi = evnthandlertype.GetMethod(action, BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
      }

      if (mi != null)
      {
        return Delegate.CreateDelegate(typeof(Action<ObjectStoreWrapper, BSonDoc>), mi) as Action<ObjectStoreWrapper, BSonDoc>;
      }
      return null;
    }

    private static Action<ObjectStoreWrapper, BSonDoc> CheckMethodCache(string type, string action)
    {
      Action<ObjectStoreWrapper, BSonDoc> mi = null;
      if (!_methods.ContainsKey(type))
        _methods.Add(type, new Dictionary<string, Action<ObjectStoreWrapper, BSonDoc>>());

      if (_methods[type].ContainsKey(action))
        mi = _methods[type][action];
      return mi;
    }

    private static void ExtractInfoFromCommand(BSonDoc command, ref string type, ref string action)
    {
      if (command.HasProperty("_action"))
        action = (command["_action"] ?? string.Empty).ToString();

      if (command.HasProperty("_type"))
        type = (command["_type"] ?? string.Empty).ToString();
    }
  }
}
