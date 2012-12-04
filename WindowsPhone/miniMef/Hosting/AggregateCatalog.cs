using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ComponentModel.Composition.Hosting
{
  public class AggregateCatalog : ComposablePartCatalog
  {
    public override IQueryable<Primitevs.ComposablePartDefinition> Parts
    {
      get
      {
        return (from c in Catalogs
                from p in c.Parts
                select p).AsQueryable();
      }
    }

    public AggregateCatalog()
      : base()
    { }

    public AggregateCatalog(IEnumerable<ComposablePartCatalog> catalogs)
      : this()
    {
      Catalogs.AddRange(catalogs);
    }

    public AggregateCatalog(params ComposablePartCatalog[] catalogs)
      : this((IEnumerable<ComposablePartCatalog>)catalogs)
    { }

  }
}
