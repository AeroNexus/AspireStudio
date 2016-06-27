using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using Aspire.Core.Utilities;
using Aspire.CoreModels;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.BowsingUI.Dialogs
{
	public partial class SendMessageDialog : Form
	{
		private Xteds.Interface.Message mMessage;

		public SendMessageDialog(Xteds.Interface.Message msg)
		{
            if (msg == null) throw new ArgumentNullException("The msg parameter cannot be null.");

			InitializeComponent();

			mMessage = msg;
			lblMessageName.Text = msg.Name;

			PropertyBag bag = new PropertyBag();
			bag.GetValue += new PropertySpecEventHandler(bag_GetValue);
			bag.SetValue += new PropertySpecEventHandler(bag_SetValue);
			foreach (Xteds.Interface.VariableValue varValue in msg.Variables)
			{
				Xteds.Interface.Variable var = varValue.Variable;
				PropertySpec spec;
				if (var.Drange == null)
				{
					if (varValue.Value != null && varValue.Value.GetType().IsArray)
					{
						spec = new PropertySpec(var.Name, varValue.Value.GetType(), string.Empty, var.Description, varValue.Value);
						//    typeof(System.Drawing.Design.UITypeEditor), typeof(ExpandableObjectConverter));
					}
					else
					{
						spec = new PropertySpec(var.Name, typeof(string));
					}
				}
				else if (varValue.Value.GetType().IsEnum)
				{
					spec = new PropertySpec(var.Name, varValue.Value.GetType());
				}
				else
				{
					//spec = new DRangePropertySpecOptions(var.Name, var.Drange);
					spec = new PropertySpec(var.Name, var.EnumType);
				}

				spec.Description = var.Description;

				bag.Properties.Add(spec);
			}

			propertyGrid1.SelectedObject = bag;
		}

		/// <summary>
		/// Access the Aspire Message
		/// </summary>
		public Xteds.Interface.Message Message
		{
			get { return mMessage; }
		}

		private void bag_GetValue(object sender, PropertySpecEventArgs e)
		{
			foreach( Xteds.Interface.VariableValue varValue in Message.Variables)
			{
				Xteds.Interface.Variable var = varValue.Variable;
				if( var != null && var.Name == e.Property.Name )
				{
					if (varValue.Value.GetType().IsEnum)
					{
						e.Value = varValue.Value;
					}
					else if (var.Drange == null)
					{
						e.Value = varValue.Value;
					}
					else if (var.Drange != null)
					{
						foreach( Xteds.Interface.Option o in var.Drange.Options )
						{
							if( o.Value.Equals( Convert.ToInt32(varValue.Value)) ) 
							{
								e.Value = o.Value;
								return;
							}
						}
						e.Value = var.Drange.Options[0].Value;
					}
					break;
				}
			}
			
		}

		private void bag_SetValue(object sender, PropertySpecEventArgs e)
		{
			foreach( Xteds.Interface.VariableValue varValue in Message.Variables)
			{
				Xteds.Interface.Variable var = varValue.Variable;
				if( var != null && var.Name == e.Property.Name )
				{
					//if( var.Drange != null )
					//{
					//    foreach( Xteds.Interface.Option o in var.Drange.Options )
					//    {
					//        if( o.Name.Equals(e.Value.ToString()) ) 
					//        {
					//            varValue.Value = Convert.ChangeType(o.Value,varValue.Value.GetType());
					//            return;
					//        }
					//    }
					//    varValue.Value = Convert.ChangeType(0,varValue.Value.GetType());
					//}
					//else
					//{
					if (varValue.Value is MarshaledString)
						(varValue.Value as MarshaledString).Set(e.Value.ToString());
					else if (varValue.Value is Vector3)
						varValue.Value = new Vector3(e.Value.ToString());
					else if (varValue.Value is Quaternion)
						varValue.Value = new Quaternion(e.Value.ToString());
					else
                        varValue.Value = Convert.ChangeType(e.Value, varValue.Value.GetType());
					//}
					break;
				}
			}

		}

		private void btnSend_Click(object sender, System.EventArgs e)
		{
			//this.mMessage.Send();
			this.DialogResult = DialogResult.OK;
		}
	}

	/// <summary>
	/// Extended properties
	/// </summary>
	public class DRangePropertySpecOptions : PropertySpec
	{
		/// <summary>
		/// List of values
		/// </summary>
		public Dictionary<string,object> ValuesList = new Dictionary<string,object>();

		/// <summary>
		/// Construct using Drange properties
		/// </summary>
		/// <param name="name">Property name</param>
		/// <param name="drange">Drange name</param>
		public DRangePropertySpecOptions(string name, Xteds.Interface.Drange drange) : base(name, typeof(string))
		{
			ConverterTypeName = typeof(DRangePropertySpecOptionsTypeConverter).AssemblyQualifiedName;
			foreach( Xteds.Interface.Option option in drange.Options )
				ValuesList[option.Name] = option.Value;
		}

		/// <summary>
		/// Construct using name, category
		/// </summary>
		/// <param name="name"></param>
		/// <param name="category"></param>
		public DRangePropertySpecOptions(string name, string category) : base(name, typeof(string), category)
		{
			ConverterTypeName = typeof(DRangePropertySpecOptionsTypeConverter).AssemblyQualifiedName;
		}
	}

	/// <summary>
	/// Drange properties type converter
	/// </summary>
	public class DRangePropertySpecOptionsTypeConverter : StringConverter
	{
		/// <summary>
		/// Which standard vales are supported
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

// 		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
// 		{
// 			if (context == null) return null;
// 			if (context.PropertyDescriptor == null) return null;
// 			if (context.PropertyDescriptor.ComponentType == null) return null;
// 			
// 			PropertyBag.PropertySpecDescriptor psd = (PropertyBag.PropertySpecDescriptor)context.PropertyDescriptor;
// 			if( psd != null )			
// 			{
// 				DRangePropertySpecOptions pso = psd.PropertySpec as DRangePropertySpecOptions;
// 				if( pso != null )
// 				{
// 					if( value is string )
// 					{
// 						if( pso.ValuesList[value] != null ) return pso.ValuesList[value];
// 						return pso.DefaultValue;
// 					}
// 					else
// 					{
// 						foreach( string key in pso.ValuesList.Keys )
// 						{
// 							if( pso.ValuesList[key].Equals(value) ) return key;
// 						}
// 						return pso.DefaultOption;
// 					}
// 				}
// 			}
// 			
// 			return base.ConvertFrom (context, culture, value);
// 		}
// 
// 		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
// 		{
// 			if (context == null) return null;
// 			if (context.PropertyDescriptor == null) return null;
// 			if (context.PropertyDescriptor.ComponentType == null) return null;
// 			
// 			PropertyBag.PropertySpecDescriptor psd = (PropertyBag.PropertySpecDescriptor)context.PropertyDescriptor;
// 			if( psd != null )			
// 			{
// 				DRangePropertySpecOptions pso = psd.PropertySpec as DRangePropertySpecOptions;
// 				if( pso != null )
// 				{
// 					if( destinationType == typeof(int) )
// 					{
// 						if( pso.ValuesList[value] != null )
// 						{
// 							return pso.ValuesList[value];
// 						}
// 						return pso.DefaultValue;
// 					}
// 					else
// 					{
// 						foreach( string key in pso.ValuesList.Keys )
// 						{
// 							if( pso.ValuesList[key].Equals(value) ) return key;
// 						}
// 						return pso.DefaultOption;
// 					}
// 				}
// 			}
// 			return base.ConvertTo (context, culture, value, destinationType);
// 		}

		/// <summary>
		/// A list of te standard values supported
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if (context == null) return null;
			if (context.PropertyDescriptor == null) return null;
			if (context.PropertyDescriptor.ComponentType == null) return null;
			
			PropertyBag.PropertySpecDescriptor psd = (PropertyBag.PropertySpecDescriptor)context.PropertyDescriptor;
			if( psd != null )			
			{
				DRangePropertySpecOptions pso = psd.PropertySpec as DRangePropertySpecOptions;
				if( pso != null )
					return new StandardValuesCollection(pso.ValuesList.Keys);
			}
			return null;
		}
	}
}
