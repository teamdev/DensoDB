using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Linq.Expressions;
using DeNSo.Meta.BSon;

namespace DeNSo.Meta
{
  public class Generic2BsonLambdaConverter : ExpressionVisitor
  {
    private Dictionary<string, ParameterExpression> parameters = new Dictionary<string, ParameterExpression>();

    public override Expression Visit(Expression node)
    {
      LogDebugActions(node);
      return base.Visit(node);
    }

    private Type GetRightExpressionType(Expression node)
    {
      return node.Type;
    }

    private Expression GetFirstExpression(Expression node)
    {
      return null;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
      var type = GetRightExpressionType(node.Right);
      switch (node.NodeType)
      {
        case ExpressionType.And:
          return Expression.And(Visit(node.Left), Visit(node.Right));
        case ExpressionType.AndAlso:
          return Expression.AndAlso(Visit(node.Left), Visit(node.Right));
        case ExpressionType.Or:
          return Expression.Or(Visit(node.Left), Visit(node.Right));
        case ExpressionType.OrElse:
          return Expression.OrElse(Visit(node.Left), Visit(node.Right));

        case ExpressionType.Equal:
          return Expression.Equal(Expression.Convert(Visit(node.Left), GetRightExpressionType(node.Right)), Visit(node.Right));
        case ExpressionType.NotEqual:
          return Expression.NotEqual(Expression.Convert(Visit(node.Left), GetRightExpressionType(node.Right)), Visit(node.Right));

        case ExpressionType.GreaterThan:
          return Expression.GreaterThan(Expression.Convert(Visit(node.Left), GetRightExpressionType(node.Right)), Visit(node.Right));
        case ExpressionType.GreaterThanOrEqual:
          return Expression.GreaterThanOrEqual(Expression.Convert(Visit(node.Left), GetRightExpressionType(node.Right)), Visit(node.Right));
        case ExpressionType.LessThan:
          return Expression.LessThan(Expression.Convert(Visit(node.Left), GetRightExpressionType(node.Right)), Visit(node.Right));
        case ExpressionType.LessThanOrEqual:
          return Expression.LessThanOrEqual(Expression.Convert(Visit(node.Left), GetRightExpressionType(node.Right)), Visit(node.Right));
      }
      return base.VisitBinary(node);
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
      return base.VisitConstant(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
      var baseresult = Visit(node.Expression);
      if (baseresult.Type == typeof(BSonDoc))
      {
        var mi = typeof(BSonDoc).GetMethod("get_Item", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        return Expression.Call(baseresult, mi, Expression.Constant(node.Member.Name));
      }
      else
      {
        var mi = node.Expression.Type.GetProperty(node.Member.Name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (node.Expression.Type != baseresult.Type)
          baseresult = Expression.Convert(baseresult, node.Expression.Type);
        return Expression.MakeMemberAccess(baseresult, mi);
      }
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
      if (parameters.ContainsKey(node.Name))
        return parameters[node.Name];
      return base.VisitParameter(node);
    }

#if WINDOWS_PHONE
    protected override Expression VisitLambda(LambdaExpression node)
#else
    protected override Expression VisitLambda<T>(Expression<T> node)
#endif
    {
      foreach (var p in node.Parameters)
        if (!parameters.ContainsKey(p.Name))
          parameters.Add(p.Name, Expression.Parameter(typeof(BSonDoc), p.Name));

      var nlambda = Expression.Lambda<Func<BSonDoc, bool>>(Visit(node.Body), parameters.Values.ToArray());

      return nlambda;
    }


    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      var member = Visit(node.Object);
      var declaringtype = node.Method.DeclaringType;
      var convertedexpression = Expression.Convert(member, declaringtype);
      return Expression.Call(convertedexpression, node.Method, node.Arguments);
      //return base.VisitMethodCall(node);

      //var nn = new XElement(node.NodeType.ToString());
      //_current.Peek().Add(nn);

      //nn.SetAttributeValue("method", node.Method.Name);
      //nn.SetAttributeValue("type", node.Method.DeclaringType.AssemblyQualifiedName);

      //_current.Push(nn);
      //this.Visit(node.Object);
      //_current.Pop();

      //foreach (var a in node.Arguments)
      //{
      //  var v = Expression.Lambda(a).Compile().DynamicInvoke();
      //  var sn = new XElement("arg", v);
      //  if (v != null)
      //    sn.SetAttributeValue("type", v.GetType().AssemblyQualifiedName);
      //  nn.Add(sn);
      //}

      //return node;
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
