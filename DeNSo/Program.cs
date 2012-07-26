using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace DeNSo
{
  static class Program
  {
    static ConsoleColor originalcolor;
    static DeNSoService _service = new DeNSoService();

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main()
    {
      originalcolor = Console.ForegroundColor;
      StartDeNSo();
      Console.ForegroundColor = originalcolor;
    }

    private static void StartDeNSo()
    {
      if (Environment.UserInteractive)
      {
        try
        {
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine("--------------------------------------------------------------");
          Console.WriteLine("DENSO: Starting Denso Service in Interactive mode");
          Console.WriteLine("--------------------------------------------------------------");

          _service.InteractiveStart();

          Console.WriteLine("--------------------------------------------------------------");
          Console.WriteLine("DENSO: Started in Interactive mode");
          Console.WriteLine("--------------------------------------------------------------");
          TrapConsoleClose();
          while (true)
            Console.ReadLine();

          //_service.InteractiveStop();
        }
        catch (Exception ex)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine(ex.Message);
          Console.WriteLine(ex.StackTrace);
        }
      }
      else
      {
        try
        {
          ServiceBase.Run(_service);
        }
        catch (Exception ex)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine(ex.Message);
          Console.WriteLine(ex.StackTrace);
        }
      }
    }

    private static void TrapConsoleClose()
    {
      Console.CancelKeyPress += (s, e) =>
      {
        _service.InteractiveStop();
        Console.ResetColor();
        System.Diagnostics.Process.GetCurrentProcess().Close();
      };
    }
  }
}
