using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using System.Reflection;

namespace DeNSo.Meta.BSon
{
  public static class BSonSerializer
  {
    private static MethodInfo _IEnumToBSonG;
    private static MethodInfo _IEnumToBSon;
    private static MethodInfo _IDictToBSonG;
    private static MethodInfo _IDictToBSon;

    static BSonSerializer()
    {
      var t = typeof(BSonSerializer);
      _IEnumToBSonG = t.GetMethod("IEnumerableToBSonG", BindingFlags.Static | BindingFlags.NonPublic);
      _IDictToBSonG = t.GetMethod("IDictionaryToBSonG", BindingFlags.Static | BindingFlags.NonPublic);
      _IEnumToBSon = t.GetMethod("IEnumerableToBSon", BindingFlags.Static | BindingFlags.NonPublic);
      _IDictToBSon = t.GetMethod("IDictionaryToBSon", BindingFlags.Static | BindingFlags.NonPublic);
    }

    public static byte[] Serialize(this BSonDoc document)
    {
      MemoryStream ms = new MemoryStream();
      BinaryWriter writer = new BinaryWriter(ms);
      document.GetBytes(writer);
      return ms.ToArray();
    }

    public static BSonDoc Deserialize(this byte[] data)
    {
      MemoryStream ms = new MemoryStream(data);
      BinaryReader br = new BinaryReader(ms);

      BSonDoc result = NavigateStream(br) as BSonDoc;
      return result;
    }

    public static BSonDoc ToBSon(this object entity)
    {
      var tt = entity.GetType();
      if (entity is IEnumerable)
      {
        if (tt.IsGenericType)
        {
          var gmi = _IEnumToBSonG.MakeGenericMethod(tt.GetGenericArguments());
          return gmi.Invoke(null, new object[] { entity }) as BSonDoc;
        }
        else
        {
          return _IEnumToBSon.Invoke(null, new object[] { entity }) as BSonDoc;
        }
      }

      if (entity is IDictionary)
      {
        if (tt.IsGenericType)
        {
          var gmi = _IDictToBSonG.MakeGenericMethod(tt.GetGenericArguments());
          return gmi.Invoke(null, new object[] { entity }) as BSonDoc;
        }
        else
        {
          return _IDictToBSon.Invoke(null, new object[] { entity }) as BSonDoc;
        }
      }

      return entity.DocumentToBSon();
    }

    public static T FromBSon<T>(this BSonDoc doc) where T : class, new()
    {
      var entity = new T();
      var tt = typeof(T);

      var properties = tt.GetProperties();
      foreach (var p in properties)
        if (doc.HasProperty(p.Name))
          entity.FastSet(p.Name, doc[p.Name]);

      return entity;
    }

    private static BSonDoc DocumentToBSon<T>(this T entity, bool includetype = true) where T : class
    {
      if (entity is BSonDoc) return entity as BSonDoc;

      BSonDoc doc = new BSonDoc();
      var tt = entity.GetType();

      //if (includetype)
      //  doc["_type_"] = tt.FullName;

      var properties = tt.GetProperties();
      foreach (var p in properties)
        doc[p.Name] = entity.NavigateProperty<T>(p.Name);
      return doc;
    }

    private static BSonDoc IEnumerableToBSonG<T>(this IEnumerable<T> entities) where T : class
    {
      BSonDoc doc = new BSonDoc(BSonDocumentType.BSON_DocumentArray);
      var tt = typeof(T);
      //doc["_type_"] = tt.FullName;

      int x = 0;
      foreach (var item in entities)
      {
        doc[string.Format("i{0}", x++)] = item.ToBSon();
      }
      return doc;
    }

    private static BSonDoc IEnumerableToBSon(this IEnumerable entities)
    {
      BSonDoc doc = new BSonDoc(BSonDocumentType.BSON_DocumentArray);
      int x = 0;
      foreach (var item in entities)
      {
        doc[string.Format("i{0}", x++)] = item.ToBSon();
      }
      return doc;
    }

    private static BSonDoc IDictionaryToBSonG<TK, T>(this IDictionary<TK, T> entities) where T : class
    {
      BSonDoc doc = new BSonDoc(BSonDocumentType.BSON_Dictionary);
      var tt = typeof(T);
      //doc["_type_"] = tt.FullName;

      foreach (var item in entities)
      {
        doc[item.Key.ToString()] = item.Value.ToBSon();
      }
      return doc;
    }

    private static BSonDoc IDictionaryToBSon(this IDictionary entities)
    {
      BSonDoc doc = new BSonDoc(BSonDocumentType.BSON_Dictionary);
      foreach (DictionaryEntry item in entities)
      {
        doc[item.Key.ToString()] = item.Value.ToBSon();
      }
      return doc;
    }

