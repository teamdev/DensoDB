using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using DeNSo.Core.CQRS;
using System.Runtime.Serialization.Json;

namespace DeNSo
{
  [ServiceContract]
  public class restservice 
  {
    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate="{database}")]
    public IEnumerable<string> CollectionList(string database)
    {
      return new Query().Collections(database);
    }

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "{database}/{collection}/count")]
    public IEnumerable<string> CollectionCount(string database, string collection)
    {
      return null;
    }

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "{database}/{collection}/count/{filter}")]
    public IEnumerable<string> CollectionCount(string database, string collection, string filter)
    {
      return null;
    }

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "{database}/{collection}/{id}")]
    public IEnumerable<string> GetItem(string database, string collection, int id)
    {
      var jw = JsonReaderWriterFactory.CreateJsonWriter(null);
 
      return null;
    }


    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "{database}/{collection}")]
    public IEnumerable<string> Get(string database, string collection)
    {
      return null;
    }
  }
}
