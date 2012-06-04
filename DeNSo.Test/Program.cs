using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Core;
using DeNSo.Meta.BSon;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Linq.Expressions;
using DeNSo.Meta;

namespace DeNSo.Test
{
  class Program
  {
    static void Main(string[] args)
    {

      Configuration.BasePath = @"c:\densodb";
      Configuration.EnableJournaling = true;

      Session.DefaultDataBase = "prova123";
      var ss = Session.New;

      var invariant = System.Globalization.CultureInfo.InvariantCulture;

      var points = new List<myPoint>();
      ReadPoints(invariant, points);
      ReadPoints(invariant, points);

      GC.AddMemoryPressure(100000000);

      DateTime start = DateTime.Now;
      for (int i = 0; i < points.Count; i++)
      {
        var point = points[i];
        ss.Set(point);
      }

      GC.RemoveMemoryPressure(100000000);

      points.Clear();
      points = null;
      
      Console.ReadLine();

      GC.Collect(1);
      GC.WaitForPendingFinalizers();
      GC.Collect(2);
      GC.WaitForPendingFinalizers();
      GC.Collect(2);

      GC.GetTotalMemory(true);

      Console.ReadLine();

      Expression<Func<myPoint, bool>> expression = p => p.Z == 30.003743 && p.Y > 0;
      Expression<Func<BSonDoc, bool>> expression2 = p => (double)p["Z"] == 30.003743;

      Generic2BsonLambdaConverter visitor = new Generic2BsonLambdaConverter();
      visitor.Visit(expression);

      //var rr = ss.Get("myPoint", d => (double)d["Z"] == 30.003743);

      var epr = visitor.Visit(expression) as Expression<Func<BSonDoc, bool>>;


      var rr = ss.Get("myPoint", epr);
      Console.ReadLine();
      Session.ShutDown();

      GC.Collect();
    }

    private static void ReadPoints(System.Globalization.CultureInfo invariant, List<myPoint> points)
    {
      using (var reader = File.OpenText("pcm-house4.xyz"))
      {
        while (!reader.EndOfStream)
        {
          var row = reader.ReadLine().Split(' ');
          points.Add(new myPoint()
          {
            X = double.Parse(row[0], invariant),
            Y = double.Parse(row[1], invariant),
            Z = Double.Parse(row[2], invariant)
          });
        }
      }
    }
  }

  public class myPoint
  {
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
  }

  [DeNSo.Meta.EntityUniqueIdentifier("Id")]
  public class myEntity
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string SurName { get; set; }
  }
}
