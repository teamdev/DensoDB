using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Meta.BSon
{
  public static class Bson2JsonExtensions
  {
    public static string ToJson(this BSon.BSonDoc doc)
    {
      var sb = new StringBuilder();
      if (doc != null)
      {
        sb.Append("{");
        foreach (var propertyname in doc.Properties)
        {
          var property = doc[propertyname];
          sb.AppendFormat("{0}:\"{1}\" ", propertyname,
            property is BSon.BSonDoc ? ((BSon.BSonDoc)property).ToJson() : property);
        }
        sb.Append("}");
      }
      return sb.ToString(); 
    }
  }
}
