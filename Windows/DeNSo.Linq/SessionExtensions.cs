using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Linq
{
  public static class SessionExtensions
  {
    public static IQueryable<T> Query<T>(this DeNSo.Core.Session session) where T : class, new()
    {
      return new QueryObject<T>(session);
    }
  }
}
