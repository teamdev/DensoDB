using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using DeNSo;

using Newtonsoft.Json.Linq;

namespace DeNSo.EventHandlers
{
  [HandlesCommand(DensoBuiltinCommands.Update)]
  [Export(typeof(ICommandHandler))]
  public class UpdateHandler : BaseCommandHandler
  {
    public override void OnHandle(IStore store,
                                  string collection,
                                  JObject command,
                                  JObject document)
    {
      if (document == null || string.IsNullOrEmpty(collection)) return;
      IObjectStore st = store.GetCollection(collection);

      var r = document.Property(DocumentMetadata.IdPropertyName);
      if (r != null)
      {
        UpdateSingleDocument(document, st); return;
      }

      var r2 = document.Property(CommandKeyword.Filter);
      if (r2 != null)
      {
        UpdateCollection(document, st); return;
      }
    }

    private static void UpdateSingleDocument(JObject document, IObjectStore store)
    {
      var objid = (string)document[DocumentMetadata.IdPropertyName];
      var obj = JObject.Parse(store.GetById(objid));
      foreach (var p in GetRealProperties(document)) // remove properties starting with  
        obj[p] = document[p];

      store.Set(objid, obj);
    }

    private static void UpdateCollection(JObject document, IObjectStore store)
    {
      // TODO: completely to do. 
    }
  }
}
