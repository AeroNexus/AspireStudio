using System;
using System.Collections.Generic;

namespace Aspire.Framework
{
	/// <summary>
	/// External definition of actions that can be added to a blackboard item's context menu
	/// </summary>
	public class BlackboardAction
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public BlackboardAction()
		{
			int i = 0;
			foreach (var action in mActions)
			{
				if (action.Name.CompareTo(Name) > 0)
				{
					mActions.Insert(i, this);
					return;
				}
				i++;
			}
			mActions.Add(this);
		}

		/// <summary>
		/// Create an instance with the specified string for a name
		/// </summary>
		/// <param name="name"></param>
		public BlackboardAction(string name)
		{
			Name = name;
			int i = 0;
			foreach (var action in mActions)
			{
				if (action.Name.CompareTo(name) > 0)
				{
					mActions.Insert(i, this);
					return;
				}
				i++;
			}
			mActions.Add(this);
		}

		static List<BlackboardAction> mActions = new List<BlackboardAction>();
		/// <summary>
		/// All actions discovered in the running system
		/// </summary>
		public static List<BlackboardAction> Actions { get { return mActions; } }

		/// <summary>
		/// Gets a Boolean indicating whether the action should be invoked, if Enabled, on double-click of the <see cref="Blackboard.Item"/>
		/// </summary>
		public virtual bool ExecuteOnDoubleClick
		{
			get { return(false); }
		}
		
		/// <summary>
		/// Gets a Boolean indicating whether the action should appear on the context menu
		/// </summary>
		public virtual bool ShowOnContextMenu		
		{
			get { return(true); }
		}

		/// <summary>
		/// Returns a boolean indicating whether the action should be visible for the given <see cref="Blackboard.Item"/>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="enabled"></param>
		/// <returns>True if the item is not null; false if it is null.</returns>
		public virtual bool Visible(Blackboard.Item item, out bool enabled)
		{
			enabled = item != null;
			return item != null;
		}

		/// <summary>
		/// This is a no-op.
		/// </summary>
		/// <param name="item"></param>
		public virtual void Execute(Blackboard.Item item)
		{
		}

		/// <summary>
		/// Moniker
		/// </summary>
		public virtual string Name { get; set; }
	}
}
