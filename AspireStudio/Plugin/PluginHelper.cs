using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Aspire.Utilities;
using Aspire.Utilities.Extensions;

namespace Aspire.Studio.Plugin
{
	public static class PluginHelper
	{
		private static string pluginsDirectory = Path.GetDirectoryName(
			Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6);
		public static string PluginsDirectory
		{
			get { return pluginsDirectory; }
			set
			{
				pluginsDirectory = value;
				if (pluginsDirectory[pluginsDirectory.Length - 1] != '\\')
					pluginsDirectory += '\\';
			}
		}

		/// <summary>
		/// Returns a new plugin and the assembly location.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static PluginInfo AddPlugin(string fileName)
		{
			var assembly = Assembly.LoadFile(fileName);

			var pluginAttribute = Attribute.GetCustomAttribute(assembly,
				typeof(PluginAttribute)) as PluginAttribute;
			if (pluginAttribute != null)
			{
				var title = Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
				var info = new PluginInfo()
				{
					Name = title != null ? title.Title : assembly.GetName().Name,
					AssemblyPath = FileUtilities.RelativePath(fileName,PluginsDirectory)
				};

				foreach (var type in assembly.GetExportedTypes())
				{
					if (type.GetInterface("IRegistration") != null)
						info.Registration = type;
					if ( type.GetInterface("ITypePlugin") != null )
						info.Types.Add(type);
				}

				return info;
			}

			Log.WriteLine("{0} contains no plugins and should be removed from the Plugins folder",
				fileName);
			return null;
		}

		public static void Bind(PluginInfoCollection plugins)
		{
			foreach (var plugin in plugins)
			{
				Assembly assembly;
				try
				{
					assembly = Assembly.LoadFile(Path.Combine(PluginsDirectory,plugin.AssemblyPath));
				}
				catch (FileNotFoundException ex)
				{
					Log.ReportException(ex, "{0}\nTrying to load the {1} plugin", plugin.AssemblyPath,
						plugin.Name);
					continue;
				}
				foreach (var typeName in plugin.TypeNames)
				{
					foreach (var type in assembly.GetExportedTypes())
						if (type.Name == typeName)
						{
							plugin.Types.Add(type);
							break;
						}
				}
			}
			plugins.IsBound = true;
		}

		/// <summary>
		/// Creates a new instance of the plugin inside the specified assembly file
		/// </summary>
		/// <typeparam name="T">Form / UserControl</typeparam>
		/// <param name="assemblyFile">The assembly file to load</param>
		/// <returns></returns>
		public static T CreateNewInstance<T>(string assemblyFile)
		{
			var assembly = Assembly.LoadFile(assemblyFile);

			var pluginAttribute = Attribute.GetCustomAttribute(assembly,
					typeof(PluginAttribute)) as PluginAttribute;
			//T item = (T)assembly.CreateInstance(contentAttribute.Content, true);

			return default(T);// item;
		}

		/// <summary>
		/// <para>Looks for plugins in the directory specified by the PluginsDirectory</para>
		/// <para>property</para>
		/// </summary>
		/// <returns>a List with plugin Title as the Key and Assembly path as the Value</returns>
		public static List<PluginInfo> FindPlugins()
		{
			var plugins = new List<PluginInfo>();
			PluginInfo pluginInfo;
			foreach (var file in Directory.GetFiles(PluginsDirectory))
			{
				var fileInfo = new FileInfo(file);
				if (fileInfo.Extension.In(".dll", ".exe"))
				{
					try
					{
						pluginInfo = AddPlugin(file);
						string title = pluginInfo.Name != null ?
							pluginInfo.Name : pluginInfo.AssemblyPath;
						plugins.Add(pluginInfo);
					}
					catch
					{
					}
				}
			}

			return plugins;
		}
		/// <summary>
		/// Gets all plug-ins from the PluginDirectory
		/// </summary>
		/// <returns></returns>
		public static IDictionary<string, PluginInfo> GetPlugins()
		{
			var plugins = new Dictionary<string, PluginInfo>();
			PluginInfo pluginInfo;
			foreach (var file in Directory.GetFiles(PluginsDirectory))
			{
				var fileInfo = new FileInfo(file);
				if (fileInfo.Extension.In(".dll", ".exe"))
				{
					try
					{
						pluginInfo = AddPlugin(file);
						plugins.Add(pluginInfo.Name, pluginInfo);
					}
					catch
					{
					}
				}
			}

			return plugins;
		}
		/// <summary>
		/// Gets the specified plugins
		/// </summary>
		/// <param name="pluginsToLoad">List of assembly paths</param>
		/// <returns></returns>
		public static PluginInfoCollection GetPlugins(PluginInfoCollection pluginsToLoad)
		{
			var plugins = new PluginInfoCollection();
			foreach (var plugin in pluginsToLoad)
			{
				var fileInfo = new FileInfo(plugin.AssemblyPath);
				if (fileInfo.Extension.In(".dll", ".exe"))
				{
					try
					{
						var pluginInfo = AddPlugin(Path.Combine(PluginsDirectory,plugin.AssemblyPath));
						plugins.Add(pluginInfo);
					}
					catch
					{
					}
				}
			}

			return plugins;
		}

		public static ISettingsPlugin GetSettingsPlugin(string assemblyFile)
		{
			var assembly = Assembly.LoadFile(Path.Combine(PluginsDirectory,assemblyFile));

			var contentAttribute = Attribute.GetCustomAttribute(assembly,
				typeof(SettingsContentAttribute)) as SettingsContentAttribute;

			if (contentAttribute != null)
				return assembly.CreateInstance(contentAttribute.Content, true) as ISettingsPlugin;

			return null;
		}
	}
}
