using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DeNSo.Core
{
  public static class Configuration
  {
    public static string BasePath { get; set; }
    public static TimeSpan SaveInterval { get; set; }
    public static TimeSpan DBCheckTimeSpan { get; set; }
    public static int DictionarySplitSize { get; set; }
    public static bool EnableJournaling { get; set; }

    private static DensoExtensions _extensions = new DensoExtensions();
    public static DensoExtensions Extensions { get { return _extensions; } }

 
    public static Guid NodeIdentity { get; private set; }

    static Configuration()
    {
      BasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeNSo");

      if (File.Exists(Path.Combine(BasePath, "d.cfg")))
      {
        NodeIdentity = new Guid(File.ReadAllBytes(Path.Combine(BasePath, "d.cfg")));
      }
      else
      {
        NodeIdentity = Guid.NewGuid();
      }

      if (!Directory.Exists(BasePath))
        Directory.CreateDirectory(BasePath);

      File.WriteAllBytes(Path.Combine(BasePath, "d.cfg"), NodeIdentity.ToByteArray());

      SaveInterval = new TimeSpan(0, 5, 0);
      DBCheckTimeSpan = new TimeSpan(0, 0, 10);
      DictionarySplitSize = 20000;
      EnableJournaling = true;
    }
  }
}
