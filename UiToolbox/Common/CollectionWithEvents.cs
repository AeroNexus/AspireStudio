using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Aspire.UiToolbox.Common
{
  public abstract class CollectionWithEvents : CollectionBase
  {
    private int _suspendCount;
    public event CollectionClear Clearing;

    public event CollectionClear Cleared;

    public event CollectionChange Inserting;

    public event CollectionChange Inserted;

    public event CollectionChange Removing;

    public event CollectionChange Removed;

    public bool IsSuspended
    {
      get
      {
        return _suspendCount > 0;
      }
    }
    public CollectionWithEvents()
    {
      _suspendCount = 0;
    }
    public void SuspendEvents()
    {
      _suspendCount++;
    }
    public void ResumeEvents()
    {
      _suspendCount--;
    }
    protected override void OnClear()
    {
      if (!this.IsSuspended && this.Clearing != null)
      {
        this.Clearing();
      }
    }
    protected override void OnClearComplete()
    {
      if (!this.IsSuspended && this.Cleared != null)
      {
        this.Cleared();
      }
    }
    protected override void OnInsert(int index, object value)
    {
      if (!this.IsSuspended && this.Inserting != null)
      {
        this.Inserting(index, value);
      }
    }
    protected override void OnInsertComplete(int index, object value)
    {
      if (!this.IsSuspended && this.Inserted != null)
      {
        this.Inserted(index, value);
      }
    }
    protected override void OnRemove(int index, object value)
    {
      if (!this.IsSuspended && this.Removing != null)
      {
        this.Removing(index, value);
      }
    }
    protected override void OnRemoveComplete(int index, object value)
    {
      if (!this.IsSuspended && this.Removed != null)
      {
        this.Removed(index, value);
      }
    }
    protected int IndexOf(object value)
    {
      return base.List.IndexOf(value);
    }
  }
}
