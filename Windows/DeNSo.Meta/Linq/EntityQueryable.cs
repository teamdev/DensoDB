using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo
{
  public class EntityQueryable<T> : IQueryable<T>, IQueryable, IQueryProvider
  {
    public IEnumerator<T> GetEnumerator()
    {
      return null;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return null;
    }

    public Type ElementType
    {
      get { return typeof(T); }
    }

    public System.Linq.Expressions.Expression Expression
    {
      get;
      internal set;
    }

    public IQueryProvider Provider
    {
      get { return this; }
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
      EntityQueryable<TElement> obj = new EntityQueryable<TElement>();
      obj.Expression = expression;
      return (IQueryable<TElement>)obj;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
      return CreateQuery<T>(expression);
    }

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    {
      return default(TResult);
    }

    public object Execute(System.Linq.Expressions.Expression expression)
    {
      return null;
    }
  }
}
