using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Core;
using DeNSo.Meta.BSon;

using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Analysis;
using io = System.IO;
using System.Linq.Expressions;
using DeNSo.Core.DiskIO;

namespace DensoDB.Lucene
{
  public class Indexer
  {
    private string _databasename = string.Empty;
    private static Dictionary<string, Dictionary<string, Expression>> _indexes = new Dictionary<string, Dictionary<string, Expression>>();

    public static Indexer GetIndexerFor(string database)
    {
      return new Indexer(database);
    }

    static Indexer()
    {

      //Directory directory = FSDirectory.GetDirectory(io.Path.Combine(Configuration. "LuceneIndex");
      //Analyzer analyzer = new StandardAnalyzer();
      //IndexWriter writer = new IndexWriter(directory, analyzer);

      EventHandlerManager.RegisterGlobalEventHandler(
        (store, eventcommand) =>
        {
          eventcommand.Command.ToBSon();
        });
    }

    public void SetIndex<T>(Expression<Func<T, dynamic>> selector)
    {
      lock (_indexes)
        if (!_indexes.ContainsKey(_databasename))
          _indexes.Add(_databasename, new Dictionary<string, Expression>());

      lock (_indexes[_databasename])
        if (!_indexes[_databasename].ContainsKey(typeof(T).Name))
          _indexes[_databasename].Add(typeof(T).Name, selector);
        else
          _indexes[_databasename][typeof(T).Name] = selector;
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

    private static Directory GetIndexDirectory(string database, string collection)
    {
      string path = io.Path.Combine(Configuration.IndexBasePath, database, collection, "L-Index");
      Directory dir = FSDirectory.GetDirectory(path, true);
      return dir;
    }

    private void CalcIndexFor(EventCommand eventcommand)
    {
      var command = eventcommand.Command.ToBSon();
    }
  }
}
