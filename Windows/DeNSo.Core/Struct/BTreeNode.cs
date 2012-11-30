using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Core.Struct
{
  public class BTreeNode<TKey, TValue> where TKey : IComparable<TKey>
  {
    private BTreeNode<TKey, TValue> Left;
    private BTreeNode<TKey, TValue> Right;

    public TKey Key { get; set; }
    public TValue Value { get; set; }

    public void AddNode(BTreeNode<TKey, TValue> parent, BTreeNode<TKey, TValue> newnode)
    {
      var comresult = newnode.Key.CompareTo(this.Key);

      if (comresult > 0 && Right == null)
      { Right = newnode; return; }

      if (comresult < 0 && Left == null)
      { Left = newnode; return; }


    }
  }
}
