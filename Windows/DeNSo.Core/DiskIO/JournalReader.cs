using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
//using System.Runtime.Serialization.Formatters.Binary;
using DeNSo.DiskIO;

#if WINDOWS
using System.Runtime.Remoting;
#endif

namespace DeNSo
{
  internal class JournalReader
  {
    #region private fields

#if WINDOWS_PHONE
    private System.IO.IsolatedStorage.IsolatedStorageFile iss = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
#endif

    private FileStream _logfile = null;
    private BinaryReader _reader = null;

    #endregion

    #region public properties

    public long CommandSN { get; internal set; }
    public string FileName { get; private set; }
    public string DataBaseName { get; private set; }
    public string BasePath { get; private set; }

    #endregion

    internal JournalReader(string basepath, string databasename, bool isoperationlog = false)
    {
      BasePath = basepath;
      DataBaseName = databasename;
      Init(isoperationlog);
    }

    private bool Init(bool isoperationslog = false)
    {
      FileName = Path.Combine(BasePath, DataBaseName);
      LogWriter.SeparationLine();
      LogWriter.LogInformation("Initializing Journal Reader", LogEntryType.Information);

      try
      {
#if WINDOWS_PHONE
        if (!iss.DirectoryExists(FileName))
        {
          LogWriter.LogInformation("Directory for Journaling does not exists. creating it", LogEntryType.Warning);
          iss.CreateDirectory(FileName);
        }
#else
        if (!Directory.Exists(FileName))
        {
          LogWriter.LogInformation("Directory for Journaling does not exists. creating it", LogEntryType.Warning);
          Directory.CreateDirectory(FileName);
        }
#endif
      }
      catch (Exception ex)
      {
        LogWriter.LogException(ex);
        return false;
      }

      if (isoperationslog)
        FileName = Path.Combine(FileName, "denso.log");
      else
        FileName = Path.Combine(FileName, "denso.jnl");


      LogWriter.LogInformation("Completed", LogEntryType.SuccessAudit);
      return OpenLogFile();
    }

    public void CloseFile()
    {
      if (_reader != null)
      {
        _reader.Close();
        _reader = null;
      }

      if (_logfile != null)
      {
        _logfile.Close();
        _logfile = null;
      }
    }

    private bool OpenLogFile()
    {
      try
      {

#if WINDOWS_PHONE
          _logfile = iss.OpenFile(FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
#else
        _logfile = new FileStream(FileName,
                                  FileMode.OpenOrCreate,
                                  System.Security.AccessControl.FileSystemRights.Read,
                                  FileShare.ReadWrite, 4096,
                                  FileOptions.None);
#endif
        _reader = new BinaryReader(_logfile);

        if (_logfile != null)
          LogWriter.LogMessage(string.Format("Log File ready: {0}", FileName), LogEntryType.Information);
        else
        {
          LogWriter.LogMessage(string.Format("Unable to open logfile: {0}", FileName), LogEntryType.Error);
        }

        JournalWriter.Closing += (s, e) => CloseFile();
      }
      catch (Exception ex)
      {
        LogWriter.LogException(ex);
        return false;
      }
      return true;
    }

    internal bool HasCommandsPending()
    {
      if (_logfile != null)
        return _logfile.Position < _logfile.Length;
      return false;

    }

    internal EventCommand ReadCommand()
    {
      var k = _reader.ReadChar();
      if (k == 'K')
      {
        var csn = _reader.ReadInt64();
        var marker = _reader.ReadString();
        var d = _reader.ReadChar();
        if (d == 'D')
        {
          string command = _reader.ReadString();
          return new EventCommand() { Command = command, CommandSN = csn, CommandMarker = marker };
        }
      }
      return null;
    }
  }
}
