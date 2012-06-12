using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using DeNSo.Meta;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace DeNSo.Core
{
  /// <summary>
  /// DensoExtensions use MEF to load extensions in densodb
  /// </summary>
  public class DensoExtensions
  {
    [ImportMany(AllowRecomposition = true)]
    public IExtensionPlugin[] Extensions { get; set; }

    public void Init()
    {
      DirectoryCatalog ac = new DirectoryCatalog("Extensions");
      ac.Refresh();
      CompositionContainer container = new CompositionContainer(ac);
      container.ComposeParts(this);
    }
  }
}
