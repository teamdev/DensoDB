using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.BSon
{
  public static class BSonExtensions
  {
    public static BSonDoc AsDoc(this IBSonNode node)
    {
      return node as BSonDoc;
    }
  }
}
