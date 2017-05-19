using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;

using Aspire.Utilities;
using System.Linq;

namespace Aspire.Framework
{

	public partial class Blackboard
	{
		/// <summary>
		/// An Item on the Blackboard
		/// </summary>
		public class Item
		{
			//static int tags;
			//public int tag = ++tags;

			internal enum How { Nondescrip, Enum, EnumString, IsArray, Null, ObjectInfo, ValueInfo };
			internal How mHow;

			/// <summary>
			/// Event raised when the item is expanded in a tree view
			/// </summary>
			public event System.EventHandler Expanded;

			/// <summary>
			/// Event raised when the item is subscribed to by a client.
			/// </summary>
			public event EventHandler SubscribedTo;

			/// <summary>
			/// Event raised when the underlying value is changed through calls to set_Value, etc.
			/// </summary>
			public event EventHandler ValueChanged;

			/// <summary>
			/// Default constructor
			/// </summary>
			public Item() { }

			/// <summary>
			/// Construct using a full path
			/// </summary>
			/// <param name="path"></param>
			internal Item(string path)
			{
				Path = path;
			}

			///// <summary>
			///// Indexer for a one-dimensional array item
			///// </summary>
			//internal object this[int index]
			//{
			//	get { return null; }
			//	set
			//	{
			//		mValueInfo[index] = value;
			//	}
			//}

			/// <summary>
			/// Decorate a DataItem with Attributes
			/// </summary>
			/// <param name="attribute"></param>
			public void AddAttributeInfo(BlackboardAttribute attribute)
			{
				if (attribute != null)
				{
					Description = attribute.Description;
					Units = attribute.Units;
					//DefaultDisplayFormat = attribute.DefaultDisplayFormat;
					//ExtraAttributes.Clear();
					//ExtraAttributes.Add(attribute.ValueCollection);
					if (mValueInfo != null)
						mValueInfo.IsReadOnly = attribute.IsReadOnly;
					if (!IsPublished)
					{
						Blackboard.RaisePublished(this);
						IsPublished = true;
					}
				}
			}

			/// <summary>
			/// Decorate an Item with Attributes, using the <see cref="DescriptionAttribute"/> if present and <see cref="BlackboardAttribute.Description"/> is empty
			/// </summary>
			/// <param name="memberInfo"></param>
			public void AddAttributeInfo(MemberInfo memberInfo)
			{
				if (Attribute.IsDefined(memberInfo, typeof(BlackboardAttribute), true))
				{
					var attribute = Attribute.GetCustomAttribute(memberInfo, typeof(BlackboardAttribute), true) as BlackboardAttribute;
					AddAttributeInfo(attribute);
				}

				//if (Attribute.IsDefined(memberInfo, typeof(MinMaxAttribute), true))
				//{
				//	MinMaxAttribute attribute = Attribute.GetCustomAttribute(memberInfo, typeof(MinMaxAttribute), true) as MinMaxAttribute;
				//	item.ExtraAttributes.Add(MinimumValueExtraAttributeName, attribute.Min.ToString());
				//	item.ExtraAttributes.Add(MaximumValueExtraAttributeName, attribute.Max.ToString());
				//	item.ExtraAttributes.Add(UseTrackbarEditorExtraAttributeName, attribute.UseTrackbarEditor.ToString());
				//	item.ExtraAttributes.Add(SignificantDigitsInEditorExtraAttributeName, attribute.SignificantDigitsInEditor.ToString());
				//}

				if (Description == null ||Description.Length == 0 && Attribute.IsDefined(memberInfo, typeof(DescriptionAttribute), true))
				{
					var attr = Attribute.GetCustomAttribute(memberInfo, typeof(DescriptionAttribute), true) as DescriptionAttribute;
					if (attr != null)
						Description = attr.Description;
				}
			}

			/// <summary>
			/// Add an item to a Properties view category name
			/// </summary>
			/// <param name="categoryName"></param>
			public void AddToCategory(string categoryName)
			{
			}

			/// <summary>
			/// The target datum the Item points to
			/// </summary>
			internal object Datum { get; set; }

			/// <summary>
			/// Description of the item
			/// </summary>
			public string Description { get; set; }

