using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeNSo.Meta.Tests.Entities;
using DeNSo.Meta.BSon;
using System.Collections;

namespace DeNSo.Meta.Tests
{
  [TestClass]
  public class BSonSerializerTests
  {
    [TestMethod]
    public void SimpleEntityToBSon()
    {
      var ent = new myEntity() { Property1 = 1, Property2 = 56.09, Property3 = "testtest" };
      ent.ToBSon();
    }

    [TestMethod]
    public void SimpleEntityToBSonSerialized()
    {
      var ent = new myEntity() { Property1 = 1, Property2 = 56.09, Property3 = "testtest" };
      var rr = ent.ToBSon().Serialize();
      Assert.AreEqual(true, rr.Length > 0);
    }

    [TestMethod]
    public void SimpleEntityGenericArrayToBSonSerialized()
    {
      var ent = new myEntity() { Property1 = 1, Property2 = 56.09, Property3 = "testtest" };
      var list = new List<myEntity>();
      list.Add(ent);
      var rr = list.ToBSon().Serialize();
      Assert.AreEqual(true, rr.Length > 0);
    }

    [TestMethod]
    public void SimpleEntityOjectArrayToBSonSerialized()
    {
      var ent = new myEntity() { Property1 = 1, Property2 = 56.09, Property3 = "testtest" };
      var list = new ArrayList();
      list.Add(ent);
      var rr = list.ToBSon().Serialize();
      Assert.AreEqual(true, rr.Length > 0);
    }
  }
}
