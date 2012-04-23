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
    private int _increasefileby = 5 * MByte; 
    #endregion

    #region public properties
    public long CommandSN { get; private set; }
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

    public long LogCommand(byte[] command)
    {
      lock (_filelock)
      {
        if (OpenLogFile())
        {
          // Writing the journal
          _writer.Write('K');
          _writer.Write(++CommandSN);
          _writer.Write('D');
          _writer.Write((int)command.Length);
          _writer.Write(command);
          //_logfile.Flush();

          if (_logfile.Length - _logfile.Position < MByte)
          {
            IncreaseFileSize();
          }
        }
      }
      return CommandSN;
    }

    private bool Init()
    {
      FileName = Path.Combine(BasePath, DataBaseName);

      try
      {
        if (!Directory.Exists(FileName))
        {
          Directory.CreateDirectory(FileName);
        }
      }
      catch (Exception ex)
      {
        WindowsLogWriter.LogException(ex);
        return false;
      }

      FileName = Path.Combine(FileName, "denso.jnl");
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
            WindowsLogWriter.LogMessage(string.Format("Log File ready: {0}", FileName), System.Diagnostics.EventLogEntryType.Information);
          else
          {
            WindowsLogWriter.LogMessage(string.Format("Unable to open logfile: {0}", FileName), System.Diagnostics.EventLogEntryType.Error);
            //Server.EmergencyShutdown();
          }

          Journaling.Closing += (s, e) => CloseFile();
        }
      }
      catch (Exception ex)
      {
        WindowsLogWriter.LogException(ex);
        //Server.EmergencyShutdown();
        return false;
      }
      return true;
    }

    internal void ShrinkToSN(long commandsn)
    {
      CloseFile();
      lock (_filelock)
      {
        if (File.Exists(FileName))
          using (var readerfs = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Write))
          using (var writerfs = File.Open(FileName, FileMode.Open, FileAccess.Write, FileShare.Read))
          using (var br = new BinaryReader(readerfs))
          using (var bw = new BinaryWriter(writerfs))
          {
            while (readerfs.Position < readerfs.Length)
            {
              var cmd = ReadCommand(br);
              if (cmd != null)
                if (cmd.CommandSN > commandsn)
                  WriteCommand(bw, cmd);
            }

            readerfs.Close();
            writerfs.Flush();
            writerfs.SetLength(writerfs.Position);
            writerfs.Close();
          }
      }
    }

    internal static void WriteCommand(BinaryWriter bw, EventCommand command)
    {
      bw.Write('K');
      bw.Write(command.CommandSN);
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
        var d = br.ReadChar();
        if (d == 'D')
        {
          var len = br.ReadInt32();
          byte[] command = br.ReadBytes(len);
          return new EventCommand() { Command = command, CommandSN = csn };
        }
      }
      return null;
    }
  }
}
