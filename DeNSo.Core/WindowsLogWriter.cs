using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Security.Permissions;
namespace DeNSo.Core
{
	[Description("Log messages and exceptions in the Machine EventLog")]
	[EventLogPermission(SecurityAction.Demand)]
	public static class WindowsLogWriter
	{
		private static string _logname = "Application";
		private static string _eventsourcename = "DeNSo DB";
		public static string LogName
		{
			get
			{
        return WindowsLogWriter._logname;
			}
		}
		public static string EventDefaultSourceName
		{
			get
			{
        return WindowsLogWriter._eventsourcename;
			}
			set
			{
        WindowsLogWriter._eventsourcename = value;
			}
		}
		[Description("Log the exception in Machine EventLog")]
		public static void LogException(Exception ex)
		{
      WindowsLogWriter.LogException(ex, EventLogEntryType.Error);
		}
		[Description("Log the exception in Machine EventLog")]
		public static void LogException(Exception ex, EventLogEntryType entrytype)
		{
      WindowsLogWriter.LogException(ex, entrytype, WindowsLogWriter._logname);
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
      WindowsLogWriter.LogMessage(message, applicationname, entrytype);
		}
		[Description("Log a message in the Machine EventLog")]
		public static void LogMessage(string message, EventLogEntryType entrytype)
		{
      WindowsLogWriter.LogMessage(message, WindowsLogWriter._logname, entrytype);
		}
		[Description("Log a message in the Machine EventLog")]
		public static void LogMessage(string message, string applicationname, EventLogEntryType entrytype)
		{
			string verbose = string.Empty;
			try
			{
				if (!(verbose == "0") || entrytype == EventLogEntryType.Error)
				{
					if (!(verbose == "1") || entrytype == EventLogEntryType.Error || entrytype == EventLogEntryType.Warning)
					{
            if (!EventLog.SourceExists(WindowsLogWriter._eventsourcename))
						{
              EventLog.CreateEventSource(WindowsLogWriter._eventsourcename, applicationname);
						}
						new EventLog(applicationname)
						{
              Source = WindowsLogWriter._eventsourcename
						}.WriteEntry(message, entrytype);
					}
				}
			}
			catch
			{
			}
		}
	}
}
