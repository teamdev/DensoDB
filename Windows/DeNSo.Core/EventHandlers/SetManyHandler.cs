using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using DeNSo;

using Newtonsoft.Json.Linq;

namespace DeNSo.EventHandlers
{
  [HandlesCommand(DensoBuiltinCommands.SetMany)]
  [Export(typeof(ICommandHandler))]
  public class SetManyHandler : BaseCommandHandler
  {
    public override void OnHandle(IStore store,
                                  string collection,
                                  JObject command,
                                  JObject document)
    {
      IObjectStore st = store.GetCollection(collection);

      if (document.Type == JTokenType.Array)
      {
        var documents = document.Values();
        if (documents != null)
          foreach (JObject d in documents)
          {
            var k = d.Property(DocumentMetadata.IdPropertyName);
            if (k != null)
              st.Set((string)k, d);
          }

      }
      else
      {
        var k = document.Property(DocumentMetadata.IdPropertyName);
        if (k != null)
          st.Set((string)k, document);
      }
    }
  }
}
