using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace DeNSo.P2P.Filters
{
  public class MessagePropagationFilter : PeerMessagePropagationFilter
  {
    public MessagePropagationFilter()
    { }

    public override PeerMessagePropagation ShouldMessagePropagate(Message message, PeerMessageOrigination origination)
    {
      PeerMessagePropagation destination = PeerMessagePropagation.LocalAndRemote;

      if (origination == PeerMessageOrigination.Local)
      {
        destination = PeerMessagePropagation.Remote;
      }

      return destination;
    }
  }
}
