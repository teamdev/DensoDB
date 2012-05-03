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
  [ServiceContract(CallbackContract = typeof(IEventP2PServices))]
  public interface IEventP2PServices
  {
    /// <summary>
    /// Put a new event in the mesh. 
    /// The event will be dispatched to ALL nodes in the mesh and every node can choose to use the message or discard it. 
    /// </summary>
    /// <param name="message">The message object that will be dispatched</param>

    [OperationContract(IsOneWay = true)]
    void GlobalEvent(EventMessage message);

    /// <summary>
    /// Communicate to nodes the collections i'm interested in to receive message notification if something in that 
    /// collection changes. If something changes, i'll receive directly a message notification.
    /// Every server should maintain a non persistent list of notifications request and 
    /// have to notify every changes directly to the requesters. 
    /// </summary>
    /// <param name="database"></param>
    /// <param name="collection"></param>
    [OperationContract(IsOneWay = true)]
    void InterestedInCollection(string database, string collection);

    [OperationContract(IsOneWay = true)]
    void InterestedInMessageType(string database, string messagetype);

    [OperationContract(IsOneWay = true)]
    void InterestedInMessageContent(string database, string filter);
  }
}
