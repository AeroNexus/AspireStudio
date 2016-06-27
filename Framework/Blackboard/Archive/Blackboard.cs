using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Aspire.Utilities;

namespace Aspire.Framework
{
	/// <summary>
	/// A repository of published items referring to data, events and rules
	/// </summary>
	public partial class Blackboard
	{
		static Blackboard theBlackboard;
		static List<Item> mItems = new List<Item>();
		static Dictionary<string, Item> mItemsByFullPath = new Dictionary<string, Item>();
		static Dictionary<Type, MemberInfo> mMemberInfoCache = new Dictionary<Type, MemberInfo>();

		/// <summary>
		/// Lists IAsyncItemProvider instances that have registered via RegisterAsyncItemProvider
		/// </summary>
		private static List<IAsyncItemProvider> mAsyncItemProviders = new List<IAsyncItemProvider>();

		private const System.Reflection.BindingFlags mBindingFlags =
			BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.Instance | BindingFlags.Static;

		/// <summary>
		/// PathSeparator is the character that will be used as the name separator when constructing the path to an item
		/// </summary>
		public const char PathSeparator = '.';

		/// <summary>
		/// PathParent is the character used to mean "the current item's parent"
		/// </summary>
		private const char PathParent = '^';

		/// <summary>
		/// Event raised when Blackboard begins clearing its items
		/// </summary>
		public static event EventHandler Clearing;
		/// <summary>
		/// Event raised when Blackboard finishes clearing its items
		/// </summary>
		public static event EventHandler Cleared;


		/// <summary>
		/// Build the full path 
		/// </summary>
		/// <param name="publisher"></param>
		/// <returns></returns>
		public static string BuildPath(IPublishable publisher)
		{
			if (publisher == null)
				throw new ArgumentNullException("publisher", "'publisher' cannot be null");
			if (publisher.Parent == null)
				return publisher.Name;
			else if (publisher.Parent.Path == null)
			{
				// Build parent Path
				IPublishable parent = publisher.Parent;
				System.Text.StringBuilder path = new System.Text.StringBuilder(parent.Name);
				parent = parent.Parent;
				while (parent != null)
				{
					path.Insert(0, PathSeparator);
					path.Insert(0, parent.Name);
					parent = parent.Parent;
				}
				parent.Path = path.ToString();
			}
			return publisher.Parent.Path + '.' + publisher.Name;
		}

		/// <summary>
		/// Clears the Blackboard and sets all of the value references to null.
		/// </summary>
		public static void Clear()
		{
            if (Clearing != null)
				Clearing(null, EventArgs.Empty);

			lock (mItems)
			{
				mItems.ForEach(item => item.ValueInfo = null);
				mItems.Clear();

				mMemberInfoCache.Clear();

				//if (m_DataItemProviders != null) m_DataItemProviders.Clear();

				if (Cleared != null)
					Cleared(null, EventArgs.Empty);
			}
		}

		Item Find(IPublishable parent)
		{
			Item item;
			if (mItemsByFullPath.TryGetValue(parent.Path, out item))
				return item;
			return null;
		}

		Item FindOrAdd(IPublishable parent)
		{
			Item item;
			if (mItemsByFullPath.TryGetValue(parent.Path, out item))
				return item;
			item = new Item(parent.Path);
			item.Owner = parent;
			return item;
		}

