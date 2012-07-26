using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Security.Permissions;
namespace DeNSo.Core
{
  [Description("Log messages and exceptions in the Machine EventLog")]
  [EventLogPermission(SecurityAction.Demand)]
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
      LogWriter.LogException(ex, EventLogEntryType.Error);
    }
    [Description("Log the exception in Machine EventLog")]
    public static void LogException(Exception ex, EventLogEntryType entrytype)
    {
      LogWriter.LogException(ex, entrytype, LogWriter._logname);
    }
    [Description("Log the exception in Machine EventLog")]
    public static void LogException(Exception ex, EventLogEntryType entrytype, string applicationname)
    {
      string message = "Error Message:" + ex.Message + "\r\n";
      message = message + "StackTrace: " + ex.StackTrace + "\r\n";
      message = message + "Source: " + ex.Source + "\r\n";
      if (ex.InnerException != null)
      {
        message = message + "InnerException: " + ex.InnerException.Message + "\r\n";
      }

      LogWriter.LogMessage(message, applicationname, entrytype);
    }

    [Description("Log a message in the Machine EventLog")]
    public static void LogMessage(string message, EventLogEntryType entrytype)
    {
      LogWriter.LogMessage(message, LogWriter._logname, entrytype);
    }
    [Description("Log a message in the Machine EventLog")]
    public static void LogMessage(string message, string applicationname, EventLogEntryType entrytype)
    {
      string verbose = string.Empty;

      if (Environment.UserInteractive)
      {
        switch (entrytype)
        {
          case EventLogEntryType.Error:
            Console.ForegroundColor = ConsoleColor.Red;
            break;
          case EventLogEntryType.FailureAudit:
            Console.ForegroundColor = ConsoleColor.DarkRed;
            break;
          case EventLogEntryType.Information:
            Console.ForegroundColor = ConsoleColor.White;
            break;
          case EventLogEntryType.SuccessAudit:
            Console.ForegroundColor = ConsoleColor.Green;
            break;
          case EventLogEntryType.Warning:
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
        if (!(verbose == "0") || entrytype == EventLogEntryType.Error)
        {
          if (!(verbose == "1") || entrytype == EventLogEntryType.Error || entrytype == EventLogEntryType.Warning)
          {
            if (!EventLog.SourceExists(LogWriter._eventsourcename))
            {
              EventLog.CreateEventSource(LogWriter._eventsourcename, applicationname);
            }
            new EventLog(applicationname)
            {
              Source = LogWriter._eventsourcename
            }.WriteEntry(message, entrytype);
          }
        }
      }
      catch
      {
      }
    }

    public static void LogInformation(string message, EventLogEntryType entrytype)
    {
      if (Environment.UserInteractive)
        LogMessage(string.Format("{0}: {1}", DateTime.Now.ToString("hh:mm:ss.ffff"), message),
                   EventDefaultSourceName, entrytype);
    }

    public static void SeparationLine()
    {
      if (Environment.UserInteractive)
        Console.WriteLine("------------------------------------------------------------------------");
    }
  }
}
