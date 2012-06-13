using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using DeNSo.Meta;
using DeNSo.Meta.BSon;

namespace DeNSo.Core.EventHandlers
{
  [HandlesCommand(DensoBuiltinCommands.DeleteMany)]
  [Export(typeof(ICommandHandler))]
  public class DeleteManyHandler : BaseCommandHandler
  {
    public override void OnHandle(IStore store,
                                  string collection,
                                  BSonDoc command,
                                  BSonDoc document)
    {
      if (document == null || string.IsNullOrEmpty(collection)) return;
      IObjectStore st = store.GetCollection(collection);

      if (document.DocType == BSonDocumentType.BSON_DocumentArray || 
          document.DocType == BSonDocumentType.BSON_Dictionary)
      {
        var documents = document.ToList();
        if (documents != null)
          foreach (var d in documents)
          { 
            //if(d is BSonDoc && ((BSonDoc)d).HasProperty(CommandKeyword.Id))



          }
      }
    }
  }
}
