using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ComponentModel.Composition
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
  public class ExportAttribute : Attribute
  {
    public Type ContractType { get; private set; }

    public ExportAttribute()
    { }

    public ExportAttribute(Type contractType)
    {
      ContractType = contractType;
    }
  }
}
