using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using DeNSo.P2P.Interfaces;
using DeNSo.P2P.Messages;
using DeNSo.Core;
using DeNSo.Meta.BSon;
using DeNSo.Core.DiskIO;

namespace DeNSo.P2P.Services
{
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
  public class EventP2PServices : IEventP2PServices
  {
    private static Dictionary<Guid, EventMessage> _nodes = new Dictionary<Guid, EventMessage>();
    private static Dictionary<Guid, IEventP2PServices> _channels = new Dictionary<Guid, IEventP2PServices>();

    public static Dictionary<Guid, IEventP2PServices> NodeChannels { get { return _channels; } }
    public static Dictionary<Guid, EventMessage> Nodes { get { return _nodes; } }

    public void GlobalEvent(EventMessage message)
    {
      var store = StoreManager.GetEventStore(message.Database);

      EventCommand cmd = new EventCommand() { Command = message.Command, CommandMarker = message.CommandMarker };
      cmd.SetMarker(P2PConfiguration.NoRedispatch);

      store.Enqueue(cmd);
    }

    public void InterestedIn(string database, string collection)
    {
      throw new NotImplementedException();
    }

    protected IEventP2PServices CallbackChannel
    {
      get
      {
        return OperationContext.Current.GetCallbackChannel<IEventP2PServices>();
      }
    }


    public void InterestedInCollection(string database, string collection)
    {
      throw new NotImplementedException();
    }

    public void InterestedInMessageType(string database, string messagetype)
    {
      throw new NotImplementedException();
    }

    public void InterestedInMessageContent(string database, string filter)
    {
      throw new NotImplementedException();
    }
  }
}
