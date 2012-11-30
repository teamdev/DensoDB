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

      OnHandle(store, collection, command, value);
      
    }

    public abstract void OnHandle(IStore store, 
                             string collection , 
                             BSonDoc command, 
                             BSonDoc document);

    protected static BSonDoc GetValue(BSonDoc document)
    {
      if (document.HasProperty(CommandKeyword.Value))
        return document[CommandKeyword.Value] as BSonDoc;
      return document;
    }

    protected static string[] GetRealProperties(BSonDoc document)
    {
      string[] invalidproperties = new string[] { CommandKeyword.Action, 
                                                  CommandKeyword.Collection, 
                                                  CommandKeyword.Filter, 
                                                  CommandKeyword.Id, 
                                                  CommandKeyword.Type, 
                                                  CommandKeyword.Value, 
                                                  DocumentMetadata.IdPropertyName, 
                                                  DocumentMetadata.TimeStampPropertyName };
      return document.Properties.Except(invalidproperties).ToArray();
    }
  }
}
