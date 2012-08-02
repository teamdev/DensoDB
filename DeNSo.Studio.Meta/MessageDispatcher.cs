using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;

namespace DeNSo.Studio.Meta
{
  public class MessageDispatcher
  {
    private static Dictionary<Type, List<IUIMessageHandler>> _resolvedMessageHandlers = new Dictionary<Type, List<IUIMessageHandler>>();

    public static MessageDispatcher Current { get; private set; }

    [ImportMany(typeof(IUIMessageHandler))]
    private IUIMessageHandler[] MessageHandlers;

    static MessageDispatcher()
    {
      Current = new MessageDispatcher();
    }

    private MessageDispatcher()
    {
      InitMEF();
      RegisterMessageHandlers();

    }

    private void InitMEF()
    {
      DirectoryCatalog catalog = new DirectoryCatalog(".");
      catalog.Refresh();

      CompositionContainer container = new CompositionContainer(catalog);
      container.ComposeParts(this);
    }

    private void RegisterMessageHandlers()
    {
      foreach (var item in MessageHandlers)
      {
        var tt = item.GetType();
        foreach (var gt in tt.GetGenericArguments())
        {
          RegisterMessageHandler(gt, item);
        }
      }
    }

    private void RegisterMessageHandler(Type messagetype, IUIMessageHandler handler)
    {
      if (!_resolvedMessageHandlers.ContainsKey(messagetype))
        _resolvedMessageHandlers.Add(messagetype, new List<IUIMessageHandler>());

      _resolvedMessageHandlers[messagetype].Add(handler);
    }

    public void DispatchMessage<T>(T message) where T : IUIMessage
    {
      if (_resolvedMessageHandlers.ContainsKey(typeof(T)))
        foreach (var handler in _resolvedMessageHandlers[typeof(T)])
          handler.Handle(message);
    }

    public static void Dispatch<T>(T message) where T : IUIMessage
    {
      MessageDispatcher.Current.DispatchMessage(message);
    }
  }
}