			/// <summary>
			/// The Display properties of an item, set by the item's provider
			/// </summary>
			public IBlackboardDisplayProperties DisplayProperties
			{
				set
				{
					//mDisplayProperties = value;
					Blackboard.RaiseDisplayPropertiesChanged(this, value);
				}
			} //IBlackboardDisplayProperties mDisplayProperties;

			/// <summary>
			/// Get the Array associated with an item
			/// </summary>
			/// <returns></returns>
			public Array GetArray()
			{
				return null;
			}

			internal int Index
			{
				get { return mIndex; }
				set { mIndex = value; }
			} int mIndex;

			/// <summary>
			/// Is the item an Array
			/// </summary>
			[Browsable(false)]
			public bool IsArray { get; set; }

			/// <summary>
			/// Does the item implement IArrayProxy
			/// </summary>
			[Browsable(false)]
			public bool IsArrayProxy { get { return Type.GetInterface("IArrayProxy") != null; } }

			/// <summary>
			/// Is the item a primitive type
			/// </summary>
			[Browsable(false)]
			public bool IsEnum { get { return mIsEnum; } }

			/// <summary>
			/// Is the item a primitive type
			/// </summary>
			[Browsable(false)]
			public bool IsPrimitive { get; set; }

			/// <summary>
			/// Has the item already been published
			/// </summary>
			[Browsable(false)]
			internal bool IsPublished { get; set; }

			bool mIsReflectingSubItemValueChanged;

			///<summary>
			///Gets and sets the value for IsReflectingSubItemValueChanged
			///</summary>
			[Browsable(false)]
			public bool IsReflectingSubItemValueChanged
			{
				get { return mIsReflectingSubItemValueChanged; }
				set
				{
					if (mIsReflectingSubItemValueChanged != value)
					{
						if (IsArray && ((Value != null && Value is IHostArray && (Value as IHostArray).HostedArray != null && (Value as IHostArray).HostedArray.GetType().GetElementType() != null &&
							(Value as IHostArray).HostedArray.GetType().GetElementType().IsPrimitive) || (Type != null && Type.GetElementType() != null && Type.GetElementType().IsPrimitive)))
						{
							mIsReflectingSubItemValueChanged = value;
							foreach (var item in Items)
							{
								if (value)
									item.ValueChanged += new EventHandler(SubItem_ValueChanged);
								else
									item.ValueChanged -= new EventHandler(SubItem_ValueChanged);
							}
						}
					}
				}
			}

			internal int[] Indices
			{
				get { return mIndicies; }
				set
				{
					mIndicies = value;
					mIndex = mIndicies[0];
				}
			} int[] mIndicies;

			internal Array ValueArray = null;

			/// <summary>
			/// An item's sub-items
			/// </summary>
			[Browsable(false)]
			public ItemList Items
			{
				get
				{
					if (mItems == null)
						mItems = new ItemList();
					return mItems;
				}
			} ItemList mItems;

			/// <summary>
			/// The simple name of an item, ignoring its antecedants path
			/// </summary>
			public string LeafName
			{
				get
				{
					int lastSep = Path.LastIndexOf(PathSeparator);
					if (lastSep > 0) return Path.Substring(lastSep + 1);
					return Path;
				}
			}

			/// <summary>
			/// The name of an items member name in a class
			/// </summary>
			internal string MemberName { get; set; }

			/// <summary>
			/// The last element of the item's full path. The relative name from the parent node
			/// </summary>
			internal string Name { get; set; }

			/// <summary>
			/// Containing object for this item's value info
			/// </summary>
			[Browsable(false)]
			public object Owner { get; set; }

			/// <summary>
			/// An item's parent item
			/// </summary>
			[Browsable(false)]
			public Item ParentItem { get; set; }

			/// <summary>
			/// The full path to an item, including all of its antecedants names
			/// </summary>
			[ReadOnly(true)]
			public string Path { get; set; }

			/// <summary>
			/// Allow a view to express interest in an item's details
			/// </summary>
			public void RaiseExpanded()
			{
				if (Expanded != null)
					Expanded(this, EventArgs.Empty);
			}

