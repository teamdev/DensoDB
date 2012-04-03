using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DeNSo.Core.DiskIO
{
  public class EventCommand
  {
    public long CommandSN
    { get; set; }

    public byte[] Command { get; set; }
  }
}
