using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Linq.Expressions;
using DeNSo;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace DeNSo.Test
{
  class Program
  {
    static void Main(string[] args)
    {

      Configuration.BasePath = @"c:\densodb";
      Configuration.EnableJournaling = true;
      //Configuration.EnableOperationsLog = true;

      LogWriter.VerboseLevel = 2;

      Session.DefaultDataBase = "prova123";
      var ss = Session.New;

      var invariant = System.Globalization.CultureInfo.InvariantCulture;

      var cc = ss.Get<myPoint>().Count();

      var rnd = new Random();
      var rndindx = new List<string>();
      for (int x = 0; x < 1000000; x++)
        rndindx.Add(rnd.Next(10000000).ToString());

      // Start of performance measuring.

      int truefound = 0;
      int falsefount = 0;
      Thread.Sleep(15000);
      var watch = new Stopwatch();
      watch.Start();

      foreach (var i in rndindx)
        if (ss.GetById("myPoint", i) != null)
          truefound++;
        else
          falsefount++;


      watch.Stop();
      Console.WriteLine(watch.Elapsed);
      Console.ReadLine();
      //Session.ShutDown();
      //return;
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

      Console.WriteLine("Press Enter.");
      Console.ReadLine();

      GC.Collect(1);
      GC.WaitForPendingFinalizers();
      GC.Collect(2);
      GC.WaitForPendingFinalizers();
      GC.Collect(2);

      GC.GetTotalMemory(true);

      Console.WriteLine("Press Enter.");
      Console.ReadLine();

      Expression<Func<myPoint, bool>> expression = p => p.Z == 30.003743 && p.Y > 0;
      Expression<Func<JObject, bool>> expression2 = p => (double)p["Z"] == 30.003743;

      //Generic2BsonLambdaConverter visitor = new Generic2BsonLambdaConverter();
      //visitor.Visit(expression);

      //var rr = ss.Get("myPoint", d => (double)d["Z"] == 30.003743);

      //var epr = visitor.Visit(expression) as Expression<Func<BSonDoc, bool>>;


      //var rr = ss.Get("myPoint", epr.Compile());

      Console.WriteLine("Press Enter.");
      Console.ReadLine();
      Session.ShutDown();

      GC.Collect();
    }

    private static void ReadPoints(System.Globalization.CultureInfo invariant, List<myPoint> points)
    {
      int x = 0;
      using (var reader = File.OpenText("pcm-house4.xyz"))
      {
        while (!reader.EndOfStream)
        {
          var row = reader.ReadLine().Split(' ');
          points.Add(new myPoint()
          {
            _Id = x++.ToString(),
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
    public string _Id { get; set; }

    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
  }

  public class myEntity
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string SurName { get; set; }
  }
}
