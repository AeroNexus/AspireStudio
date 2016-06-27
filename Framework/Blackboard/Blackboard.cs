using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Aspire.Utilities;
using Aspire.Utilities.Extensions;

namespace Aspire.Framework
{
	/// <summary>
	/// A namespace for advertizing objects in the running system to the user interface elements
	/// </summary>
	public partial class Blackboard
	{
		/// <summary>
		/// Allow read / write priveledge
		/// </summary>
		public enum Access {
			/// <summary>
			/// Allow the item to be read from / written to
			/// </summary>
			ReadWrite,
			/// <summary>
			/// Only allow read access
			/// </summary>
			ReadOnly
		};

		static ItemList mItems = new ItemList();
		static Dictionary<string, Item> mItemsByFullPath = new Dictionary<string, Item>();
		static Dictionary<Type, MemberInfo[]> mMemberInfoCache = new Dictionary<Type, MemberInfo[]>();
		static Dictionary<Type, MemberInfo[]> MemberInfoCache { get { return mMemberInfoCache; } }
		// For now
		static bool mAllowDuplicateItems = true;

		/// <summary>
		/// Event raised when Blackboard begins clearing its items
		/// </summary>
		public static event EventHandler Clearing;
		/// <summary>
		/// Event raised when Blackboard finishes clearing its items
		/// </summary>
		public static event EventHandler Cleared;

		/// <summary>
		/// Event raised when the BlackboardView finished its initial populate after a clear
		/// </summary>
		public static event EventHandler ViewPopulated;

		/// <summary>
		/// Flag indicating that the BlackboardView has been populated and is listening for ItemPublished events
		/// </summary>
		public static bool ViewHasPopulated;

		/// <summary>
		/// DisplayPropertiesChanged event handler signature
		/// </summary>
		/// <param name="item"></param>
		/// <param name="displayProperties"></param>
		public delegate void DisplayPropertiesHandler(Blackboard.Item item, IBlackboardDisplayProperties displayProperties);
		/// <summary>
		/// An item's display properties have changed
		/// </summary>
		public static event DisplayPropertiesHandler DisplayPropertiesChanged;

		/// <summary>
		/// A new item has been published
		/// </summary>
		public static event EventHandler ItemPublished;
		/// <summary>
		/// An existing item has been unpublished
		/// </summary>
		public static event EventHandler ItemUnpublished;

		private const System.Reflection.BindingFlags BindingFlags =
			System.Reflection.BindingFlags.FlattenHierarchy |
			System.Reflection.BindingFlags.Public |
			System.Reflection.BindingFlags.NonPublic |
			System.Reflection.BindingFlags.Instance |
			System.Reflection.BindingFlags.Static;

		/// <summary>
		/// Lists IAsyncItemProvider instances that have registered via RegisterAsyncItemProvider
		/// </summary>
		private static List<IAsyncItemProvider> mAsyncItemProviders = new List<IAsyncItemProvider>();

		/// <summary>
		/// PathSeparator is the character that will be used as the name separator when constructing the path to an item
		/// </summary>
		public const char PathSeparator = '.';

		/// <summary>
		/// PathParent is the character used to mean "the current item's parent"
		/// </summary>
		const char PathParent = '^';

		private static void AddArrayItem(Item parent, object owner, string path, Type typeHint, Array array, IValueInfo valueInfo, int[] indices, string memberName)
		{
			var elementItem = GetItem(owner, path, typeHint);
			elementItem.Owner = owner;
			elementItem.MemberName = memberName;
			elementItem.Indices = indices;
			elementItem.ValueInfo = valueInfo;
			elementItem.ValueArray = array;
			elementItem.ParentItem = parent;
			elementItem.Units = parent.Units;
			//elementItem.DefaultDisplayFormat = parent.DefaultDisplayFormat;
			elementItem.Description = parent.Description;
			//elementItem.ExtraAttributes = parent.ExtraAttributes;

			if (elementItem.Value is IPublishable)
			{
				IPublishable publisher = elementItem.Value as IPublishable;
				publisher.Name = elementItem.Path;
				Publish(publisher);
			}

			parent.Items.Add(elementItem);

			//FME If you publish here, you don't have to find them in Form1, line 179-224
			elementItem.IsPublished = true;
			if (ItemPublished != null)
				ItemPublished(elementItem, EventArgs.Empty);
		}

