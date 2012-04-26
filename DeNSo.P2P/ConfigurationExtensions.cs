using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.P2P
{
  public static class ConfigurationExtensions
  {
    private static P2PConfiguration _configuration = new P2PConfiguration();
    public static P2PConfiguration P2P(this DeNSo.Core.DensoExtensions extensions)
    {
      return _configuration;
    }
  }

  public class P2PConfiguration
  {
    public string NetworkPassword { get; set; }
    public string ServiceUri { get; set; }
    public int NetworkPort { get; set; }
    public int MaxHop { get; set; }


    public P2PConfiguration()
    {
      var rnd = new Random();
      ServiceUri = "net.p2p://mesh.denso.net";
      NetworkPassword = "DensoTest";
      NetworkPort = 8090 + rnd.Next(1000);
      MaxHop = 5;
    }
  }
}
