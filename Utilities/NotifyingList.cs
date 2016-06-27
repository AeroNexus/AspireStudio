using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Aspire.Utilities
{
	/// <summary>
	/// Summary description for NotifyingList<T>.
	/// </summary>
	//[Editor(typeof(CollectionEditor),typeof(UITypeEditor))]
	public class NotifyingList<T> : IList<T>

	{
		List<T> list = new List<T>();

		public event EventHandler Added;
		public event EventHandler Removed;

		public override string ToString()
		{
			return string.Format("(Collection[{0}])",Count);
		}

		public T[] ToArray() { return list.ToArray(); }

		#region IList<T> Members

		public int IndexOf(T item) { return list.IndexOf(item); }

		public void Insert(int index, T item)
		{
			list.Insert(index, item);
			if (Added != null) Added(item, EventArgs.Empty);
		}

		public void RemoveAt(int index)
		{
			list.RemoveAt(index);
			if (Removed != null && index < list.Count) Removed(list[index], EventArgs.Empty);
		}

		public T this[int index]
		{
			get { return list[index]; }
			set
			{
				if (value == null && Removed != null) Removed(list[index], EventArgs.Empty);
				list[index] = value;
				if (Added != null) Added(value, EventArgs.Empty);
			}
		}

		#endregion

		#region ICollection<T> Members

		public void Add(T item)
		{
			list.Add(item);
			if (Added != null) Added(item, EventArgs.Empty);
		}

		public void Clear() { list.Clear(); }

		public bool Contains(T item) { return list.Contains(item); }

		public void CopyTo(T[] array, int arrayIndex) { list.CopyTo(array,arrayIndex); }

		public int Count { get { return list.Count; } }

		bool ICollection<T>.IsReadOnly { get { return (list as ICollection<T>).IsReadOnly; } }

		public bool Remove(T item)
		{
			if (Removed != null) Removed(item, EventArgs.Empty);
			return list.Remove(item);
		}

		#endregion

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator() { return list.GetEnumerator(); }

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion
	}
}
