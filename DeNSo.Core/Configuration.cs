using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Core
{
  public static class Configuration
  {
    public static string BasePath { get; set; }
    public static TimeSpan SaveInterval { get; set; }
    public static TimeSpan DBCheckTimeSpan { get; set; }
    public static int DictionarySplitSize { get; set; }
    public static bool EnableJournaling { get; set; }

    static Configuration()
    {
      BasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeNSo");

      SaveInterval = new TimeSpan(0, 5, 0);
      DBCheckTimeSpan = new TimeSpan(0, 0, 10);
      DictionarySplitSize = 20000;
      EnableJournaling = true;
    }
  }
}
