using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo;

using Newtonsoft.Json.Linq;

namespace DeNSo.EventHandlers
{
  public abstract class BaseCommandHandler : ICommandHandler
  {
    public void HandleCommand(IStore store, JObject command)
    {
      JObject value = null;
      var collection = string.Empty;

      var r = command.Property(CommandKeyword.Value);
      if (r != null)
      {
        value = r.Value as JObject;
      }

      r = command.Property(CommandKeyword.Collection);
      if (r != null)
      {
        collection = (string)r.Value;
      }

      OnHandle(store, collection, command, value);

    }

    public abstract void OnHandle(IStore store,
                             string collection,
                             JObject command,
                             JObject document);

    //protected static JToken GetValue(JObject document)
    //{
    //  if (document.HasProperty(CommandKeyword.Value))
    //    return document[CommandKeyword.Value] as BSonDoc;
    //  return document;
    //}

    protected static string[] GetRealProperties(JObject document)
    {
      string[] invalidproperties = new string[] { CommandKeyword.Action, 
                                                  CommandKeyword.Collection, 
                                                  CommandKeyword.Filter, 
                                                  CommandKeyword.Id, 
                                                  CommandKeyword.Type, 
                                                  CommandKeyword.Value, 
                                                  DocumentMetadata.IdPropertyName, 
                                                  DocumentMetadata.TimeStampPropertyName };
      return document.Properties().Select(p => p.Name).Except(invalidproperties).ToArray();
    }
  }
}
