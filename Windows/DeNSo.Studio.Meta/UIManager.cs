using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;
using System.Drawing.Printing;
using System.Windows.Interop;
using System.Windows.Controls;

namespace DeNSo.Studio.Meta
{
  public static class UIManager
  {
    private static Dictionary<string, Border> _regions = new Dictionary<string, Border>();
    public static void RegisterRegion(this Border item, string name)
    {
      if (_regions.ContainsKey(name))
        _regions[name] = item;
      else
        _regions.Add(name, item);
    }


    public static void ShowInRegion(this UIElement item, string name)
    {
      if (_regions.ContainsKey(name))
        _regions[name].Child = item;
    }

    #region DropShadow the main Window
    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

    public static void EnableSystemDropShadow(this Window window)
    {
      if (!DropShadow(window))
      {
        window.SourceInitialized += new EventHandler(window_SourceInitialized);
      }
    }

    private static void window_SourceInitialized(object sender, EventArgs e)
    {
      Window window = (Window)sender;

      DropShadow(window);

      window.SourceInitialized -= new EventHandler(window_SourceInitialized);
    }

    private static bool DropShadow(Window window)
    {
      try
      {
        WindowInteropHelper helper = new WindowInteropHelper(window);
        int val = 2;
        int ret1 = DwmSetWindowAttribute(helper.Handle, 2, ref val, 4);

        if (ret1 == 0)
        {
          Margins m = new Margins { Bottom = 0, Left = 0, Right = 0, Top = 0 };
          int ret2 = DwmExtendFrameIntoClientArea(helper.Handle, ref m);
          return ret2 == 0;
        }
        else
        {
          return false;
        }
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    #endregion
  }
}
