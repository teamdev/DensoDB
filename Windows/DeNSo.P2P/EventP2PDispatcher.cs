using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.P2P.Services;
using DeNSo.P2P.Interfaces;
using DeNSo.P2P.Messages;
using System.Net.PeerToPeer;

namespace DeNSo.P2P
{
  /// <summary>
  /// Dispatch database messages to the mesh. 
  /// </summary>
  public static class EventP2PDispatcher
  {
    private static NodeService _node = null;
    private static EventP2PServices _eventservice = null;
    private static IEventP2PServiceChannel _eventchannel = null;
    private static INodeServiceChannel _nodechannel = null;

    /// <summary>
    /// Enable the P2P channel and register P2P dispatcher handler in event store. 
    /// </summary>
    public static void EnableP2PEventMesh()
    {
      if (_eventchannel == null)
      {
        _eventservice = new EventP2PServices();
        _eventchannel = Mesh.OpenDuplexPeerChannel<IEventP2PServices, IEventP2PServiceChannel>(_eventservice);
      }

      RegisterP2PDispatcherInEventStore();
    }

    public static void MakeNodeAvaiableToPNRP(Cloud cloud = null)
    {
      Mesh.RegisterNodeInPNRP(cloud);
    }

    public static void RemoveNodeFromPNRP()
    {
      Mesh.DeregisterNodeInPNRP();
    }

    /// <summary>
    /// register p2p event dispatcher. 
    /// </summary>
    private static void RegisterP2PDispatcherInEventStore()
    {
      DeNSo.Core.EventHandlerManager.RegisterGlobalEventHandler(
        (store, doc) =>
        {
          if (doc.HasMarker(P2PConfiguration.NoRedispatch))
            return;

          Dispatch(new Messages.EventMessage()
          {
            NodeIdentity = DeNSo.Core.Configuration.NodeIdentity,
            Database = store.DataBaseName,
            Command = doc.Command,
            CommandSN = doc.CommandSN, 
            MaxHop = DeNSo.Core.Configuration.Extensions.P2P().MaxHop
          });
        });
    }

    // dispatch a message in the mesh. 
    private static void Dispatch(EventMessage message)
    {
      _eventchannel.GlobalEvent(message);
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
