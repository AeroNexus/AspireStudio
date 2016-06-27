using System;

namespace Aspire.Framework
{
	/// <summary>
	/// BlackboardActionAttribute is an <see cref="Attribute"/> class for classes that act upon a <see cref="Blackboard.Item"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class BlackboardActionAttribute : Attribute
	{
		/// <summary>
		/// Construct an instance using the provided <see cref="MenuText"/>
		/// </summary>
		/// <param name="menuText"></param>
		public BlackboardActionAttribute(string menuText)
		{
			MenuText = menuText;
		}

		private string m_MenuText;

		///<summary>
		///Gets and sets the value for MenuText
		///</summary>
		public string MenuText
		{
			get { return m_MenuText; }
			set { m_MenuText = value; }
		}
	}
}
