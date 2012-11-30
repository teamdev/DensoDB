using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;

namespace DeNSo.Core.DiskIO
{
  /// <summary>
  /// this class is used to store the command at the event dispatcher level
  /// </summary>
  public class EventCommand
  {
    /// <summary>
    /// a command serial number generated from local journaling. 
    /// </summary>
    [Description("A command serial number generated from local journaling.")]
    public long CommandSN
    { get; set; }

    /// <summary>
    /// Sign a command with specific marker for serverside manipulations. 
    /// </summary>
    [Description("Sign a command with specific marker for serverside manipulations. ")]
    internal string CommandMarker
    { get; set; }

    /// <summary>
    /// the user command
    /// </summary>
    [Description("the user command. ")]
    public byte[] Command { get; set; }

    /// <summary>
    /// set the marker to the command
    /// </summary>
    /// <remarks>
    /// A marker is a tag used to sign a message during event dispatching. 
    /// marker are not stored in the database, but only during journaling.
    /// </remarks>
    /// <param name="marker"></param>
    [Description("Set the marker to the command. ")]
    public void SetMarker(string marker)
    {
      var m = string.Format("§{0}#", marker);
      if (string.IsNullOrEmpty(CommandMarker) || CommandMarker.Contains(m))
      {
        CommandMarker = string.Format("{0}{1}", CommandMarker ?? string.Empty, m);
      }
    }

    /// <summary>
    /// check if i have the marker
    /// </summary>
    /// <remarks>
    /// A marker is a tag used to sign a message during event dispatching. 
    /// marker are not stored in the database, but only during journaling.
    /// </remarks>
    /// <param name="marker"></param>
    /// <returns></returns>
    [Description("check if i have the marker")]
    public bool HasMarker(string marker)
    {
      var m = string.Format("§{0}#", marker);
      return (CommandMarker ?? string.Empty).Contains(m);
    }

    /// <summary>
    /// remove the marker
    /// </summary>
    /// <remarks>
    /// A marker is a tag used to sign a message during event dispatching. 
    /// marker are not stored in the database, but only during journaling.
    /// </remarks>
    /// <param name="marker"></param>
    [Description("Remove the marker")]
    public void RemoveMarker(string marker)
    {
      var m = string.Format("§{0}#", marker);
      CommandMarker = (CommandMarker ?? string.Empty).Replace(m, string.Empty);
    }
  }
}
