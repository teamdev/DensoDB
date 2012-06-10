using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using DeNSo.Core;
using DeNSo.Core.DiskIO;
using DeNSo.Meta;
using DeNSo.Meta.BSon;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;

using io = System.IO;
using luc = Lucene.Net.Search;


namespace DeNSo.Lucene
{
  public class Indexer
  {
    private string _databasename = string.Empty;
    private static Dictionary<string, Dictionary<string, Dictionary<string, Expression>>> _indexes =
                    new Dictionary<string, Dictionary<string, Dictionary<string, Expression>>>();

    public static Indexer GetIndexerFor(string database)
    {
      return new Indexer(database);
    }

    static Indexer()
    {
      EventHandlerManager.RegisterGlobalEventHandler(new Action<IStore, EventCommand>(CalcIndexFor));
    }

    public void SetIndex<T>(string indexname, Expression<Func<T, dynamic>> selector)
    {
      lock (_indexes)
        if (!_indexes.ContainsKey(_databasename))
          _indexes.Add(_databasename, new Dictionary<string, Dictionary<string, Expression>>());

      var g2bsonconv = new DeNSo.Meta.Generic2BsonLambdaConverter();
      var bsonselector = g2bsonconv.Visit(selector);

      var typename = typeof(T).Name;

      lock (_indexes[_databasename])
        if (!_indexes[_databasename].ContainsKey(typename))
          _indexes[_databasename].Add(typename, new Dictionary<string, Expression>());

      lock (_indexes[_databasename][typename])
        if (!_indexes[_databasename][typename].ContainsKey(indexname))
          _indexes[_databasename][typename].Add(indexname, bsonselector);
        else
          _indexes[_databasename][typename][indexname] = bsonselector;
    }

    public void SetIndex<T, TResult>(Func<T, TResult> selector)
    {
      var t = typeof(TResult);
      var props = t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

      List<string> result = new List<string>();
      foreach (var p in props)
        result.Add(p.Name);
    }

    private Indexer(string databasename)
    {
      _databasename = databasename;
    }

    private static IndexWriter GetIndexWriter(string database, string collection, string indexname)
    {
      lock (_indexes[database][collection][indexname])
      {
        string path = io.Path.Combine(Configuration.IndexBasePath, database, collection, "L-Indexes", indexname);
        var dirinfo = new io.DirectoryInfo(path);
        if (!dirinfo.Exists) dirinfo.Create();
        var dir = FSDirectory.Open(dirinfo);

        // maybe somethimes the index can remain locked from previous operations 
        // and if the process id killed without a soft shutdown. 
        if (IndexWriter.IsLocked(dir)) IndexWriter.Unlock(dir);

        var idx = new IndexWriter(dir, new StandardAnalyzer(global::Lucene.Net.Util.Version.LUCENE_29), false, IndexWriter.MaxFieldLength.UNLIMITED);

        return idx;
      }
    }

    private static void CalcIndexFor(IStore store, EventCommand eventcommand)
    {
      var command = eventcommand.Command.ToBSon();
      var collection = string.Empty;
      var action = string.Empty;

      if (command.HasProperty(CommandKeyword.Collection))
        collection = (command[CommandKeyword.Collection] ?? string.Empty).ToString();

      if (command.HasProperty(CommandKeyword.Action))
        action = (command[CommandKeyword.Action] ?? string.Empty).ToString();

      switch (action.ToLower())
      {
        // I have to remove the document from the index
        case "delete":
          RemoveFromIndex(store, collection, command);
          break;

        // I have to flush the index.
        case "flush":
          FlushIndexes(store, collection);
          break;

        case "":
        case null:
          return;

        default:
          AddToIndexes(store, collection, command);
          break;
      }
    }

    private static void FlushIndexes(IStore store, string collection)
    {
      if (_indexes.ContainsKey(store.DataBaseName) && _indexes[store.DataBaseName].ContainsKey(collection))
        foreach (var index in _indexes[store.DataBaseName][collection])
          lock (index.Value)
            using (var iw = GetIndexWriter(store.DataBaseName, collection, index.Key))
            {
              iw.DeleteAll();
              iw.Commit();
            }
    }

    private static luc.Query CreateDocumentIdFixedQuery(string id)
    {
      var q = new luc.BooleanQuery();
      q.Add(new luc.TermQuery(new Term(CommandKeyword.Id, id)), luc.BooleanClause.Occur.MUST);
      return q;
    }

    private static void AddToIndexes(IStore store, string collection, BSonDoc commanddoc)
    {
      if (commanddoc.HasProperty(CommandKeyword.Id))
      {
        AddDocumentToIndexViaId(store, collection, commanddoc);
        return;
      }

      if (commanddoc.HasProperty(CommandKeyword.Filter))
        return;
    }

    private static void AddDocumentToIndexViaId(IStore store, string collection, BSonDoc commanddoc)
    {
      if (_indexes.ContainsKey(store.DataBaseName) && _indexes[store.DataBaseName].ContainsKey(collection))
      {
        var queryid = CreateDocumentIdFixedQuery(commanddoc[CommandKeyword.Id].ToString());
        foreach (var index in _indexes[store.DataBaseName][collection])
          lock (index.Value)
            using (var iw = GetIndexWriter(store.DataBaseName, collection, index.Key))
            {
              iw.DeleteDocuments(queryid);
              iw.Commit();
            }
      }
    }

    private static void AddDocumentToIndexViaFilter(IStore store, string collection, BSonDoc commanddoc)
    {

    }

    private static void RemoveFromIndex(IStore store, string collection, BSonDoc commanddoc)
    {
      if (commanddoc.HasProperty(CommandKeyword.Id))
      {
        RemoveItemFromIndexViaId(store, collection, commanddoc);
        return;
      }

      if (commanddoc.HasProperty(CommandKeyword.Filter))
        RemoveItemsFromIndexViaFilter(store, collection, commanddoc);
    }

    private static void RemoveItemFromIndexViaId(IStore store, string collection, BSonDoc commanddoc)
    {
      if (_indexes.ContainsKey(store.DataBaseName) && _indexes[store.DataBaseName].ContainsKey(collection))
      {
        var queryid = CreateDocumentIdFixedQuery(commanddoc[CommandKeyword.Id].ToString());
        foreach (var index in _indexes[store.DataBaseName][collection])
          lock (index.Value)
            using (var iw = GetIndexWriter(store.DataBaseName, collection, index.Key))
            {
              iw.DeleteDocuments(queryid);
              iw.Commit();
            }
      }
    }

    private static void RemoveItemsFromIndexViaFilter(IStore store, string collection, BSonDoc commanddoc)
    {
      if (_indexes.ContainsKey(store.DataBaseName) && _indexes[store.DataBaseName].ContainsKey(collection))
        foreach (var index in _indexes[store.DataBaseName][collection])
        {
          //TODO: write index removal logic via _filter command.
        }
    }
  }
}
