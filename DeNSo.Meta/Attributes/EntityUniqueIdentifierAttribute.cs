using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Meta
{
  /// <summary>
  /// This attribute
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public class EntityUniqueIdentifierAttribute : System.Attribute
  {
    public string UniqueIdentifier { get; private set; }

    public EntityUniqueIdentifierAttribute(string name)
    {
      UniqueIdentifier = name;
    }
  }
}
