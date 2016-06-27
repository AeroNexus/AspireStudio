using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

using Aspire.Utilities;
using Aspire.Utilities.Extensions;

namespace Aspire.Studio.Plugin
{
	public class ConfigurationFile
	{
		public Startup Startup { get; set; }
		public PluginConfiguration PluginConfiguration { get; set; }
		[XmlIgnore]
		public bool IsDirty { get; set; }

		public ConfigurationFile()
		{
			Startup = new Startup();
			PluginConfiguration = new PluginConfiguration();
		}

		public static ConfigurationFile Load(string filePath)
		{
			return FlatFile.ReadFile(filePath).XmlDeserialize<ConfigurationFile>();
		}

		public void Save(string filePath)
		{
			this.XmlSerialize(filePath);
			IsDirty = false;

		}
	}

	public class Startup
	{
		[XmlElement(ElementName = "Plugin")]
		public PluginInfoCollection Plugins { get; set; }

		public Startup()
		{
			Plugins = new PluginInfoCollection();
		}
	}
	public class PluginConfiguration
	{
		[XmlElement("Plugin")]
		public PluginConfigCollection Plugins { get; set; }
	}

	public class PluginConfig
	{
		[XmlAttribute("Title")]
		public string Title { get; set; }
		//[XmlElement("Configuration")]
		//public XElement Configuration { get; set; }
	}

	public class PluginConfigCollection : List<PluginConfig>
	{
		public PluginConfig this[string title]
		{
			get { return this.SingleOrDefault(x => x.Title == title); }
		}
	}
}
