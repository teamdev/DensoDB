using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Security.Permissions;
namespace DeNSo.Core
{
  [Description("Log messages and exceptions in the Machine EventLog")]
#if WINDOWS
  [EventLogPermission(SecurityAction.Demand)]
#endif
  public static class LogWriter
  {
    private static string _logname = "Application";
    private static string _eventsourcename = "DeNSo DB";
    public static string LogName
    {
      get
      {
        return LogWriter._logname;
      }
    }
    public static string EventDefaultSourceName
    {
      get
      {
        return LogWriter._eventsourcename;
      }
      set
      {
        LogWriter._eventsourcename = value;
      }
    }

    [Description("Log the exception in Machine EventLog")]
    public static void LogException(Exception ex)
    {
      LogWriter.LogException(ex, LogEntryType.Error);
    }
    [Description("Log the exception in Machine EventLog")]
    public static void LogException(Exception ex, LogEntryType entrytype)
    {
      LogWriter.LogException(ex, entrytype, LogWriter._logname);
    }
    [Description("Log the exception in Machine EventLog")]
    public static void LogException(Exception ex, LogEntryType entrytype, string applicationname)
    {
      string message = "Error Message:" + ex.Message + "\r\n";
      message = message + "StackTrace: " + ex.StackTrace + "\r\n";
#if WINDOWS
      message = message + "Source: " + ex.Source + "\r\n";
#endif
      if (ex.InnerException != null)
      {
        message = message + "InnerException: " + ex.InnerException.Message + "\r\n";
      }
#if WINDOWS
      LogWriter.LogMessage(message, applicationname, (LogEntryType)entrytype);
#endif
    }

    [Description("Log a message in the Machine EventLog")]
    public static void LogMessage(string message, LogEntryType entrytype)
    {
      LogWriter.LogMessage(message, LogWriter._logname, entrytype);
    }
    [Description("Log a message in the Machine EventLog")]
    public static void LogMessage(string message, string applicationname, LogEntryType entrytype)
    {
#if WINDOWS
      string verbose = string.Empty;

      if (Environment.UserInteractive)
      {
        switch (entrytype)
        {
          case LogEntryType.Error:
            Console.ForegroundColor = ConsoleColor.Red;
            break;
          case LogEntryType.FailureAudit:
            Console.ForegroundColor = ConsoleColor.DarkRed;
            break;
          case LogEntryType.Information:
            Console.ForegroundColor = ConsoleColor.White;
            break;
          case LogEntryType.SuccessAudit:
            Console.ForegroundColor = ConsoleColor.Green;
            break;
          case LogEntryType.Warning:
            Console.ForegroundColor = ConsoleColor.Yellow;
            break;
          default:
            break;
        }
        Console.WriteLine(message);
        Console.ResetColor();
      }

      try
      {
        if (!(verbose == "0") || entrytype == LogEntryType.Error)
        {
          if (!(verbose == "1") || entrytype == LogEntryType.Error || entrytype == LogEntryType.Warning)
          {
            if (!EventLog.SourceExists(LogWriter._eventsourcename))
            {
              EventLog.CreateEventSource(LogWriter._eventsourcename, applicationname);
            }
            new EventLog(applicationname)
            {
              Source = LogWriter._eventsourcename
            }.WriteEntry(message, (EventLogEntryType)entrytype);
          }
        }
      }
      catch
      {
      }
#endif
    }

    public static void LogInformation(string message, LogEntryType entrytype)
    {
#if WINDOWS
      if (Environment.UserInteractive)
        LogMessage(string.Format("{0}: {1}", DateTime.Now.ToString("hh:mm:ss.ffff"), message),
                   EventDefaultSourceName, entrytype);
#endif
    }

    public static void SeparationLine()
    {
#if WINDOWS
      if (Environment.UserInteractive)
        Console.WriteLine("------------------------------------------------------------------------");
#endif
    }
  }

  public enum LogEntryType
  {
    Error = 1,
    Warning = 2,
    Information = 4,
    SuccessAudit = 8,
    FailureAudit = 16,
  }
}
