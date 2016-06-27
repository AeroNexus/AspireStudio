using System;

namespace Aspire.Framework
{
	/// <summary>
	/// ItemCategoryAttribute is used to add a <see cref="Blackboard.Item"/> to a category. Categories provide a way to organize data into various buckets.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true,Inherited=true)]
	public class ItemCategoryAttribute : Attribute
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="category">The name of the category</param>
		public ItemCategoryAttribute( string category )
		{
			Category = category;
		}

		/// <summary>
		/// The name of the category
		/// </summary>
		public string Category{get; private set;}
	}
}
