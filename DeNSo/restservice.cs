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
    public int CollectionCount(string database, string collection)
    {
      return new Query().Count(database, collection);
    }

    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "{database}/{collection}/count/{filter}")]
    public int CollectionCount(string database, string collection, string filter)
    {
      //TODO: complete count filtered implementation. 
      // depends on filter Linq deserialization
      return new Query().Count(database, collection, null);
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
