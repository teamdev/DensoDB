using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using DeNSo;

using Newtonsoft.Json.Linq;

namespace DeNSo.EventHandlers
{
  [HandlesCommand(DensoBuiltinCommands.Set)]
  [Export(typeof(ICommandHandler))]
  public class SetHandler : BaseCommandHandler
  {
    public override void OnHandle(IStore store,
                                  string collection,
                                  JObject command,
                                  JObject document)
    {
      IObjectStore st = store.GetCollection(collection);
      if (document != null)
      {
        JToken r = document.Property(CommandKeyword.Id);
        st.Set((string)r, document);
        return;
      }
    }
  }
}
