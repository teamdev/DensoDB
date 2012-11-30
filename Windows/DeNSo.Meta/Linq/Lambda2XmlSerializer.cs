using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Linq;
using System.Diagnostics;

namespace DeNSo.Meta
{
  public class Lambda2XmlSerializer : ExpressionVisitor
  {
    private Dictionary<string, Type> _parameterstype = new Dictionary<string, Type>();
    private XElement _result = new XElement("Filter");
    private Stack<XElement> _current = new Stack<XElement>();

    public XElement Result { get { return _result; } }

    public Lambda2XmlSerializer()
    {
      _result = new XElement("Filter");
      _current.Push(_result);
    }

    public override Expression Visit(Expression node)
    {
      LogDebugActions(node);
      return base.Visit(node);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
      var mynode = new XElement(node.NodeType.ToString());
      _current.Peek().Add(mynode);
      _current.Push(mynode);

      var result = base.VisitBinary(node);
      _current.Pop();
      return result;
    }

    protected override Expression VisitConditional(ConditionalExpression node)
    {
      return base.VisitConditional(node);
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
      if (node.NodeType == ExpressionType.Constant)
      {
        var mynode = new XElement("Value");
        mynode.SetAttributeValue("type", ((ConstantExpression)node).Type);
        if (node.Value != null)
          mynode.Value = node.Value.ToString();

        _current.Peek().Add(mynode);
      }
      return base.VisitConstant(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
      if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
      {
        var mynode = new XElement("Parameter");
        mynode.SetAttributeValue("name", ((ParameterExpression)node.Expression).Name);
        mynode.Value = node.Member.Name;

        _current.Peek().Add(mynode);

        return base.VisitMember(node);
      }

      if (node.NodeType == ExpressionType.MemberAccess)
      {
        var mynode = new XElement("Value");
        var value = Expression.Lambda(node).Compile().DynamicInvoke();
        if (value != null)
          mynode.SetAttributeValue("type", value.GetType().AssemblyQualifiedName);
        if (value != null)
          mynode.Value = value.ToString();

        _current.Peek().Add(mynode);
      }

      return base.VisitMember(node);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
      return base.VisitParameter(node);
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
      var types = new List<XElement>();
      foreach (var p in node.Parameters)
      {
        _parameterstype.Add(p.Name, p.Type);
        types.Add(new XElement(p.Name, p.Type.AssemblyQualifiedName));
      }

      _current.Peek().Add(new XElement("Types", types.ToArray()));

      return base.VisitLambda<T>(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      var nn = new XElement(node.NodeType.ToString());
      _current.Peek().Add(nn);

      nn.SetAttributeValue("method", node.Method.Name);
      nn.SetAttributeValue("type", node.Method.DeclaringType.AssemblyQualifiedName);

      _current.Push(nn);
      this.Visit(node.Object);
      _current.Pop();

      foreach (var a in node.Arguments)
      {
        var v = Expression.Lambda(a).Compile().DynamicInvoke();
        var sn = new XElement("arg", v);
        if (v != null)
          sn.SetAttributeValue("type", v.GetType().AssemblyQualifiedName);
        nn.Add(sn);
      }

      return node;
    }

    [Conditional("DEBUG")]
    [DebuggerStepThrough]
    private void LogDebugActions(Expression node)
    {
      if (Debugger.IsAttached && node != null)
        Debug.WriteLine(node.ToString());
    }
  }
}
