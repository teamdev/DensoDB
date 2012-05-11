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
      Session.New.Flush<TestDataModel>();
    }

    [TestMethod]
    public void SetSingleItemTest()
    {
      var denso = Session.New;
    }
  }
}
