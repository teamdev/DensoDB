using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitevs;
using System.Linq;
using System.Text;

namespace System.ComponentModel.Composition.Hosting
{
  public abstract class ComposablePartCatalog : IDisposable
  {
    public List<ComposablePartCatalog> Catalogs { get; private set; }
    public abstract IQueryable<ComposablePartDefinition> Parts { get; }


    public ComposablePartCatalog()
    {
      Catalogs = new List<ComposablePartCatalog>();
    }

    public void Dispose()
    {
    }
  }
}
