﻿using System;
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

    [TestMethod]
    public void SetSingleItemTest()
    {
      var denso = Session.New;

      var item = new TestDataModel();

      item.DateValue1 = new DateTime(1975, 02, 13);
      item.IntValue1 = 99;
      item.StringValue1 = "jdasljdlas";

      var cn = denso.Set(item);
      denso.WaitForNonStaleDataAt(cn);

      Assert.AreEqual(1, denso.Count<TestDataModel>());
    }

    [TestMethod]
    public void SetMultipleItem()
    {
      var denso = Session.New;

      var item = new TestDataModel();

      item.DateValue1 = new DateTime(1975, 02, 13);
      item.IntValue1 = 99;
      item.StringValue1 = "jdasljdlas";

      var cn = denso.Set(item);

      item = new TestDataModel();
      item.DateValue1 = new DateTime(1975, 02, 23);
      item.IntValue1 = 96;
      item.StringValue1 = "sasd";

      cn = denso.Set(item);

      item = new TestDataModel();
      item.DateValue1 = new DateTime(1975, 02, 25);
      item.IntValue1 = 90;
      item.StringValue1 = "sasddasdas";

      cn = denso.Set(item);

      denso.WaitForNonStaleDataAt(cn);
      Assert.AreEqual(3, denso.Count<TestDataModel>());
    }

    [TestMethod]
    public void OpenCloseReopenDatabase()
    {
      //
      var denso = Session.New;

      var item = new TestDataModel();

      item.DateValue1 = new DateTime(1975, 02, 13);
      item.IntValue1 = 99;
      item.StringValue1 = "jdasljdlas";

      var cn = denso.Set(item);
      var cn2 = denso.Flush<TestDataModel>();

      // Close DB
      Session.ShutDown();

      // reopen DB
      Session.Start();

      item = new TestDataModel();

      item.DateValue1 = new DateTime(1975, 02, 13);
      item.IntValue1 = 99;
      item.StringValue1 = "jdasljdlas";

      cn = denso.Set(item);
      Assert.IsTrue(cn > cn2);
    }

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
  }
}
