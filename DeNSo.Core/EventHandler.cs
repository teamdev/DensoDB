using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using DeNSo.Meta.BSon;
using System.Diagnostics;
using DeNSo.Meta;
using DeNSo.Core.DiskIO;
using System.ComponentModel.Composition;

namespace DeNSo.Core
{
  public static class EventHandlerManager
  {
    //private static Dictionary<string, Dictionary<string, Action<ObjectStoreWrapper, BSonDoc>>> _methods =
    //           new Dictionary<string, Dictionary<string, Action<ObjectStoreWrapper, BSonDoc>>>();


    private static Dictionary<string, List<ICommandHandler>> _commandHandlers = new Dictionary<string, List<ICommandHandler>>();
    private static List<Action<IStore, EventCommand>> _globaleventhandlers = new List<Action<IStore, EventCommand>>();

    internal static void AnalyzeCommandHandlers(ICommandHandler[] handlers)
    {
      foreach (var hand in handlers)
      {
        var attrs = hand.GetType().GetCustomAttributes(typeof(DeNSo.Meta.HandlesCommandAttribute), true);
        foreach (var at in attrs)
        {
          string commandname = ((DeNSo.Meta.HandlesCommandAttribute)at).Command;
          if (!_commandHandlers.ContainsKey(commandname))
            _commandHandlers.Add(commandname, new List<ICommandHandler>());
          _commandHandlers[commandname].Add(hand);
        }
      }
    }

    public static void RegisterGlobalEventHandler(Action<IStore, EventCommand> eventhandler)
    {
      _globaleventhandlers.Add(eventhandler);
    }

    public static void UnRegisterGlobalEventHandler(Action<IStore, EventCommand> eventhandler)
    {
      _globaleventhandlers.Remove(eventhandler);
    }

    internal static void ExecuteCommandEvent(string database, EventCommand waitingevent)
    {
      var store = new ObjectStoreWrapper(database);

      #region Execute inline global event handlers
      foreach (var ge in _globaleventhandlers)
      {
        try
        {
          if (ge != null) ge(store, waitingevent);
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
        }
      }
      #endregion

      var command = waitingevent.Command.Deserialize();

      var currenthandlers = ChechHandlers(command);
      if (currenthandlers != null)
        foreach (var hh in currenthandlers)
        {
          try
          {
            hh.HandleCommand(store, command);
          }
          catch (Exception ex)
          {
            Debug.WriteLine(ex.Message);
          }
        }
    }

    private static ICommandHandler[] ChechHandlers(BSonDoc command)
    {
      string actionname = string.Empty;
      if (command.HasProperty(CommandKeyword.Action))
        actionname = (command[CommandKeyword.Action] ?? string.Empty).ToString();

      if (_commandHandlers.ContainsKey(actionname))
        return _commandHandlers[actionname].ToArray();

      return null;
    }

  }
}
