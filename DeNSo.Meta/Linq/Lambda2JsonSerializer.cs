using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DeNSo.Meta
{

  public class Lambda2JsonSerializer : ExpressionVisitor
  {
    private Dictionary<string, Type> _parameterstype = new Dictionary<string, Type>();
    private StringBuilder _result = new StringBuilder();

    public string Result { get { return _result.ToString(); } }

    public Lambda2JsonSerializer()
    {
    }

    public override Expression Visit(Expression node)
    {
      LogDebugActions(node);
      return base.Visit(node);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
      _result.AppendFormat("{{node:\"{0}\" value:", node.NodeType.ToString());
      var result = base.VisitBinary(node);
      _result.AppendFormat("}}");
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
        _result.AppendFormat("{{node:\"value\" vtype:\"{0}\" value:{1}}}",
                    ((ConstantExpression)node).Type,
                    node.Value != null ? string.Format("\"{0}\"", node.Value) : "null");
      }
      return base.VisitConstant(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
      if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
      {
        _result.AppendFormat("{{node:\"parameter\" name:\"{0}\" value:\"{1}\"}}",
                             ((ParameterExpression)node.Expression).Name,
                             node.Member.Name);

        return base.VisitMember(node);
      }

      if (node.NodeType == ExpressionType.MemberAccess)
      {
        var value = Expression.Lambda(node).Compile().DynamicInvoke();

        _result.AppendFormat("{{node:\"value\" vtype:\"{0}\" value:{1}}}",
          value != null ? value.GetType().AssemblyQualifiedName : "null",
          value != null ? string.Format("\"{0}\"", value) : "null");
      }

      return base.VisitMember(node);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
      return base.VisitParameter(node);
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
      _result.AppendFormat("{{node:\"types\" value:[");
      int x = 0;
      foreach (var p in node.Parameters)
      {
        _parameterstype.Add(p.Name, p.Type);
        if (x++ > 0) _result.Append(",");

        _result.AppendFormat("{{{0}:\"{1}\"}}", p.Name, p.Type.AssemblyQualifiedName);
      }
      _result.AppendFormat("]}}");
      return base.VisitLambda<T>(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {

      _result.AppendFormat("{{node:\"{0}\" method:\"{1}\" mtype:\"{2}\"",
                           node.NodeType,
                           node.Method.Name,
                           node.Method.DeclaringType.AssemblyQualifiedName);

      this.Visit(node.Object);

      _result.AppendFormat(" args:[");
      int x = 0;
      foreach (var a in node.Arguments)
      {
        if (x++ > 0) _result.Append(",");

        var v = Expression.Lambda(a).Compile().DynamicInvoke();
        _result.AppendFormat("{{vtype:\"{0}\" value:\"{1}\"}}",
          v != null ? v.GetType().AssemblyQualifiedName : "null",
          v != null ? string.Format("\"{0}\"", v) : "null");
      }

      _result.AppendFormat("]}}");

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
