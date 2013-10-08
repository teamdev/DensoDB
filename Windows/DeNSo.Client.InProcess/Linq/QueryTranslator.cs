using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DeNSo.Linq
{
  public class QueryTranslator<T> : ExpressionVisitor where T : class , new()
  {
    ParameterExpression _jdocparameter = Expression.Parameter(typeof(JObject));
    HashSet<Type> _navigableobjects = new HashSet<Type>();

    public ParameterExpression JObjectParameter { get { return _jdocparameter; } }

    public Expression Translate(Expression expression)
    {
      _navigableobjects.Add(typeof(T));
      var result = Visit(expression);

      if (expression.NodeType == ExpressionType.Lambda)
      {
        var le = result as LambdaExpression;
        return Expression.Lambda<Func<JObject, bool>>(le.Body, JObjectParameter);
      }
      return result;
    }

    private static Expression StripQuotes(Expression e)
    {
      while (e.NodeType == ExpressionType.Quote)
      {
        e = ((UnaryExpression)e).Operand;
      }
      return e;
    }

#if DEBUG
    public override Expression Visit(Expression node)
    {
      if (node != null)
        Debug.WriteLine(node.ToString());
      return base.Visit(node);
    }
#endif

    //protected override Expression VisitDynamic(DynamicExpression node)
    //{
    //  return base.VisitDynamic(node);
    //}

    //protected override Expression VisitMethodCall(MethodCallExpression node)
    //{
    //  var result = base.Visit(node.Object);
    //  if (typeof(JToken).IsAssignableFrom(result.Type))
    //  { 
    //    Expression.MakeDynamic(
    //  }
    //  return base.VisitMethodCall(node);
    //}

    //protected override Expression VisitInvocation(InvocationExpression node)
    //{
    //  return base.VisitInvocation(node);
    //}

    //protected override Expression VisitExtension(Expression node)
    //{      
    //  return base.VisitExtension(node);
    //}

    protected override Expression VisitMember(MemberExpression node)
    {
      var result = base.Visit(node.Expression);
      if (_navigableobjects.Contains(node.Member.DeclaringType) ||
          (result != null && result.NodeType == ExpressionType.Parameter &&
           _navigableobjects.Contains(((ParameterExpression)result).Type)))
      {
        var tc = Type.GetTypeCode(node.Type);

        var myexpression = Expression.Property(result.Type == typeof(JToken) ? Expression.Convert(result, typeof(JObject)) : (Expression)_jdocparameter,
                                               typeof(JObject).GetProperty("Item", new Type[] { typeof(string) }),
                                               Expression.Constant(node.Member.Name));
        switch (tc)
        {
          case TypeCode.Boolean:
          case TypeCode.Byte:
          case TypeCode.Char:
          case TypeCode.DBNull:
          case TypeCode.DateTime:
          case TypeCode.Decimal:
          case TypeCode.Double:
          case TypeCode.Int16:
          case TypeCode.Int32:
          case TypeCode.Int64:
          case TypeCode.SByte:
          case TypeCode.Single:
          case TypeCode.String:
          case TypeCode.UInt16:
          case TypeCode.UInt32:
          case TypeCode.UInt64:
            return Expression.Convert(myexpression, node.Type);
          case TypeCode.Object:
            _navigableobjects.Add(node.Type);
            return myexpression;
          case TypeCode.Empty:
          default:
            break;
        }
        throw new NotSupportedException(string.Format("Navigation of type '{0}' is not supported", node.Type));
      }
      return base.VisitMember(node);
    }
  }
}
