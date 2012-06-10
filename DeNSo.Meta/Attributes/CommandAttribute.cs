using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Meta
{
  public class HandlesCommandAttribute : System.Attribute
  {
    public string Command { get; set; }
    public HandlesCommandAttribute(string command)
    {
      Command = command;
    }
  }
}
