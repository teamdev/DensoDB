using DeNSo.Meta.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace System.Linq.Expressions
{
  public abstract class ExpressionVisitor
  {
    public ExpressionVisitor()
    {
    }

    public virtual Expression Visit(Expression exp)
    {
      if (exp == null)
      {
        return exp;
      }
      switch (exp.NodeType)
      {
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
        case ExpressionType.And:
        case ExpressionType.AndAlso:
        case ExpressionType.ArrayIndex:
        case ExpressionType.Coalesce:
        case ExpressionType.Divide:
        case ExpressionType.Equal:
        case ExpressionType.ExclusiveOr:
        case ExpressionType.GreaterThan:
        case ExpressionType.GreaterThanOrEqual:
        case ExpressionType.LeftShift:
        case ExpressionType.LessThan:
        case ExpressionType.LessThanOrEqual:
        case ExpressionType.Modulo:
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
        case ExpressionType.NotEqual:
        case ExpressionType.Or:
        case ExpressionType.OrElse:
        case ExpressionType.Power:
        case ExpressionType.RightShift:
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
          return this.VisitBinary((BinaryExpression)exp);
        case ExpressionType.ArrayLength:
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
        case ExpressionType.Not:
        case ExpressionType.Quote:
        case ExpressionType.TypeAs:
          return this.VisitUnary((UnaryExpression)exp);
        case ExpressionType.Call:
          return this.VisitMethodCall((MethodCallExpression)exp);
        case ExpressionType.Conditional:
          return this.VisitConditional((ConditionalExpression)exp);
        case ExpressionType.Constant:
          return this.VisitConstant((ConstantExpression)exp);
        case ExpressionType.Invoke:
          return this.VisitInvocation((InvocationExpression)exp);
        case ExpressionType.Lambda:
          return this.VisitLambda((LambdaExpression)exp);
        case ExpressionType.ListInit:
          return this.VisitListInit((ListInitExpression)exp);
        case ExpressionType.MemberAccess:
          return this.VisitMember((MemberExpression)exp);
        case ExpressionType.MemberInit:
          return this.VisitMemberInit((MemberInitExpression)exp);
        case ExpressionType.UnaryPlus:
          if (exp.Type == typeof(TimeSpan))
          {
            return this.VisitUnary((UnaryExpression)exp);
          }
          throw new UnhandledExpressionException(exp.NodeType);
        case ExpressionType.New:
          return this.VisitNew((NewExpression)exp);
        case ExpressionType.NewArrayInit:
        case ExpressionType.NewArrayBounds:
          return this.VisitNewArray((NewArrayExpression)exp);
        case ExpressionType.Parameter:
          return this.VisitParameter((ParameterExpression)exp);
        case ExpressionType.TypeIs:
          return this.VisitTypeIs((TypeBinaryExpression)exp);
        default:
          throw new UnhandledExpressionException(exp.NodeType);
      }
    }

    protected virtual Expression VisitBinary(BinaryExpression b)
    {
      Expression expression = this.Visit(b.Left);
      Expression expression2 = this.Visit(b.Right);
      if (expression != b.Left || expression2 != b.Right)
      {
        return Expression.MakeBinary(b.NodeType, expression, expression2, b.IsLiftedToNull, b.Method);
      }
      return b;
    }

    protected virtual Expression VisitConstant(ConstantExpression c)
    {
      return c;
    }    

    protected virtual Expression VisitParameter(ParameterExpression p)
    {
      return p;
    }

    protected virtual Expression VisitMember(MemberExpression m)
    {
      Expression expression = this.Visit(m.Expression);
      if (expression != m.Expression)
      {
        return Expression.MakeMemberAccess(expression, m.Member);
      }
      return m;
    }

    protected virtual Expression VisitMethodCall(MethodCallExpression m)
    {
      Expression expression = this.Visit(m.Object);
      IEnumerable<Expression> enumerable = this.VisitExpressionList(m.Arguments);
      if (expression != m.Object || enumerable != m.Arguments)
      {
        return Expression.Call(expression, m.Method, enumerable);
      }
      return m;
    }

    protected virtual Expression VisitLambda(LambdaExpression lambda)
    {
      Expression expression = this.Visit(lambda.Body);
      if (expression != lambda.Body)
      {
        return Expression.Lambda(lambda.Type, expression, lambda.Parameters);
      }
      return lambda;
    }

    internal virtual Expression VisitTypeIs(TypeBinaryExpression b)
    {
      Expression expression = this.Visit(b.Expression);
      if (expression != b.Expression)
      {
        return Expression.TypeIs(expression, b.TypeOperand);
      }
      return b;
    }

    internal virtual Expression VisitConditional(ConditionalExpression c)
    {
      Expression expression = this.Visit(c.Test);
      Expression expression2 = this.Visit(c.IfTrue);
      Expression expression3 = this.Visit(c.IfFalse);
      if (expression != c.Test || expression2 != c.IfTrue || expression3 != c.IfFalse)
      {
        return Expression.Condition(expression, expression2, expression3);
      }
      return c;
    }

    internal virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
    {
      List<Expression> list = null;
      int i = 0;
      int count = original.Count;
      while (i < count)
      {
        Expression expression = this.Visit(original[i]);
        if (list != null)
        {
          list.Add(expression);
        }
        else
        {
          if (expression != original[i])
          {
            list = new List<Expression>(count);
            for (int j = 0; j < i; j++)
            {
              list.Add(original[j]);
            }
            list.Add(expression);
          }
        }
        i++;
      }
      if (list != null)
      {
        return new ReadOnlyCollection<Expression>(list);
      }
      return original;
    }

    internal virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
    {
      Expression expression = this.Visit(assignment.Expression);
      if (expression != assignment.Expression)
      {
        return Expression.Bind(assignment.Member, expression);
      }
      return assignment;
    }

    internal virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
    {
      IEnumerable<MemberBinding> enumerable = this.VisitBindingList(binding.Bindings);
      if (enumerable != binding.Bindings)
      {
        return Expression.MemberBind(binding.Member, enumerable);
      }
      return binding;
    }

    internal virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
    {
      IEnumerable<ElementInit> enumerable = this.VisitElementInitializerList(binding.Initializers);
      if (enumerable != binding.Initializers)
      {
        return Expression.ListBind(binding.Member, enumerable);
      }
      return binding;
    }

    internal virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
    {
      List<MemberBinding> list = null;
      int i = 0;
      int count = original.Count;
      while (i < count)
      {
        MemberBinding memberBinding = this.VisitBinding(original[i]);
        if (list != null)
        {
          list.Add(memberBinding);
        }
        else
        {
          if (memberBinding != original[i])
          {
            list = new List<MemberBinding>(count);
            for (int j = 0; j < i; j++)
            {
              list.Add(original[j]);
            }
            list.Add(memberBinding);
          }
        }
        i++;
      }
      if (list != null)
      {
        return list;
      }
      return original;
    }

    internal virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
    {
      List<ElementInit> list = null;
      int i = 0;
      int count = original.Count;
      while (i < count)
      {
        ElementInit elementInit = this.VisitElementInitializer(original[i]);
        if (list != null)
        {
          list.Add(elementInit);
        }
        else
        {
          if (elementInit != original[i])
          {
            list = new List<ElementInit>(count);
            for (int j = 0; j < i; j++)
            {
              list.Add(original[j]);
            }
            list.Add(elementInit);
          }
        }
        i++;
      }
      if (list != null)
      {
        return list;
      }
      return original;
    }

    internal virtual NewExpression VisitNew(NewExpression nex)
    {
      IEnumerable<Expression> enumerable = this.VisitExpressionList(nex.Arguments);
      if (enumerable == nex.Arguments)
      {
        return nex;
      }
      if (nex.Members != null)
      {
        return Expression.New(nex.Constructor, enumerable, nex.Members);
      }
      return Expression.New(nex.Constructor, enumerable);
    }

    internal virtual Expression VisitMemberInit(MemberInitExpression init)
    {
      NewExpression newExpression = this.VisitNew(init.NewExpression);
      IEnumerable<MemberBinding> enumerable = this.VisitBindingList(init.Bindings);
      if (newExpression != init.NewExpression || enumerable != init.Bindings)
      {
        return Expression.MemberInit(newExpression, enumerable);
      }
      return init;
    }
    
    internal virtual Expression VisitListInit(ListInitExpression init)
    {
      NewExpression newExpression = this.VisitNew(init.NewExpression);
      IEnumerable<ElementInit> enumerable = this.VisitElementInitializerList(init.Initializers);
      if (newExpression != init.NewExpression || enumerable != init.Initializers)
      {
        return Expression.ListInit(newExpression, enumerable);
      }
      return init;
    }
    
    internal virtual Expression VisitNewArray(NewArrayExpression na)
    {
      IEnumerable<Expression> enumerable = this.VisitExpressionList(na.Expressions);
      if (enumerable == na.Expressions)
      {
        return na;
      }
      if (na.NodeType == ExpressionType.NewArrayInit)
      {
        return Expression.NewArrayInit(na.Type.GetElementType(), enumerable);
      }
      return Expression.NewArrayBounds(na.Type.GetElementType(), enumerable);
    }
    
    internal virtual Expression VisitInvocation(InvocationExpression iv)
    {
      IEnumerable<Expression> enumerable = this.VisitExpressionList(iv.Arguments);
      Expression expression = this.Visit(iv.Expression);
      if (enumerable != iv.Arguments || expression != iv.Expression)
      {
        return Expression.Invoke(expression, enumerable);
      }
      return iv;
    }

    internal virtual MemberBinding VisitBinding(MemberBinding binding)
    {
      switch (binding.BindingType)
      {
        case MemberBindingType.Assignment:
          return this.VisitMemberAssignment((MemberAssignment)binding);
        case MemberBindingType.MemberBinding:
          return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
        case MemberBindingType.ListBinding:
          return this.VisitMemberListBinding((MemberListBinding)binding);
        default:
          throw new UnhandledExpressionException();
      }
    }

    internal virtual ElementInit VisitElementInitializer(ElementInit initializer)
    {
      ReadOnlyCollection<Expression> readOnlyCollection = this.VisitExpressionList(initializer.Arguments);
      if (readOnlyCollection != initializer.Arguments)
      {
        return Expression.ElementInit(initializer.AddMethod, readOnlyCollection);
      }
      return initializer;
    }

    internal virtual Expression VisitUnary(UnaryExpression u)
    {
      Expression expression = this.Visit(u.Operand);
      if (expression != u.Operand)
      {
        return Expression.MakeUnary(u.NodeType, expression, u.Type, u.Method);
      }
      return u;
    }

  }
}
