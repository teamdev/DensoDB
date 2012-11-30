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
    public static string IndexBasePath { get; set; }
    public static TimeSpan SaveInterval { get; set; }
    public static TimeSpan DBCheckTimeSpan { get; set; }
    public static int DictionarySplitSize { get; set; }

    public static bool EnableJournaling { get; set; }
    public static bool EnableOperationsLog { get; set; }

    private static DensoExtensions _extensions = new DensoExtensions();
    public static DensoExtensions Extensions { get { return _extensions; } }

    public static Guid NodeIdentity { get; private set; }

    static Configuration()
    {
      BasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeNSo");
      IndexBasePath = BasePath;

      if (File.Exists(Path.Combine(BasePath, "d.cfg")))
      {
#if WINDOWS_PHONE
        NodeIdentity = new Guid(ReadAllBytes(Path.Combine(BasePath, "d.cfg")));
#else
        NodeIdentity = new Guid(File.ReadAllBytes(Path.Combine(BasePath, "d.cfg")));
#endif
      }
      else
      {
        NodeIdentity = Guid.NewGuid();
      }

      if (!Directory.Exists(BasePath))
        Directory.CreateDirectory(BasePath);

#if WINDOWS_PHONE
      WriteAllBytes(Path.Combine(BasePath, "d.cfg"), NodeIdentity.ToByteArray());
#else
      File.WriteAllBytes(Path.Combine(BasePath, "d.cfg"), NodeIdentity.ToByteArray());
#endif

      SaveInterval = new TimeSpan(0, 5, 0);
      DBCheckTimeSpan = new TimeSpan(0, 0, 10);
      DictionarySplitSize = 20000;
      EnableJournaling = true;
    }

#if WINDOWS_PHONE
    // In windows Phone we does not  have this method in File class
    private static byte[] ReadAllBytes(string path)
    {
      using (var file = File.OpenRead(path))
      {
        var buffer = new byte[file.Length];
        file.Read(buffer, 0, (int)file.Length);
        return buffer;
      }
    }

    // In windows Phone we does not  have this method in File class
    private static void WriteAllBytes(string path, byte[] data)
    {
      using (var file = File.OpenWrite(path))
      {
        file.Write(data, 0, data.Length);
      }
    }
#endif
  }
}
