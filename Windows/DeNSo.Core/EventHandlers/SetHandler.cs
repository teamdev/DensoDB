using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using DeNSo;
using DeNSo.BSon;

namespace DeNSo.EventHandlers
{
  [HandlesCommand(DensoBuiltinCommands.Set)]
  [Export(typeof(ICommandHandler))]
  public class SetHandler : BaseCommandHandler
  {
    public override void OnHandle(IStore store,
                                  string collection,
                                  BSonDoc command,
                                  BSonDoc document)
    {
      IObjectStore st = store.GetCollection(collection);
      st.Set(document);
    }
  }
}
