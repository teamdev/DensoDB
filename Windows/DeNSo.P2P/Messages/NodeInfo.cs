using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using DeNSo;

namespace DeNSo.P2P.Messages
{
  [MessageContract]
  public class NodeInfo
  {
    [MessageBodyMember]
    public Guid Identity { get; set; }

    [MessageBodyMember]
    public string FriendlyName { get; set; }

    [PeerHopCount]
    public int HopCount { get; internal set; }

    public static NodeInfo Current
    { get; set; }

    static NodeInfo()
    {
      Current = new NodeInfo()
      {
        Identity = Configuration.NodeIdentity,
        HopCount = Configuration.Extensions.P2P().MaxHop
      };
    }
  }
}
