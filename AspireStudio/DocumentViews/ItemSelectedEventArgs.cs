using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aspire.Studio.DocumentViews
{
    /// <summary>
    /// When an item is selected in the gui, an ItemSelectedEventHandler event is raised. The ItemSelectedEventArgs are passed to the event listeners.
    /// </summary>
    public class ItemSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs an ItemSelectedEventArgs instance for the given item.
        /// </summary>
        /// <param name="item"></param>
        public ItemSelectedEventArgs(object item)
        {
            mSelectedItem = item;
        }

        private object mSelectedItem = null;

        /// <summary>
        /// The selected item.
        /// </summary>
        public object SelectedItem
        {
            get { return mSelectedItem; }
        }
    }

    /// <summary>
    /// Delegate for the ItemSelected event
    /// </summary>
    public delegate void ItemSelectedEventHandler(object sender, ItemSelectedEventArgs e);
}

