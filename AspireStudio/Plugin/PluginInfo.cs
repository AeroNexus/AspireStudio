using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Aspire.Studio.Plugin
{
	public class PluginInfo
	{
		[XmlAttribute("name")]
		public string Name { get; set; }
		public string AssemblyPath { get; set; }
		[XmlIgnore]
		public Type Registration { get; set; }
		[XmlIgnore]
		public List<Type> Types { get; set; }
		[XmlElement("Type", typeof(string))]
		public string[] TypeNames
		{
			get { return typeNames != null ? typeNames : (from x in Types select x.Name).ToArray(); }
			set { typeNames = value; }
		} string[] typeNames;
		public PluginInfo()
		{
			Name = string.Empty;
			AssemblyPath = string.Empty;
			Types = new List<Type>();
		}
		public override string ToString()
		{
			return Name;
		}
	}

	public class PluginInfoCollection : List<PluginInfo>
	{
		public PluginInfo this[string name]
		{
			get { return this.SingleOrDefault(x => x.Name == name); }
		}
		public bool Contains(string name)
		{
			return this[name] != null;
		}
		[XmlIgnore]
		public bool IsBound { get; set; }
	}


}
