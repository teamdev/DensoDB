using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Client.Linqtests
{
  [TestClass]
  public class Linq2JsonTests
  {
    [ClassInitialize]
    public static void Initialize(TestContext context)
    {

    }

    [ClassCleanup]
    public static void Cleanup()
    {

    }

    [TestMethod]
    public void TestCompiledQuery()
    {
      var fn = QueryCompiler.Compile((string id) => db.Customers.Where(c => c.CustomerID == id));
      var items = fn("ALKFI").ToList();
    }

    [TestMethod]
    public void TestCompiledQuerySingleton()
    {
      var fn = QueryCompiler.Compile((string id) => db.Customers.SingleOrDefault(c => c.CustomerID == id));
      Customer cust = fn("ALKFI");
    }

    [TestMethod]
    public void TestCompiledQueryCount()
    {
      var fn = QueryCompiler.Compile((string id) => db.Customers.Count(c => c.CustomerID == id));
      int n = fn("ALKFI");
    }

    [TestMethod]
    public void TestCompiledQueryIsolated()
    {
      var fn = QueryCompiler.Compile((Northwind n, string id) => n.Customers.Where(c => c.CustomerID == id));
      var items = fn(db, "ALFKI").ToList();
    }

    [TestMethod]
    public void TestCompiledQueryIsolatedWithHeirarchy()
    {
      var fn = QueryCompiler.Compile((Northwind n, string id) => n.Customers.Where(c => c.CustomerID == id).Select(c => n.Orders.Where(o => o.CustomerID == c.CustomerID)));
      var items = fn(db, "ALFKI").ToList();
    }

    [TestMethod]
    public void TestCount()
    {
      var count = db.Customers.Count();
      AssertValue(91, count);
    }

    [TestMethod]
    public void TestCountPredicate()
    {
      var count = db.Customers.Count(c => c.City == "London");
      AssertValue(6, count);
    }

    [TestMethod]
    public void TestSessionCount()
    {
      var nw = new NorthwindSession(_provider);
      var count = nw.Customers.Count();
      AssertValue(91, count);
    }

    [TestMethod]
    public void TestSessionCountPredicate()
    {
      var nw = new NorthwindSession(_provider);
      var count = nw.Customers.Count(c => c.City == "London");
      AssertValue(6, count);
    }

    [TestMethod]
    public void TestWhere()
    {
      var list = db.Customers.Where(c => c.City == "London").ToList();
      AssertValue(6, list.Count);
    }

    [TestMethod]
    public void TestWhereTrue()
    {
      var list = db.Customers.Where(c => true).ToList();
      AssertValue(91, list.Count);
    }

    [TestMethod]
    public void TestCompareEntityEqual()
    {
      Customer alfki = new Customer { CustomerID = "ALFKI" };
      var list = db.Customers.Where(c => c == alfki).ToList();
      AssertValue(1, list.Count);
      AssertValue("ALFKI", list[0].CustomerID);
    }

    [TestMethod]
    public void TestCompareEntityNotEqual()
    {
      Customer alfki = new Customer { CustomerID = "ALFKI" };
      var list = db.Customers.Where(c => c != alfki).ToList();
      AssertValue(90, list.Count);
    }

    [TestMethod]
    public void TestCompareConstructedEqual()
    {
      var list = db.Customers.Where(c => new { x = c.City } == new { x = "London" }).ToList();
      AssertValue(6, list.Count);
    }

    [TestMethod]
    public void TestCompareConstructedMultiValueEqual()
    {
      var list = db.Customers.Where(c => new { x = c.City, y = c.Country } == new { x = "London", y = "UK" }).ToList();
      AssertValue(6, list.Count);
    }

    [TestMethod]
    public void TestCompareConstructedMultiValueNotEqual()
    {
      var list = db.Customers.Where(c => new { x = c.City, y = c.Country } != new { x = "London", y = "UK" }).ToList();
      AssertValue(85, list.Count);
    }

    [TestMethod]
    public void TestCompareEntityEntityEqualRelationship()
    {
      Customer alfki = new Customer { CustomerID = "ALFKI" };
      var testQuery = from o in db.Orders
                      from c in db.Customers
                      where o.Customer == c &&
                            c.CustomerID == alfki.CustomerID
                      select o;

      var test = testQuery.ToList();

      AssertValue(6, test.Count);
      AssertValue("ALFKI", test[0].CustomerID);
    }

    [TestMethod]
    public void TestCompareEntityConstantEqualRelationship()
    {
      Customer alfki = new Customer { CustomerID = "ALFKI" };
      var testQuery = from o in db.Orders
                      where o.Customer == alfki
                      select o;

      var test = testQuery.ToList();

      AssertValue(6, test.Count);
      AssertValue("ALFKI", test[0].CustomerID);
    }

    [TestMethod]
    public void TestCompareEntityConstantDirectEqualRelationship()
    {
      Customer alfki = new Customer { CustomerID = "ALFKI" };
      var testQuery = from c in db.Customers
                      where c == alfki
                      select c;

      var test = testQuery.ToList();

      AssertValue(1, test.Count);
      AssertValue("ALFKI", test[0].CustomerID);
    }

    [TestMethod]
    public void TestCompareConstantEntityNestedRelationshipNegation()
    {
      var exclude = new Order() { OrderID = 10702 };

      var testQuery = from c in db.Customers
                      from o in c.Orders
                      from d in o.Details
                      where o != exclude
                      select d;

      var test = testQuery.ToList();

      AssertValue(2153, test.Count);
    }

    [TestMethod]
    public void TestCompareTwoConstantEntityNestedRelationshipNegation()
    {
      Order exclude = new Order() { OrderID = 10702 };
      Customer alfki = new Customer() { CustomerID = "ALFKI" };

      var testQuery = from o in db.Orders
                      where o.Customer == alfki &&
                            o != exclude
                      select o;

      var test = testQuery.ToList();
      AssertValue(5, test.Count);
      AssertValue("ALFKI", test[0].CustomerID);
    }

    [TestMethod]
    public void TestSelectScalar()
    {
      var list = db.Customers.Where(c => c.City == "London").Select(c => c.City).ToList();
      AssertValue(6, list.Count);
      AssertValue("London", list[0]);
      AssertTrue(list.All(x => x == "London"));
    }

    [TestMethod]
    public void TestSelectAnonymousOne()
    {
      var list = db.Customers.Where(c => c.City == "London").Select(c => new { c.City }).ToList();
      AssertValue(6, list.Count);
      AssertValue("London", list[0].City);
      AssertTrue(list.All(x => x.City == "London"));
    }

    [TestMethod]
    public void TestSelectAnonymousTwo()
    {
      var list = db.Customers.Where(c => c.City == "London").Select(c => new { c.City, c.Phone }).ToList();
      AssertValue(6, list.Count);
      AssertValue("London", list[0].City);
      AssertTrue(list.All(x => x.City == "London"));
      AssertTrue(list.All(x => x.Phone != null));
    }

    [TestMethod]
    public void TestSelectCustomerTable()
    {
      var list = db.Customers.ToList();
      AssertValue(91, list.Count);
    }

    [TestMethod]
    public void TestSelectAnonymousWithObject()
    {
      var list = db.Customers.Where(c => c.City == "London").Select(c => new { c.City, c }).ToList();
      AssertValue(6, list.Count);
      AssertValue("London", list[0].City);
      AssertTrue(list.All(x => x.City == "London"));
      AssertTrue(list.All(x => x.c.City == x.City));
    }

    [TestMethod]
    public void TestSelectAnonymousLiteral()
    {
      var list = db.Customers.Where(c => c.City == "London").Select(c => new { X = 10 }).ToList();
      AssertValue(6, list.Count);
      AssertTrue(list.All(x => x.X == 10));
    }

    [TestMethod]
    public void TestSelectConstantInt()
    {
      var list = db.Customers.Select(c => 10).ToList();
      AssertValue(91, list.Count);
      AssertTrue(list.All(x => x == 10));
    }

    [TestMethod]
    public void TestSelectConstantNullString()
    {
      var list = db.Customers.Select(c => (string)null).ToList();
      AssertValue(91, list.Count);
      AssertTrue(list.All(x => x == null));
    }

    [TestMethod]
    public void TestSelectLocal()
    {
      int x = 10;
      var list = db.Customers.Select(c => x).ToList();
      AssertValue(91, list.Count);
      AssertTrue(list.All(y => y == 10));
    }

    [TestMethod]
    public void TestSelectNestedCollection()
    {
      var list = (
          from c in db.Customers
          where c.CustomerID == "ALFKI"
          select db.Orders.Where(o => o.CustomerID == c.CustomerID).Select(o => o.OrderID)
          ).ToList();
      AssertValue(1, list.Count);
      AssertValue(6, list[0].Count());
    }

    [TestMethod]
    public void TestSelectNestedCollectionInAnonymousType()
    {
      var list = (
          from c in db.Customers
          where c.CustomerID == "ALFKI"
          select new { Foos = db.Orders.Where(o => o.CustomerID == c.CustomerID).Select(o => o.OrderID).ToList() }
          ).ToList();
      AssertValue(1, list.Count);
      AssertValue(6, list[0].Foos.Count);
    }

    [TestMethod]
    public void TestJoinCustomerOrders()
    {
      var list = (
          from c in db.Customers
          where c.CustomerID == "ALFKI"
          join o in db.Orders on c.CustomerID equals o.CustomerID
          select new { c.ContactName, o.OrderID }
          ).ToList();
      AssertValue(6, list.Count);
    }

    [TestMethod]
    public void TestJoinMultiKey()
    {
      var list = (
          from c in db.Customers
          where c.CustomerID == "ALFKI"
          join o in db.Orders on new { a = c.CustomerID, b = c.CustomerID } equals new { a = o.CustomerID, b = o.CustomerID }
          select new { c, o }
          ).ToList();
      AssertValue(6, list.Count);
    }

    [TestMethod]
    public void TestJoinIntoCustomersOrdersCount()
    {
      var list = (
          from c in db.Customers
          where c.CustomerID == "ALFKI"
          join o in db.Orders on c.CustomerID equals o.CustomerID into ords
          select new { cust = c, ords = ords.Count() }
          ).ToList();
      AssertValue(1, list.Count);
      AssertValue(6, list[0].ords);
    }

    [TestMethod]
    public void TestJoinIntoDefaultIfEmpty()
    {
      var list = (
          from c in db.Customers
          where c.CustomerID == "PARIS"
          join o in db.Orders on c.CustomerID equals o.CustomerID into ords
          from o in ords.DefaultIfEmpty()
          select new { c, o }
          ).ToList();

      AssertValue(1, list.Count);
      AssertValue(null, list[0].o);
    }

    [TestMethod]
    public void TestMultipleJoinsWithJoinConditionsInWhere()
    {
      // this should reduce to inner joins
      var list = (
          from c in db.Customers
          from o in db.Orders
          from d in db.OrderDetails
          where o.CustomerID == c.CustomerID && o.OrderID == d.OrderID
          where c.CustomerID == "ALFKI"
          select d
          ).ToList();

      AssertValue(12, list.Count);
    }

    [TestMethod]
#if MYSQL
        [Ignore]
#endif
    public void TestMultipleJoinsWithMissingJoinCondition()
    {
      // this should force a naked cross join
      var list = (
          from c in db.Customers
          from o in db.Orders
          from d in db.OrderDetails
          where o.CustomerID == c.CustomerID /*&& o.OrderID == d.OrderID*/
          where c.CustomerID == "ALFKI"
          select d
          ).ToList();

      AssertValue(12930, list.Count);
    }

    [TestMethod]
    public void TestOrderBy()
    {
      var list = db.Customers.OrderBy(c => c.CustomerID).Select(c => c.CustomerID).ToList();
      var sorted = list.OrderBy(c => c).ToList();
      AssertValue(91, list.Count);
      AssertTrue(Enumerable.SequenceEqual(list, sorted));
    }

    [TestMethod]
    public void TestOrderByOrderBy()
    {
      var list = db.Customers.OrderBy(c => c.Phone).OrderBy(c => c.CustomerID).ToList();
      var sorted = list.OrderBy(c => c.CustomerID).ToList();
      AssertValue(91, list.Count);
      AssertTrue(Enumerable.SequenceEqual(list, sorted));
    }

    [TestMethod]
    public void TestOrderByThenBy()
    {
      var list = db.Customers.OrderBy(c => c.CustomerID).ThenBy(c => c.Phone).ToList();
      var sorted = list.OrderBy(c => c.CustomerID).ThenBy(c => c.Phone).ToList();
      AssertValue(91, list.Count);
      AssertTrue(Enumerable.SequenceEqual(list, sorted));
    }

    [TestMethod]
    public void TestOrderByDescending()
    {
      var list = db.Customers.OrderByDescending(c => c.CustomerID).ToList();
      var sorted = list.OrderByDescending(c => c.CustomerID).ToList();
      AssertValue(91, list.Count);
      AssertTrue(Enumerable.SequenceEqual(list, sorted));
    }

    [TestMethod]
    public void TestOrderByDescendingThenBy()
    {
      var list = db.Customers.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).ToList();
      var sorted = list.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).ToList();
      AssertValue(91, list.Count);
      AssertTrue(Enumerable.SequenceEqual(list, sorted));
    }

    [TestMethod]
    public void TestOrderByDescendingThenByDescending()
    {
      var list = db.Customers.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).ToList();
      var sorted = list.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).ToList();
      AssertValue(91, list.Count);
      AssertTrue(Enumerable.SequenceEqual(list, sorted));
    }

    [TestMethod]
    public void TestOrderByJoin()
    {
      var list = (
          from c in db.Customers.OrderBy(c => c.CustomerID)
          join o in db.Orders.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
          select new { c.CustomerID, o.OrderID }
          ).ToList();

      var sorted = list.OrderBy(x => x.CustomerID).ThenBy(x => x.OrderID);
      AssertTrue(Enumerable.SequenceEqual(list, sorted));
    }

    [TestMethod]
    public void TestOrderBySelectMany()
    {
      var list = (
          from c in db.Customers.OrderBy(c => c.CustomerID)
          from o in db.Orders.OrderBy(o => o.OrderID)
          where c.CustomerID == o.CustomerID
          select new { c.CustomerID, o.OrderID }
          ).ToList();
      var sorted = list.OrderBy(x => x.CustomerID).ThenBy(x => x.OrderID).ToList();
      AssertTrue(Enumerable.SequenceEqual(list, sorted));
    }

    [TestMethod]
    public void TestCountProperty()
    {
      var list = db.Customers.Where(c => c.Orders.Count > 0).ToList();
      AssertValue(89, list.Count);
    }

    [TestMethod]
    public void TestGroupBy()
    {
      var list = db.Customers.GroupBy(c => c.City).ToList();
      AssertValue(69, list.Count);
    }

    [TestMethod]
    public void TestGroupByOne()
    {
      var list = db.Customers.Where(c => c.City == "London").GroupBy(c => c.City).ToList();
      AssertValue(1, list.Count);
      AssertValue(6, list[0].Count());
    }

    [TestMethod]
    public void TestGroupBySelectMany()
    {
      var list = db.Customers.GroupBy(c => c.City).SelectMany(g => g).ToList();
      AssertValue(91, list.Count);
    }

    [TestMethod]
    public void TestGroupBySum()
    {
      var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1))).ToList();
      AssertValue(1, list.Count);
      AssertValue(6, list[0]);
    }

    [TestMethod]
    public void TestGroupByCount()
    {
      var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => g.Count()).ToList();
      AssertValue(1, list.Count);
      AssertValue(6, list[0]);
    }

    [TestMethod]
    public void TestGroupByLongCount()
    {
      var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => g.LongCount()).ToList();
      AssertValue(1, list.Count);
      AssertValue(6L, list[0]);
    }

    [TestMethod]
    public void TestGroupBySumMinMaxAvg()
    {
      var list =
          db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g =>
              new
              {
                Sum = g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1)),
                Min = g.Min(o => o.OrderID),
                Max = g.Max(o => o.OrderID),
                Avg = g.Average(o => o.OrderID)
              }).ToList();
      AssertValue(1, list.Count);
      AssertValue(6, list[0].Sum);
    }

    [TestMethod]
    public void TestGroupByWithResultSelector()
    {
      var list =
          db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, (k, g) =>
              new
              {
                Sum = g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1)),
                Min = g.Min(o => o.OrderID),
                Max = g.Max(o => o.OrderID),
                Avg = g.Average(o => o.OrderID)
              }).ToList();
      AssertValue(1, list.Count);
      AssertValue(6, list[0].Sum);
    }

    [TestMethod]
    public void TestGroupByWithElementSelectorSum()
    {
      var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => (o.CustomerID == "ALFKI" ? 1 : 1)).Select(g => g.Sum()).ToList();
      AssertValue(1, list.Count);
      AssertValue(6, list[0]);
    }

    [TestMethod]
    public void TestGroupByWithElementSelector()
    {
      // note: groups are retrieved through a separately execute subquery per row
      var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => (o.CustomerID == "ALFKI" ? 1 : 1)).ToList();
      AssertValue(1, list.Count);
      AssertValue(6, list[0].Count());
      AssertValue(6, list[0].Sum());
    }

    [TestMethod]
    public void TestGroupByWithElementSelectorSumMax()
    {
      var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => (o.CustomerID == "ALFKI" ? 1 : 1)).Select(g => new { Sum = g.Sum(), Max = g.Max() }).ToList();
      AssertValue(1, list.Count);
      AssertValue(6, list[0].Sum);
      AssertValue(1, list[0].Max);
    }

    [TestMethod]
    public void TestGroupByWithAnonymousElement()
    {
      var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => new { X = (o.CustomerID == "ALFKI" ? 1 : 1) }).Select(g => g.Sum(x => x.X)).ToList();
      AssertValue(1, list.Count);
      AssertValue(6, list[0]);
    }

    [TestMethod]
    public void TestGroupByWithTwoPartKey()
    {
      var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => new { o.CustomerID, o.OrderDate }).Select(g => g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1))).ToList();
      AssertValue(6, list.Count);
    }

    [TestMethod]
    public void TestGroupByWithCountInWhere()
    {
      var list = db.Customers.Where(a => a.Orders.Count() > 15).GroupBy(a => a.City).ToList();
      AssertValue(9, list.Count);
    }

    [TestMethod]
    public void TestOrderByGroupBy()
    {
      // note: order-by is lost when group-by is applied (the sequence of groups is not ordered)
      var list = db.Orders.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).ToList();
      AssertValue(1, list.Count);
      var grp = list[0].ToList();
      var sorted = grp.OrderBy(o => o.OrderID);
      AssertTrue(Enumerable.SequenceEqual(grp, sorted));
    }

    [TestMethod]
    public void TestOrderByGroupBySelectMany()
    {
      var list = db.Orders.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).SelectMany(g => g).ToList();
      AssertValue(6, list.Count);
      var sorted = list.OrderBy(o => o.OrderID).ToList();
      AssertTrue(Enumerable.SequenceEqual(list, sorted));
    }

    [TestMethod]
    public void TestSumWithNoArg()
    {
      var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Select(o => (o.CustomerID == "ALFKI" ? 1 : 1)).Sum();
      AssertValue(6, sum);
    }

    [TestMethod]
    public void TestSumWithArg()
    {
      var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1));
      AssertValue(6, sum);
    }

    [TestMethod]
    public void TestCountWithNoPredicate()
    {
      var cnt = db.Orders.Count();
      AssertValue(830, cnt);
    }

    [TestMethod]
    public void TestCountWithPredicate()
    {
      var cnt = db.Orders.Count(o => o.CustomerID == "ALFKI");
      AssertValue(6, cnt);
    }

    [TestMethod]
    public void TestDistinctNoDupes()
    {
      var list = db.Customers.Distinct().ToList();
      AssertValue(91, list.Count);
    }

    [TestMethod]
    public void TestDistinctScalar()
    {
      var list = db.Customers.Select(c => c.City).Distinct().ToList();
      AssertValue(69, list.Count);
    }

    [TestMethod]
    public void TestOrderByDistinct()
    {
      // these are un-ordered, because using distinct negates any existing ordering.
      var list = db.Customers.Where(c => c.City.StartsWith("P")).OrderBy(c => c.City).Select(c => c.City).Distinct().ToList();
      AssertValue(2, list.Count);
      var sorted = list.OrderBy(x => x).ToList();
      AssertValue(2, list.Count);
      AssertValue(sorted[0], "Paris");
      AssertValue(sorted[1], "Portland");
    }

    [TestMethod]
    public void TestDistinctOrderBy()
    {
      var list = db.Customers.Where(c => c.City.StartsWith("P")).Select(c => c.City).Distinct().OrderBy(c => c).ToList();
      var sorted = list.OrderBy(x => x).ToList();
      AssertValue(list[0], sorted[0]);
      AssertValue(list[list.Count - 1], sorted[list.Count - 1]);
    }

    [TestMethod]
    public void TestDistinctGroupBy()
    {
      var list = db.Orders.Where(o => o.CustomerID == "ALFKI").Distinct().GroupBy(o => o.CustomerID).ToList();
      AssertValue(1, list.Count);
    }

    [TestMethod]
    public void TestGroupByDistinct()
    {
      // distinct after group-by should not do anything
      var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Distinct().ToList();
      AssertValue(1, list.Count);
    }

    [TestMethod]
    public void TestDistinctCount()
    {
      var cnt = db.Customers.Distinct().Count();
      AssertValue(91, cnt);
    }

    [TestMethod]
    public void TestSelectDistinctCount()
    {
      // cannot do: SELECT COUNT(DISTINCT some-colum) FROM some-table
      // because COUNT(DISTINCT some-column) does not count nulls
      var cnt = db.Customers.Select(c => c.City).Distinct().Count();
      AssertValue(69, cnt);
    }

    [TestMethod]
    public void TestSelectSelectDistinctCount()
    {
      var cnt = db.Customers.Select(c => c.City).Select(c => c).Distinct().Count();
      AssertValue(69, cnt);
    }

    [TestMethod]
    public void TestDistinctCountPredicate()
    {
      var cnt = db.Customers.Select(c => new { c.City, c.Country }).Distinct().Count(c => c.City == "London");
      AssertValue(1, cnt);
    }

    [TestMethod]
    public void TestDistinctSumWithArg()
    {
      var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Distinct().Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1));
      AssertValue(6, sum);
    }

    [TestMethod]
    public void TestSelectDistinctSum()
    {
      var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Select(o => o.OrderID).Distinct().Sum();
      AssertValue(64835, sum);
    }

    [TestMethod]
    public void TestTake()
    {
      var list = db.Orders.Take(5).ToList();
      AssertValue(5, list.Count);
    }

    [TestMethod]
    public void TestTakeDistinct()
    {
      // distinct must be forced to apply after top has been computed
      var list = db.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Take(5).Distinct().ToList();
      AssertValue(1, list.Count);
    }

    [TestMethod]
    public void TestDistinctTake()
    {
      // top must be forced to apply after distinct has been computed
      var list = db.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Distinct().Take(5).ToList();
      AssertValue(5, list.Count);
    }

    [TestMethod]
