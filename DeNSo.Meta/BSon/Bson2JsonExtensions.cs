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
      ToJson(doc, sb);
      return sb.ToString();
    }

    public static void ToJson(this BSon.BSonDoc doc, StringBuilder sb)
    {
      if (doc != null)
      {
        sb.Append("{");
        bool firstproperty = true;
        foreach (var propertyname in doc.Properties)
        {
          var property = doc[propertyname];
          sb.AppendFormat("{2}{0}:\"{1}\" ", propertyname,
            property is BSon.BSonDoc ? ((BSon.BSonDoc)property).ToJson() : property,
            firstproperty ? string.Empty : ",");

          firstproperty = false;
        }
        sb.Append("}");
      }
    }

    public static string ToJson(this IEnumerable<BSon.BSonDoc> docs)
    {
      var sb = new StringBuilder();

      sb.Append("[");
      bool firstdoc = true;
      foreach (var doc in docs)
      {
        if (!firstdoc)
          sb.Append(",");
        else
          firstdoc = false;
        doc.ToJson(sb);
      }
      sb.Append("]");

      return sb.ToString();
    }

  }
}
