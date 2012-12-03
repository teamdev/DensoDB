using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo;
using DeNSo.BSon;
using System.Diagnostics;
using System.ComponentModel.Composition;

namespace DeNSo.EventHandlers
{
  [HandlesCommand(DensoBuiltinCommands.Delete)]
  [Export(typeof(ICommandHandler))]
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
        var ent = st.GetById((string)command[CommandKeyword.Id]);
        if (ent != null) st.Remove(ent);
      }

      if (document != null && document.HasProperty(DocumentMetadata.IdPropertyName))
      {
        var ent = st.GetById((string)document[DocumentMetadata.IdPropertyName]);
        if (ent != null) st.Remove(ent);
      }
    }
  }
}
