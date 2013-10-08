using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
  public class Class1
  {
    public string Prop1 { get; set; }
    public string Prop2 { get; set; }
    public int Prop3 { get; set; }
    public int Prop4 { get; set; }

    public List<string> Lista { get; set; }
    public Class2 Child { get; set; }
  }

  public class Class2
  {
    public string Pppp1 { get; set; }
    public string Pppp2 { get; set; }
  }
}
