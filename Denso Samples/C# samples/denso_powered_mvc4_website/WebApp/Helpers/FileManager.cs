using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp
{
  /// <summary>
  /// The file manager enable you to save files in Windows Azure Store. 
  /// </summary>
  public static class FileManager
  {
    public static string FilesFolder { get; set; }
    public static StoreIn StoreFilesIn { get; set; }

    private const string connectionString = "DataConnectionString";

    #region private objects
    private static CloudBlobClient blobClient;
    private static CloudBlobContainer blobContainer;
    private static CloudStorageAccount storageAccount;
    #endregion

    static FileManager()
    {
      FilesFolder = "cms_uploaded_files";
      StoreFilesIn = StoreIn.FileSystem;
    }

    /// <summary>
    /// Save file in configured repository (filesystem or azure)
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string SaveFile(string filename, MemoryStream content)
    {
      switch (StoreFilesIn)
      {
        default:
        case StoreIn.FileSystem:
          return SaveFileToDisk(filename, content);

        case StoreIn.AzureStorageBlob:
        case StoreIn.AzureStorageTable:
          return SaveFileToBlob(filename, content);
      }
    }

    #region Azure
  
    /// <summary>
    /// Init Azure Storage if needed
    /// </summary>
    private static void InitAzureStorage()
    {
      if (storageAccount == null)
      {
        storageAccount = CloudStorageAccount.FromConfigurationSetting(connectionString);

        if (blobClient == null)
        {
          blobClient = storageAccount.CreateCloudBlobClient();
          blobContainer = blobClient.GetContainerReference(FilesFolder);
          blobContainer.CreateIfNotExist();

          var permissions = blobContainer.GetPermissions();
          permissions.PublicAccess = BlobContainerPublicAccessType.Container;
          blobContainer.SetPermissions(permissions);
        }
      }
    }

    /// <summary>
    /// Save file in Azure Storage
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string SaveFileToBlob(string filename, MemoryStream content)
    {
      // Inizializzo lo storage... 
      InitAzureStorage();

      string fileblobname = string.Format("{0}/{1}", FilesFolder, filename);
      CloudBlockBlob blob = blobClient.GetBlockBlobReference(fileblobname);
      blob.Properties.ContentType = System.IO.Path.GetExtension(filename);

      // Attenzione: devo riavvolgere lo stream..
      // perchè quando è stato scritto lo stream è avanzato per tutta la lunghezza dei dati. 
      content.Seek(0, SeekOrigin.Begin);

      blob.UploadFromStream(content);
      return blob.Uri.ToString();
    }

    #endregion

    /// <summary>
    /// Save file in FileSystem
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string SaveFileToDisk(string filename, MemoryStream content)
    {
      if (HttpContext.Current == null)
        throw new Exception("FileManager: Cannot obtain HttpContext");

      var realpath = HttpContext.Current.Server.MapPath(string.Format("~/App_Data/{0}", FilesFolder));
      if (!Directory.Exists(realpath))
      {
        Directory.CreateDirectory(realpath);
      }

      var fullfilename = Path.Combine(realpath, filename);
      File.WriteAllBytes(fullfilename, content.ToArray());
      return filename;
    }

    public static string GetRealPath(string filename)
    {
      if (HttpContext.Current == null)
        throw new Exception("FileManager: Cannot obtain HttpContext");

      var realpath = HttpContext.Current.Server.MapPath(string.Format("~/App_Data/{0}", FilesFolder));
      return Path.Combine(realpath, filename);
    }
  }

  public enum StoreIn
  {
    FileSystem,
    AzureStorageBlob,
    AzureStorageTable
  }
}