using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Aspire.Utilities
{
	public interface INamed
	{
		string Name{get;}
	}

	/// <summary>
	/// Summary description for PropertyList<T>.
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class PropertyList<T> : List<T>, ICustomTypeDescriptor where T:INamed
	{
		#region ICustomTypeDescriptor Members

		public AttributeCollection GetAttributes() { return TypeDescriptor.GetAttributes(this, true); }

		public string GetClassName() { return TypeDescriptor.GetClassName(this, true); }

		public string GetComponentName() { return TypeDescriptor.GetComponentName(this, true); }

		public TypeConverter GetConverter() { return TypeDescriptor.GetConverter(this, true); }

		public EventDescriptor GetDefaultEvent() { return TypeDescriptor.GetDefaultEvent(this, true); }

		public PropertyDescriptor GetDefaultProperty() { return TypeDescriptor.GetDefaultProperty(this, true); }

		public object GetEditor(Type editorBaseType) { return TypeDescriptor.GetEditor(this, editorBaseType, true); }

		public EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

		public EventDescriptorCollection GetEvents() { return TypeDescriptor.GetEvents(this, true); }

		public PropertyDescriptorCollection GetProperties(Attribute[] attributes) { return GetProperties(); }

		public PropertyDescriptorCollection GetProperties()
		{
			var pds = new PropertyDescriptorCollection(null);

			for (int i = 0; i < Count; i++)
			{
				var pd = new PropertyDescr(this, i);
				pds.Add(pd);
			}
			return pds;
		}

		public object GetPropertyOwner(PropertyDescriptor pd) { return this; }

		#endregion

		#region PropertyDescriptor
		
		/// <summary>
		/// Provides an abstraction of a property on the AbstactList
		/// </summary>
		public class PropertyDescr : PropertyDescriptor
		{
			private PropertyList<T> collection = null;
			private int index = -1;

			/// <summary>
			/// Constructor that initializes the descriptor from the list and a unique index.
			/// </summary>
			/// <param name="coll">An AbstractList collection.</param>
			/// <param name="idx">A unique index.</param>
			public PropertyDescr( PropertyList<T> collection, int index )
				: base( "#"+index.ToString(), null )
			{
				this.collection = collection;
				this.index = index;
			} 
			/// <summary>
			/// Gets the collection of attributes for this member.
			/// </summary>
			public override AttributeCollection Attributes{ get { return new AttributeCollection(null);	}}
			/// <summary>
			/// Returns whether resetting an object changes its value.
			/// </summary>
			/// <param name="component"></param>
			/// <returns></returns>
			public override bool CanResetValue(object component){	return true;}
			/// <summary>
			/// Gets the type of the component this property is bound to (AbstractList)
			/// </summary>
			public override Type ComponentType{	get { return collection.GetType();	} }
			/// <summary>
			/// Gets the name that can be displayed in a window, such as a Properties window.
			/// </summary>
			public override string DisplayName{ get { return (collection[index] as INamed).Name; } }
			/// <summary>
			/// Gets the description of the member, as specified in the DescriptionAttribute.
			/// </summary>
			public override string Description
			{
				get
				{
					object item = collection[index];
					object[] attrs = item.GetType().GetCustomAttributes(typeof(DescriptionAttribute),false);
					if ( attrs.Length == 1 )
					{
						DescriptionAttribute descr = attrs[0] as DescriptionAttribute;
						return descr.Description;
					}
					else
						return "PropertyList item";
				}
			}
			/// <summary>
			/// Gets the current value of the property on a component.
			/// </summary>
			/// <param name="component"></param>
			/// <returns></returns>
			public override object GetValue(object component){return collection[index];}
			/// <summary>
			/// Gets a value indicating whether this property is read-only.
			/// </summary>
			public override bool IsReadOnly	{get { return true;  }}
			/// <summary>
			/// Gets the name of the member.
			/// </summary>
			public override string Name	{get { return "#"+index.ToString(); }}
			/// <summary>
			/// Gets the type of the property.
			/// </summary>
			public override Type PropertyType{get { return collection[index].GetType(); }}
			/// <summary>
			/// Resets the value for this property of the component to the default value.
			/// </summary>
			/// <param name="component"></param>
			public override void ResetValue(object component) {}
			/// <summary>
			/// determines a value indicating whether the value of this property needs to be
			/// persisted.
			/// </summary>
			/// <param name="component"></param>
			/// <returns></returns>
			public override bool ShouldSerializeValue(object component)	{return true;}
			/// <summary>
			/// Sets the value of the component to a different value.
			/// </summary>
			/// <param name="component"></param>
			/// <param name="value"></param>
			public override void SetValue(object component, object value)
			{
				// this.collection[index] = value;
			}
		}

		#endregion PropertyDescriptor

		public override string ToString()
		{
			return "(Properties)";
		}

	}
}
