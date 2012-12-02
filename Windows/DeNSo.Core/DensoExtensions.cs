using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Diagnostics;
using DeNSo;

namespace DeNSo
{
  /// <summary>
  /// DensoExtensions use MEF to load extensions in densodb
  /// </summary>
  public class DensoExtensions
  {
    private AggregateCatalog catalog = new AggregateCatalog();

    [ImportMany(typeof(IExtensionPlugin))]
    internal IExtensionPlugin[] Extensions { get; set; }

    [ImportMany(typeof(ICommandHandler))]
    internal ICommandHandler[] ImportedHandlers { get; set; }

    public DensoExtensions()
    {
      catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
    }

    public void RegisterExtensionAssembly(Assembly assembly)
    {
      catalog.Catalogs.Add(new AssemblyCatalog(assembly));
    }

    public void Init()
    {
#if WINDOWS
      AddDirectoryCatalog(catalog, "Extensions");
      AddDirectoryCatalog(catalog, "EventHandlers");
#endif

      CompositionContainer container = new CompositionContainer(catalog);
      container.ComposeParts(this);

      if (Extensions != null)
        foreach (var plugin in Extensions)
        {
          plugin.Init();
        }

      EventHandlerManager.AnalyzeCommandHandlers(ImportedHandlers);
    }

#if WINDOWS
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
        LogWriter.LogMessage("Error occurred while composing Denso Extensions", LogEntryType.Error);
        LogWriter.LogException(ex);
      }
    }
#endif
  }
}