		/// <summary>
		/// Gets the absolute path to an item, given the relative path.
		/// <p>If <c>relativePath</c> starts with '.', then the path is relative to the current component</p>
		/// <p>If <c>relativePath</c> field name starts with '@', then the path is relative to the parent component. Recursion
		/// applies as in: "@.@.x" references the grandparent's "x" field</p>
		/// <p>Otherwise, the path is not relative</p>
		/// </summary>
		/// <param name="root">The <c>IPublishable</c> instance which hosts the <c>DataItem</c> whose path is specified in <c>relativePath</c></param>
		/// <param name="relativePath">The relative path to the <c>DataItem</c></param>
		/// <returns>A string representing the absolute path to the <c>DataItem</c>. This path can be passed to <see cref="GetExistingItem"/>, for example.</returns>
		/// <example>The following code uses <c>AbsolutePath</c> to obtain the path to a <c>DataItem</c> hosted by an <c>IPublishable</c>.
		/// <code>
		///		class TestClass: IPublishable
		///		{
		///			public TestClass( IPublishable parent, string name )
		///			{
		///				m_Name = name;
		///				m_Parent = parent;
		///			}
		///			
		///			[DataDictionaryEntry("DottedEntry.Dotted")]
		///			public double TestDotted = 34;
		///			
		///			private string m_Name = string.Empty;
		///			public string Name
		///			{ get{ return m_Name; } set { m_Name = value; } } 
		///			
		///			private IPublishable m_Parent = null;
		///			public IPublishable Parent
		///			{ get{ return m_Parent; } }
		///		}
		///		
		///		TestClass parent = new TestClass( "Parent" );
		///		DataDictionary.Publish( parent, true );
		///	
		///		TestClass child = new TestClass( parent, "Child" );
		///		DataDictionary.Publish( child, true );
		///		
		///		string s = DataDictionary.AbsolutePath( child, "@.DottedEntry.Dotted" );
		///		Console.WriteLine( s );
		///		
		///		s = DataDictionary.AbsolutePath( child, "@.@.DottedEntry.Dotted" );
		///		Console.WriteLine( s );
		/// </code>
		/// This will output
		/// <code>
		/// Child.DottedEntry.Dotted
		/// Parent.Child.DottedEntry.Dotted
		/// </code>
		/// </example>
		public static string FullPath(IPublishable root, string relativePath)
		{
			if (root == null)
				return relativePath;

			int start = 0;
			if (relativePath.Length > 0)
			{
				while (relativePath[start] == PathParent)
				{
					root = root.Parent;
					start++;

					if (start + 1 >= relativePath.Length)
						break;
					if (relativePath[start + 1] == PathParent)
					{
						start++;
					}
				}
			}

			string path = string.Empty;

			if (start == relativePath.Length)
				path = BuildPath(root);
			else if (relativePath[start] == '.' && start > 0)
				path = BuildPath(root) + relativePath.Substring(start);
			else if (relativePath[start] == '.')
				path = BuildPath(root) + relativePath;
			else
				path = relativePath;

			return path;
		}