		static void AddArrayItems(Array array, object owner, string basePath, IValueInfo valueInfo, Item parent, string memberName)
		{
			// check the array's item to see if it already exists
			Item existingParent = GetExistingItem(basePath);
			if (existingParent != null)
				// if it does, clear its sub Items collection
				existingParent.Items.Clear();

			if (array == null)
				return;

			parent.IsArray = true;
			if (array.Rank == 1)
			{
				for (int i = 0; i < array.Length; i++)
				{
					string path = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0}[{1}]", basePath, i);
					Type typeHint = typeof(object);
					if (array.GetValue(i) != null)
						typeHint = array.GetValue(i).GetType();
					int[] indices = new int[1];
					indices[0] = i;

					AddArrayItem(parent, owner, path, typeHint, array, valueInfo, indices, memberName);
				}
			}
			else if (array.Rank == 2 && array.Length > 0)
			{
				int firstIndex = 0;
				int secondIndex = 0;

				for (int i = 0; i < array.Length; i++)
				{
					int[] indices = new int[2];
					indices[0] = firstIndex;
					indices[1] = secondIndex++;
					if (secondIndex > array.GetUpperBound(1))
					{
						firstIndex++;
						secondIndex = 0;
					}

					Type typeHint = typeof(object);
					if (array.GetValue(indices) != null)
					{
						typeHint = array.GetValue(indices).GetType();
					}
					string path = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0}[{1},{2}]", basePath, indices[0], indices[1]);
					AddArrayItem(parent, owner, path, typeHint, array, valueInfo, indices, memberName);
				}
			}
		}

		#region Binding

		/// <summary>
		/// Add a binding between two items
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		public static void AddBinding(Item source, Item destination)
		{
			if (mBindings == null)
			{
				if (Scenario.The.BlackboardBindings != null)
					mBindings = Scenario.The.BlackboardBindings;
				else
				{
					mBindings = new List<Binding>();
					Scenario.The.BlackboardBindings = mBindings;
				}
			}

			foreach (var bind in mBindings)
				if (bind.Destination == destination) return;
			//Log.WriteLine("Bind {0} to {1}", source, destination);
			var binding = new Binding() { Source = source, Destination = destination };
			mBindings.Add(binding);
		} static List<Binding> mBindings;

		#endregion

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
			return publisher.Parent.Path + PathSeparator + publisher.Name;
		}

		static void CheckForAdditionalItemsToPublish(object obj, Item item, BlackboardAttribute bbAttribute, string path)
		{
		}

		private static void CheckForArray(object obj, Type ownerType, object owner, string pathAbsolute, IValueInfo valueInfo, Item parent, string memberName=null)
		{
			if (ownerType.IsArray)
				AddArrayItems((Array)obj, owner, pathAbsolute, valueInfo, parent, memberName);
			else if (obj != null && obj is IHostArray)
				AddArrayItems((obj as IHostArray).HostedArray, owner, pathAbsolute,
					valueInfo, parent, memberName);
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
				mItems.ForEach(item => item.ClearValueInfo());
				mItems.Clear();
				mItemsByFullPath.Clear();
				//mMemberInfoCache.Clear();

				//if (m_DataItemProviders != null) m_DataItemProviders.Clear();

				ViewHasPopulated = false;
				if (Cleared != null)
					Cleared(null, EventArgs.Empty);
			}
		}

		/// <summary>
		/// When the next item is published, collect all subsequent items as sub items
		/// </summary>
		public static string CollectSubItems
		{
			set
			{
				if (value == null)
					mCollectParent = null;
				else
					mItemsByFullPath.TryGetValue(value, out mCollectParent);
			}
		} static Item mCollectParent;

		static void CollectSubItem(Item child)
		{
			int dot = child.Path.LastIndexOf('.');
			if (dot < 0) return;
			string branchName = child.Path.Substring(0, dot);
			if (branchName == mCollectParent.Path)
				//{
				mCollectParent.Items.Add(child);
			//	Log.WriteLine("{0} += {1}", mCollectParent.LeafName, child.LeafName);
			//}
			else
			{
				Item parent;
				if (mItemsByFullPath.TryGetValue(branchName, out parent))
				{
					parent.Items.Add(child);
					//Log.WriteLine("{0} += {1}; parent={0}", parent.LeafName, child.LeafName);
					mCollectParent = parent;
				}
			}
		}

		static Item FindOrAdd(IPublishable parent)
		{
			Item item;
			if (mItemsByFullPath.TryGetValue(parent.Path, out item))
				return item;
			item = new Item(parent.Path) { Owner = parent };
			return item;
		}

		/// <summary>
		/// Gets the absolute path to an item, given the relative path.
		/// <p>If <c>relativePath</c> starts with '.', then the path is relative to the current IPublishable</p>
		/// <p>If <c>relativePath</c> field name starts with '^', then the path is relative to the parent IPublishable. Recursion
		/// applies as in: "^.^.x" references the grandparent's "x" field</p>
		/// <p>Otherwise, the path is not relative</p>
		/// </summary>
		/// <param name="root">The <c>IPublishable</c> instance which hosts the <c>Blackboard.Item</c> whose path is specified in <c>relativePath</c></param>
		/// <param name="relativePath">The relative path to the <c>Blackboard.Item</c></param>
		/// <returns>A string representing the absolute path to the <c>Blackboard.Item</c>. This path can be passed to <see cref="GetExistingItem"/>, for example.</returns>
		/// <example>The following code uses <c>FullPath</c> to obtain the path to a <c>Blackboard.Item</c> hosted by an <c>IPublishable</c>.
		/// <code>
		///		class TestClass: IPublishable
		///		{
		///			public TestClass( IPublishable parent, string name )
		///		
		///		}
		///		
		///		TestClass parent = new TestClass( "Parent" );
		///		Blackboard.Publish( parent );
		///	
		///		TestClass child = new TestClass( parent, "Child" );
		///		Blackboard.Publish( child );
		///		
		///		string s = Blackboard.FullPath( child, "^.DottedEntry.Dotted" );
		///		Console.WriteLine( s );
		///		
		///		s = Blackboard.FullPath( child, "^.^.DottedEntry.Dotted" );
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
			else if (relativePath[start] == PathSeparator && start > 0)
				path = BuildPath(root) + relativePath.Substring(start);
			else if (relativePath[start] == '.')
				path = BuildPath(root) + relativePath;
			else
				path = relativePath;

			return path;
		}

		/// <summary>
		/// Returns the <c>Item</c> specified if it exists in the dictionary; null otherwise. Unlike the <see cref="Subscribe(string)"/> method,
		/// this method will not create a new <c>Item</c> if the specified path does not exist. 
		/// </summary>
		/// <param name="fullPath">The <c>Path</c> value of the <c>Item</c> to retrieve</param>
		/// <returns>The <c>Item</c> specified if it exists; null otherwise.</returns>
		/// <example>The following code will attempt to retrieve an existing <c>Item</c> from the dictionary.
		/// <code>
		///		Blackboard.Item item = Blackboard.GetExistingItem( "Sample.TestPath" );
		/// </code></example>
		public static Item GetExistingItem(string fullPath)
		{
			Item item = null;
			mItemsByFullPath.TryGetValue(fullPath, out item);
			return item;
		}

		private static Item GetItem(object owner, string pathAbsolute, Type typeHint)
		{
			Item item = null;
			mItemsByFullPath.TryGetValue(pathAbsolute, out item);

			if (item == null)
			{
				item = new Item(pathAbsolute) { Owner = owner };
				lock (mItemsByFullPath)
				{
					mItemsByFullPath.Add(pathAbsolute, item);
					mItems.Add(item);
				}
			}
			return item;
		}

		/// <summary>
		/// Get a cached list of members. Recursively get base class members. This lets derived classes
		/// get at private published base class members. Cache the results.
		/// </summary>
		/// <param name="type">Top level class</param>
		/// <param name="bf">Binding flags</param>
		/// <param name="declaredOnly">Don't recurse. Only get the members at the top class and don't cache.</param>
		/// <returns></returns>
		private static MemberInfo[] GetMemberInfo(Type type, BindingFlags bf, bool declaredOnly)
		{
			var list = new List<MemberInfo>();
			MemberInfo[] members;
			if (declaredOnly)
			{
				members = DataInfo.GetMembers(type, bf);

				foreach (MemberInfo info in members)
					if (Attribute.IsDefined(info, typeof(BlackboardAttribute), true))
						list.Add(info);
				return list.ToArray();
			}

			MemberInfo[] mia;
			lock (mMemberInfoCache)
			{
				if (mMemberInfoCache.TryGetValue(type, out mia)) return mia;

				bf &= ~BindingFlags.FlattenHierarchy;
				bf |= BindingFlags.DeclaredOnly;
				members = DataInfo.GetMembers(type, bf);

				foreach (MemberInfo info in members)
					if (Attribute.IsDefined(info, typeof(BlackboardAttribute), true))
						list.Add(info);
				if (type.BaseType != typeof(object))
				{
					MemberInfo[] baseMembers = GetMemberInfo(type.BaseType, bf, declaredOnly);
					foreach (MemberInfo mib in baseMembers)
					{
						bool already = false;
						foreach (MemberInfo mi in list)
							if (mi.Name == mib.Name)
							{
								already = true;
								break;
							}
						if (!already)
							list.Add(mib);
					}
				}

				mia = list.ToArray();
				mMemberInfoCache.Add(type, mia);
			}
			return mia;
		}

		static string GetPathFromAttribute(string basePath, BlackboardAttribute attr, string defaultName)
		{
			var name = attr.EntryName;
			if (name.Length == 0)
			{
				name = defaultName;
			}
			return string.Format(System.Globalization.CultureInfo.InvariantCulture,
				"{0}{1}{2}",
				basePath, PathSeparator, name);
		}

		//static string GetPathFromAttribute(IPublisher owner, DataDictionaryEntryAttribute attr, string defaultName)
		//{
		//	string basePath = ConstructPathToPublisher(owner);
		//	return GetPathFromAttribute(basePath, attr, defaultName);
		//}

		/// <summary>
		/// The items on the Blackboard
		/// </summary>
		public static List<Item> Items { get { return mItems; } }
		/// <summary>
		/// The items on the Blackboard
		/// </summary>
		public static Dictionary<string, Item> ItemsByFullPath { get { return mItemsByFullPath; } }

		/// <summary>
		/// Number of items published on the Blackboard
		/// </summary>
		public static int ItemsCount { get { return mItems.Count; } }

		/// <summary>
		/// Verify an item with the async providers
		/// </summary>
		/// <param name="item"></param>
		public static void ProviderVerify(Item item)
		{
			string path = item.Path;
			foreach (var provider in mAsyncItemProviders.Where(prov => prov.Matches(path)))
			{
				try
				{
					provider.Verify(path);
				}
				catch (Exception ex)
				{
					Log.ReportException(ex, "Error invoking an IAsyncItemProvider's Verify method");
				}
			}
		}

		/// <summary>
		/// Publish an <see cref="IPublishable"/> to the Blackboard
		/// </summary>
		/// <param name="publisher"></param>
		/// <returns></returns>
		public static Item Publish(IPublishable publisher)
		{
			var rootItem = PublishNew(publisher, Access.ReadOnly);

			publisher.Path = BuildPath(publisher);

			//var rootItem = new Item(publisher.Path);

			//if (rootItem == null) return rootItem;

			//var bf = BindingFlags;
			//object[] attrs = new object[0];//owner.GetType().GetCustomAttributes(typeof(PublisherInfoAttribute), false);
			////if (attrs.Length > 0)
			////{
			////	if ((attrs[0] as PublisherInfoAttribute).SuppressBaseClassItems)
			////	{
			////		bf |= BindingFlags.DeclaredOnly;
			////	}
			////}
			//var members = GetMemberInfo(publisher.GetType(), bf, attrs.Length > 0);
			//foreach (var info in members)
			//{
			//	var attr = Attribute.GetCustomAttribute(info, typeof(BlackboardAttribute), true) as BlackboardAttribute;

			//	string path = GetPathFromAttribute(publisher.Name, attr, info.Name);

			//	var item = Publish(publisher, path, info.Name, info);
			//	if (item != null)
			//		rootItem.Items.Add(item);
			//}

			return rootItem;
		}

		/// <summary>
		/// Publish an ownder member variable
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="path"></param>
		/// <param name="memberName"></param>
		/// <param name="memInfo"></param>
		/// <param name="publish"></param>
		/// <returns></returns>
		public static Item Publish(object owner, string path, string memberName, MemberInfo memInfo=null, bool publish=true)
		{
			Item item = null;
			if (mItemsByFullPath.TryGetValue(path, out item))
			{
				if (item.mHow == Item.How.Null)
					item.mHow = Item.How.Nondescrip;

				if (item.ValueInfo != null)
				{
					if (!mAllowDuplicateItems)
						return item;
				}
			}
			MemberInfo memberInfo;
			BlackboardAttribute bbAttribute = null;
			if (memInfo != null)
			{
				memberInfo = memInfo;
				bbAttribute = Attribute.GetCustomAttribute(memberInfo, typeof(BlackboardAttribute), true) as BlackboardAttribute;
			}
			else
			{
				MemberInfo[] infos = owner.GetType().FindMembers(
					MemberTypes.Field | MemberTypes.Property,
					BindingFlags, System.Type.FilterName, memberName);
				if (infos.Length == 0)
				{
					throw new MissingMemberException(owner.GetType().Name, memberName);
				}

				memberInfo = infos[0];
				if (Attribute.IsDefined(memberInfo, typeof(BlackboardAttribute), true))
				{
				}
			}
			IValueInfo valueInfo = null;

			if (memberInfo.MemberType == MemberTypes.Field)
			{
				var info = memberInfo as FieldInfo; // owner.GetType().GetField( memberName, BindingFlags);
				if (info != null)
				{
					if (item == null)
						item = GetItem(owner, path, info.FieldType);

					bool readOnly = false;
					if ((info.Attributes & FieldAttributes.Literal) == FieldAttributes.Literal)
						readOnly = true;

					valueInfo = new FieldValueInfo(info, owner, readOnly);
					item.ValueInfo = valueInfo;

					item.IsPublished = publish;
					item.AddAttributeInfo(info);

					if (info.FieldType != null)
					{
						object obj = info.GetValue(owner);
						CheckForArray(obj, obj != null ? obj.GetType() : info.FieldType, owner, path, valueInfo, item, memberName);
						CheckForAdditionalItemsToPublish(obj, item, bbAttribute, path);
					}
				}
			}
			else if (memberInfo.MemberType == MemberTypes.Property)
			{
				var info = memberInfo as PropertyInfo; /* owner.GetType().GetProperty(
					memberName,
					BindingFlags.Public |
					BindingFlags.NonPublic |
					BindingFlags.Instance
					); */
				if (info != null)
				{
					if (!info.CanRead)
						Log.WriteLine("DataDictionary.Publish", Log.Severity.Warning, "An attempt was made to publish a write-only data item: {0}. There will be no way to retrieve the item's value.", path);

					if (item == null)
						item = GetItem(owner, path, info.PropertyType);
					valueInfo = new PropertyValueInfo(info, owner);

					item.ValueInfo = valueInfo;
					item.IsPublished = publish;
					item.AddAttributeInfo(info);

					if (info.PropertyType != null)
					{
						object obj = null;
						if (info.TryGetValue(owner, null, out obj))
						{
							CheckForArray(obj, obj != null ? obj.GetType() : info.PropertyType, owner, path, valueInfo, item, memberName);
							CheckForAdditionalItemsToPublish(obj, item, bbAttribute, path);
						}
					}
				}
			}

			if (valueInfo != null)
			{
				object value = null;
				try
				{
					value = valueInfo.Value;
				}
				catch (Exception ex)
				{
					Log.ReportException(ex, "Blackboard.Publish: Exception getting valueInfo.Value on member {0}:", memberName);
				}

				if (value != null && !(value is IHostArray))
				{
					var valueType = valueInfo.Value.GetType();
					if (!valueType.IsPrimitive && !valueType.IsArray && !valueType.IsEnum && (valueType.IsClass || valueType.IsValueType))
					{
						var members = DataInfo.GetMembers(valueType, BindingFlags);
						foreach (var subinfo in members)
						{
							if (Attribute.IsDefined(subinfo, typeof(BlackboardAttribute), true))
							{
								BlackboardAttribute attr = Attribute.GetCustomAttribute(subinfo, typeof(BlackboardAttribute), true) as BlackboardAttribute;
								Item newItem = Publish(valueInfo.Value,
									GetPathFromAttribute(path, attr, subinfo.Name), subinfo.Name, subinfo);
								newItem.ParentItem = item;

								item.Items.Add(newItem);
								newItem.IsPublished = publish;
								newItem.AddAttributeInfo(subinfo);
							}
						}
					}
				}
			}

			if (item != null)
			{
				item.MemberName = memberName;
				item.Owner = owner;

				if (mCollectParent != null)
					CollectSubItem(item);

				if (publish && ItemPublished != null)
					ItemPublished(item, EventArgs.Empty);
			}

			return item;
		}

		/// <summary>
		/// Publish an object at a specified path
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="path"></param>
		/// <param name="valueInfo"></param>
		/// <returns></returns>
		public static Item Publish(object owner, string path, IValueInfo valueInfo)
		{
			Item item = null;
			mItemsByFullPath.TryGetValue(path, out item);
			if (item != null)
			{
				if (item.ValueInfo != null)
				{
					 if (!mAllowDuplicateItems)
						return item;
				}
			}
			else
				item = GetItem(owner, path, typeof(object));

			item.Owner = owner;
			item.ValueInfo = valueInfo;
			item.IsPublished = true;
			if (mCollectParent != null)
				CollectSubItem(item);
			if (ItemPublished != null)
				ItemPublished(item, EventArgs.Empty);

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
			var item = new Item(fullPath) { Owner = owner };
			mItems.Add(item);
			mItemsByFullPath.Add(fullPath, item);
			mItems.Add(item);
			if (ItemPublished != null) ItemPublished(item, EventArgs.Empty);
			return item;
		}

		/// <summary>
		/// Add an item to the Blackboard
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <param name="valueInfo"></param>
		/// <returns></returns>
		public static Item Publish(IPublishable parent, string name, IValueInfo valueInfo)
		{
			var parentItem = FindOrAdd(parent);
			var fullPath = BuildPath(parent) + PathSeparator + name;
			var item = new Item(fullPath) { Owner = parent, ValueInfo = valueInfo };
			parentItem.Items.Add(item);
			mItemsByFullPath.Add(fullPath, item);
			if (ItemPublished != null) ItemPublished(item, EventArgs.Empty);
			return item;
		}

		static Item PublishNew(IPublishable root, Access access)
		{
			root.Path = BuildPath(root);
			Item item = null;
			if ( mItemsByFullPath.TryGetValue(root.Path, out item) )
			{
				if ( item.ValueInfo != null )
				{
					if (!mAllowDuplicateItems)
						return item;
				}
				item.ValueInfo = new ObjectValueInfo(root,access==Access.ReadOnly);
			}
			else
			{
				var vi = new ObjectValueInfo(root,access==Access.ReadOnly);
				item = new Item(root.Path) { ValueInfo = vi };
				lock (mItemsByFullPath)
				{
					mItemsByFullPath.Add(root.Path, item);
					mItems.Add(item);
				}
				if (item.Value != null)
					CheckForArray(vi.Value, vi.Value.GetType(), vi, root.Path, vi, item);
			}

			if (item == null) return item;

			item.IsPublished = true;

			// Add itemAttributes as method arg
			//if (itemAttributes != null)
			//{
			//  item.IsPublished = publish;
			//	item.AddAttributeInfo(itemAttributes);
			//	foreach (var subItem in item.Items)
			//  {
			//      subItem.IsPublished = publish;
			//		subItem.AddAttributeInfo(itemAttributes);
			//  }
			//}

			if (ItemPublished != null) ItemPublished(item, EventArgs.Empty);

			object value = item.Value;

			if (value == null) return item;

			var bf = BindingFlags;

			// Suppress base class items
			object[] attrs = new object[0];
			//if (attrs.Length > 0)
			//{
			//	if ((attrs[0] as PublisherInfoAttribute).SuppressBaseClassItems)
			//		bf |= BindingFlags.DeclaredOnly;
			//}

			var members = GetMemberInfo(value.GetType(), bf, attrs.Length > 0);
			foreach (MemberInfo info in members)
			{
				var attr = Attribute.GetCustomAttribute(info, typeof(BlackboardAttribute), true) as BlackboardAttribute;

				var path = GetPathFromAttribute(root.Path, attr, info.Name);

				var subItem = Publish(value, path, info.Name, info);
				if (subItem != null)
					item.Items.Add(subItem);
			}

			return item;
		}

		internal static void RaiseDisplayPropertiesChanged(Blackboard.Item item, IBlackboardDisplayProperties displayProperties)
		{
			if (DisplayPropertiesChanged != null)
				DisplayPropertiesChanged(item, displayProperties);
		}

		internal static void RaisePublished(Blackboard.Item item)
		{
			if (ItemPublished != null)
				ItemPublished(item, EventArgs.Empty);
		}

		/// <summary>
		/// Allow the BlackboardView to indicate that all Published items are now in the view
		/// </summary>
		public static void RaiseViewPopulated()
		{
			ViewHasPopulated = true;
			if (ViewPopulated != null)
				ViewPopulated(null, EventArgs.Empty);
		}

		/// <summary>
		/// Register an <see cref="IAsyncItemProvider"/> with the <see cref="Blackboard"/>. 
		/// The provider will be notified when a subscription fails for a path that starts with the provider's 
		/// <see cref="IAsyncItemProvider.Matches"/>.
		/// </summary>
		/// <param name="provider">The <see cref="IAsyncItemProvider"/> being registered.</param>
		/// <returns>The number of <see cref="IAsyncItemProvider"/> instances now registered.</returns>
		public static int RegisterAsyncItemProvider(IAsyncItemProvider provider)
		{
			if (mAsyncItemProviders == null)
				mAsyncItemProviders = new List<IAsyncItemProvider>();

			if (!mAsyncItemProviders.Contains(provider))
			{
				bool added = false;
				// Check for a more specific provider and re-allocate items
				for (int i = 0; i < mAsyncItemProviders.Count; i++)
				{
					var p = mAsyncItemProviders[i];
					if (p.Matches(provider.Prefix))
					{
						p.TransferItemsTo(provider);
						mAsyncItemProviders.Insert(i, provider);
						added = true;
						break;
					}
				}
				if ( !added ) mAsyncItemProviders.Add(provider);
			}

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
					foreach (var provider in mAsyncItemProviders.Where(prov => prov.Matches(path)))
					{
						try
						{
							provider.Provide(path);
						}
						catch (Exception ex)
						{
							Log.ReportException(ex, "Error invoking an IAsyncItemProvider's Provide method");
						}
					}
				}
			}

			if (item.ValueInfo == null && defaultValue != null)
				item.ValueInfo = new ObjectValueInfo(defaultValue, null);
			item.RaiseSubscribedTo();
			return item;
		}

		/// <summary>
		/// Subscribe to the specified item and use the provided default value if the item has not yet been published.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static Item Subscribe(string path, IObjectInfo defaultValue = null)
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
		/// <seealso cref="FullPath"/>
		public static Item Subscribe(IPublishable root, string path, IObjectInfo defaultValue = null)
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
		public static Item Subscribe(IPublishable root, string path, Type typeHint, IObjectInfo defaultValue = null)
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
		/// <seealso cref="FullPath"/>
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
		/// <seealso cref="FullPath"/>
		public static Item Subscribe(IPublishable root, string path)
		{
			return Subscribe(FullPath(root, path));
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
			
			if ( !mItemsByFullPath.TryGetValue(path, out item) )
			{
				item = Subscribe(path, typeof(object), null);
				if (publish)
				{
					item.IsPublished = true;
				}
			}
			item.RaiseSubscribedTo();
			return item;
		}

		/// <summary>
		/// Subscribe to the specified item.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		static Item SubscribeExisting(string path)
		{
			Item item = null;
			
			mItemsByFullPath.TryGetValue(path, out item);

			return item;
		}

		/// <summary>
		/// Unpublish the specified <see cref="Blackboard.Item"/>
		/// </summary>
		/// <param name="item">The <see cref="Blackboard.Item"/> to be unpublished.</param>
		/// <returns>True if successful; false otherwise</returns>
		public static Item Unpublish(Blackboard.Item item)
		{
			if (item != null)
			{
				// remove from Hashtable
				lock (mItemsByFullPath)
				{
					mItemsByFullPath.Remove(item.Path);
					mItems.Remove(item);
				}

				item.Items.ForEach(subItem => Unpublish(subItem));

				item.Unpublish();
			}
			return item;
		}

		/// <summary>
		/// Unpublish the <see cref="Blackboard.Item"/> specified by the given path.
		/// </summary>
		/// <param name="path">The path to the <see cref="Blackboard.Item"/> to be unpublished</param>
		/// <returns>True if successful; false otherwise</returns>
		/// <exception cref="ArgumentNullException">Raised if the path parameter is null.</exception>
		public static Item Unpublish(string path)
		{
			if (path == null || path.Length == 0) return null;

			var item = GetExistingItem(path);
			return Unpublish(item);
		}
	}
}
