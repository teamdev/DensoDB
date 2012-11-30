using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using DeNSo.Meta;
using DeNSo.Meta.BSon;

namespace DeNSo.Core.EventHandlers
{
  [HandlesCommand(DensoBuiltinCommands.Update)]
  [Export(typeof(ICommandHandler))]
  public class UpdateHandler : BaseCommandHandler
  {
    public override void OnHandle(IStore store,
                                  string collection,
                                  BSonDoc command,
                                  BSonDoc document)
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

      st.Set(document);
    }

    private static void UpdateSingleDocument(BSonDoc document, IObjectStore store)
    {
      var obj = store.GetById((byte[])document[DocumentMetadata.IdPropertyName]);
      BSonDoc val = GetValue(document);
      foreach (var p in GetRealProperties(val)) // remove properties starting with  
        if (document.HasProperty(p))
          obj[p] = val[p];

      store.Set(obj);
    }

    private static void UpdateCollection(BSonDoc document, IObjectStore store)
    {
      // TODO: completely to do. 
    }
  }
}
