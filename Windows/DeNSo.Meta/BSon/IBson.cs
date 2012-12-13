using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DeNSo.BSon
{
  public interface IBSonNode
  {
    string Name { get; set; }
    object Value { get; set; }
    bool IsDoc { get; }
    int GetLength();
    byte[] GetBytes();
    void GetBytes(BinaryWriter writer);
  }
}
