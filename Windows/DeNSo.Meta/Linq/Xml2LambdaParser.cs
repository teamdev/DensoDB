using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Linq.Expressions;
using System.Diagnostics;

namespace DeNSo
{
  public class XmlToLambdaParser
  {
    public XElement Filter { get; private set; }
    public Dictionary<string, Type> ElementTypes { get; private set; }
    public Dictionary<string, ParameterExpression> Parameters { get; set; }

    public XmlToLambdaParser(XElement filter)
    {
      ElementTypes = new Dictionary<string, Type>();
      Parameters = new Dictionary<string, ParameterExpression>();
      Filter = filter;
    }

    public Expression GetLambda()
    {
      try
      {
        var lambdabody = ExploreNode(Filter);

        return Expression.Lambda(lambdabody, Parameters.Values.ToArray());
      }
      catch (Exception ex)
      {
        Debug.WriteLine(string.Format("EXCEPTION ON DESERIALIZE: {0}", ex.Message));
        throw;
      }
    }

    private Expression ExploreNode(XElement element)
    {
      if (element != null)
        switch (element.Name.LocalName)
        {
          case "Filter": return NavigateFilter(element);
          case "Types": NavigateTypes(element); break;
          case "AndAlso": return NavigateAndAlso(element);
          case "OrElse": return NavigateOrElse(element);
          case "GreaterThan": return NavigateGreaterThan(element);
          case "GreaterThanOrEqual": return NavigateGreaterThanOrEqual(element);
          case "LessThan": return NavigateLessThan(element);
          case "LessThanOrEqual": return NavigateLessThanOrEqual(element);
          case "Equal": return NavigateEqual(element);
          case "NotEqual": return NavigateNotEqual(element);
          case "Call": return NavigateCall(element);
          case "Parameter": return NavigateParameter(element);
          case "arg":
          case "Value": return NavigateValue(element);
          default:
            break;
        }
      return null;
    }

    private Expression NavigateCall(XElement node)
    {
      var nodes = node.Nodes().ToArray();
      Expression par = ExploreNode(nodes[0] as XElement);
      List<Expression> arguments = new List<Expression>();

      string name = node.Attribute("method").Value;
      string typename = node.Attribute("type").Value;

      Type tt = null;
      if (SerializationExtensions.TypeResolve != null)
        tt = SerializationExtensions.TypeResolve(typename);
      if (tt == null)
        tt = Type.GetType(typename, true);

      var mi = tt.GetMethod(name);

      //Expression call = Expression.Call(par
      for (int i = 1; i < nodes.Length; i++)
      {
        arguments.Add(ExploreNode(nodes[i] as XElement));
      }

      Expression result = null;
      if (arguments.Count > 0)
        result = Expression.Call(par, mi, arguments.ToArray());
      else
        result = Expression.Call(par, mi);

      return result;
    }

    private Expression NavigateParameter(XElement node)
    {
      var parname = node.Attribute("name").Value;
      if (Parameters.ContainsKey(parname))
      {
        return Expression.PropertyOrField(Parameters[parname], node.Value);
      }
      return null;
    }

    private Expression NavigateValue(XElement node)
    {
      var tt = Type.GetType(node.FirstAttribute.Value);
      if (tt != typeof(string) && string.IsNullOrEmpty(node.Value))
        return Expression.Constant(null);

      if (tt == typeof(Guid))
        return Expression.Constant(new Guid(node.Value));

      return Expression.Constant(Convert.ChangeType(node.Value, tt));
    }

    private Expression NavigateFilter(XElement node)
    {
      Expression result = null;
      foreach (var el in node.Nodes())
      {
        result = (ExploreNode(el as XElement) ?? result);
      }
      return result;
    }

    private void NavigateTypes(XElement node)
    {
      foreach (XElement el in node.Nodes())
      {
        var n = el.Name.LocalName;
        Type t = null;
        if (SerializationExtensions.TypeResolve != null)
          t = SerializationExtensions.TypeResolve(el.Value);

        if (t == null)
          t = Type.GetType(el.Value, false);

        if (t != null)
          if (!this.ElementTypes.ContainsKey(n))
          {
            this.ElementTypes.Add(n, t);
            this.Parameters.Add(n, Expression.Parameter(t, n));
          }
        //else
        //{
        //  this.ElementTypes[n] = t;
        //}
      }
    }

    private Expression NavigateOrElse(XElement node)
    {
      var subnodes = node.Nodes().ToArray();
      return Expression.OrElse(ExploreNode(subnodes[0] as XElement), ExploreNode(subnodes[1] as XElement));
    }

    private Expression NavigateAndAlso(XElement node)
    {
      var subnodes = node.Nodes().ToArray();
      return Expression.AndAlso(ExploreNode(subnodes[0] as XElement), ExploreNode(subnodes[1] as XElement));
    }

    private Expression NavigateLessThan(XElement node)
    {
      var subnodes = node.Nodes().ToArray();
      return Expression.LessThan(ExploreNode(subnodes[0] as XElement), ExploreNode(subnodes[1] as XElement));
    }

    private Expression NavigateLessThanOrEqual(XElement node)
    {
      var subnodes = node.Nodes().ToArray();
      return Expression.LessThanOrEqual(ExploreNode(subnodes[0] as XElement), ExploreNode(subnodes[1] as XElement));
    }

    private Expression NavigateGreaterThan(XElement node)
    {
      var subnodes = node.Nodes().ToArray();
      return Expression.GreaterThan(ExploreNode(subnodes[0] as XElement), ExploreNode(subnodes[1] as XElement));
    }

    private Expression NavigateGreaterThanOrEqual(XElement node)
    {
      var subnodes = node.Nodes().ToArray();
      return Expression.GreaterThanOrEqual(ExploreNode(subnodes[0] as XElement), ExploreNode(subnodes[1] as XElement));
    }

    private Expression NavigateEqual(XElement node)
    {
      var subnodes = node.Nodes().ToArray();
      return Expression.Equal(ExploreNode(subnodes[0] as XElement), ExploreNode(subnodes[1] as XElement));
    }

    private Expression NavigateNotEqual(XElement node)
    {
      var subnodes = node.Nodes().ToArray();
      return Expression.NotEqual(ExploreNode(subnodes[0] as XElement), ExploreNode(subnodes[1] as XElement));
    }
  }
}
