using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Aspire.Studio.DocumentViews;
using Aspire.Studio.Plugin;
using Aspire.Utilities;

namespace Aspire.Studio
{
	public partial class MainForm
	{
		void  BuildNewTool()
		{
			if (newToolStripButton == null) return;

			newToolStripButton.DropDownItems.Clear();

			foreach ( var viewName in DocumentMgr.DocViews )
			{
				var item = newToolStripButton.DropDownItems.Add(viewName);
				item.Click += OnNewDocView;
			}
		}

		void InitializePlugins()
		{
			DocumentMgr.DocViewDefChanged += DocumentMgr_DocViewDefChanged;

			PluginHelper.PluginsDirectory = Path.Combine(Application.StartupPath, "Plugins");

			var configFile = Path.Combine(Application.StartupPath, StudioSettings.Default.PluginConfigFile);
			if (File.Exists(configFile))
			{
				try
				{
					if (!Directory.Exists(PluginHelper.PluginsDirectory))
						Directory.CreateDirectory(PluginHelper.PluginsDirectory);
				}
				catch (Exception ex)
				{
					Log.ReportException(ex);
					return;
				}

				AppState.ConfigurationFile = ConfigurationFile.Load(configFile);
				PluginHelper.Bind(AppState.ConfigurationFile.Startup.Plugins);
				LoadPlugins(AppState.ConfigurationFile.Startup.Plugins);
			}
			else
			{
				AppState.ConfigurationFile = new ConfigurationFile();
				AppState.ConfigurationFile.Save(configFile);
			}
		}

		void DocumentMgr_DocViewDefChanged(object sender, EventArgs e)
		{
			BuildNewTool();
		}

		void ClosePlugins()
		{
			if (AppState.ConfigurationFile != null && AppState.ConfigurationFile.IsDirty &&
				AppState.ConfigurationFile.PluginConfiguration.Plugins != null)
			{
				foreach (var pi in mPlugins)
				{
					PluginConfig config = AppState.ConfigurationFile.PluginConfiguration.Plugins[pi.Name];
					if (config == null)
					{
						config = new PluginConfig();
						config.Title = pi.Name;
						AppState.ConfigurationFile.PluginConfiguration.Plugins.Add(config);
					}

					try
					{
						//config.Configuration = kv.Value.Plugin.Configuration;
					}
					catch (NotImplementedException) { }
				}

				AppState.ConfigurationFile.Save(StudioSettings.Default.PluginConfigFile);
			}
		}

		void LoadPlugins(PluginInfoCollection plugins)
		{
			if ( mPlugins != null )
				foreach (var plugin in mPlugins)
					foreach (var type in plugin.Types)
					{
						var mi = type.GetMethod("Register", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod);
						if (mi != null)
							mi.Invoke(null, new object[] { true });
						else
							Log.WriteLine("Can't invoke {0}.Register: method not found", type.Name);
					}
			mPlugins = PluginHelper.GetPlugins(plugins);
			foreach ( var plugin in plugins)
				foreach (var type in plugin.Types)
				{
					var mi = type.GetMethod("Register", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod);
					if ( mi != null )
						mi.Invoke(null, new object[] { false });
					else
						Log.WriteLine("Can't invoke {0}.Register: method not found", type.Name);
				}
			//pluginMenuStrip.RemovePlugins();
			//pluginTreeView.Nodes.Clear();

			//foreach (PluginInfo pluginInfo in plugins.Values)
			//{
			//	if (pluginInfo.Plugin is IFormPlugin)
			//	{
					//pluginMenuStrip.AddPlugin(pluginInfo);
					//pluginTreeView.AddPlugin(pluginInfo);
				//}
				//else if (pluginInfo.Plugin is IUserControlPlugin)
				//{
					//pluginTreeView.AddPlugin(pluginInfo);
				//}
			//}
		}

		void OnNewDocView(object sender, EventArgs e)
		{
			var ai = sender as ToolStripItem;
			DocumentMgr.NewDocumentView(ai.Text);
		}
	}
}
