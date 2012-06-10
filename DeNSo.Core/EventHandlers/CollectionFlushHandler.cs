using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Meta;

namespace DeNSo.Core.EventHandlers
{
  [HandlesCommand(DensoBuiltinCommands.CollectionFlush)]
  public class CollectionFlushHandler : ICommandHandler
  {
    public void HandleCommand(IStore store, Meta.BSon.BSonDoc command)
    {
      throw new NotImplementedException();
    }
  }
}
