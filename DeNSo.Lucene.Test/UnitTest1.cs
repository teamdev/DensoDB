using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeNSo.Lucene;

namespace DeNSo.Lucene.Test
{
  [TestClass]
  public class UnitTest1
  {

    class Game
    {
      public string Name { get; set; }
      public string Id { get; set; }
      public Game subgame { get; set; }
    }

    [TestMethod]
    public void TestMethod1()
    {
      Indexer indx = Indexer.GetIndexerFor("i");
      indx.SetIndex<Game>("gamesIndexName", g => g.subgame.Name);
    }
  }
}
