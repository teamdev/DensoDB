using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using DeNSo.Core.CQRS;
using DeNSo.Meta.BSon;

namespace DeNSo
{
  [ServiceContract]
  public class restservice
  {
    /// <summary>
    /// Gets all collections in a database
    /// </summary>
    /// <param name="database">database name</param>
    /// <returns>CollectionList</returns>
    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "{database}")]
    public IEnumerable<string> CollectionList(string database)
    {
      return new Query().Collections(database);
    }

    /// <summary>
    /// Get documents count in a collection
    /// </summary>
    /// <param name="database">database name</param>
    /// <param name="collection">collection name</param>
    /// <returns>Count of all documents in a collection</returns>
    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "{database}/{collection}/count")]
    public int CollectionCount(string database, string collection)
    {
      return new Query().Count(database, collection);
    }

    /// <summary>
    /// Get documents count in a collection that are compatible with the specified filter 
    /// </summary>
    /// <param name="database"></param>
    /// <param name="collection"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "{database}/{collection}/count/{filter}")]
    public int CollectionCount(string database, string collection, string filter)
    {
      //TODO: complete count filtered implementation. 
      // depends on filter Linq deserialization
      return new Query().Count(database, collection, null);
    }

    /// <summary>
    /// Get document with given id
    /// </summary>
    /// <param name="database"></param>
    /// <param name="collection"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "{database}/{collection}/{id}")]
    public IEnumerable<string> GetItem(string database, string collection, int id)
    {
      var jw = JsonReaderWriterFactory.CreateJsonWriter(null);

      return null;
    }

    /// <summary>
    /// Get all documents in the collection
    /// </summary>
    /// <param name="database"></param>
    /// <param name="collection"></param>
    /// <returns></returns>
    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "{database}/{collection}")]
    public IEnumerable<byte[]> Get(string database, string collection)
    {
      var docs = new Query().Get(database, collection, null);
      List<byte[]> result = new List<byte[]>();

      foreach (var d in docs)
        result.Add(d.Serialize());

      return result.AsEnumerable();
    }
  }
}
