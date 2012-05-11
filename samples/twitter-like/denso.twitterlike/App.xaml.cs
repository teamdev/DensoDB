using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using DeNSo.Core;
using DeNSo.P2P;
using System.Net.PeerToPeer;

namespace denso.twitterlike
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      Session.DefaultDataBase = "twitterlike" ;
      DeNSo.Core.Configuration.BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "densosamples/" + System.Diagnostics.Process.GetCurrentProcess().Id);

      if (!Directory.Exists(DeNSo.Core.Configuration.BasePath))
        Directory.CreateDirectory(DeNSo.Core.Configuration.BasePath);

      EventP2PDispatcher.EnableP2PEventMesh();
      EventP2PDispatcher.MakeNodeAvaiableToPNRP(Cloud.Available);

    }

    protected override void OnExit(ExitEventArgs e)
    {
      EventP2PDispatcher.StopP2PEventMesh();
      Session.ShutDown();
      base.OnExit(e);
      
    }

  }
}
