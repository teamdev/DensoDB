using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Struct
{
  public struct StoreKey
  {
    public byte[] Value;

    public static implicit operator StoreKey(int item)
    {
      return new StoreKey() { Value = BitConverter.GetBytes(item) };
    }

    public static implicit operator StoreKey(long item)
    {
      return new StoreKey() { Value = BitConverter.GetBytes(item) };
    }

    public static implicit operator StoreKey(Guid item)
    {
      return new StoreKey() { Value = item.ToByteArray() };
    }
  }
}
