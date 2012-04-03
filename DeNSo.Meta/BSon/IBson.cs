using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DeNSo.Meta.BSon
{
  public interface IBSonNode
  {
    string Name { get; set; }
    object Value { get; set; }
    int GetLength();
    byte[] GetBytes();
    void GetBytes(BinaryWriter writer);
  }
}
