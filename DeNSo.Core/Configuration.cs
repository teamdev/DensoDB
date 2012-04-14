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

    public static string PeerNetworkPassword { get; set; }
    public static string PeerService { get; set; }
    public static int PeerNetworkPort { get; set; }

    public const string DensoIDKeyName = "_id";
    public const string DensoTSKeyName = "@ts#";

    public static Guid NodeIdentity { get; private set; }

    static Configuration()
    {
      var rnd = new Random();
      BasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeNSo");

      if (File.Exists(Path.Combine(BasePath, "d.cfg")))
      {
        NodeIdentity = new Guid(File.ReadAllBytes(Path.Combine(BasePath, "d.cfg")));
      }
      else
      {
        NodeIdentity = Guid.NewGuid();
      }

      File.WriteAllBytes(Path.Combine(BasePath, "d.cfg"), NodeIdentity.ToByteArray());

      PeerService = "net.p2p://mesh.denso.net";
      PeerNetworkPassword = "DensoTest";
      PeerNetworkPort = 8090 + rnd.Next(1000);

      SaveInterval = new TimeSpan(0, 5, 0);
      DBCheckTimeSpan = new TimeSpan(0, 0, 10);
      DictionarySplitSize = 20000;
      EnableJournaling = true;
    }
  }
}
