using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using DeNSo.Core;
using System.ServiceModel.Web;
using System.ServiceModel.Description;
using System.ServiceModel;

namespace DeNSo
{
  public partial class DeNSoService : ServiceBase
  {
    private WebServiceHost host = null;

    public DeNSoService()
    {
      InitializeComponent();
    }

    public void InteractiveStart()
    {
      OnStart(null);
    }

    public void InteractiveStop()
    {
      Console.ForegroundColor = ConsoleColor.DarkRed;
      Console.WriteLine("DENSO: Stopping Service");

      OnStop();

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("DENSO: Stopped Successfully");
    }

    protected override void OnStart(string[] args)
    {

      DeNSo.Core.Configuration.BasePath = ConfigurationManager.AppSettings["DataBasePath"];
      DeNSo.Core.Configuration.EnableJournaling = ConfigurationManager.AppSettings["EnableJournaling"] == "true";
      DeNSo.Core.Configuration.EnableOperationsLog = ConfigurationManager.AppSettings["EnableOperationsLog"] == "true";
      DeNSo.Core.Configuration.IndexBasePath = ConfigurationManager.AppSettings["IndexBasePath"];

      Session.Start();

      StartSelfHostedRest();
    
    
    }

    protected override void OnStop()
    {
      if (host != null)
        try
        {
          host.Close();
        }
        catch { }

      Session.ShutDown();
    }

    public void StartSelfHostedRest()
    {
      // Problems with Design times access rights
      // http://go.microsoft.com/fwlink/?LinkId=70353

      host = new WebServiceHost(typeof(restservice));
      //ServiceEndpoint ep = host.AddServiceEndpoint(typeof(restservice), new WebHttpBinding(), "");
      ServiceDebugBehavior stp = host.Description.Behaviors.Find<ServiceDebugBehavior>();
      stp.HttpHelpPageEnabled = false;
      host.Open();

      Console.WriteLine("Service is up and running");
      //Console.WriteLine("Press enter to quit ");
      //Console.ReadLine();
      //host.Close();
    }
  }
}
