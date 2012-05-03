using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using DeNSo.P2P.Interfaces;
using DeNSo.P2P.Messages;

namespace DeNSo.P2P.Services
{
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
  internal class NodeService : INodeServices
  {
    #region private fields

    private static Dictionary<Guid, NodeInfo> _nodes = new Dictionary<Guid, NodeInfo>();
    private static Dictionary<Guid, INodeServices> _channels = new Dictionary<Guid, INodeServices>();
    
    #endregion

    #region public properties

    public static Dictionary<Guid, INodeServices> NodeChannels { get { return _channels; } }
    public static Dictionary<Guid, NodeInfo> Nodes { get { return _nodes; } }

    #endregion

    protected INodeServices CallbackChannel
    {
      get
      {
        return OperationContext.Current.GetCallbackChannel<INodeServices>();
      }
    }

    public void NotifyNodeInfo(NodeInfo node)
    {
      RegisterNodeInfo(node);
    }

    public void RequestNodeInfo()
    {
      CallbackChannel.NodeInfoResponse(NodeInfo.Current);
    }
    
    public void NodeInfoResponse(NodeInfo node)
    {
      RegisterNodeInfo(node);
    }

    private void RegisterNodeInfo(NodeInfo node)
    {
      if (!Nodes.ContainsKey(node.Identity))
      {
        Nodes.Add(node.Identity, node);
        NodeChannels.Add(node.Identity, CallbackChannel);
      }
      else
      {
        Nodes[node.Identity] = node;
        NodeChannels[node.Identity] = CallbackChannel;
      }
    }
  }
}
