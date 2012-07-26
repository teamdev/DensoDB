using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using DeNSo.Meta;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Diagnostics;

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
      AggregateCatalog catalog = new AggregateCatalog();
      catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

      AddDirectoryCatalog(catalog, "Extensions");
      AddDirectoryCatalog(catalog, "EventHandlers");

      CompositionContainer container = new CompositionContainer(catalog);
      container.ComposeParts(this);


      if (Extensions != null)
        foreach (var plugin in Extensions)
        {
          plugin.Init();
        }

      EventHandlerManager.AnalyzeCommandHandlers(ImportedHandlers);
    }

    private static void AddDirectoryCatalog(AggregateCatalog catalog, string directoryname)
    {
      try
      {
        var c1 = new DirectoryCatalog(directoryname);
        c1.Refresh();
        catalog.Catalogs.Add(c1);
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
        LogWriter.LogMessage("Error occurred while composing Denso Extensions", System.Diagnostics.EventLogEntryType.Error);
        LogWriter.LogException(ex);
      }
    }
  }
}
