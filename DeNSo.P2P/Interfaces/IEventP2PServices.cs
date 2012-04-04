using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using DeNSo.P2P.Messages;

namespace DeNSo.P2P.Interfaces
{
  //[ServiceContract(CallbackContract=typeof(IEventP2PServices))]
  [ServiceContract()]
  public interface IEventP2PServices
  {
    [OperationContract(IsOneWay = true)]
    void NewEventInTheMesh(EventMessage message);
  }
}
