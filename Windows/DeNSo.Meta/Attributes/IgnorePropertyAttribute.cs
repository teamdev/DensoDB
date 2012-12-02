using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo
{
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
  public class IgnorePropertyAttribute : System.Attribute
  {
  }
}
