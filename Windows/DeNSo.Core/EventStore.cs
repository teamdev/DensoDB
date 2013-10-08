using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DeNSo.DiskIO;

using System.Diagnostics;
using System.IO;

namespace DeNSo
{
  public class EventStore
  {
    internal JournalWriter _jwriter = null;
    //internal JournalReader _jreader = null;

    internal JournalWriter _operationsLog = null;

#if WINDOWS_PHONE
    internal System.IO.IsolatedStorage.IsolatedStorageFile iss = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
#endif

    private volatile Queue<EventCommand> _waitingevents = new Queue<EventCommand>();
    private Thread _eventHandlerThread = null;

    public long LastExecutedCommandSN { get; private set; }

    internal ManualResetEvent CommandsReady = new ManualResetEvent(false);
    public string DatabaseName { get; private set; }

    internal EventStore(string dbname, long lastcommittedcommandsn)
    {
      DatabaseName = dbname;

      LastExecutedCommandSN = lastcommittedcommandsn;

      JournalReader _jreader = new JournalReader(Configuration.BasePath, dbname);
      long jsn = LoadUncommittedEventsFromJournal(_jreader);
      _jreader.CloseFile();

      _eventHandlerThread = new Thread(new ThreadStart(ExecuteEventCommands));
      _eventHandlerThread.Start();

      _jwriter = new JournalWriter(Configuration.BasePath, dbname);

      if (Configuration.EnableOperationsLog)
        _operationsLog = new JournalWriter(Configuration.BasePath, dbname, true);

      // The journal can be empty so i have to evaluate the last committed command serial number 
      // and reset Command Serial number in the journal to ensure command execution coherency.
      _jwriter.CommandSN = Math.Max(jsn, lastcommittedcommandsn);
    }

    internal long LoadUncommittedEventsFromJournal(JournalReader _jreader)
    {
      long journalsn = 0;

      while (_jreader.HasCommandsPending())
      {
        var cmd = _jreader.ReadCommand();
        if (cmd != null)
        {
          if (cmd.CommandSN > LastExecutedCommandSN)
          {
            _waitingevents.Enqueue(cmd);
          }
          journalsn = Math.Max(journalsn, cmd.CommandSN);
        }
      }
      return journalsn;
    }

    private void ExecuteEventCommands()
    {
      while (!StoreManager.ShuttingDown)
      {
        //Debug.Write(string.Format("step1 : {0}", DateTime.Now.ToString("ss:ffff")));
        CommandsReady.WaitOne(5000);
        //Debug.Write(string.Format("step2 : {0}", DateTime.Now.ToString("ss:ffff")));
        if (_waitingevents.Count == 0)
        {
          CommandsReady.Reset();
          continue;
        }

        EventCommand we;
        lock (_waitingevents)
          we = _waitingevents.Dequeue();

        EventHandlerManager.ExecuteCommandEvent(DatabaseName, we);

        LastExecutedCommandSN = we.CommandSN;

        if (Debugger.IsAttached)
          if (LastExecutedCommandSN % 1000 == 0)
            Console.Write(string.Format("LEC: {0} - ", LastExecutedCommandSN));

        if (Configuration.EnableOperationsLog)
          _operationsLog.LogCommand(we);

        //if (_waitingevents.Count == 0)
        //  Session.RaiseStoreUpdated(LastExecutedCommandSN);

      }
    }

    public long Enqueue(string command)
    {
      var cmd = new EventCommand() { Command = command };
      return Enqueue(cmd);
    }

    public long Enqueue(EventCommand command)
    {
      var csn = _jwriter.LogCommand(command);
      command.CommandSN = csn;
      lock (_waitingevents)
        _waitingevents.Enqueue(command);

      CommandsReady.Set();

      return csn;
    }

    public void ShrinkEventStore()
    {
      if (_jwriter != null)
        _jwriter.ShrinkToSN(LastExecutedCommandSN);
    }
  }
}