#if ACCESS
        [Ignore]  // ??? this produces a count of 6 ???
#endif
    public void TestDistinctTakeCount()
    {
      var cnt = db.Orders.Distinct().OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Take(5).Count();
      AssertValue(5, cnt);
    }

    [TestMethod]
    public void TestTakeDistinctCount()
    {
      var cnt = db.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Take(5).Distinct().Count();
      AssertValue(1, cnt);
    }

    [TestMethod]
    public void TestFirst()
    {
      var first = db.Customers.OrderBy(c => c.ContactName).First();
      AssertNotValue(null, first);
      AssertValue("ROMEY", first.CustomerID);
    }

    [TestMethod]
    public void TestFirstPredicate()
    {
      var first = db.Customers.OrderBy(c => c.ContactName).First(c => c.City == "London");
      AssertNotValue(null, first);
      AssertValue("EASTC", first.CustomerID);
    }

    [TestMethod]
    public void TestWhereFirst()
    {
      var first = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").First();
      AssertNotValue(null, first);
      AssertValue("EASTC", first.CustomerID);
    }

    [TestMethod]
    public void TestFirstOrDefault()
    {
      var first = db.Customers.OrderBy(c => c.ContactName).FirstOrDefault();
      AssertNotValue(null, first);
      AssertValue("ROMEY", first.CustomerID);
    }

    [TestMethod]
    public void TestFirstOrDefaultPredicate()
    {
      var first = db.Customers.OrderBy(c => c.ContactName).FirstOrDefault(c => c.City == "London");
      AssertNotValue(null, first);
      AssertValue("EASTC", first.CustomerID);
    }

    [TestMethod]
    public void TestWhereFirstOrDefault()
    {
      var first = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").FirstOrDefault();
      AssertNotValue(null, first);
      AssertValue("EASTC", first.CustomerID);
    }

    [TestMethod]
    public void TestFirstOrDefaultPredicateNoMatch()
    {
      var first = db.Customers.OrderBy(c => c.ContactName).FirstOrDefault(c => c.City == "SpongeBob");
      AssertValue(null, first);
    }

    [TestMethod]
    public void TestReverse()
    {
      var list = db.Customers.OrderBy(c => c.ContactName).Reverse().ToList();
      AssertValue(91, list.Count);
      AssertValue("WOLZA", list[0].CustomerID);
      AssertValue("ROMEY", list[90].CustomerID);
    }

    [TestMethod]
    public void TestReverseReverse()
    {
      var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Reverse().ToList();
      AssertValue(91, list.Count);
      AssertValue("ROMEY", list[0].CustomerID);
      AssertValue("WOLZA", list[90].CustomerID);
    }

    [TestMethod]
    public void TestReverseWhereReverse()
    {
      var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Where(c => c.City == "London").Reverse().ToList();
      AssertValue(6, list.Count);
      AssertValue("EASTC", list[0].CustomerID);
      AssertValue("BSBEV", list[5].CustomerID);
    }

    [TestMethod]
    public void TestReverseTakeReverse()
    {
      var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Take(5).Reverse().ToList();
      AssertValue(5, list.Count);
      AssertValue("CHOPS", list[0].CustomerID);
      AssertValue("WOLZA", list[4].CustomerID);
    }

    [TestMethod]
    public void TestReverseWhereTakeReverse()
    {
      var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Where(c => c.City == "London").Take(5).Reverse().ToList();
      AssertValue(5, list.Count);
      AssertValue("CONSH", list[0].CustomerID);
      AssertValue("BSBEV", list[4].CustomerID);
    }

    [TestMethod]
    public void TestLast()
    {
      var last = db.Customers.OrderBy(c => c.ContactName).Last();
      AssertNotValue(null, last);
      AssertValue("WOLZA", last.CustomerID);
    }

    [TestMethod]
    public void TestLastPredicate()
    {
      var last = db.Customers.OrderBy(c => c.ContactName).Last(c => c.City == "London");
      AssertNotValue(null, last);
      AssertValue("BSBEV", last.CustomerID);
    }

    [TestMethod]
    public void TestWhereLast()
    {
      var last = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").Last();
      AssertNotValue(null, last);
      AssertValue("BSBEV", last.CustomerID);
    }

    [TestMethod]
    public void TestLastOrDefault()
    {
      var last = db.Customers.OrderBy(c => c.ContactName).LastOrDefault();
      AssertNotValue(null, last);
      AssertValue("WOLZA", last.CustomerID);
    }

    [TestMethod]
    public void TestLastOrDefaultPredicate()
    {
      var last = db.Customers.OrderBy(c => c.ContactName).LastOrDefault(c => c.City == "London");
      AssertNotValue(null, last);
      AssertValue("BSBEV", last.CustomerID);
    }

    [TestMethod]
    public void TestWhereLastOrDefault()
    {
      var last = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").LastOrDefault();
      AssertNotValue(null, last);
      AssertValue("BSBEV", last.CustomerID);
    }

    [TestMethod]
    public void TestLastOrDefaultNoMatches()
    {
      var last = db.Customers.OrderBy(c => c.ContactName).LastOrDefault(c => c.City == "SpongeBob");
      AssertValue(null, last);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void TestSingleFails()
    {
      var single = db.Customers.Single();
    }

    [TestMethod]
    public void TestSinglePredicate()
    {
      var single = db.Customers.Single(c => c.CustomerID == "ALFKI");
      AssertNotValue(null, single);
      AssertValue("ALFKI", single.CustomerID);
    }

    [TestMethod]
    public void TestWhereSingle()
    {
      var single = db.Customers.Where(c => c.CustomerID == "ALFKI").Single();
      AssertNotValue(null, single);
      AssertValue("ALFKI", single.CustomerID);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void TestSingleOrDefaultFails()
    {
      var single = db.Customers.SingleOrDefault();
    }

    [TestMethod]
    public void TestSingleOrDefaultPredicate()
    {
      var single = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI");
      AssertNotValue(null, single);
      AssertValue("ALFKI", single.CustomerID);
    }

    [TestMethod]
    public void TestWhereSingleOrDefault()
    {
      var single = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault();
      AssertNotValue(null, single);
      AssertValue("ALFKI", single.CustomerID);
    }

    [TestMethod]
    public void TestSingleOrDefaultNoMatches()
    {
      var single = db.Customers.SingleOrDefault(c => c.CustomerID == "SpongeBob");
      AssertValue(null, single);
    }

    [TestMethod]
    public void TestAnyTopLevel()
    {
      var any = db.Customers.Any();
      AssertTrue(any);
    }

    [TestMethod]
    public void TestAnyWithSubquery()
    {
      var list = db.Customers.Where(c => c.Orders.Any(o => o.CustomerID == "ALFKI")).ToList();
      AssertValue(1, list.Count);
    }

    [TestMethod]
    public void TestAnyWithSubqueryNoPredicate()
    {
      // customers with at least one order
      var list = db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).Any()).ToList();
      AssertValue(89, list.Count);
    }

    [TestMethod]
    public void TestAnyWithLocalCollection()
    {
      // get customers for any one of these IDs
      string[] ids = new[] { "ALFKI", "WOLZA", "NOONE" };
      var list = db.Customers.Where(c => ids.Any(id => c.CustomerID == id)).ToList();
      AssertValue(2, list.Count);
    }

    [TestMethod]
    public void TestAllWithSubquery()
    {
      var list = db.Customers.Where(c => c.Orders.All(o => o.CustomerID == "ALFKI")).ToList();
      // includes customers w/ no orders
      AssertValue(3, list.Count);
    }

    [TestMethod]
    public void TestAllWithLocalCollection()
    {
      // get all customers with a name that contains both 'm' and 'd'  (don't use vowels since these often depend on collation)
      string[] patterns = new[] { "m", "d" };

      var list = db.Customers.Where(c => patterns.All(p => c.ContactName.Contains(p))).Select(c => c.ContactName).ToList();
      var local = db.Customers.AsEnumerable().Where(c => patterns.All(p => c.ContactName.ToLower().Contains(p))).Select(c => c.ContactName).ToList();

      AssertValue(local.Count, list.Count);
    }

    [TestMethod]
    public void TestAllTopLevel()
    {
      // all customers have name length > 0?
      var all = db.Customers.All(c => c.ContactName.Length > 0);
      AssertTrue(all);
    }

    [TestMethod]
    public void TestAllTopLevelNoMatches()
    {
      // all customers have name with 'a'
      var all = db.Customers.All(c => c.ContactName.Contains("a"));
      AssertFalse(all);
    }

    [TestMethod]
    public void TestContainsWithSubquery()
    {
      // this is the long-way to determine all customers that have at least one order
      var list = db.Customers.Where(c => db.Orders.Select(o => o.CustomerID).Contains(c.CustomerID)).ToList();
      AssertValue(89, list.Count);
    }

    [TestMethod]
    public void TestContainsWithLocalCollection()
    {
      string[] ids = new[] { "ALFKI", "WOLZA", "NOONE" };
      var list = db.Customers.Where(c => ids.Contains(c.CustomerID)).ToList();
      AssertValue(2, list.Count);
    }

    [TestMethod]
    public void TestContainsTopLevel()
    {
      var contains = db.Customers.Select(c => c.CustomerID).Contains("ALFKI");
      AssertTrue(contains);
    }

    [TestMethod]
    public void TestSkipTake()
    {
      var list = db.Customers.OrderBy(c => c.CustomerID).Skip(5).Take(10).ToList();
      AssertValue(10, list.Count);
      AssertValue("BLAUS", list[0].CustomerID);
      AssertValue("COMMI", list[9].CustomerID);
    }

    [TestMethod]
    public void TestDistinctSkipTake()
    {
      var list = db.Customers.Select(c => c.City).Distinct().OrderBy(c => c).Skip(5).Take(10).ToList();
      AssertValue(10, list.Count);
      var hs = new HashSet<string>(list);
      AssertValue(10, hs.Count);
    }

    [TestMethod]
    public void TestCoalesce()
    {
      var list = db.Customers.Select(c => new { City = (c.City == "London" ? null : c.City), Country = (c.CustomerID == "EASTC" ? null : c.Country) })
                   .Where(x => (x.City ?? "NoCity") == "NoCity").ToList();
      AssertValue(6, list.Count);
      AssertValue(null, list[0].City);
    }

    [TestMethod]
    public void TestCoalesce2()
    {
      var list = db.Customers.Select(c => new { City = (c.City == "London" ? null : c.City), Country = (c.CustomerID == "EASTC" ? null : c.Country) })
                   .Where(x => (x.City ?? x.Country ?? "NoCityOrCountry") == "NoCityOrCountry").ToList();
      AssertValue(1, list.Count);
      AssertValue(null, list[0].City);
      AssertValue(null, list[0].Country);
    }

    // framework function tests

    [TestMethod]
    public void TestStringLength()
    {
      var list = db.Customers.Where(c => c.City.Length == 7).ToList();
      AssertValue(9, list.Count);
    }

    [TestMethod]
    public void TestStringStartsWithLiteral()
    {
      var list = db.Customers.Where(c => c.ContactName.StartsWith("M")).ToList();
      AssertValue(12, list.Count);
    }

    [TestMethod]
    public void TestStringStartsWithColumn()
    {
      var list = db.Customers.Where(c => c.ContactName.StartsWith(c.ContactName)).ToList();
      AssertValue(91, list.Count);
    }

    [TestMethod]
    public void TestStringEndsWithLiteral()
    {
      var list = db.Customers.Where(c => c.ContactName.EndsWith("s")).ToList();
      AssertValue(9, list.Count);
    }

    [TestMethod]
    public void TestStringEndsWithColumn()
    {
      var list = db.Customers.Where(c => c.ContactName.EndsWith(c.ContactName)).ToList();
      AssertValue(91, list.Count);
    }

    [TestMethod]
    public void TestStringContainsLiteral()
    {
      var list = db.Customers.Where(c => c.ContactName.Contains("nd")).Select(c => c.ContactName).ToList();
      var local = db.Customers.AsEnumerable().Where(c => c.ContactName.ToLower().Contains("nd")).Select(c => c.ContactName).ToList();
      AssertValue(local.Count, list.Count);
    }

    [TestMethod]
    public void TestStringContainsColumn()
    {
      var list = db.Customers.Where(c => c.ContactName.Contains(c.ContactName)).ToList();
      AssertValue(91, list.Count);
    }

    [TestMethod]
    public void TestStringConcatImplicit2Args()
    {
      var list = db.Customers.Where(c => c.ContactName + "X" == "Maria AndersX").ToList();
      AssertValue(1, list.Count);
    }

    [TestMethod]
    public void TestStringConcatExplicit2Args()
    {
      var list = db.Customers.Where(c => string.Concat(c.ContactName, "X") == "Maria AndersX").ToList();
      AssertValue(1, list.Count);
    }

    [TestMethod]
    public void TestStringConcatExplicit3Args()
    {
      var list = db.Customers.Where(c => string.Concat(c.ContactName, "X", c.Country) == "Maria AndersXGermany").ToList();
      AssertValue(1, list.Count);
    }

    [TestMethod]
    public void TestStringConcatExplicitNArgs()
    {
      var list = db.Customers.Where(c => string.Concat(new string[] { c.ContactName, "X", c.Country }) == "Maria AndersXGermany").ToList();
      AssertValue(1, list.Count);
    }

    [TestMethod]
    public void TestStringIsNullOrEmpty()
    {
      var list = db.Customers.Select(c => c.City == "London" ? null : c.CustomerID).Where(x => string.IsNullOrEmpty(x)).ToList();
      AssertValue(6, list.Count);
    }

    [TestMethod]
    public void TestStringToUpper()
    {
      var str = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => (c.CustomerID == "ALFKI" ? "abc" : "abc").ToUpper());
      AssertValue("ABC", str);
    }

    [TestMethod]
    public void TestStringToLower()
    {
      var str = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => (c.CustomerID == "ALFKI" ? "ABC" : "ABC").ToLower());
      AssertValue("abc", str);
    }

    [TestMethod]
    public void TestStringSubstring()
    {
      var list = db.Customers.Where(c => c.City.Substring(0, 4) == "Seat").ToList();
      AssertValue(1, list.Count);
      AssertValue("Seattle", list[0].City);
    }

    [TestMethod]
    public void TestStringSubstringNoLength()
    {
      var list = db.Customers.Where(c => c.City.Substring(4) == "tle").ToList();
      AssertValue(1, list.Count);
      AssertValue("Seattle", list[0].City);
    }

    [TestMethod]
