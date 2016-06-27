using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Serialization;

namespace Aspire.Framework
{
	/// <summary>
	/// Reference to an assembly specified in the solution, used prior to loading the scenario
	/// </summary>
	public class AssemblyReference
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public AssemblyReference()
		{
			mTypes.CollectionChanged += mTypes_CollectionChanged;
		}

		void mTypes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			Scenario.RaiseIsDirty();
		}
		/// <summary>
		/// Assembly/reference name
		/// </summary>
		[XmlAttribute("name")]
		public string Name { get; set; }

		/// <summary>
		/// Should the Register class be used when loading an assembly
		/// </summary>
		[XmlAttribute("register")]
		public string Register { get; set; }

		/// <summary>
		/// The assembly's Type
		/// </summary>
		[XmlElement("Type", typeof(ClassName))]
		public ObservableCollection<ClassName> Types
		{
			get { return mTypes; }
			set { mTypes = value; }
		} ObservableCollection<ClassName> mTypes = new ObservableCollection<ClassName>();

		[XmlIgnore]
		internal Assembly Assembly { get; set; }

		[XmlIgnore]
		internal Type[] AssemblyTypes { get; set; }

		/// <summary>
		/// String representation
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name;
		}

		/// <summary>
		/// 
		/// </summary>
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class ClassName
		{
			/// <summary>
			/// 
			/// </summary>
			[XmlAttribute("name")]
			public string Name { get; set; }
			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return Name;
			}
		}
	}

}
