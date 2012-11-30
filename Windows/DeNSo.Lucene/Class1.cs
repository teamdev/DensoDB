using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace Basic_Concepts
{
  class Program
  {
    static void Main(string[] args)
    {
      //Setup indexer

      Directory directory = FSDirectory.GetDirectory("LuceneIndex");
      Analyzer analyzer = new StandardAnalyzer();
      IndexWriter writer = new IndexWriter(directory, analyzer);

      IndexReader red = IndexReader.Open(directory);
      int totDocs = red.MaxDoc();
      red.Close();

      //Add documents to the index
      string text = String.Empty;
      Console.WriteLine("Enter the text you want to add to the index:");
      Console.Write(">");
      int txts = totDocs;
      int j = 0;
      while ((text = Console.ReadLine()) != String.Empty)
      {
        AddTextToIndex(txts++, text, writer);
        j++;
        Console.Write(">");
      }

      writer.Optimize();
      //Close the writer
      writer.Flush();
      writer.Close();

      Console.WriteLine(j + " lines added, " + txts + " documents total");

      //Setup searcher
      IndexSearcher searcher = new IndexSearcher(directory);
      QueryParser parser = new QueryParser("postBody", analyzer);

      Console.WriteLine("Enter the search string:");
      Console.Write(">");

      while ((text = Console.ReadLine()) != String.Empty)
      {
        Search(text, searcher, parser);
        Console.Write(">");
      }

      //Clean up everything
      searcher.Close();
      directory.Close();
    }

    private static void Search(string text, IndexSearcher searcher, QueryParser parser)
    {
      //Supply conditions
      Query query = parser.Parse(text);

      //Do the search
      Hits hits = searcher.Search(query);

      //Display results
      Console.WriteLine("Searching for '" + text + "'");
      int results = hits.Length();
      Console.WriteLine("Found {0} results", results);
      for (int i = 0; i < results; i++)
      {
        Document doc = hits.Doc(i);
        float score = hits.Score(i);
        Console.WriteLine("--Result num {0}, score {1}", i + 1, score);
        Console.WriteLine("--ID: {0}", doc.Get("id"));
        Console.WriteLine("--Text found: {0}" + Environment.NewLine, doc.Get("postBody"));
      }
    }

    private static void AddTextToIndex(int txts, string text, IndexWriter writer)
    {
      Document doc = new Document();
      doc.Add(new Field("id", txts.ToString(), Field.Store.YES, Field.Index.UN_TOKENIZED));
      doc.Add(new Field("postBody", text, Field.Store.YES, Field.Index.TOKENIZED));
      writer.AddDocument(doc);
    }
  }
}