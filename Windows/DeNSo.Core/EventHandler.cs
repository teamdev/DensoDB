using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

using System.Diagnostics;
using DeNSo;
using DeNSo.DiskIO;
using System.ComponentModel.Composition;
using Newtonsoft.Json.Linq;

namespace DeNSo
{
  public static class EventHandlerManager
  {

    private static Dictionary<string, List<ICommandHandler>> _commandHandlers = new Dictionary<string, List<ICommandHandler>>();
    private static List<Action<IStore, EventCommand>> _globaleventhandlers = new List<Action<IStore, EventCommand>>();

    internal static void AnalyzeCommandHandlers(ICommandHandler[] handlers)
    {
      LogWriter.LogInformation("Start analyzing and preparing command handlers", LogEntryType.Warning);
      foreach (var hand in handlers)
      {
        LogWriter.LogInformation(string.Format("Registering command handler {0}", hand.GetType().Name), LogEntryType.Information);

        var attrs = hand.GetType().GetCustomAttributes(typeof(DeNSo.HandlesCommandAttribute), true);
        foreach (var at in attrs)
        {
          string commandname = ((DeNSo.HandlesCommandAttribute)at).Command;
          if (!_commandHandlers.ContainsKey(commandname))
            _commandHandlers.Add(commandname, new List<ICommandHandler>());
          _commandHandlers[commandname].Add(hand);
          LogWriter.LogInformation(string.Format(" Handler registered for command {0}", commandname), LogEntryType.SuccessAudit);
        }
      }
    }

    public static void RegisterGlobalEventHandler(Action<IStore, EventCommand> eventhandler)
    {
      LogWriter.LogInformation("Registering a global event handler", LogEntryType.SuccessAudit);
      _globaleventhandlers.Add(eventhandler);
    }

    public static void UnRegisterGlobalEventHandler(Action<IStore, EventCommand> eventhandler)
    {
      LogWriter.LogInformation("Unregistering a global event handler", LogEntryType.SuccessAudit);
      _globaleventhandlers.Remove(eventhandler);
    }

    internal static void ExecuteCommandEvent(string database, EventCommand waitingevent)
    {
      var store = new ObjectStoreWrapper(database);

      LogWriter.LogInformation("Executing waiting event", LogEntryType.Information);
      #region Execute inline global event handlers
      foreach (var ge in _globaleventhandlers)
      {
        try
        {
          if (ge != null) ge(store, waitingevent);
        }
        catch (Exception ex)
        {
          LogWriter.LogException(ex);
        }
      }
      #endregion

      var command = JObject.Parse(waitingevent.Command);

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
            LogWriter.LogException(ex);
          }
        }
    }

    private static ICommandHandler[] ChechHandlers(JObject command)
    {
      string actionname = string.Empty;
      var r = command.Property(CommandKeyword.Action);
      if (r != null)
        actionname = ((string)r ?? string.Empty).ToString();

      LogWriter.LogInformation(string.Format("Executing action {0}", string.Empty), LogEntryType.Information);

      if (_commandHandlers.ContainsKey(actionname))
        return _commandHandlers[actionname].ToArray();

      return null;
    }

  }
}
