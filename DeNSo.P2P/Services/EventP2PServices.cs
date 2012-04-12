using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using DeNSo.P2P.Interfaces;
using DeNSo.P2P.Messages;
using DeNSo.Core;

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
      store.Enqueue(message.Command);
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
  }
}
