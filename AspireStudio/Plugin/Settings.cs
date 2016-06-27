using System.Collections.Generic;
using System.Windows.Forms;

namespace Aspire.Studio.Plugin
{
	public partial class Settings : Form
	{
		private PluginInfoCollection plugins = null;
        private Dictionary<string, ISettingsPlugin> settingsPlugins = new Dictionary<string, ISettingsPlugin>();
        private ImageList imageList = new ImageList();

        public Settings(PluginInfoCollection plugins)
		{
			InitializeComponent();
			//ToDo: merge plugin settings with preferences
			this.plugins = plugins;
			LoadSettings();

			if (settingsPlugins.Count == 0)
			{
				MessageBox.Show(
					"None of the plugins have any settings to configure",
					"No Settings",
					MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation);
				this.Dispose();
			}
		}

		private void LoadSettings()
		{
			foreach (var pi in plugins)
			{
				ISettingsPlugin settingsPlugin = PluginHelper.GetSettingsPlugin(pi.AssemblyPath);
				if (settingsPlugin != null)
					settingsPlugins.Add(pi.Name, settingsPlugin);
			}

			foreach (KeyValuePair<string, ISettingsPlugin> kv in settingsPlugins)
			{
				pluginTreeView.AddSettingsPlugin(plugins[kv.Key], kv.Value);
			}
		}

		private void pluginTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (e.Node.Tag == null)
				return;

			var settingsPlugin = e.Node.Tag as ISettingsPlugin;

			var control = settingsPlugin.Content;
			contentPanel.Controls.Clear();
			contentPanel.Controls.Add(control);
			control.Dock = DockStyle.Fill;
		}

	}
}
