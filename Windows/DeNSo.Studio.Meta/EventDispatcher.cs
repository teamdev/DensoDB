using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Studio.Meta
{
  public class EventDispatcher
  {
    private static EventDispatcher _current = new EventDispatcher();
    public static EventDispatcher Current
    { get { return _current; } }

    private EventDispatcher()
    { 
      
    }


//    using System;
//using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using System.ComponentModel.Composition.Hosting;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;

//namespace EOM
//{
//  public class EventHandlerManager
//  {
//    private static EventHandlerManager _current;
//    public static EventHandlerManager Current
//    {
//      get { return _current; }
//      private set { _current = value; }
//    }

//    private static Dictionary<Type, List<IEventHandler>> _registeredhandlers = new Dictionary<Type, List<IEventHandler>>();
//    private static Dictionary<string, List<ISimpleEventHandler>> _registeredSimpleHandlers = new Dictionary<string, List<ISimpleEventHandler>>();

//    [ImportMany(typeof(IEventHandler), AllowRecomposition = true)]
//    public IEventHandler[] EventHandlers { get; set; }

//    [ImportMany(typeof(ISimpleEventHandler), AllowRecomposition = true)]
//    public ISimpleEventHandler[] SimpleEventHandlers { get; set; }

//    static EventHandlerManager()
//    {
//      var dc = new DirectoryCatalog(".");
//      var cc = new CompositionContainer(dc);

//      _current = new EventHandlerManager();

//      try
//      {
//        cc.ComposeParts(_current);
//      }
//      catch (Exception ex)
//      {
//        Debug.WriteLine(ex.Message);
//      }

//      foreach (var item in _current.EventHandlers)
//      {
//        var ttinterfaces = item.GetType().GetInterfaces();

//        foreach (var tint in ttinterfaces)
//        {
//          if (tint.IsGenericType)
//          {
//            foreach (var arg in tint.GetGenericArguments())
//              RegisterHandler(arg, item);
//          }
//        }
//      }

//      foreach (var item in _current.SimpleEventHandlers)
//      {
//        var mm = (item.Handledmessage ?? string.Empty).ToLower();
//        if (!_registeredSimpleHandlers.ContainsKey(mm))
//          _registeredSimpleHandlers.Add(mm, new List<ISimpleEventHandler>());

//        _registeredSimpleHandlers[mm].Add(item);
//      }
//    }

//    private static void RegisterHandler(Type messagetype, IEventHandler handler)
//    {
//      if (!_registeredhandlers.ContainsKey(messagetype))
//        _registeredhandlers.Add(messagetype, new List<IEventHandler>());

//      _registeredhandlers[messagetype].Add(handler);
//    }

//    public static void ExecuteEvent<T>(T message) where T : class
//    {
//      if (_registeredhandlers.ContainsKey(typeof(T)))
//        foreach (var eh in _registeredhandlers[typeof(T)])
//          try
//          {
//            ((dynamic)eh).Handle(message);
//          }
//          catch (Exception ex)
//          {
//            Debug.WriteLine(string.Format(" Error executing handler '{0}' for message '{1}': {2}", eh.GetType().Name, typeof(T).Name, ex.Message));
//          }

//      if (_registeredhandlers.ContainsKey(typeof(object)))
//        foreach (var ee in _registeredhandlers[typeof(object)])
//          try
//          {
//            ((dynamic)ee).Handle(message);
//          }
//          catch (Exception ex)
//          {
//            Debug.WriteLine(ex.Message);
//            Debug.WriteLine(ex.StackTrace);
//          }

//    }

//    public static void ExecuteSimpleEvent(string eventname)
//    {
//      eventname = eventname.ToLower();

//      if (_registeredSimpleHandlers.ContainsKey(eventname))
//        foreach (var i in _registeredSimpleHandlers[eventname])
//          try
//          {
//            i.Handle(eventname);
//          }
//          catch (Exception ex)
//          {
//            Debug.WriteLine(ex.Message);
//            Debug.WriteLine(ex.StackTrace);
//          }

//      if (_registeredSimpleHandlers.ContainsKey(string.Empty))
//        foreach (var i in _registeredSimpleHandlers[string.Empty])
//          try
//          {
//            i.Handle(eventname);
//          }
//          catch (Exception ex)
//          {
//            Debug.WriteLine(ex.Message);
//            Debug.WriteLine(ex.StackTrace);
//          }
//    }
//  }
//}








  }
}
