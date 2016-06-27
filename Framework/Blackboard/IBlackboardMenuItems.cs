using System;

namespace Aspire.Framework
{
	/// <summary>
	/// An implementor provides menu items for the Blackboard's context menu
	/// </summary>
	public interface IBlackboardMenuItems
	{
		/// <summary>
		/// The menu items an implementor provides for the Blackboard context menu
		/// </summary>
		string[] MenuItems { get; }

		/// <summary>
		/// A specific menu item has been clicked
		/// </summary>
		/// <param name="menuItemIndex"></param>
		void OnMenu(int menuItemIndex);
	}
}