#if SQLITE
        [Ignore]  // no equivalent function
#endif
    public void TestStringIndexOf()
    {
      var n = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.ContactName.IndexOf("ar"));
      AssertValue(1, n);
    }

    [TestMethod]
#if SQLITE
        [Ignore]  // no equivalent function
#endif
    public void TestStringIndexOfChar()
    {
      var n = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.ContactName.IndexOf('r'));
      AssertValue(2, n);
    }

    [TestMethod]
#if SQLITE
        [Ignore]  // no equivalent function
#endif
    public void TestStringIndexOfWithStart()
    {
      var n = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.ContactName.IndexOf("a", 3));
      AssertValue(4, n);
    }

    [TestMethod]
    public void TestStringTrim()
    {
      var notrim = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => ("  " + c.City + " "));
      var trim = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => ("  " + c.City + " ").Trim());
      AssertNotValue(notrim, trim);
      AssertValue(notrim.Trim(), trim);
    }

    [TestMethod]
#if SQLITE || MYSQL
        [Ignore]  // SQLITE: no equivalent function
                  // MYSQL: returns datetime as binary after combination of MAX and CONVERT
#endif
    public void TestDateTimeConstructYMD()
    {
      var dt = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4));
      AssertValue(1997, dt.Year);
      AssertValue(7, dt.Month);
      AssertValue(4, dt.Day);
      AssertValue(0, dt.Hour);
      AssertValue(0, dt.Minute);
      AssertValue(0, dt.Second);
    }

    [TestMethod]
