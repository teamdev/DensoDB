using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DeNSo.Core.DiskIO
{
  public static class Serializer
  {
    private static Dictionary<Type, List<PropertyInfo>> _propertiescache = new Dictionary<Type, List<PropertyInfo>>();

    public static byte[] Serialize(dynamic obj)
    {
      if (obj == null) return null;

      Type objtype = (Type)obj.GetType();
      List<PropertyInfo> properties;

      if (_propertiescache.ContainsKey(objtype))
      {
        properties = _propertiescache[objtype];
      }
      else
      {
        properties = new List<PropertyInfo>();
        properties.AddRange(objtype.GetProperties(BindingFlags.Instance | BindingFlags.Public));
        _propertiescache.Add(objtype, properties);
      }
      
    
    }

  }
}
