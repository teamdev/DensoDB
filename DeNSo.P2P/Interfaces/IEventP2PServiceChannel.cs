using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace DeNSo.P2P.Interfaces
{
  public interface IEventP2PServiceChannel : IEventP2PServices, IClientChannel
  {
  }
}
