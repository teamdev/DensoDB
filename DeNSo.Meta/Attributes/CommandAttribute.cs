using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Meta
{
  public class CommandAttribute : System.Attribute
  {
    public string Action { get; set; }
    public Type EntityType { get; set; }

    public CommandAttribute()
    { }
  }
}
