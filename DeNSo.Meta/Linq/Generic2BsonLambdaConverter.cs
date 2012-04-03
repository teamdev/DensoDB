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
      var mi = typeof(BSonDoc).GetMethod("get_Item", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
      return Expression.Call(Visit(node.Expression), mi, Expression.Constant(node.Member.Name));
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
      if (parameters.ContainsKey(node.Name))
        return parameters[node.Name];
      return base.VisitParameter(node);
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
      foreach (var p in node.Parameters)
        if (!parameters.ContainsKey(p.Name))
          parameters.Add(p.Name, Expression.Parameter(typeof(BSonDoc), p.Name));

      var nlambda = Expression.Lambda<Func<BSonDoc, bool>>(Visit(node.Body), parameters.Values.ToArray());

      return nlambda;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      return base.VisitMethodCall(node);
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
