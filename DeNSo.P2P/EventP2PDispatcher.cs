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
    private static NodeService _node = null;
    private static IEventP2PServiceChannel _eventchannel = null;
    private static INodeServiceChannel _nodechannel = null;

    public static void EnableP2PEventMesh()
    {
      if (_eventchannel == null)
        _eventchannel = Mesh.OpenPeerChannel<IEventP2PServices, IEventP2PServiceChannel>();

      if (_nodechannel == null)
      {
        _node = new NodeService();
        _nodechannel = Mesh.OpenDuplexPeerChannel<INodeServices, INodeServiceChannel>(_node);
      }
    }

    private static void RegisterP2PDispatcherInEventStore()
    {
      DeNSo.Core.EventHandlerManager.RegisterGlobalEventHandler(
        (store, doc) =>
        {
          _eventchannel.GlobalEvent(new Messages.EventMessage()
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
      if (_eventchannel != null)
      {
        _eventchannel.Close();
      }
    }
  }
}
