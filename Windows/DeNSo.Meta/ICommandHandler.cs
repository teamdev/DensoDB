using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Meta.BSon;

namespace DeNSo.Meta
{
  public interface ICommandHandler
  {
    void HandleCommand(IStore store, BSonDoc command);
  }
}
