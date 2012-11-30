using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Meta;
using System.ComponentModel.Composition;

namespace DeNSo.P2P
{
  [Export(typeof(IExtensionPlugin))]
  public class InitExtension : IExtensionPlugin
  {
    public void Init()
    {
      EventP2PDispatcher.EnableP2PEventMesh();
      DeNSo.Core.Configuration.EnableOperationsLog = true;
    }
  }
}
