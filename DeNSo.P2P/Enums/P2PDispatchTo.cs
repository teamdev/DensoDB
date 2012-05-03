using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.P2P
{
  public enum P2PDispatchTo : byte
  {
    NoOne = 0, 
    SpecificPeerNode = 1, 
    All = 99
  }
}
