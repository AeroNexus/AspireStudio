using System;

namespace Aspire.Framework
{
	/// <summary>
	/// All Managers in the system
	/// </summary>
	public interface IManager
	{
		/// <summary>
		/// The manager is unloading
		/// </summary>
		void Unload();
	}
}
