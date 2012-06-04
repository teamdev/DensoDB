using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DeNSo.Meta.BSon
{
  [Serializable]
  public class BSonProp : IBSonNode
  {
    public string Name { get; set; }
    public object Value { get; set; }

    public int GetLength()
    {
      return -1;
    }

    public byte[] GetBytes()
    {
      MemoryStream ms = new MemoryStream();
      BinaryWriter bw = new BinaryWriter(ms);
      GetBytes(bw);
      return ms.ToArray();
    }

    public void GetBytes(BinaryWriter writer)
    {
      if (Value is BSonDoc)
      {
        var doc = Value as BSonDoc;
        doc.Name = Name;
        doc.GetBytes(writer);
        return;
      }

      var type = BSonSerializer.GetBSonType(Value);
      writer.Write((byte)type);
      writer.Write(Name);
      BSonSerializer.WriteCorrectType(Value, writer, type);
    }
  }
}
