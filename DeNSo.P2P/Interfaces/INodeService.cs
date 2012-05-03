using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using DeNSo.P2P.Messages;

namespace DeNSo.P2P.Interfaces
{
  [ServiceContract(CallbackContract = typeof(INodeServices))]
  public interface INodeServices
  {
    #region Requests

    [OperationContract(IsOneWay = true)]
    void RequestNodeInfo();

    [OperationContract(IsOneWay = true)]
    void NotifyNodeInfo(NodeInfo node);

    #endregion

    #region Responses

    [OperationContract(IsOneWay = true)]
    void NodeInfoResponse(NodeInfo node);

    #endregion
  }
}
