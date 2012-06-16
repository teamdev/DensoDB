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
    internal IExtensionPlugin[] Extensions { get; set; }

    [ImportMany(AllowRecomposition = true)]
    internal ICommandHandler[] ImportedHandlers { get; set; }

    public void Init()
    {
      try
      {
        AggregateCatalog catalog = new AggregateCatalog();

        var c1 = new DirectoryCatalog("Extensions");
        c1.Refresh();
        var c2 = new DirectoryCatalog("EventHandlers");
        c2.Refresh();
        var c3 = new AssemblyCatalog(Assembly.GetExecutingAssembly());

        catalog.Catalogs.Add(c1);
        catalog.Catalogs.Add(c2);
        catalog.Catalogs.Add(c3);

        CompositionContainer container = new CompositionContainer(catalog);
        container.ComposeParts(this);
      }
      catch (Exception ex)
      {
<<<<<<< HEAD
        LogWriter.LogMessage("Error occurred while composing Denso Extensions", System.Diagnostics.EventLogEntryType.Error);
        LogWriter.LogException(ex);
=======
        WindowsLogWriter.LogMessage("Error occurred while composing Denso Extensions", System.Diagnostics.EventLogEntryType.Error);
        WindowsLogWriter.LogException(ex);
>>>>>>> adc13efc0c52db8701845b1cf36c7d124128966d
      }

      foreach (var plugin in Extensions)
      {
        plugin.Init();
      }

      EventHandlerManager.AnalyzeCommandHandlers(ImportedHandlers);
    }
  }
}
