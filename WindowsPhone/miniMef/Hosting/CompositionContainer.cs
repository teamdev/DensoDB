using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.ComponentModel.Composition.Hosting
{
  public class CompositionContainer
  {
    private ComposablePartCatalog _catalog = null;

    public CompositionContainer()
    { }

    public CompositionContainer(ComposablePartCatalog catalog)
    {
      _catalog = catalog;
    }

    public void ComposeParts(object objecttocompose)
    {
      var otype = objecttocompose.GetType();
      var oproperties = otype.GetProperties(Reflection.BindingFlags.NonPublic |
                                            Reflection.BindingFlags.Public | 
                                            BindingFlags.Instance);

      foreach (var p in oproperties)
      {
        foreach (var ca in p.GetCustomAttributes(typeof(ImportAttribute), false))
          ComposeImport(p, objecttocompose, ca as ImportAttribute);

        foreach (var ca in p.GetCustomAttributes(typeof(ImportManyAttribute), false))
          ComposeImportMany(p, objecttocompose, ca as ImportManyAttribute);
      }
    }

    public void ComposeImport(PropertyInfo p, Object target, ImportAttribute att)
    {
      if (p != null && target != null && att != null)
      {
        var r = from part in _catalog.Parts
                from d in part.ExportDefinitions.Where(k => k.Key == att.ContractType).Select(k => k.Value)
                from i in d
                select i;

        var targettype = r.FirstOrDefault();
        if (targettype != null)
          p.SetValue(target, Convert.ChangeType(Activator.CreateInstance(targettype), att.ContractType, null), null);
      }
    }

    public void ComposeImportMany(PropertyInfo p, Object target, ImportManyAttribute att)
    {
      if (p != null && target != null && att != null)
      {
        var r = from part in _catalog.Parts
                from d in part.ExportDefinitions.Where(k => k.Key == att.ContractType).Select(k => k.Value)
                from i in d
                select i;

        var createmethod = this.GetType().GetMethod("CreateItemsArray", BindingFlags.NonPublic | BindingFlags.Instance);
        createmethod.MakeGenericMethod(att.ContractType).Invoke(this, new object[] { p, target, r.AsEnumerable() });
      }
    }

    private void CreateItemsArray<T>(PropertyInfo p, Object target, IEnumerable<Type> rtypes) where T : class
    {
      List<T> result = new List<T>();
      foreach (var tt in rtypes)
        result.Add(Activator.CreateInstance(tt) as T);
      p.SetValue(target, result.ToArray(), null);
    }
  }
}
