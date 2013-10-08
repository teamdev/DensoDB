using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
#endif

namespace DeNSo
{
  public static class Configuration
  {
    public static string BasePath { get; set; }
    public static string IndexBasePath { get; set; }
    public static TimeSpan SaveInterval { get; set; }
    public static TimeSpan DBCheckTimeSpan { get; set; }
    public static TimeSpan ReindexCheck { get; set; }
    public static int DictionarySplitSize { get; set; }

    public static bool EnableJournaling { get; set; }
    public static bool EnableOperationsLog { get; set; }

    public static bool EnsureAtomicWrites { get; set; }

    private static DensoExtensions _extensions = new DensoExtensions();
    public static DensoExtensions Extensions { get { return _extensions; } }

    public static Guid NodeIdentity { get; private set; }

    static Configuration()
    {
#if WINDOWS_PHONE
      BasePath = "DeNSo";
#else
      BasePath = "DeNSo";
      try
      {
        BasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeNSo");
      }
      catch (Exception ex)
      {

      }
#endif
      IndexBasePath = BasePath;


      SaveInterval = new TimeSpan(0, 5, 0);
      ReindexCheck = new TimeSpan(0, 0, 10);
      DBCheckTimeSpan = new TimeSpan(0, 0, 10);
      DictionarySplitSize = 20000;
      EnableJournaling = true;
      EnsureAtomicWrites = false;
    }

#if WINDOWS_PHONE
    private static bool FileExists(string path)
    {
      using (IsolatedStorageFile iss = IsolatedStorageFile.GetUserStoreForApplication())
        return iss.FileExists(path);
    }

    private static bool DirectoryExists(string path)
    {
      using (IsolatedStorageFile iss = IsolatedStorageFile.GetUserStoreForApplication())
        return iss.DirectoryExists(path);
    }

    private static void DirectoryCreate(string path)
    {
      using (IsolatedStorageFile iss = IsolatedStorageFile.GetUserStoreForApplication())
        iss.CreateDirectory(path);
    }

    // In windows Phone we does not  have this method in File class
    private static byte[] ReadAllBytes(string path)
    {
      using (IsolatedStorageFile iss = IsolatedStorageFile.GetUserStoreForApplication())
      using (var file = iss.OpenFile(path, FileMode.Open))
      {
        var buffer = new byte[file.Length];
        file.Read(buffer, 0, (int)file.Length);
        return buffer;
      }
    }

    // In windows Phone we does not  have this method in File class
    private static void WriteAllBytes(string path, byte[] data)
    {
      using (IsolatedStorageFile iss = IsolatedStorageFile.GetUserStoreForApplication())
      using (var file = iss.OpenFile(path, FileMode.Create))
      {
        file.Write(data, 0, data.Length);
      }
    }
#endif
  }
}