    private static object NavigateProperty<T>(this T entity, string name) where T : class
    {
      var value = entity.FastGet(name);
      if (value == null) return null; // Return Null if Null

      var tt = value.GetType();
      if (tt.IsValueType) return value; // Return value if is Struct or Numeric

      if (value is string) return value;// Return value if is String
      if (value is Guid) return value; // Return value if is Guid
      if (value is DateTime) return value; // Return value if is DateTime
      if (value is byte[]) return value; // return value if is byte[]
      if (value is IEnumerable) return value.ToBSon();
      if (value is IDictionary) return value.ToBSon();

      return value.ToBSon();
    }

    private static object NavigateStream(this BinaryReader reader)
    {
      byte check = reader.ReadByte();
      BSonTypeEnum type = (BSonTypeEnum)check;

      switch (type)
      {
        case BSonTypeEnum.BSON_Document:
          return DeserializeDocument(reader, BSonDocumentType.BSON_Document);
        case BSonTypeEnum.BSON_DocumentArray:
          return DeserializeDocument(reader, BSonDocumentType.BSON_DocumentArray);
        case BSonTypeEnum.BSON_Dictionary:
          return DeserializeDocument(reader, BSonDocumentType.BSON_Dictionary);
        case BSonTypeEnum.BSON_Null:
        case BSonTypeEnum.BSON_Bool:
        case BSonTypeEnum.BSON_Byte:
        case BSonTypeEnum.BSON_int16:
        case BSonTypeEnum.BSON_int32:
        case BSonTypeEnum.BSON_int64:
        case BSonTypeEnum.BSON_single:
        case BSonTypeEnum.BSON_double:
        case BSonTypeEnum.BSON_decimal:
        case BSonTypeEnum.BSON_GUID:
        case BSonTypeEnum.BSON_bynary:
        case BSonTypeEnum.BSON_chararray:
        case BSonTypeEnum.BSON_DateTime:
        case BSonTypeEnum.BSON_string:
        case BSonTypeEnum.BSON_ObjectId:
        case BSonTypeEnum.BSON_timestamp:
        case BSonTypeEnum.BSON_objectType:
        default:
          return DeserializeProperty(reader, type);
      }
    }

    private static BSonDoc DeserializeDocument(BinaryReader reader, BSonDocumentType doctype)
    {
      var doc = new BSonDoc(doctype);
      doc.Name = reader.ReadString();
      var size = reader.ReadInt32();
      var endposition = reader.BaseStream.Position + size;

      while (reader.BaseStream.Position < endposition)
      {
        var res = NavigateStream(reader);

        var pp = res as IBSonNode;
        if (pp != null)
        {
          doc[pp.Name] = pp.Value;
        }
      }

      return doc;
    }

    private static BSonProp DeserializeProperty(BinaryReader reader, BSonTypeEnum type)
    {
      string name = reader.ReadString();
      return new BSonProp() { Name = name, Value = ReadCorrectType(type, reader) };
    }

    internal static BSonTypeEnum GetBSonType(this object value)
    {
      if (value == null) return BSonTypeEnum.BSON_Null;
      if (value is Int32) return BSonTypeEnum.BSON_int32;
      if (value is Int64) return BSonTypeEnum.BSON_int64;
      if (value is Int16) return BSonTypeEnum.BSON_int16;
      if (value is Byte) return BSonTypeEnum.BSON_Byte;
      if (value is Boolean) return BSonTypeEnum.BSON_Bool;
      if (value is String) return BSonTypeEnum.BSON_string;
      if (value is Double) return BSonTypeEnum.BSON_double;
      if (value is Single) return BSonTypeEnum.BSON_single;
      if (value is Decimal) return BSonTypeEnum.BSON_decimal;
      if (value is Guid) return BSonTypeEnum.BSON_GUID;
      if (value is byte[]) return BSonTypeEnum.BSON_bynary;
      if (value is char[]) return BSonTypeEnum.BSON_chararray;
      if (value is DateTime) return BSonTypeEnum.BSON_DateTime;
      if (value is IEnumerable) return BSonTypeEnum.BSON_DocumentArray;
      if (value is IDictionary) return BSonTypeEnum.BSON_Dictionary;
      if (value is Type) return BSonTypeEnum.BSON_objectType;
      return BSonTypeEnum.BSON_Document;
    }

