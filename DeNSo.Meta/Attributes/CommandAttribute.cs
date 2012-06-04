using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Meta
{
  public class CommandAttribute : System.Attribute
  {
    public string Action { get; set; }
    public Type MessageType { get; set; }
    public string Method { get; set; }

    public CommandAttribute()
    { }
  }
}
