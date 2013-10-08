using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json.Linq;

namespace DeNSo
{
  public interface ICommandHandler
  {
    void HandleCommand(IStore store, JObject command);
  }
}