    internal static void WriteCorrectType(this object value, BinaryWriter writer, BSonTypeEnum type)
    {
      switch (type)
      {
        case BSonTypeEnum.BSON_Null: return;
        case BSonTypeEnum.BSON_Bool:
          writer.Write((bool)value); break;
        case BSonTypeEnum.BSON_Byte:
          writer.Write((byte)value); break;
        case BSonTypeEnum.BSON_int16:
          writer.Write((short)value); break;
        case BSonTypeEnum.BSON_int32:
          writer.Write((int)value); break;
        case BSonTypeEnum.BSON_int64:
          writer.Write((long)value); break;
        case BSonTypeEnum.BSON_single:
          writer.Write((Single)value); break;
        case BSonTypeEnum.BSON_double:
          writer.Write((double)value); break;
        case BSonTypeEnum.BSON_decimal:
          writer.Write((decimal)value); break;
        case BSonTypeEnum.BSON_GUID:
          writer.Write(((Guid)value).ToByteArray()); break;
        case BSonTypeEnum.BSON_bynary:
          var barr = (byte[])value; writer.Write((int)barr.Length); writer.Write(barr); break;
        case BSonTypeEnum.BSON_chararray:
          var carr = (char[])value; writer.Write((int)carr.Length); writer.Write(carr); break;
        case BSonTypeEnum.BSON_DateTime:
          writer.Write(((DateTime)value).Ticks); break; // Writes the Date in GMT/UTC format pattern
        case BSonTypeEnum.BSON_string:
          writer.Write(value.ToString()); break;
        case BSonTypeEnum.BSON_ObjectId:
          break;
        case BSonTypeEnum.BSON_timestamp:
          break;
        case BSonTypeEnum.BSON_Document:
        case BSonTypeEnum.BSON_DocumentArray:
        case BSonTypeEnum.BSON_Dictionary:
          writer.Write(((BSonDoc)value).Serialize());
          break;
        case BSonTypeEnum.BSON_objectType:
          writer.Write(((Type)value).AssemblyQualifiedName);
          break;
        default:
          break;
      }
    }

    internal static void WriteIEnumerable(this IEnumerable values, BinaryWriter writer)
    {
      bool first = true;
      var position = 0L;
      var count = 0;
      BSonTypeEnum type = BSonTypeEnum.BSON_Null;
      foreach (var item in values)
      {
        if (first)
        {
          first = false;
          type = item.GetBSonType();
          writer.Write((byte)type);   // scrivo il tipo dell'item nell'array 
          position = writer.BaseStream.Position; // salvo la posizione per scrivere la dimenzione dell'array
          writer.Write((int)0); // scrivo 0 perchè non so quato è grande l'array
          BSonSerializer.WriteCorrectType(item, writer, type); // scrivo l'item
        }
        else
        {
          BSonSerializer.WriteCorrectType(item, writer, type); // scrivo l'item
        }
        count++;
      }

      writer.BaseStream.Position = position;
      writer.Write(count);
      writer.BaseStream.Seek(0, SeekOrigin.End);
    }

    internal static object ReadCorrectType(this BSonTypeEnum type, BinaryReader reader)
    {
      switch (type)
      {
        case BSonTypeEnum.BSON_Null: return null;
        case BSonTypeEnum.BSON_Bool: return reader.ReadBoolean();
        case BSonTypeEnum.BSON_Byte: return reader.ReadByte();
        case BSonTypeEnum.BSON_int16: return reader.ReadInt16();
        case BSonTypeEnum.BSON_int32: return reader.ReadInt32();
        case BSonTypeEnum.BSON_int64: return reader.ReadInt64();
        case BSonTypeEnum.BSON_single: return reader.ReadSingle();
        case BSonTypeEnum.BSON_double: return reader.ReadDouble();
        case BSonTypeEnum.BSON_decimal: return reader.ReadDecimal();
        case BSonTypeEnum.BSON_GUID: return new Guid(reader.ReadBytes(16));
        case BSonTypeEnum.BSON_bynary:
          var alen = reader.ReadInt32(); return reader.ReadBytes(alen);
        case BSonTypeEnum.BSON_chararray:
          var clen = reader.ReadInt32(); return reader.ReadChars(clen);
        case BSonTypeEnum.BSON_DateTime: return new DateTime(reader.ReadInt64());
        case BSonTypeEnum.BSON_string: return reader.ReadString();
        case BSonTypeEnum.BSON_ObjectId:
          break;
        case BSonTypeEnum.BSON_timestamp:
          break;
        case BSonTypeEnum.BSON_Document:
          break;
        case BSonTypeEnum.BSON_DocumentArray:
          break;
        case BSonTypeEnum.BSON_Dictionary:
          break;
        case BSonTypeEnum.BSON_objectType: return Type.GetType(reader.ReadString());
        default:
          break;
      }
      return null;
    }
  }
}
