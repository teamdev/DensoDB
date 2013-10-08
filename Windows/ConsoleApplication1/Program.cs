using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using d = DeNSo;

namespace ConsoleApplication1
{
  class Program
  {
    static void Main(string[] args)
    {
      d.Configuration.EnableJournaling = true;
      d.Configuration.BasePath = @"c:\densodb";
      d.Session.Start();
      d.Session.DefaultDataBase = "test";

      var ss = d.Session.New;

      //var result = ss.Get<Class1>(c => c.Prop1 == "1" && c.Lista.Contains("test") || (c.Child != null && c.Child.Pppp1 == "kk")).ToList();

      var cc = new Class1() { Prop1 = "1", 
                              Prop2 = "2", 
                              Prop3 = 3, Prop4 = 7, Child = new Class2() { Pppp1 = "kk", Pppp2 = "kkkkkk" } };
      ss.Set(cc);

      d.Session.ShutDown();
    }
  }
}