#if SQLITE || MYSQL
        [Ignore]  // SQLITE: no equivalent function
                  // MYSQL: returns datetime as binary after combination of MAX and CONVERT
#endif
    public void TestDateTimeConstructYMDHMS()
    {
      var dt = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6));
      AssertValue(1997, dt.Year);
      AssertValue(7, dt.Month);
      AssertValue(4, dt.Day);
      AssertValue(3, dt.Hour);
      AssertValue(5, dt.Minute);
      AssertValue(6, dt.Second);
    }

    [TestMethod]
    public void TestDateTimeDay()
    {
      var v = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Day);
      AssertValue(25, v);
    }

    [TestMethod]
    public void TestDateTimeMonth()
    {
      var v = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Month);
      AssertValue(8, v);
    }

    [TestMethod]
    public void TestDateTimeYear()
    {
      var v = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Year);
      AssertValue(1997, v);
    }

    [TestMethod]
#if SQLITE
        [Ignore]   // not able to test via construction
#endif
    public void TestDateTimeHour()
    {
      var hour = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6).Hour);
      AssertValue(3, hour);
    }

    [TestMethod]
#if SQLITE
        [Ignore]   // not able to test via construction
#endif
    public void TestDateTimeMinute()
    {
      var minute = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6).Minute);
      AssertValue(5, minute);
    }

    [TestMethod]