		private static Item GetItem(object owner, string pathAbsolute, Type typeHint)
		{
			Item item = null;
			mItemsByFullPath.TryGetValue(pathAbsolute, out item);

			if (item == null)
			{
				if (typeHint == typeof(double) || typeHint == typeof(double[]))
					item = new DoubleItem(pathAbsolute);
				else
					item = new Item(pathAbsolute);
				item.Owner = owner;
				lock (mItemsByFullPath)
					mItemsByFullPath[pathAbsolute] = item;
			}
			return item;
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

		/// <summary>
		/// Number of items published on the Blackboard
		/// </summary>
		public int ItemsCount { get { return mItems.Count; } }

		/// <summary>
		/// Publish an <see cref="IPublishable"/> to the Blackboard
		/// </summary>
		/// <param name="publisher"></param>
		/// <returns></returns>
		public static Item Publish(IPublishable publisher)
		{
			publisher.Path = BuildPath(publisher);

			var item = new Item(publisher.Path);
			item.Owner = publisher;
			mItems.Add(item);
			mItemsByFullPath.Add(publisher.Path, item);
			return item;
		}

		/// <summary>
		/// Add an Item to the Blackboard
		/// </summary>
		/// <param name="fullPath"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static Item Publish(string fullPath, object owner)
		{
			var item = new Item(fullPath);
			item.Owner = owner;
			mItems.Add(item);
			mItemsByFullPath.Add(fullPath, item);
			return item;
		}

		/// <summary>
		/// Add an item to the Blackboard
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static Item Publish(IPublishable parent, string name, object owner)
		{
			var parentItem = The.FindOrAdd(parent);
			var fullPath = BuildPath(parent) + '.' + name;
			var item = new Item(fullPath);
			item.Owner = owner;
			parentItem.Items.Add(item);
			mItemsByFullPath.Add(fullPath, item);
			return item;
		}

		/// <summary>
		/// Register an <see cref="IAsyncItemProvider"/> with the <see cref="Blackboard"/>. 
		/// The provider will be notified when a subscription fails for a path that starts with the provider's 
		/// <see cref="IAsyncItemProvider.Prefix"/>.
		/// </summary>
		/// <param name="provider">The <see cref="IAsyncItemProvider"/> being registered.</param>
		/// <returns>The number of <see cref="IAsyncItemProvider"/> instances now registered.</returns>
		public static int RegisterAsyncItemProvider(IAsyncItemProvider provider)
		{
			if (mAsyncItemProviders == null)
				mAsyncItemProviders = new List<IAsyncItemProvider>();

			if (!mAsyncItemProviders.Contains(provider))
				mAsyncItemProviders.Add(provider);

			return mAsyncItemProviders.Count;
		}

		/// <summary>
		/// Subscribe to the specified item and use the provided default value if the item has not yet been published.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="typeHint"></param>
		/// <param name="defaultValue"></param>
		/// <returns>The item or null</returns>
		public static Item Subscribe(string path, Type typeHint, IObjectInfo defaultValue = null)
		{
			Item item = null;
			if (!mItemsByFullPath.TryGetValue(path, out item))
			{
				item = GetItem(null, path, typeHint);
				if (mAsyncItemProviders != null)
				{
					foreach (var provider in mAsyncItemProviders.Where(prov => path.StartsWith(prov.Prefix)))
					{
						try
						{
							provider.Provide(path);
						}
						catch (Exception ex)
						{
							MsgConsole.ReportException(ex, "Error invoking an IAsyncItemProvider's Provide method");
						}
					}
				}
			}

			if (item.ValueInfo == null && defaultValue != null)
				item.ValueInfo = new ObjectValueInfo(defaultValue, null);
			return item;
		}

		/// <summary>
		/// Subscribe to the specified item and use the provided default value if the item has not yet been published.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static Item Subscribe(string path, IObjectInfo defaultValue=null)
		{
			return Subscribe(path, typeof(Object), defaultValue);
		}

		/// <summary>
		/// Subscribe to the specified item using relative paths and use the provided default value if the item has not yet been published. 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="path"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		/// <seealso cref="AbsolutePath"/>
		public static Item Subscribe(IPublishable root, string path, IObjectInfo defaultValue=null)
		{
			return Subscribe(FullPath(root, path), typeof(Object), defaultValue);
		}

		/// <summary>
		/// Subscribe to an Item using a path relative to the root IPublishable. Use the provided default value if the item has not yet been published
		/// </summary>
		/// <param name="root"></param>
		/// <param name="path"></param>
		/// <param name="typeHint"></param>
		/// <param name="defaultValue"></param>
		/// <returns>The item or null</returns>
		/// <seealso cref="FullPath"/>
		public static Item Subscribe(IPublishable root, string path, Type typeHint, IObjectInfo defaultValue=null)
		{
			return Subscribe(FullPath(root, path), typeHint, defaultValue);
		}

		/// <summary>
		/// Subscribe to the specified item using a relative path. 
		/// </summary>
		/// <param name="root">The IPublishable item that is the root of the relative path</param>
		/// <param name="path"></param>
		/// <param name="typeHint"></param>
		/// <returns></returns>
		/// <seealso cref="AbsolutePath"/>
		public static Item Subscribe(IPublishable root, string path, Type typeHint)
		{
			return Subscribe(FullPath(root, path), typeHint);
		}

		/// <summary>
		/// Subscribe to the specified item using a relative path. 
		/// </summary>
		/// <param name="root">The IPublishable item that is the root of the relative path</param>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <seealso cref="AbsolutePath"/>
		public static Item Subscribe(IPublishable root, string path)
		{
			return null;// Subscribe(AbsolutePath(root, path));
		}

		/// <summary>
		/// Subscribe to the specified item.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static Item Subscribe(string path)
		{
			return Subscribe(path, typeof(object), null);
		}

		/// <summary>
		/// Subscribe to the specified item and publish if new.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="publish"></param>
		/// <returns></returns>
		public static Item Subscribe(string path, bool publish)
		{
			Item item = null;
			mItemsByFullPath.TryGetValue(path, out item);
			if (item == null)
			{
				item = Subscribe(path, typeof(object), null);
				if (publish)
				{
					item.IsPublished = true;
					//if (DataItemPublished != null)
					//{
					//	DataItemPublished(null, new DataDictionaryEventArgs(item));
					//}
				}
			}
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
