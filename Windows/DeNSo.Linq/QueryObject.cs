using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Core;

namespace DeNSo.Linq
{
  public class QueryObject<T> : IQueryable<T>, IQueryable, IQueryProvider where T : class, new()
  {
    private Session _session = null;

    internal QueryObject(Session currentsession)
    {
      _session = currentsession;
    }

    public IEnumerator<T> GetEnumerator()
    {
      if (Expression == null)
        return _session.Get<T>().GetEnumerator();
      return null;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator() as IEnumerator;
    }

    public Type ElementType
    {
      get { return typeof(T); }
    }

    public System.Linq.Expressions.Expression Expression { get; private set; }

    public IQueryProvider Provider
    {
      get { return this; }
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
      this.Expression = expression;
      return (IQueryable<TElement>)this;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
      this.Expression = expression;
      return this;
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