#if SQLITE
        [Ignore]   // not able to test via construction
#endif
    public void TestDateTimeSecond()
    {
      var second = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6).Second);
      AssertValue(6, second);
    }

    [TestMethod]
    public void TestDateTimeDayOfWeek()
    {
      var dow = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.DayOfWeek);
      AssertValue(DayOfWeek.Monday, dow);
    }

    [TestMethod]
#if SQLITE
        [Ignore]
#endif
    public void TestDateTimeAddYears()
    {
      var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddYears(2).Year == 1999);
      AssertNotValue(null, od);
    }

    [TestMethod]
#if SQLITE
        [Ignore]
#endif
    public void TestDateTimeAddMonths()
    {
      var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddMonths(2).Month == 10);
      AssertNotValue(null, od);
    }

    [TestMethod]
#if SQLITE
        [Ignore]
#endif
    public void TestDateTimeAddDays()
    {
      var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddDays(2).Day == 27);
      AssertNotValue(null, od);
    }

    [TestMethod]
#if SQLITE
        [Ignore]
#endif
    public void TestDateTimeAddHours()
    {
      var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddHours(3).Hour == 3);
      AssertNotValue(null, od);
    }

    [TestMethod]
#if SQLITE
        [Ignore]
#endif
    public void TestDateTimeAddMinutes()
    {
      var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddMinutes(5).Minute == 5);
      AssertNotValue(null, od);
    }

    [TestMethod]
#if SQLITE
        [Ignore]
#endif
    public void TestDateTimeAddSeconds()
    {
      var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddSeconds(6).Second == 6);
      AssertNotValue(null, od);
    }

    [TestMethod]
    public void TestMathAbs()
    {
      var neg1 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Abs((c.CustomerID == "ALFKI") ? -1 : 0));
      var pos1 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Abs((c.CustomerID == "ALFKI") ? 1 : 0));
      AssertValue(Math.Abs(-1), neg1);
      AssertValue(Math.Abs(1), pos1);
    }

    [TestMethod]
    public void TestMathAtan()
    {
      var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Atan((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
      var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Atan((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
      AssertValue(Math.Atan(0.0), zero, 0.0001);
      AssertValue(Math.Atan(1.0), one, 0.0001);
    }

    [TestMethod]
    public void TestMathCos()
    {
      var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Cos((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
      var pi = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Cos((c.CustomerID == "ALFKI") ? Math.PI : Math.PI));
      AssertValue(Math.Cos(0.0), zero, 0.0001);
      AssertValue(Math.Cos(Math.PI), pi, 0.0001);
    }

    [TestMethod]
    public void TestMathSin()
    {
      var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sin((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
      var pi = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sin((c.CustomerID == "ALFKI") ? Math.PI : Math.PI));
      var pi2 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sin(((c.CustomerID == "ALFKI") ? Math.PI : Math.PI) / 2.0));
      AssertValue(Math.Sin(0.0), zero);
      AssertValue(Math.Sin(Math.PI), pi, 0.0001);
      AssertValue(Math.Sin(Math.PI / 2.0), pi2, 0.0001);
    }

    [TestMethod]
    public void TestMathTan()
    {
      var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Tan((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
      var pi = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Tan((c.CustomerID == "ALFKI") ? Math.PI : Math.PI));
      AssertValue(Math.Tan(0.0), zero, 0.0001);
      AssertValue(Math.Tan(Math.PI), pi, 0.0001);
    }

    [TestMethod]
    public void TestMathExp()
    {
      var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Exp((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
      var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Exp((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
      var two = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Exp((c.CustomerID == "ALFKI") ? 2.0 : 2.0));
      AssertValue(Math.Exp(0.0), zero, 0.0001);
      AssertValue(Math.Exp(1.0), one, 0.0001);
      AssertValue(Math.Exp(2.0), two, 0.0001);
    }

    [TestMethod]
    public void TestMathLog()
    {
      var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Log((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
      var e = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Log((c.CustomerID == "ALFKI") ? Math.E : Math.E));
      AssertValue(Math.Log(1.0), one, 0.0001);
      AssertValue(Math.Log(Math.E), e, 0.0001);
    }

    [TestMethod]
    public void TestMathSqrt()
    {
      var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sqrt((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
      var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sqrt((c.CustomerID == "ALFKI") ? 4.0 : 4.0));
      var nine = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sqrt((c.CustomerID == "ALFKI") ? 9.0 : 9.0));
      AssertValue(1.0, one);
      AssertValue(2.0, four);
      AssertValue(3.0, nine);
    }

    [TestMethod]
    public void TestMathPow()
    {
      // 2^n
      var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 0.0));
      var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 1.0));
      var two = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 2.0));
      var three = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 3.0));
      AssertValue(1.0, zero);
      AssertValue(2.0, one);
      AssertValue(4.0, two);
      AssertValue(8.0, three);
    }

    [TestMethod]
    public void TestMathRoundDefault()
    {
      var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Round((c.CustomerID == "ALFKI") ? 3.4 : 3.4));
      var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Round((c.CustomerID == "ALFKI") ? 3.6 : 3.6));
      AssertValue(3.0, four);
      AssertValue(4.0, six);
    }

    [TestMethod]
