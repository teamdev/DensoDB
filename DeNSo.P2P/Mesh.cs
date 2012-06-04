using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using DeNSo.P2P.Filters;
using System.Diagnostics;
using System.Net.PeerToPeer;

namespace DeNSo.P2P
{
  internal static class Mesh
  {
    private static volatile PeerNameRegistration _peerNameRegistration;

    private static Uri GetURI()
    {
      return new Uri(DeNSo.Core.Configuration.Extensions.P2P().ServiceUri);
    }

    public static TChannel OpenPeerChannel<TService, TChannel>() where TChannel : IClientChannel
    {
      var portnumber = DeNSo.Core.Configuration.Extensions.P2P().NetworkPort;
      NetPeerTcpBinding binding = new NetPeerTcpBinding()
      {
        Port = portnumber,
        Name = GetURI().OriginalString + "@" + portnumber,
      };

      binding.Security.Mode = SecurityMode.None;

      EndpointAddress address = new EndpointAddress(GetURI().OriginalString);
      ChannelFactory<TChannel> sourceFactory = new ChannelFactory<TChannel>(binding, address);
      //sourceFactory.Credentials.Peer.MeshPassword = DeNSo.Core.Configuration.Extensions.P2P().NetworkPassword;

      TChannel sourceProxy = (TChannel)sourceFactory.CreateChannel();

      MessagePropagationFilter remoteOnlyFilter = new MessagePropagationFilter();

      PeerNode peerNode = ((IClientChannel)sourceProxy).GetProperty<PeerNode>();
      //peerNode.MessagePropagationFilter = remoteOnlyFilter;

      sourceProxy.Open();
      return sourceProxy;
    }

    public static TChannel OpenDuplexPeerChannel<TService, TChannel>(TService sourceObject, Action<NetPeerTcpBinding> bindingconfiguration = null) where TChannel : IClientChannel
    {
      InstanceContext sourceContext = new InstanceContext(sourceObject);
      var portnumber = DeNSo.Core.Configuration.Extensions.P2P().NetworkPort;
      NetPeerTcpBinding binding = new NetPeerTcpBinding()
      {
        Port = portnumber,
        Name = GetURI().OriginalString + "@" + portnumber,
      };

      binding.Security.Mode = SecurityMode.None;
      binding.Resolver.Mode = System.ServiceModel.PeerResolvers.PeerResolverMode.Pnrp;

      if (bindingconfiguration != null)
        bindingconfiguration(binding);

      EndpointAddress address = new EndpointAddress(GetURI().OriginalString);
      DuplexChannelFactory<TChannel> sourceFactory = new DuplexChannelFactory<TChannel>(sourceContext, binding, address);
      //ChannelFactory<TChannel> sourceFactory = new ChannelFactory<TChannel>(sourceContext, binding, address);
      sourceFactory.Credentials.Peer.MeshPassword = DeNSo.Core.Configuration.Extensions.P2P().NetworkPassword;

      TChannel sourceProxy = (TChannel)sourceFactory.CreateChannel();

      MessagePropagationFilter remoteOnlyFilter = new MessagePropagationFilter();

      PeerNode peerNode = ((IClientChannel)sourceProxy).GetProperty<PeerNode>();
      peerNode.MessagePropagationFilter = remoteOnlyFilter;

      sourceProxy.Open();
      Debug.WriteLine(string.Format("PNRP is {0} avaiable", NetPeerTcpBinding.IsPnrpAvailable ? string.Empty : "not"));
      return sourceProxy;
    }

    public static void RegisterNodeInPNRP(Cloud cloud = null)
    {
      PeerName name = new PeerName(DeNSo.Core.Configuration.NodeIdentity.ToString(), PeerNameType.Secured);
      _peerNameRegistration = new PeerNameRegistration(name, DeNSo.Core.Configuration.Extensions.P2P().NetworkPort);
      _peerNameRegistration.Cloud = cloud ?? Cloud.Available;
      _peerNameRegistration.Start();
    }

    public static void DeregisterNodeInPNRP()
    {
      _peerNameRegistration.Stop();
    }
  }
}
