
namespace Aspire.Framework
{
	/// <summary>
	/// A repository of published items referring to data, events and rules
	/// </summary>
	public partial class Blackboard
	{
		static Blackboard theBlackboard;

		private const System.Reflection.BindingFlags mBindingFlags =
			BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.Instance | BindingFlags.Static;


		Item Find(IPublishable parent)
		{
			Item item;
			if (mItemsByFullPath.TryGetValue(parent.Path, out item))
				return item;
			return null;
		}

		/// <summary>
		/// Gets an item using its full path.
		/// </summary>
		/// <param name="fullPath"></param>
		/// <returns></returns>
		public Item GetExistingItem(string fullPath)
		{
			Item item = null;
			mItemsByFullPath.TryGetValue(fullPath, out item);
			return item;
		}


		static Blackboard The
		{
			get
			{
				if (theBlackboard == null)
					theBlackboard = new Blackboard();
				return theBlackboard;
			}
		}

	}
}
