using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DeNSo.Core.DiskIO;
using DeNSo.Meta.BSon;
using System.Diagnostics;
using System.IO;

namespace DeNSo.Core
{
  public class EventStore
  {
    internal Journaling _journal = null;
    private volatile Queue<EventCommand> _waitingevents = new Queue<EventCommand>();
    private Thread _eventHandlerThread = null;

    public long LastExecutedCommandSN { get; private set; }

    internal ManualResetEvent CommandsReady = new ManualResetEvent(false);
    public string DatabaseName { get; private set; }

    internal EventStore(string dbname, long lastcommittedcommandsn)
    {
      DatabaseName = dbname;

      LastExecutedCommandSN = lastcommittedcommandsn;
      LoadUncommittedEventsFromJournal();

      _eventHandlerThread = new Thread(new ThreadStart(ExecuteEventCommands));
      _eventHandlerThread.Start();

      _journal = new Journaling(Configuration.BasePath, dbname);
    }

    internal void LoadUncommittedEventsFromJournal()
    {
      var jnlfile = Path.Combine(Configuration.BasePath, DatabaseName, "denso.jnl");
      if (File.Exists(jnlfile))
        using (var fs = File.Open(jnlfile, FileMode.Open, FileAccess.Read))
        using (var br = new BinaryReader(fs))
        {
          while (fs.Position < fs.Length)
          {
            var cmd = Journaling.ReadCommand(br);
            if (cmd != null)
              if (cmd.CommandSN > LastExecutedCommandSN)
                _waitingevents.Enqueue(cmd);
          }
        }
    }

    private void ExecuteEventCommands()
    {
      while (!StoreManager.ShuttingDown)
      {
        //Debug.Write(string.Format("step1 : {0}", DateTime.Now.ToString("ss:ffff")));
        CommandsReady.WaitOne(500);
        //Debug.Write(string.Format("step2 : {0}", DateTime.Now.ToString("ss:ffff")));
        if (_waitingevents.Count == 0)
        {
          CommandsReady.Reset();
          continue;
        }

        EventCommand we;
        lock (_waitingevents)
          we = _waitingevents.Dequeue();
        //sDebug.Write(string.Format("step3 : {0}", DateTime.Now.ToString("ss:ffff")));

        //Debug.Write(string.Format("step4 : {0}", DateTime.Now.ToString("ss:ffff")));
        EventHandlerManager.ExecuteEvent(DatabaseName, we);
        //sDebug.Write(string.Format("step5 : {0}", DateTime.Now.ToString("ss:ffff")));
        LastExecutedCommandSN = we.CommandSN;
        if (LastExecutedCommandSN % 1000 == 0)
          Console.Write(string.Format("LEC: {0} - ", LastExecutedCommandSN));
        //sDebug.WriteLine(string.Empty);

        if (_waitingevents.Count == 0)
          Session.RaiseStoreUpdated();

      }
    }

    public long Enqueue(byte[] command)
    {
      var csn = _journal.LogCommand(command);
      lock (_waitingevents)
        _waitingevents.Enqueue(new EventCommand() { CommandSN = csn, Command = command });

      CommandsReady.Set();

      return csn;
    }
    public void ShrinkEventStore()
    {
      if (_journal != null)
        _journal.ShrinkToSN(LastExecutedCommandSN);
    }
  }

}
