using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Meta;
using DeNSo.Meta.BSon;

namespace DeNSo.Core.EventHandlers
{
  public abstract class BaseCommandHandler: ICommandHandler
  {
    public void HandleCommand(IStore store, Meta.BSon.BSonDoc command)
      {
      BSonDoc value = null;
      var collection = string.Empty;

      if (command.HasProperty(CommandKeyword.Value))
      {
        value = command[CommandKeyword.Value] as BSonDoc;
      }
      if (command.HasProperty(CommandKeyword.Collection))
      {
        collection = (command[CommandKeyword.Collection] ?? string.Empty).ToString();
      }

      OnHandle(store, collection, value, command);
      
    }

    public abstract void OnHandle(IStore store, 
                             string collection , 
                             BSonDoc command, 
                             BSonDoc document);
  }
}
