using System.Collections.Generic;

namespace Aspire.Framework
{
	/// <summary>
	/// Interface implemented by objects which want to have control over which categories are
	/// shown expanded or collapsed when the object becomes browsable
	/// </summary>
	public interface IPropertyCategoryInitializer
	{
		/// <summary>
		/// Gets the categories to initialize to expanded or collapsed when an implementor is first displayed on a property grid
		/// </summary>
		/// <returns></returns>
		Dictionary<string, bool> GetCategoryStates();
	}
}
