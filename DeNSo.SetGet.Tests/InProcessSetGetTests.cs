using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeNSo.Core;

namespace DeNSo.SetGet.Tests
{
  [TestClass]
  public class InProcessSetGetTests
  {
    #region Init and Clean Test Environment

    [TestInitialize]
    public void TestInit()
    {
      Configuration.BasePath = @"c:\DensoUnitTests";
      Session.DefaultDataBase = "UnitTests";
      var denso = Session.New;
      denso.WaitForNonStaleDataAt(denso.Flush<TestDataModel>());
    }

    [TestCleanup]
    public void TestClean()
    {
      // Clean DB
      var denso = Session.New;

      denso.WaitForNonStaleDataAt(denso.Flush<TestDataModel>());
      Session.ShutDown();
    }
    #endregion

    #region Simple Set Items
    [TestMethod]
    public void SetSingleItemTest()
    {
      var denso = Session.New;

      var item = CreateSingleItem();

      var cn = denso.Set(item);
      denso.WaitForNonStaleDataAt(cn);

      Assert.AreEqual(1, denso.Count<TestDataModel>());
    }

    [TestMethod]
    public void SetMultipleItem()
    {
      var denso = Session.New;

      var item = CreateSingleItem();
      var cn = denso.Set(item);

      item = CreateSingleItem();
      cn = denso.Set(item);

      item = CreateSingleItem();
      cn = denso.Set(item);

      denso.WaitForNonStaleDataAt(cn);
      Assert.AreEqual(3, denso.Count<TestDataModel>());
    }
    #endregion

    #region Database administrative operations
    [TestMethod]
    public void OpenCloseReopenDatabase()
    {
      var denso = Session.New;
      var item = CreateSingleItem();

      var cn = denso.Set(item);
      var cn2 = denso.Flush<TestDataModel>();

      // Close DB
      Session.ShutDown();

      // reopen DB
      Session.Start();

      item = CreateSingleItem();
      cn = denso.Set(item);
      Assert.IsTrue(cn > cn2);
    }
    #endregion

    #region Wait Methods
    [TestMethod]
    public void TestWaitExtensionMethod()
    {
      var denso = Session.New;

      var item = new TestDataModel();

      item.DateValue1 = new DateTime(1975, 02, 13);
      item.IntValue1 = 99;
      item.StringValue1 = "jdasljdlas";

      denso.Set(item).Wait();
      denso.Flush<TestDataModel>().Wait();

      Assert.AreEqual(0, denso.Count<TestDataModel>());

    }
    #endregion

    #region Get Methods

    [TestMethod]
    public void Get1Lambda_MethodCall_Contains()
    {
      var denso = Session.New;
      var item = CreateSingleItem(i => i.StringValue1 = "Prova");

      denso.Set(item).Wait();

      var result = denso.Get<TestDataModel>(m => m.StringValue1.Contains("P") );

      Assert.AreEqual(1, result.Count());
    }

    [TestMethod]
    public void Get2Lambda_MethodCall_Contains()
    {
      var denso = Session.New;
      var item = CreateSingleItem(i => i.StringValue1 = "Prova");

      denso.Set(item).Wait();

      var result = denso.Get<TestDataModel>(m => m.StringValue1.Contains("P") && m.StringValue1.StartsWith("P"));

      Assert.AreEqual(1, result.Count());
    }

    [TestMethod]
    public void Get3Lambda_MethodCall_Contains()
    {
      var denso = Session.New;
      var item = CreateSingleItem(i => i.StringValue1 = "Prova");

      denso.Set(item).Wait();

      var result = denso.Get<TestDataModel>(m => m.StringValue1.Contains("P"));

      Assert.AreEqual(1, result.Count());
    }

    [TestMethod]
    public void Get4Lambda_MethodCall_Contains()
    {
      var denso = Session.New;
      var item = CreateSingleItem(i => i.StringValue1 = "Prova");

      denso.Set(item).Wait();

      var result = denso.Get<TestDataModel>(m => m.StringValue1.Contains("P"));

      Assert.AreEqual(1, result.Count());
    }

    #endregion

    private static TestDataModel CreateSingleItem(Action<TestDataModel> refineaction = null)
    {
      var item = new TestDataModel();

      Random rnd = new Random();
      item.DateValue1 = new DateTime(rnd.Next(30) + 1970, rnd.Next(11) + 1, rnd.Next(27) + 1);
      item.IntValue1 = (int)rnd.Next();

      item.StringValue1 = string.Format("TestItem{0}", rnd.Next(100));

      if (refineaction != null)
        refineaction(item);
      return item;
    }

  }
}
