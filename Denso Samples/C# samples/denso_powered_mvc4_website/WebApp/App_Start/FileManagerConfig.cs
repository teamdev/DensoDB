using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;


namespace WebApp
{
  public static class FileManagerConfig
  {
    public static void Register()
    {
      //(CDLTLL) Configuration for Windows Azure settings 
      CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSettingPublisher) =>
          {
            var connectionString = RoleEnvironment.GetConfigurationSettingValue(configName);
            configSettingPublisher(connectionString);
          });

      FileManager.FilesFolder = ConfigurationManager.AppSettings["uplodadefilesfolder"];

      if (RoleEnvironment.IsAvailable)
        FileManager.StoreFilesIn = StoreIn.AzureStorageBlob;
      else
        FileManager.StoreFilesIn = StoreIn.FileSystem;
    }
  }
}