using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Meta;
using DeNSo.Meta.BSon;
using System.Diagnostics;

namespace DeNSo.Core.EventHandlers
{
  [HandlesCommand(DensoBuiltinCommands.Delete)]
  public class DeleteHandler : BaseCommandHandler
  {
    public override void OnHandle(IStore store,
                                  string collection,
                                  BSonDoc command,
                                  BSonDoc document)
    {
      IObjectStore st = store.GetCollection(collection);

      if (command.HasProperty(CommandKeyword.Id))
      {
        var ent = st.GetById((byte[])command[CommandKeyword.Id]);
        if (ent != null) st.Remove(ent);
      }

      if (document != null && document.HasProperty(DocumentMetadata.IdPropertyName))
      {
        var ent = st.GetById((byte[])document[DocumentMetadata.IdPropertyName]);
        if (ent != null) st.Remove(ent);
      }
    }
  }
}
