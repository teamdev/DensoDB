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
  internal class JournalWriter
  {
    #region private fields

#if WINDOWS_PHONE
    private System.IO.IsolatedStorage.IsolatedStorageFile iss = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
#endif

    private object _filelock = new object();
    private FileStream _logfile = null;
    private MemoryStream _logbuffer = null;
    private BinaryWriter _writer = null;
    private const int MByte = 1024 * 1204;
    private int _increasefileby = 2 * MByte;
    private int _logfilefreespace = (int)(0.5 * (double)MByte);
    private bool _ensureatomicwrites = Configuration.EnsureAtomicWrites;

    #endregion

    #region public properties

    public long CommandSN { get; internal set; }
    public string FileName { get; private set; }
    public string DataBaseName { get; private set; }
    public string BasePath { get; private set; }

    #endregion

    #region events
    public static event EventHandler Closing;

    internal static void RaiseCloseEvent()
    {
      if (Closing != null)
        Closing(null, EventArgs.Empty);
    }
    #endregion

    internal JournalWriter(string basepath, string databasename, bool isoperationlog = false)
    {
      BasePath = basepath;
      DataBaseName = databasename;
      Init(isoperationlog);
    }

    public long LogCommand(EventCommand command)
    {
      lock (_filelock)
      {
        CommandSN++;
        if (Configuration.EnableJournaling)
          if (OpenLogFile())
          {
            // Writing the journal
            if (_ensureatomicwrites)
              _logfile.Flush();

            _logbuffer.Seek(0, SeekOrigin.Begin);
            _logbuffer.SetLength(0);

            _writer.Write('K');
            _writer.Write(CommandSN);
            _writer.Write(command.CommandMarker ?? string.Empty);
            _writer.Write('D');
            _writer.Write(command.Command);
            _writer.Flush();

            // this should be an atomic operation due to filestream flag used during open.
            _logfile.Write(_logbuffer.ToArray(), 0, (int)_logbuffer.Length);
            _logfile.Flush(_ensureatomicwrites);

            if (_logfile.Length - _logfile.Position < _logfilefreespace)
            {
              IncreaseFileSize();
            }
          }
      }
      return CommandSN;
    }

    //  public long LogCommand(byte[] command)
    //{
    //  return LogCommand(new EventCommand() { Command = command });
    //}

    private bool Init(bool isoperationslog = false)
    {
      FileName = Path.Combine(BasePath, DataBaseName);
      LogWriter.SeparationLine();
      LogWriter.LogInformation("Initializing Journal Writer", LogEntryType.Information);

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

    private void CloseFile()
    {
      lock (_filelock)
      {
        if (_logfile != null)
        {
          //_logfile.SetLength(_logfile.Position);
          _logfile.Flush(true);
          _writer.Flush();
          _writer.Close();
          _logfile.Close();
          _logfile = null;
          _writer = null;
        }
      }
    }

    private void IncreaseFileSize()
    {
      // DO not prealloc log file during debugging. 

      // testing 
      return;
      // The following code should cannot be used due to change in file open rights. 
      // We have no longer rights to change filesize. 
      // Shrink operations are not limited, the file will be open with a different right set. 
      // 

      if (Debugger.IsAttached) return;

      LogWriter.LogInformation("Journalig file is too small, make it bigger", LogEntryType.Information);
      var pos = _logfile.Position;
      _logfile.SetLength(pos + _increasefileby);
      _logfile.Flush();
      _logfile.Position = pos;
    }

    private bool OpenLogFile()
    {
      try
      {
        if (_logfile == null)
        {
#if WINDOWS_PHONE
          _logfile = iss.OpenFile(FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
#else
          _logfile = new FileStream(FileName,
                                    FileMode.Append,
                                    System.Security.AccessControl.FileSystemRights.AppendData,
                                    FileShare.ReadWrite, 4096,
                                    FileOptions.None);
          //_logfile = File.Open(FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
#endif
          //IncreaseFileSize();
          _logbuffer = new MemoryStream();
          _writer = new BinaryWriter(_logbuffer);

          if (_logfile != null)
            LogWriter.LogMessage(string.Format("Log File ready: {0}", FileName), LogEntryType.Information);
          else
          {
            LogWriter.LogMessage(string.Format("Unable to open logfile: {0}", FileName), LogEntryType.Error);
            //Server.EmergencyShutdown();
          }

          JournalWriter.Closing += (s, e) => CloseFile();
        }
      }
      catch (Exception ex)
      {
        LogWriter.LogException(ex);
        //Server.EmergencyShutdown();
        return false;
      }
      return true;
    }

    internal void ShrinkToSN(long commandsn)
    {
      LogWriter.LogInformation("File Shrink requested", LogEntryType.Warning);
      //return;

      CloseFile();
      lock (_filelock)
      {
#if WINDOWS_PHONE
        if (iss.FileExists(FileName))
          using (var readerfs = iss.OpenFile(FileName, FileMode.Open, FileAccess.Read, FileShare.Write))
          using (var writerfs = iss.OpenFile(FileName, FileMode.Open, FileAccess.Write, FileShare.Read))
#else
        if (File.Exists(FileName))
          using (var readerfs = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Write))
          using (var writerfs = File.Open(FileName, FileMode.Open, FileAccess.Write, FileShare.Read))
#endif

          using (var br = new BinaryReader(readerfs))
          using (var bw = new BinaryWriter(writerfs))
          {
            LogWriter.LogInformation("Compressing file", LogEntryType.Information);
            while (readerfs.Position < readerfs.Length)
            {
              var cmd = ReadCommand(br);
              if (cmd != null)
                if (cmd.CommandSN > commandsn)
                  WriteCommand(bw, cmd);
            }

            LogWriter.LogInformation("File shrink completed", LogEntryType.SuccessAudit);
            readerfs.Close();
            writerfs.Flush();
            LogWriter.LogInformation("Free empty space", LogEntryType.SuccessAudit);
            writerfs.SetLength(writerfs.Position);
            writerfs.Close();

            LogWriter.LogInformation("Shringk completed", LogEntryType.SuccessAudit);
          }
      }
    }

    internal static void WriteCommand(BinaryWriter bw, EventCommand command)
    {
      bw.Write('K');
      bw.Write(command.CommandSN);
      bw.Write(command.CommandMarker ?? string.Empty);
      bw.Write('D');
      bw.Write(command.Command);
    }

    private EventCommand ReadCommand(BinaryReader br)
    {
      var k = br.ReadChar();
      if (k == 'K')
      {
        var csn = br.ReadInt64();
        var marker = br.ReadString();
        var d = br.ReadChar();
        if (d == 'D')
        {
          string command = br.ReadString();
          return new EventCommand() { Command = command, CommandSN = csn, CommandMarker = marker };
        }
      }
      return null;
    }
  }
}
