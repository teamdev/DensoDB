using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.ComponentModel.Composition.Hosting
{
  public class AssemblyCatalog : ComposablePartCatalog
  {
    private List<Primitevs.ComposablePartDefinition> _parts = new List<Primitevs.ComposablePartDefinition>();
    private Primitevs.ComposablePartDefinition _part = new Primitevs.ComposablePartDefinition();

    public override IQueryable<Primitevs.ComposablePartDefinition> Parts
    {
      get { return _parts.AsQueryable(); }
    }

    public AssemblyCatalog(Assembly assembly)
    {
      _parts.Add(_part);

      var rexport = (from t in assembly.GetTypes()
                     from a in t.GetCustomAttributes(typeof(ExportAttribute), false)
                     group t by ((ExportAttribute)a).ContractType into g 
                     select g);
      foreach(var g in rexport)
        foreach(var i in g)
        _part.AddDefinition(g.Key, i);

      //_part.ImportDefinitions.AddRange(assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(ImportAttribute), false) != null));
      //_part.ImportDefinitions.AddRange(assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(ImportManyAttribute), false) != null));
    }
  }
}
