using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Meta;
using System.Reflection;
using System.Linq.Expressions;
using DeNSo.Meta.BSon;

namespace DeNSo.Core
{
  public static class DefaultCommandHandler
  {
    private delegate void InternalMethod(BSonDoc document, string collection, IStore store);

    public static void Update(IStore store, BSonDoc command)
    {
      InternalExecute(store, command, new InternalMethod(InternalUpdate));
    }

    public static void Set(IStore store, BSonDoc command)
    {
      InternalExecute(store, command, new InternalMethod(InternalSet));
    }

    public static void Insert(IStore store, BSonDoc command)
    {
      InternalExecute(store, command, new InternalMethod(InternalSet));
    }

    public static void Delete(IStore store, BSonDoc command)
    {
      InternalExecute(store, command, new InternalMethod(InternalDelete));
    }

    public static void Flush(IStore store, BSonDoc command)
    {
      InternalExecute(store, command, new InternalMethod(InternalFlush));
    }

    private static void InternalExecute(IStore store, BSonDoc command, InternalMethod method)
    {
      if (command.HasProperty("_value"))
      {
        var value = command["_value"] as BSonDoc;
        var collection = (command["_collection"] ?? string.Empty).ToString();

        method(value, collection, store);
      }
    }

    private static Type GetDocumentType(BSonDoc command)
    {
      string result = null;
      if (command.HasProperty("_type_"))
        result = command["_type_"].ToString();
      else
        if (command.HasProperty("_value"))
        {
          var value = command["_value"] as BSonDoc;
          if (value != null && value.HasProperty("_type_"))
            result = value["_type_"].ToString();

        }
      if (!string.IsNullOrEmpty(result))
        return Type.GetType(result, false);
      return null;
    }

    private static BSonDoc GetValue(BSonDoc document)
    {
      if (document.HasProperty("_value"))
        return document["_value"] as BSonDoc;
      return document;
    }

    private static string[] GetRealProperties(this BSonDoc document)
    {
      string[] invalidproperties = new string[] { "_collection", "_action", "_value", "_id", "_filter", "_type", 
                                                  Configuration.DensoIDKeyName, Configuration.DensoTSKeyName };
      return document.Properties.Except(invalidproperties).ToArray();
    }

    private static void InternalUpdate(BSonDoc document, string collection, IStore store)
    {
      IObjectStore st = store.GetCollection(collection);

      if (document.HasProperty(Configuration.DensoIDKeyName))
      {
        UpdateSingleDocument(document, st); return;
      }

      if (document.HasProperty("_filter"))
      {
        UpdateCollection(document, st); return;
      }

      InsertElement(document, st); return;
    }

    private static void InternalSet(BSonDoc document, string collection, IStore store)
    {
      IObjectStore st = store.GetCollection(collection);

      if (document.HasProperty(Configuration.DensoIDKeyName))
      {
        ReplaceSingleDocument(document, st); return;
      }

      if (document.HasProperty("_filter"))
      {
        UpdateCollection(document, st); return;
      }

      InsertElement(document, st); return;
    }

    private static void InternalDelete(BSonDoc document, string collection, IStore store)
    {
      IObjectStore st = store.GetCollection(collection);
      if (document.HasProperty(Configuration.DensoIDKeyName))
      {
        var ent = st.GetById((byte[])document[Configuration.DensoIDKeyName]);
        if (ent != null)
          st.Remove(ent);
      }
    }

    private static void InternalFlush(BSonDoc document, string collection, IStore store)
    {
      IObjectStore st = store.GetCollection(collection);
      st.Flush();
    }

    private static void UpdateSingleDocument(BSonDoc document, IObjectStore store)
    {
      var obj = store.GetById((byte[])document[Configuration.DensoIDKeyName]);
      BSonDoc val = GetValue(document);
      foreach (var p in val.GetRealProperties()) // remove properties starting with  
        if (document.HasProperty(p))
          obj[p] = val[p];

      store.Set(obj);
    }

    private static void ReplaceSingleDocument(BSonDoc document, IObjectStore store)
    {
      BSonDoc val = GetValue(document);
      store.Set(val);
    }

    private static void UpdateCollection(BSonDoc document, IObjectStore store)
    {

    }

    private static void InsertElement(BSonDoc document, IObjectStore store)
    {
      BSonDoc val = GetValue(document);
      store.Set(val);
    }
  }
}
