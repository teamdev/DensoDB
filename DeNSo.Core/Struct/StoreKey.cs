using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DeNSo.Core.Struct
{
  /// <summary>
  /// StoreGuid create an ordinated Guid so that data can be order also using GUID. 
  /// Every StoreGuid is Store specific and any request to a new GUID causes a StoreGuid to increment it's value 
  /// </summary>
  /// 
  [Serializable]
  [StructLayout(LayoutKind.Explicit, Size = 16)]
  public struct StoreGuid
  {
#pragma warning disable 1591

    /// <summary>
    /// Array of byte containing real values of GUID. 
    /// </summary>
    [FieldOffset(0)]
    public byte[] values;

    public void init()
    {
      if (values == null)
        values = new byte[16];
    }

    public Guid Increment()
    {
      //init();
      for (short x = 15; x > 0; x--)
      {
        values[x] = (byte)(values[x] + 1);
        var cv = values[x];
        if (cv != 0) break;
      }

      return GetUid();
    }

    public Guid GetUid()
    {
      //init();
      var g = new Guid(values);
      return g;
    }

    public void SetUid(Guid value)
    {
      //init();
      values = value.ToByteArray();
    }
#pragma warning restore 1591
  }
}