			/// <summary>
			/// Allow a view to express interest in an item's details
			/// </summary>
			public void RaiseSubscribedTo()
			{
				if (SubscribedTo != null)
					SubscribedTo(this, EventArgs.Empty);
			}
			/// <summary>
			/// Allow a subscriber to trigger the value changed event without going through set_Value.
			/// </summary>
			public void RaiseValueChanged()
			{
				if ( ValueChanged != null )
					ValueChanged( this, null );
			}

			private void SubItem_ValueChanged(object sender, EventArgs e)
			{
				RaiseValueChanged();
			}

			/// <summary>
			/// String equivalent
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return Path;
			}

			/// <summary>
			/// Get the underlying  Type
			/// </summary>
			public Type Type
			{
				get
				{
					if (mUnderlyingType == null)
					{
						if (Value != null)
							Type = Value.GetType();
						else
							mUnderlyingType = typeof(object);
					}
					return mUnderlyingType;
				}
				private set
				{
					mUnderlyingType = value;
					if (value != null)
					{
						mIsArray = Type.IsArray;// ||
												//(Value != null && Value is IHostArray);
						mIsEnum = Type.IsEnum;
					}
				}
			} Type mUnderlyingType;
			bool mIsArray, mIsEnum;

			/// <summary>
			/// Units associated wit the item's value
			/// </summary>
			public string Units { get; set; }

			/// <summary>
			/// Unpublish an item from the Blackboard
			/// </summary>
			public void Unpublish()
			{
				mValueInfo = null;
				mHow = How.Nondescrip;
				Type = null;
				mIsArray = false;
				IsPublished = false;

				IsReflectingSubItemValueChanged = false;
				mItems.Clear();

				if (ItemUnpublished != null)
					ItemUnpublished(this, EventArgs.Empty);
			}

			/// <summary>
			/// Accesses the value of a Blackboard item
			/// </summary>
			[TypeConverter(typeof(ObjectValueConverter))]
			//[Editor(typeof(X),typeof(System.Drawing.Design.UITypeEditor))]
			public object Value
			{
				get
				{
					switch (mHow)
					{
						case How.Nondescrip:
							object value = null;
							if (mValueInfo != null) value = mValueInfo.Value;
							if (value != null && value.GetType().IsArray)
							{
								mHow = How.IsArray;
								return (((Array)value).GetValue(mIndex));
							}
							//else if (ValueArray != null)
							//{
							//	mHow = How.IsArray;
							//    return ValueArray.GetValue(mIndex);
							//}
							//else if (value is IHostArray)
							//{
							//	mHow = How.IsArray;
							//	return (value as IHostArray);
							//}
							else if (value is IObjectInfo)
							{
								mHow = How.IsArray;
								return (value as IObjectInfo).Value;
							}
							else if (mValueInfo != null)
							{
								mHow = How.ValueInfo;
								return value;
							}
							else
							{
								mHow = How.Null;
								return value;
							}
						case How.IsArray:
							return (((Array)mValueInfo.Value).GetValue(mIndex));
						case How.ObjectInfo:
							return (mValueInfo.Value as IObjectInfo).Value;
						case How.ValueInfo:
						default:
							return mValueInfo.Value;
						case How.Null:
							return null;
					}
				}
				set
				{
					if (mValueInfo==null || mValueInfo.IsReadOnly) return;
					switch (mHow)
					{
						case How.Nondescrip:
						case How.Null:
							if (mValueInfo.Value != null && mValueInfo.Value.GetType().IsArray)
							{
								((Array)mValueInfo.Value).SetValue(value, mIndex);
								mHow = How.IsArray;
							}
							//else if (ValueArray != null)
							//    ValueArray.SetValue(value, mIndex);
							else if (mValueInfo.Value is IObjectInfo)
							{
								(mValueInfo.Value as IObjectInfo).Value = value;
								mHow = How.ObjectInfo;
							}
							else if (mIsEnum && value is string)
							{
								mValueInfo.Value = Enum.Parse(Type, (string)value);
								mHow = How.EnumString;
							}
							else if (mIsEnum && value.GetType().Equals(Enum.GetUnderlyingType(Type)))
							{
								mValueInfo.Value = Enum.ToObject(Type, value);
								mHow = How.Enum;
							}
							else if (mValueInfo != null)
							{
								mValueInfo.Value = value;
								mHow = How.ValueInfo;
							}
							else
								mHow = How.Null;
							break;
						case How.IsArray:
							((Array)mValueInfo.Value).SetValue(value, mIndex);
							break;
						case How.ObjectInfo:
							(mValueInfo.Value as IObjectInfo).Value = value;
							break;
						case How.EnumString:
							mValueInfo.Value = Enum.Parse(Type, (string)value);
							break;
						case How.Enum:
							mValueInfo.Value = Enum.ToObject(Type, value);
							break;
						case How.ValueInfo:
							mValueInfo.Value = value;
							break;
					}

					RaiseValueChanged();
				}
			}

