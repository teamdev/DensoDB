using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Studio.Meta;

namespace DeNSo.Studio.Management.Messages
{
  public class OpenDB : IUIMessage
  {
    public string ConfigName { get; set; }
    public string DBUri { get; set; }
  }
}
