using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using DeNSo;

namespace DeNSo.EventHandlers
{
  [HandlesCommand(DensoBuiltinCommands.CollectionFlush)]
  [Export(typeof(ICommandHandler))]
  public class CollectionFlushHandler : ICommandHandler
  {
    public void HandleCommand(IStore store, BSon.BSonDoc command)
    {
      if (command.HasProperty(CommandKeyword.Collection))
      {
        var cc = store.GetCollection((command[CommandKeyword.Collection] ?? string.Empty).ToString());
        if (cc != null)
          cc.Flush();
      }
    }
  }
}
