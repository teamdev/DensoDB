using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using DeNSo.Core;
using DeNSo.Core.DiskIO;
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
      EventHandlerManager.RegisterGlobalEventHandler(
        (store, eventcommand) =>
        {
          eventcommand.Command.ToBSon();
        });
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
      string path = io.Path.Combine(Configuration.IndexBasePath, database, collection, "L-Indexes", indexname);
      var dirinfo = new io.DirectoryInfo(path);
      if (!dirinfo.Exists) dirinfo.Create();
      var dir = FSDirectory.Open(dirinfo);
      var idx = new IndexWriter(dir, new StandardAnalyzer(global::Lucene.Net.Util.Version.LUCENE_29), false, IndexWriter.MaxFieldLength.UNLIMITED);
      return idx;
    }

    private void CalcIndexFor(EventCommand eventcommand)
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
          break;

        // I have to flush the index.
        case "flush":
          FlushIndexes(collection);
          break;

        case "":
        case null:
          return;

        default:

          break;
      }
    }

    private void FlushIndexes(string collection)
    {
      if (_indexes.ContainsKey(_databasename) && _indexes[_databasename].ContainsKey(collection))
        foreach (var index in _indexes[_databasename][collection])
          lock (index.Value)
            using (var iw = GetIndexWriter(_databasename, collection, index.Key))
            {
              iw.DeleteAll();
              iw.Commit();
            }
    }

    private luc.Query CreateDocumentIdFixedQuery(string id)
    {
      var q = new luc.BooleanQuery();
      q.Add(new luc.TermQuery(new Term(CommandKeyword.Id, id)), luc.BooleanClause.Occur.MUST);
      return q;
    }

    private void RemoveItemFromIndexViaId(string collection, BSonDoc commanddoc)
    {
      if (_indexes.ContainsKey(_databasename) && _indexes[_databasename].ContainsKey(collection))
        foreach (var index in _indexes[_databasename][collection])
          lock (index.Value)
            using (var iw = GetIndexWriter(_databasename, collection, index.Key))
            {
              iw.DeleteDocuments(CreateDocumentIdFixedQuery(commanddoc[CommandKeyword.Id].ToString()));
              iw.Commit();
            }
    }

    private void RemoveItemFromIndexViaFilter(string collection, BSonDoc commanddoc)
    {
      if (_indexes.ContainsKey(_databasename) && _indexes[_databasename].ContainsKey(collection))
        foreach (var index in _indexes[_databasename][collection])
        {
        }
    }
  }
}
