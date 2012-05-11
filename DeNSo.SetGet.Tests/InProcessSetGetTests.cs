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
    [TestInitialize]
    public void TestInit()
    {
      Configuration.BasePath = @"E:\DensoUnitTests";
      Session.DefaultDataBase = "UnitTests";
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
  }
}
