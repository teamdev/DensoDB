using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DeNSo.Meta.BSon
{
  [Serializable]
  public class BSonDoc : IBSonNode
  {
    private Dictionary<string, IBSonNode> _props = new Dictionary<string, IBSonNode>();
    public object this[string index]
    {
      get
      {
        if (_props.ContainsKey(index))
          return _props[index].Value;
        return null;
      }
      set
      {
        if (value is IBSonNode)
        { }

        if (_props.ContainsKey(index))
          _props[index].Value = value;
        else
          _props.Add(index, new BSonProp() { Name = index, Value = value });
      }
    }

    public bool HasProperty(string index)
    {
      return _props.ContainsKey(index);
    }

    public string[] Properties { get { return _props.Keys.ToArray(); } }

    public BSonDocumentType DocType { get; private set; }

    public BSonDoc(BSonDocumentType doctype = BSonDocumentType.BSON_Document)
    {
      DocType = doctype;
    }

    public int GetLength()
    {
      return GetBytes().Length;
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
      writer.Write((byte)DocType); // Write document Type
      writer.Write(Name??string.Empty);
      var position = writer.BaseStream.Position;
      writer.Write((int)0); // write initial size

      foreach (var kvp in _props)
        kvp.Value.GetBytes(writer);

      var len = (int)(writer.BaseStream.Length - position -4);
      writer.BaseStream.Position = position;
      writer.Write((int)len);
      writer.BaseStream.Seek(0, SeekOrigin.End);
    }

    public void SetBytes(byte[] data)
    {
      BinaryReader br = new BinaryReader(new MemoryStream(data));
      int len = br.ReadInt32();
    }

    public string Name { get; set; }
    public object Value { get { return this; } set { } }

  }
}