#if ACCESS || SQLITE
        [Ignore]
#endif
    public void TestMathFloor()
    {
      // The difference between floor and truncate is how negatives are handled.  Floor drops the decimals and moves the
      // value to the more negative, so Floor(-3.4) is -4.0 and Floor(3.4) is 3.0.
      var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Floor((c.CustomerID == "ALFKI" ? 3.4 : 3.4)));
      var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Floor((c.CustomerID == "ALFKI" ? 3.6 : 3.6)));
      var nfour = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Floor((c.CustomerID == "ALFKI" ? -3.4 : -3.4)));
      AssertValue(Math.Floor(3.4), four);
      AssertValue(Math.Floor(3.6), six);
      AssertValue(Math.Floor(-3.4), nfour);
    }

    [TestMethod]
#if ACCESS || SQLITE
        [Ignore]
#endif
    public void TestDecimalFloor()
    {
      var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Floor((c.CustomerID == "ALFKI" ? 3.4m : 3.4m)));
      var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Floor((c.CustomerID == "ALFKI" ? 3.6m : 3.6m)));
      var nfour = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Floor((c.CustomerID == "ALFKI" ? -3.4m : -3.4m)));
      AssertValue(decimal.Floor(3.4m), four);
      AssertValue(decimal.Floor(3.6m), six);
      AssertValue(decimal.Floor(-3.4m), nfour);
    }

    [TestMethod]
#if SQLITE
        [Ignore]
#endif
    public void TestMathTruncate()
    {
      // The difference between floor and truncate is how negatives are handled.  Truncate drops the decimals, 
      // therefore a truncated negative often has a more positive value than non-truncated (never has a less positive),
      // so Truncate(-3.4) is -3.0 and Truncate(3.4) is 3.0.
      var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? 3.4 : 3.4));
      var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? 3.6 : 3.6));
      var neg4 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? -3.4 : -3.4));
      AssertValue(Math.Truncate(3.4), four);
      AssertValue(Math.Truncate(3.6), six);
      AssertValue(Math.Truncate(-3.4), neg4);
    }

    [TestMethod]
    public void TestStringCompareTo()
    {
      var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.City.CompareTo("Seattle"));
      var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.City.CompareTo("Aaa"));
      var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.City.CompareTo("Berlin"));
      AssertValue(-1, lt);
      AssertValue(1, gt);
      AssertValue(0, eq);
    }

    [TestMethod]
    public void TestStringCompareToLT()
    {
      var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") < 0);
      var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") < 0);
      AssertNotValue(null, cmpLT);
      AssertValue(null, cmpEQ);
    }

    [TestMethod]
    public void TestStringCompareToLE()
    {
      var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") <= 0);
      var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") <= 0);
      var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") <= 0);
      AssertNotValue(null, cmpLE);
      AssertNotValue(null, cmpEQ);
      AssertValue(null, cmpGT);
    }

    [TestMethod]
    public void TestStringCompareToGT()
    {
      var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") > 0);
      var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") > 0);
      AssertNotValue(null, cmpLT);
      AssertValue(null, cmpEQ);
    }

    [TestMethod]
    public void TestStringCompareToGE()
    {
      var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") >= 0);
      var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") >= 0);
      var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") >= 0);
      AssertValue(null, cmpLE);
      AssertNotValue(null, cmpEQ);
      AssertNotValue(null, cmpGT);
    }

    [TestMethod]
    public void TestStringCompareToEQ()
    {
      var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") == 0);
      var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") == 0);
      var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") == 0);
      AssertValue(null, cmpLE);
      AssertNotValue(null, cmpEQ);
      AssertValue(null, cmpGT);
    }

    [TestMethod]
    public void TestStringCompareToNE()
    {
      var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") != 0);
      var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") != 0);
      var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") != 0);
      AssertNotValue(null, cmpLE);
      AssertValue(null, cmpEQ);
      AssertNotValue(null, cmpGT);
    }

    [TestMethod]
    public void TestStringCompare()
    {
      var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => string.Compare(c.City, "Seattle"));
      var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => string.Compare(c.City, "Aaa"));
      var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => string.Compare(c.City, "Berlin"));
      AssertValue(-1, lt);
      AssertValue(1, gt);
      AssertValue(0, eq);
    }

    [TestMethod]
    public void TestStringCompareLT()
    {
      var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") < 0);
      var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") < 0);
      AssertNotValue(null, cmpLT);
      AssertValue(null, cmpEQ);
    }

    [TestMethod]
    public void TestStringCompareLE()
    {
      var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") <= 0);
      var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") <= 0);
      var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") <= 0);
      AssertNotValue(null, cmpLE);
      AssertNotValue(null, cmpEQ);
      AssertValue(null, cmpGT);
    }

    [TestMethod]
    public void TestStringCompareGT()
    {
      var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") > 0);
      var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") > 0);
      AssertNotValue(null, cmpLT);
      AssertValue(null, cmpEQ);
    }

    [TestMethod]
    public void TestStringCompareGE()
    {
      var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") >= 0);
      var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") >= 0);
      var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") >= 0);
      AssertValue(null, cmpLE);
      AssertNotValue(null, cmpEQ);
      AssertNotValue(null, cmpGT);
    }

    [TestMethod]
    public void TestStringCompareEQ()
    {
      var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") == 0);
      var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") == 0);
      var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") == 0);
      AssertValue(null, cmpLE);
      AssertNotValue(null, cmpEQ);
      AssertValue(null, cmpGT);
    }

    [TestMethod]
    public void TestStringCompareNE()
    {
      var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") != 0);
      var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") != 0);
      var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") != 0);
      AssertNotValue(null, cmpLE);
      AssertValue(null, cmpEQ);
      AssertNotValue(null, cmpGT);
    }

    [TestMethod]
    public void TestIntCompareTo()
    {
      // prove that x.CompareTo(y) works for types other than string
      var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => (c.CustomerID == "ALFKI" ? 10 : 10).CompareTo(10));
      var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => (c.CustomerID == "ALFKI" ? 10 : 10).CompareTo(9));
      var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => (c.CustomerID == "ALFKI" ? 10 : 10).CompareTo(11));
      AssertValue(0, eq);
      AssertValue(1, gt);
      AssertValue(-1, lt);
    }

    [TestMethod]
    public void TestDecimalCompare()
    {
      // prove that type.Compare(x,y) works with decimal
      var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Compare((c.CustomerID == "ALFKI" ? 10m : 10m), 10m));
      var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Compare((c.CustomerID == "ALFKI" ? 10m : 10m), 9m));
      var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Compare((c.CustomerID == "ALFKI" ? 10m : 10m), 11m));
      AssertValue(0, eq);
      AssertValue(1, gt);
      AssertValue(-1, lt);
    }

    [TestMethod]
    public void TestDecimalAdd()
    {
      var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Add((c.CustomerID == "ALFKI" ? 1m : 1m), 2m));
      AssertValue(3m, onetwo);
    }

    [TestMethod]
    public void TestDecimalSubtract()
    {
      var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Subtract((c.CustomerID == "ALFKI" ? 1m : 1m), 2m));
      AssertValue(-1m, onetwo);
    }

    [TestMethod]
    public void TestDecimalMultiply()
    {
      var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Multiply((c.CustomerID == "ALFKI" ? 1m : 1m), 2m));
      AssertValue(2m, onetwo);
    }

    [TestMethod]
    public void TestDecimalDivide()
    {
      var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Divide((c.CustomerID == "ALFKI" ? 1.0m : 1.0m), 2.0m));
      AssertValue(0.5m, onetwo);
    }

    [TestMethod]
    public void TestDecimalNegate()
    {
      var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Negate((c.CustomerID == "ALFKI" ? 1m : 1m)));
      AssertValue(-1m, one);
    }

    [TestMethod]
    public void TestDecimalRoundDefault()
    {
      var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Round((c.CustomerID == "ALFKI" ? 3.4m : 3.4m)));
      var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Round((c.CustomerID == "ALFKI" ? 3.5m : 3.5m)));
      AssertValue(3.0m, four);
      AssertValue(4.0m, six);
    }

    [TestMethod]
