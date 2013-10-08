using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo;

using System.Diagnostics;
using System.ComponentModel.Composition;
using Newtonsoft.Json.Linq;

namespace DeNSo.EventHandlers
{
  [HandlesCommand(DensoBuiltinCommands.Delete)]
  [Export(typeof(ICommandHandler))]
  public class DeleteHandler : BaseCommandHandler
  {
    public override void OnHandle(IStore store,
                                  string collection,
                                  JObject command,
                                  JObject document)
    {
      IObjectStore st = store.GetCollection(collection);

      JToken r = command.Property(CommandKeyword.Id);
      if (r != null)
      {
        st.Remove((string)r);
        return;
      }

      if (document != null)
      {
        JToken r2 = document.Property(DocumentMetadata.IdPropertyName);
        if (r2 != null)
        {
          st.Remove((string)r2);
        }
      }
    }
  }
}
