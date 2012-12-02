using DeNSo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace WebApp
{
  public static class DensoConfig
  {
    public static void Start(HttpServerUtility server)
    {
      DeNSo.Configuration.BasePath = server.MapPath("~/App_Data");
      DeNSo.Configuration.EnableJournaling = true;

      Session.DefaultDataBase = "densodb_webapp";
      Session.Start();
    }

    public static void ShutDown()
    {
      Session.ShutDown();
    }
  }
}