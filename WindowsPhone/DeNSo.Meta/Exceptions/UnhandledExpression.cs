using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DeNSo.Meta.Exceptions
{
  public class UnhandledExpressionException : Exception
  {
    public ExpressionType ExpressionType { get; private set; }

    public UnhandledExpressionException()
      : base()
    { }

    public UnhandledExpressionException(ExpressionType expression)
      : base()
    {
      ExpressionType = expression;
    }

    public UnhandledExpressionException(string message, Exception innerexception)
      : base(message, innerexception)
    {
    }

    public UnhandledExpressionException(ExpressionType expression, string message, Exception innerexception)
      : base(message, innerexception)
    {
      ExpressionType = expression;
    }
  }
}
