using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using DeNSo.Meta.BSon;

namespace DeNSo.Meta
{
  public static class SerializationExtensions
  {
    public static Func<string, Type> TypeResolve { get; set; }

    public static XElement Serialize<T>(this Expression<Func<T, bool>> filter)
    {
      Lambda2XmlSerializer visitor = new Lambda2XmlSerializer();
      visitor.Visit(filter);

      return visitor.Result;
    }

    public static Expression Deserialize(this XElement query)
    {
      var parser = new XmlToLambdaParser(query);
      return parser.GetLambda();
    }
  }
}
