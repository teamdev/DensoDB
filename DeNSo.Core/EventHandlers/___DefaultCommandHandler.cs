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
      BSonDoc value = null;
      var collection = string.Empty;

      if (command.HasProperty(CommandKeyword.Value))
      {
        value = command[CommandKeyword.Value] as BSonDoc;
      }
      if (command.HasProperty(CommandKeyword.Collection))
      {
        collection = (command[CommandKeyword.Collection] ?? string.Empty).ToString();
      }

      method(value, collection, store);
    }

    private static Type GetDocumentType(BSonDoc command)
    {
      string result = null;
      if (command.HasProperty(CommandKeyword.Type))
        result = command[CommandKeyword.Type].ToString();
      else
        if (command.HasProperty(CommandKeyword.Value))
        {
          var value = command[CommandKeyword.Value] as BSonDoc;
          if (value != null && value.HasProperty(CommandKeyword.Type))
            result = value[CommandKeyword.Type].ToString();

        }
      if (!string.IsNullOrEmpty(result))
        return Type.GetType(result, false);
      return null;
    }

    private static BSonDoc GetValue(BSonDoc document)
    {
      if (document.HasProperty(CommandKeyword.Value))
        return document[CommandKeyword.Value] as BSonDoc;
      return document;
    }

    private static string[] GetRealProperties(this BSonDoc document)
    {
      string[] invalidproperties = new string[] { CommandKeyword.Action, 
                                                  CommandKeyword.Collection, 
                                                  CommandKeyword.Filter, 
                                                  CommandKeyword.Id, 
                                                  CommandKeyword.Type, 
                                                  CommandKeyword.Value, 
                                                  DocumentMetadata.IdPropertyName, 
                                                  DocumentMetadata.TimeStampPropertyName };
      return document.Properties.Except(invalidproperties).ToArray();
    }

    private static void InternalUpdate(BSonDoc document, string collection, IStore store)
    {
      if (document == null || string.IsNullOrEmpty(collection)) return;
      IObjectStore st = store.GetCollection(collection);

      if (document.HasProperty(DocumentMetadata.IdPropertyName))
      {
        UpdateSingleDocument(document, st); return;
      }

      if (document.HasProperty(CommandKeyword.Filter))
      {
        UpdateCollection(document, st); return;
      }

      InsertElement(document, st); return;
    }

    private static void InternalSet(BSonDoc document, string collection, IStore store)
    {
      if (document == null || string.IsNullOrEmpty(collection)) return;
      IObjectStore st = store.GetCollection(collection);

      if (document.HasProperty(DocumentMetadata.IdPropertyName))
      {
        ReplaceSingleDocument(document, st); return;
      }

      if (document.HasProperty(CommandKeyword.Filter))
      {
        UpdateCollection(document, st); return;
      }

      InsertElement(document, st); return;
    }

    private static void InternalDelete(BSonDoc document, string collection, IStore store)
    {
      if (document == null || string.IsNullOrEmpty(collection)) return;
      IObjectStore st = store.GetCollection(collection);
      if (document.HasProperty(DocumentMetadata.IdPropertyName))
      {
        var ent = st.GetById((byte[])document[DocumentMetadata.IdPropertyName]);
        if (ent != null)
          st.Remove(ent);
      }
    }

    private static void InternalFlush(BSonDoc document, string collection, IStore store)
    {
      if (string.IsNullOrEmpty(collection)) return;
      IObjectStore st = store.GetCollection(collection);
      st.Flush();
    }

    private static void UpdateSingleDocument(BSonDoc document, IObjectStore store)
    {
      var obj = store.GetById((byte[])document[DocumentMetadata.IdPropertyName]);
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