			internal void ClearValueInfo()
			{
				mValueInfo = null; mHow = How.Nondescrip;
			}

			/// <summary>
			/// Accesses the value of a Blackboard item
			/// </summary>
			internal IValueInfo ValueInfo
			{
				get { return mValueInfo; }
				set
				{
					if (value == null)
					{
						Log.WriteLine("Won't set {0}'s ValueInfo to null", this);
						return;
					}
					mValueInfo = value;
					if (mHow == How.Null)
					{
						mHow = How.Nondescrip;
						mUnderlyingType = null;
						var type = Type;
					}
				}
			} IValueInfo mValueInfo;
		}

		/// <summary>
		/// A specific list of Items
		/// </summary>
		public class ItemList : List<Item>
		{
		}

		/// <summary>
		/// A TypeConverter to convert an ObjectValue to/from a string
		/// </summary>
		public class ObjectValueConverter : TypeConverter
		{
			/// <summary>
			/// Can it convert from a string
			/// </summary>
			/// <param name="context"></param>
			/// <param name="sourceType"></param>
			/// <returns></returns>
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				if (sourceType == typeof(String))
					return true;
				return base.CanConvertFrom(context, sourceType);
			}

			/// <summary>
			/// Can it convert to a string
			/// </summary>
			/// <param name="context"></param>
			/// <param name="destinationType"></param>
			/// <returns></returns>
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				return base.CanConvertTo(context, destinationType);
			}

			/// <summary>
			/// Convert from a string
			/// </summary>
			/// <param name="context"></param>
			/// <param name="culture"></param>
			/// <param name="value"></param>
			/// <returns></returns>
			public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
			{
				var item = context.Instance as Item;
				Type valueType = item.Type;
				if (valueType.IsEnum)
				{
					if ( value is string )
						return Enum.Parse(valueType, value as string, true);
				}
				switch (Type.GetTypeCode(valueType))
				{
					default:
						if (item.IsArrayProxy )
							return (item.Value as IArrayProxy).ConvertFrom(value as string);

						return Convert.ChangeType(value, valueType);
				}
			}

			/// <summary>
			/// Convert to a string
			/// </summary>
			/// <param name="context"></param>
			/// <param name="culture"></param>
			/// <param name="value"></param>
			/// <param name="destinationType"></param>
			/// <returns></returns>
			public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
			{
				//var item = context.Instance as Item;
				//if (item.Type.IsEnum)
				//{
				//}
				return base.ConvertTo(context, culture, value, destinationType);
			}

			/// <summary>
			/// Handle the special case of enums
			/// </summary>
			/// <param name="context"></param>
			/// <returns></returns>
			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
				var item = context.Instance as Item;
				return item.Type.IsEnum;
			}

			/// <summary>
			/// Handle the special case of enums
			/// </summary>
			/// <param name="context"></param>
			/// <returns></returns>
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
				var item = context.Instance as Item;
				return item.Type.IsEnum;
			}

            private bool IsBrowsable(Type enumType, object value)
            {
                var attr = enumType.GetMember(value.ToString())[0].GetCustomAttribute(typeof(BrowsableAttribute));
                return attr == null || ((BrowsableAttribute)attr).Browsable;
            }
			/// <summary>
			/// Handle special case of enums
			/// </summary>
			/// <param name="context"></param>
			/// <returns></returns>
			public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				var item = context.Instance as Item;
                if (item.Type.IsEnum)
                {
                    var values = Enum.GetValues(item.Type).Cast<object>()
                        .Where(x => IsBrowsable(item.Type, x))
                        .ToList();
                    return new StandardValuesCollection(values);
                }
				return null;
			}
		}
	}
}