#if SQLITE
        [Ignore]
#endif
    public void TestDecimalTruncate()
    {
      // The difference between floor and truncate is how negatives are handled.  Truncate drops the decimals, 
      // therefore a truncated negative often has a more positive value than non-truncated (never has a less positive),
      // so Truncate(-3.4) is -3.0 and Truncate(3.4) is 3.0.
      var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Truncate((c.CustomerID == "ALFKI") ? 3.4m : 3.4m));
      var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? 3.6m : 3.6m));
      var neg4 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? -3.4m : -3.4m));
      AssertValue(decimal.Truncate(3.4m), four);
      AssertValue(decimal.Truncate(3.6m), six);
      AssertValue(decimal.Truncate(-3.4m), neg4);
    }

    [TestMethod]
    public void TestDecimalLT()
    {
      // prove that decimals are treated normally with respect to normal comparison operators
      var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1.0m : 3.0m) < 2.0m);
      AssertNotValue(null, alfki);
    }

    [TestMethod]
    public void TestIntLessThan()
    {
      var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) < 2);
      var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) < 2);
      AssertNotValue(null, alfki);
      AssertValue(null, alfkiN);
    }

    [TestMethod]
    public void TestIntLessThanOrEqual()
    {
      var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) <= 2);
      var alfki2 = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 2 : 3) <= 2);
      var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) <= 2);
      AssertNotValue(null, alfki);
      AssertNotValue(null, alfki2);
      AssertValue(null, alfkiN);
    }

    [TestMethod]
    public void TestIntGreaterThan()
    {
      var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) > 2);
      var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) > 2);
      AssertNotValue(null, alfki);
      AssertValue(null, alfkiN);
    }

    [TestMethod]
    public void TestIntGreaterThanOrEqual()
    {
      var alfki = db.Customers.Single(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) >= 2);
      var alfki2 = db.Customers.Single(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 2) >= 2);
      var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) > 2);
      AssertNotValue(null, alfki);
      AssertNotValue(null, alfki2);
      AssertValue(null, alfkiN);
    }

    [TestMethod]
    public void TestIntEqual()
    {
      var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 1) == 1);
      var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 1) == 2);
      AssertNotValue(null, alfki);
      AssertValue(null, alfkiN);
    }

    [TestMethod]
    public void TestIntNotEqual()
    {
      var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 2 : 2) != 1);
      var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 2 : 2) != 2);
      AssertNotValue(null, alfki);
      AssertValue(null, alfkiN);
    }

    [TestMethod]
    public void TestIntAdd()
    {
      var three = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) + 2);
      AssertValue(3, three);
    }

    [TestMethod]
    public void TestIntSubtract()
    {
      var negone = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) - 2);
      AssertValue(-1, negone);
    }

    [TestMethod]
    public void TestIntMultiply()
    {
      var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 2 : 2) * 3);
      AssertValue(6, six);
    }

    [TestMethod]
    public void TestIntDivide()
    {
      var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 3 : 3) / 2);
      AssertValue(1, one);
    }

    [TestMethod]
    public void TestIntModulo()
    {
      var three = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 7 : 7) % 4);
      AssertValue(3, three);
    }

    [TestMethod]
    public void TestIntLeftShift()
    {
      var eight = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) << 3);
      AssertValue(8, eight);
    }

    [TestMethod]
    public void TestIntRightShift()
    {
      var eight = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 32 : 32) >> 2);
      AssertValue(8, eight);
    }

    [TestMethod]
    public void TestIntBitwiseAnd()
    {
      var band = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 6 : 6) & 3);
      AssertValue(2, band);
    }

    [TestMethod]
    public void TestIntBitwiseOr()
    {
      var eleven = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 10 : 10) | 3);
      AssertValue(11, eleven);
    }

    [TestMethod]
    public void TestIntBitwiseExclusiveOr()
    {
      var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) ^ 1);
      AssertValue(0, zero);
    }

    [TestMethod]
    public void TestIntBitwiseNot()
    {
      var bneg = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ~((c.CustomerID == "ALFKI") ? -1 : -1));
      AssertValue(~-1, bneg);
    }

    [TestMethod]
    public void TestIntNegate()
    {
      var neg = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => -((c.CustomerID == "ALFKI") ? 1 : 1));
      AssertValue(-1, neg);
    }

    [TestMethod]
    public void TestAnd()
    {
      var custs = db.Customers.Where(c => c.Country == "USA" && c.City.StartsWith("A")).Select(c => c.City).ToList();
      AssertValue(2, custs.Count);
      AssertTrue(custs.All(c => c.StartsWith("A")));
    }

    [TestMethod]
    public void TestOr()
    {
      var custs = db.Customers.Where(c => c.Country == "USA" || c.City.StartsWith("A")).Select(c => c.City).ToList();
      AssertValue(14, custs.Count);
    }

    [TestMethod]
    public void TestNot()
    {
      var custs = db.Customers.Where(c => !(c.Country == "USA")).Select(c => c.Country).ToList();
      AssertValue(78, custs.Count);
    }

    [TestMethod]
    public void TestEqualLiteralNull()
    {
      var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => x == null);
      AssertTrue(_provider.GetQueryText(q.Expression).Contains("IS NULL"));
      var n = q.Count();
      AssertValue(1, n);
    }

    [TestMethod]
    public void TestEqualLiteralNullReversed()
    {
      var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => null == x);
      AssertTrue(_provider.GetQueryText(q.Expression).Contains("IS NULL"));
      var n = q.Count();
      AssertValue(1, n);
    }

    [TestMethod]
    public void TestNotEqualLiteralNull()
    {
      var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => x != null);
      AssertTrue(_provider.GetQueryText(q.Expression).Contains("IS NOT NULL"));
      var n = q.Count();
      AssertValue(90, n);
    }

    [TestMethod]
    public void TestNotEqualLiteralNullReversed()
    {
      var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => null != x);
      AssertTrue(_provider.GetQueryText(q.Expression).Contains("IS NOT NULL"));
      var n = q.Count();
      AssertValue(90, n);
    }

    [TestMethod]
    public void TestConditionalResultsArePredicates()
    {
      bool value = db.Orders.Where(c => c.CustomerID == "ALFKI").Max(c => (c.CustomerID == "ALFKI" ? string.Compare(c.CustomerID, "POTATO") < 0 : string.Compare(c.CustomerID, "POTATO") > 0));
      AssertTrue(value);
    }

    [TestMethod]
    public void TestSelectManyJoined()
    {
      var cods =
          (from c in db.Customers
           from o in db.Orders.Where(o => o.CustomerID == c.CustomerID)
           select new { c.ContactName, o.OrderDate }).ToList();
      AssertValue(830, cods.Count);
    }

    [TestMethod]
    public void TestSelectManyJoinedDefaultIfEmpty()
    {
      var cods = (
          from c in db.Customers
          from o in db.Orders.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
          select new { c.ContactName, o.OrderDate }
          ).ToList();
      AssertValue(832, cods.Count);
    }

    [TestMethod]
    public void TestSelectWhereAssociation()
    {
      var ords = (
          from o in db.Orders
          where o.Customer.City == "Seattle"
          select o
          ).ToList();
      AssertValue(14, ords.Count);
    }

    [TestMethod]
    public void TestSelectWhereAssociationTwice()
    {
      var n = db.Orders.Where(c => c.CustomerID == "WHITC").Count();
      var ords = (
          from o in db.Orders
          where o.Customer.Country == "USA" && o.Customer.City == "Seattle"
          select o
          ).ToList();
      AssertValue(n, ords.Count);
    }

    [TestMethod]
    public void TestSelectAssociation()
    {
      var custs = (
          from o in db.Orders
          where o.CustomerID == "ALFKI"
          select o.Customer
          ).ToList();
      AssertValue(6, custs.Count);
      AssertTrue(custs.All(c => c.CustomerID == "ALFKI"));
    }

    [TestMethod]
    public void TestSelectAssociations()
    {
      var doubleCusts = (
          from o in db.Orders
          where o.CustomerID == "ALFKI"
          select new { A = o.Customer, B = o.Customer }
          ).ToList();

      AssertValue(6, doubleCusts.Count);
      AssertTrue(doubleCusts.All(c => c.A.CustomerID == "ALFKI" && c.B.CustomerID == "ALFKI"));
    }

    [TestMethod]
    public void TestSelectAssociationsWhereAssociations()
    {
      var stuff = (
          from o in db.Orders
          where o.Customer.Country == "USA"
          where o.Customer.City != "Seattle"
          select new { A = o.Customer, B = o.Customer }
          ).ToList();
      AssertValue(108, stuff.Count);
    }

    [TestMethod]
    public void TestCustomersIncludeOrders()
    {
      var policy = new EntityPolicy();
      policy.IncludeWith<Customer>(c => c.Orders);
      Northwind nw = new Northwind(_provider.New(policy));

      var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
      AssertValue(1, custs.Count);
      AssertNotValue(null, custs[0].Orders);
      AssertValue(6, custs[0].Orders.Count);
    }

    [TestMethod]
    public void TestCustomersIncludeOrdersAndDetails()
    {
      var policy = new EntityPolicy();
      policy.IncludeWith<Customer>(c => c.Orders);
      policy.IncludeWith<Order>(o => o.Details);
      Northwind nw = new Northwind(_provider.New(policy));

      var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
      AssertValue(1, custs.Count);
      AssertNotValue(null, custs[0].Orders);
      AssertValue(6, custs[0].Orders.Count);
      AssertTrue(custs[0].Orders.Any(o => o.OrderID == 10643));
      AssertNotValue(null, custs[0].Orders.Single(o => o.OrderID == 10643).Details);
      AssertValue(3, custs[0].Orders.Single(o => o.OrderID == 10643).Details.Count);
    }

    [TestMethod]
    public void TestCustomersIncludeOrdersViaConstructorOnly()
    {
      var mapping = new AttributeMapping(typeof(NorthwindX));
      var policy = new EntityPolicy();
      policy.IncludeWith<CustomerX>(c => c.Orders);
      NorthwindX nw = new NorthwindX(_provider.New(policy).New(mapping));

      var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
      AssertValue(1, custs.Count);
      AssertNotValue(null, custs[0].Orders);
      AssertValue(6, custs[0].Orders.Count);
    }

    [TestMethod]
    public void TestCustomersIncludeOrdersWhere()
    {
      var policy = new EntityPolicy();
      policy.IncludeWith<Customer>(c => c.Orders.Where(o => (o.OrderID & 1) == 0));
      Northwind nw = new Northwind(_provider.New(policy));

      var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
      AssertValue(1, custs.Count);
      AssertNotValue(null, custs[0].Orders);
      AssertValue(3, custs[0].Orders.Count);
    }

    [TestMethod]
    public void TestCustomersIncludeOrdersDeferred()
    {
      var policy = new EntityPolicy();
      policy.IncludeWith<Customer>(c => c.Orders, true);
      Northwind nw = new Northwind(_provider.New(policy));

      var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
      AssertValue(1, custs.Count);
      AssertNotValue(null, custs[0].Orders);
      AssertValue(6, custs[0].Orders.Count);
    }

    [TestMethod]
    public void TestCustomersAssociateOrders()
    {
      var policy = new EntityPolicy();
      policy.AssociateWith<Customer>(c => c.Orders.Where(o => (o.OrderID & 1) == 0));
      Northwind nw = new Northwind(_provider.New(policy));

      var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI")
          .Select(c => new { CustomerID = c.CustomerID, FilteredOrdersCount = c.Orders.Count() }).ToList();
      AssertValue(1, custs.Count);
      AssertValue(3, custs[0].FilteredOrdersCount);
    }

    [TestMethod]
    public void TestCustomersIncludeThenAssociateOrders()
    {
      var policy = new EntityPolicy();
      policy.IncludeWith<Customer>(c => c.Orders);
      policy.AssociateWith<Customer>(c => c.Orders.Where(o => (o.OrderID & 1) == 0));
      Northwind nw = new Northwind(_provider.New(policy));

      var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
      AssertValue(1, custs.Count);
      AssertNotValue(null, custs[0].Orders);
      AssertValue(3, custs[0].Orders.Count);
    }

    [TestMethod]
    public void TestCustomersAssociateThenIncludeOrders()
    {
      var policy = new EntityPolicy();
      policy.AssociateWith<Customer>(c => c.Orders.Where(o => (o.OrderID & 1) == 0));
      policy.IncludeWith<Customer>(c => c.Orders);
      Northwind nw = new Northwind(_provider.New(policy));

      var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
      AssertValue(1, custs.Count);
      AssertNotValue(null, custs[0].Orders);
      AssertValue(3, custs[0].Orders.Count);
    }

    [TestMethod]
    public void TestOrdersIncludeDetailsWithGroupBy()
    {
      var policy = new EntityPolicy();
      policy.IncludeWith<Order>(o => o.Details);
      Northwind nw = new Northwind(_provider.New(policy));
      var list = nw.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).ToList();
      AssertValue(1, list.Count);
      var grp = list[0].ToList();
      AssertValue(6, grp.Count);
      var o10643 = grp.SingleOrDefault(o => o.OrderID == 10643);
      AssertNotValue(null, o10643);
      AssertValue(3, o10643.Details.Count);
    }

    [TestMethod]
    public void TestCustomersApplyFilter()
    {
      var policy = new EntityPolicy();
      policy.Apply<Customer>(seq => seq.Where(c => c.City == "London"));
      Northwind nw = new Northwind(_provider.New(policy));

      var custs = nw.Customers.ToList();
      AssertValue(6, custs.Count);
    }

    [TestMethod]
    public void TestCustomersApplyComputedFilter()
    {
      string ci = "Lon";
      string ty = "don";
      var policy = new EntityPolicy();
      policy.Apply<Customer>(seq => seq.Where(c => c.City == ci + ty));
      Northwind nw = new Northwind(_provider.New(policy));

      var custs = nw.Customers.ToList();
      AssertValue(6, custs.Count);
    }

    [TestMethod]
    public void TestCustomersApplyFilterTwice()
    {
      var policy = new EntityPolicy();
      policy.Apply<Customer>(seq => seq.Where(c => c.City == "London"));
      policy.Apply<Customer>(seq => seq.Where(c => c.Country == "UK"));
      Northwind nw = new Northwind(_provider.New(policy));

      var custs = nw.Customers.ToList();
      AssertValue(6, custs.Count);
    }

    [TestMethod]
    public void TestCustomersApplyOrder()
    {
      var policy = new EntityPolicy();
      policy.Apply<Customer>(seq => seq.OrderBy(c => c.ContactName));
      Northwind nw = new Northwind(_provider.New(policy));

      var list = nw.Customers.Where(c => c.City == "London").ToList();

      AssertValue(6, list.Count);
      var sorted = list.OrderBy(c => c.ContactName).ToList();
      AssertTrue(Enumerable.SequenceEqual(list, sorted));
    }

    [TestMethod]
    public void TestCustomersApplyOrderAndAssociateOrders()
    {
      var policy = new EntityPolicy();
      policy.Apply<Order>(ords => ords.Where(o => o.OrderDate != null));
      policy.IncludeWith<Customer>(c => c.Orders.Where(o => (o.OrderID & 1) == 0));
      Northwind nw = new Northwind(_provider.New(policy));

      var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
      AssertValue(1, custs.Count);
      AssertNotValue(null, custs[0].Orders);
      AssertValue(3, custs[0].Orders.Count);
    }

    [TestMethod]
    public void TestOrdersIncludeDetailsWithFirst()
    {
      EntityPolicy policy = new EntityPolicy();
      policy.IncludeWith<Order>(o => o.Details);

      var ndb = new Northwind(_provider.New(policy));
      var q = from o in ndb.Orders
              where o.OrderID == 10248
              select o;

      Order so = q.Single();
      AssertValue(3, so.Details.Count);
      Order fo = q.First();
      AssertValue(3, fo.Details.Count);
    }

    [TestMethod]
    public void TestCache()
    {
      var qcache = new QueryCache(10);
      AssertValue(0, qcache.Count);

      var c1 = qcache.Execute(from c in db.Customers where c.CustomerID == "ALFKI" select c).First();
      AssertValue("ALFKI", c1.CustomerID);
      AssertValue(1, qcache.Count);

      var c2 = qcache.Execute(from c in db.Customers where c.CustomerID == "ANTON" select c).First();
      AssertValue("ANTON", c2.CustomerID);
      AssertValue(1, qcache.Count);

      string id = "ANTON";
      var c3 = qcache.Execute(from c in db.Customers where c.CustomerID == id select c).First();
      AssertValue(id, c3.CustomerID);
      AssertValue(1, qcache.Count);

      var o1 = qcache.Execute(from o in db.Orders where o.CustomerID == c1.CustomerID select o).ToList();
      AssertValue(6, o1.Count);
      AssertValue(2, qcache.Count);

      var o2 = qcache.Execute(from o in db.Orders where o.CustomerID == c2.CustomerID select o).ToList();
      AssertValue(7, o2.Count);
      AssertValue(2, qcache.Count);

      var o3 = qcache.Execute(from o in db.Orders where o.CustomerID == "ANTON" select o).ToList();
      AssertValue(7, o3.Count);
      AssertValue(2, qcache.Count);
    }

    [TestMethod]
#if ACCESS
        [Ignore] // access: can't handle scalar-subqueries referencing outer scope aliases?
#endif
    public void TestBigQueryWithOrderingGroupingAndNestedGroupCounts()
    {
      var query = db.Customers
                    .OrderBy(c => c.City)
                    .Take(10)
                    .GroupBy(c => c.City)
                    .OrderBy(g => g.Key)
                    .Select(g => new { Key = g.Key, ItemCount = g.Count(), HasSubGroups = false, Items = g });

      var results = query.ToList();
    }
  }
}
