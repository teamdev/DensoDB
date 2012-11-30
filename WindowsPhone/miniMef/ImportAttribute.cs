using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ComponentModel.Composition
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
  public class ImportAttribute : Attribute
  {
    public ImportAttribute()
    { }

    public ImportAttribute(Type contractType)
    {
      ContractType = contractType;
    }

    public bool AllowRecomposition { get; set; }
    public Type ContractType { get; set; }
  }
}
