using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.BSon;

namespace DeNSo
{
  public interface ICommandHandler
  {
    void HandleCommand(IStore store, BSonDoc command);
  }
}
