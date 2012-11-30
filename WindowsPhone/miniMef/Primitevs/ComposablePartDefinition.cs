using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ComponentModel.Composition.Primitevs
{
  public class ComposablePartDefinition
  {
    public Dictionary<Type, List<Type>> ExportDefinitions { get; private set; }
    //public List<Type> ImportDefinitions { get; private set; }

    public ComposablePartDefinition()
    {
      ExportDefinitions = new Dictionary<Type, List<Type>>();
      //ImportDefinitions = new List<Type>();
    }

    internal void AddDefinition(Type key, Type item)
    {
      if (!ExportDefinitions.ContainsKey(key))
        ExportDefinitions.Add(key, new List<Type>());

      ExportDefinitions[key].Add(item);
    }
  }
}
