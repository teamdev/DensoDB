using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using DeNSo.Core.DiskIO;

namespace DeNSo.Core
{
  internal class Journaling
  {
    #region private fields
    private object _filelock = new object();
    private FileStream _logfile = null;
    private BinaryWriter _writer = null;
    private BinaryFormatter _bf = new BinaryFormatter();
    private const int MByte = 1024 * 1204;
    private int _increasefileby = 1 * MByte;
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

    internal Journaling(string basepath, string databasename)
    {
      BasePath = basepath;
      DataBaseName = databasename;
      Init();
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
            _writer.Write('K');
            _writer.Write(CommandSN);
            _writer.Write(command.CommandMarker ?? string.Empty);
            _writer.Write('D');
            _writer.Write((int)command.Command.Length);
            _writer.Write(command.Command);
            //_logfile.Flush();

            if (_logfile.Length - _logfile.Position < MByte)
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
      LogWriter.LogInformation("Initializing Journaling", EventLogEntryType.Information);

      try
      {
        if (!Directory.Exists(FileName))
        {
          LogWriter.LogInformation("Directory for Journaling does not exists. creating it", EventLogEntryType.Warning);
          Directory.CreateDirectory(FileName);
        }
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


      LogWriter.LogInformation("Completed", EventLogEntryType.SuccessAudit);
      return OpenLogFile();
    }

    private void CloseFile()
    {
      lock (_filelock)
      {
        if (_logfile != null)
        {
          _logfile.SetLength(_logfile.Position);
          _logfile.Flush(true);
          _logfile.Close();
          _logfile = null;
        }
      }
    }

    private void IncreaseFileSize()
    {
      LogWriter.LogInformation("Journalig file is too small, make it bigger", EventLogEntryType.Information);
      var pos = _logfile.Position;
      _logfile.SetLength(pos + _increasefileby);
      //_logfile.Flush();
      _logfile.Position = pos;
    }

    private bool OpenLogFile()
    {
      try
      {
        if (_logfile == null)
        {
          _logfile = File.Open(FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
          IncreaseFileSize();
          _writer = new BinaryWriter(_logfile);

          if (_logfile != null)
            LogWriter.LogMessage(string.Format("Log File ready: {0}", FileName), System.Diagnostics.EventLogEntryType.Information);
          else
          {
            LogWriter.LogMessage(string.Format("Unable to open logfile: {0}", FileName), System.Diagnostics.EventLogEntryType.Error);
            //Server.EmergencyShutdown();
          }

          Journaling.Closing += (s, e) => CloseFile();
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
      LogWriter.LogInformation("File Shrink requested", EventLogEntryType.Warning);
      CloseFile();
      lock (_filelock)
      {
        if (File.Exists(FileName))
          using (var readerfs = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Write))
          using (var writerfs = File.Open(FileName, FileMode.Open, FileAccess.Write, FileShare.Read))
          using (var br = new BinaryReader(readerfs))
          using (var bw = new BinaryWriter(writerfs))
          {
            LogWriter.LogInformation("Compressing file", EventLogEntryType.Information);
            while (readerfs.Position < readerfs.Length)
            {
              var cmd = ReadCommand(br);
              if (cmd != null)
                if (cmd.CommandSN > commandsn)
                  WriteCommand(bw, cmd);
            }

            LogWriter.LogInformation("File shrink completed", EventLogEntryType.SuccessAudit);
            readerfs.Close();
            writerfs.Flush();
            LogWriter.LogInformation("Free empty space", EventLogEntryType.SuccessAudit);
            writerfs.SetLength(writerfs.Position);
            writerfs.Close();

            LogWriter.LogInformation("Shringk completed", EventLogEntryType.SuccessAudit);
          }
      }
    }

    internal static void WriteCommand(BinaryWriter bw, EventCommand command)
    {
      bw.Write('K');
      bw.Write(command.CommandSN);
      bw.Write(command.CommandMarker ?? string.Empty);
      bw.Write('D');
      bw.Write(command.Command.Length);
      bw.Write(command.Command);
    }

    internal static EventCommand ReadCommand(BinaryReader br)
    {
      var k = br.ReadChar();
      if (k == 'K')
      {
        var csn = br.ReadInt64();
        var marker = br.ReadString();
        var d = br.ReadChar();
        if (d == 'D')
        {
          var len = br.ReadInt32();
          byte[] command = br.ReadBytes(len);
          return new EventCommand() { Command = command, CommandSN = csn, CommandMarker = marker };
        }
      }
      return null;
    }
  }
}
