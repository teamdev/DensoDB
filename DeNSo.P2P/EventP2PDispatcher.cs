using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.P2P.Services;
using DeNSo.P2P.Interfaces;

namespace DeNSo.P2P
{
  public static class EventP2PDispatcher
  {
    private static EventP2PServices _node = null;
    private static IEventP2PServiceChannel _channel = null;

    public static void EnableP2PEventMesh()
    {
      if (_channel != null && _channel.State == System.ServiceModel.CommunicationState.Opened) return;
      _channel = Mesh.OpenPeerChannel<IEventP2PServices, IEventP2PServiceChannel>(_node = new EventP2PServices());
    }

    private static void RegisterP2PDispatcherInEventStore()
    {
      DeNSo.Core.EventHandlerManager.RegisterGlobalEventHandler(
        (store, doc) =>
        {
          _channel.NewEventInTheMesh(new Messages.EventMessage()
          {
            NodeIdentity = DeNSo.Core.Configuration.NodeIdentity,
            Database = store.DataBaseName,
            Command = doc.Command,
            CommandSN = doc.CommandSN
          });
        });
    }

    public static void StopP2PEventMesh()
    {
      if (_channel != null)
      {
        _channel.Close();
      }
    }
  }
}
