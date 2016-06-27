using System;
using System.Collections;

using Aspire.UiToolbox.Common;

namespace Aspire.UiToolbox.Controls
{
  public class SelectedNodeCollection : CollectionWithEvents
  {
    public Node this[int index]
    {
      get
      {
        return base.List[index] as Node;
      }
    }
    internal SelectedNodeCollection(Hashtable selected)
    {
      var enumerator = selected.Keys.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node = (Node)enumerator.Current;
          base.List.Add(node);
        }
      }
      finally
      {
        IDisposable disposable = enumerator as IDisposable;
        if (disposable != null)
        {
          disposable.Dispose();
        }
      }
    }
    public bool Contains(Node value)
    {
      return base.List.Contains(value);
    }
    public int IndexOf(Node value)
    {
      return base.List.IndexOf(value);
    }
  }
}
