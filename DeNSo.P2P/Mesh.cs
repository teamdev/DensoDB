using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using DeNSo.P2P.Filters;

namespace DeNSo.P2P
{
  internal static class Mesh
  {
    private static Uri GetURI()
    {
      return new Uri(DeNSo.Core.Configuration.PeerService);
    }

    public static TChannel OpenPeerChannel<TService, TChannel>(TService sourceObject) where TChannel : IClientChannel
    {
      InstanceContext sourceContext = new InstanceContext(sourceObject);
      var portnumber = DeNSo.Core.Configuration.PeerNetworkPort;
      NetPeerTcpBinding binding = new NetPeerTcpBinding()
      {
        Port = portnumber,
        Name = GetURI().OriginalString + "@" + portnumber, 
      };

      binding.Security.Mode = SecurityMode.None;

      EndpointAddress address = new EndpointAddress(GetURI().OriginalString);
      DuplexChannelFactory<TChannel> sourceFactory = new DuplexChannelFactory<TChannel>(sourceContext, binding, address);
      sourceFactory.Credentials.Peer.MeshPassword = DeNSo.Core.Configuration.PeerNetworkPassword;

      TChannel sourceProxy = (TChannel)sourceFactory.CreateChannel();

      MessagePropagationFilter remoteOnlyFilter = new MessagePropagationFilter();

      PeerNode peerNode = ((IClientChannel)sourceProxy).GetProperty<PeerNode>();
      peerNode.MessagePropagationFilter = remoteOnlyFilter;

      sourceProxy.Open();
      return sourceProxy;
    }
  }
}
