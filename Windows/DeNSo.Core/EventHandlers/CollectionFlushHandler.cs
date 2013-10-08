using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using DeNSo;
using Newtonsoft.Json.Linq;

namespace DeNSo.EventHandlers
{
  [HandlesCommand(DensoBuiltinCommands.CollectionFlush)]
  [Export(typeof(ICommandHandler))]
  public class CollectionFlushHandler : ICommandHandler
  {
    public void HandleCommand(IStore store, JObject command)
    {
      var r = command.Property(CommandKeyword.Collection);
      if (r != null)
      {
        var cc = store.GetCollection(((string)r ?? string.Empty).ToString());
        if (cc != null)
          cc.Flush();
      }
    }
  }
}
