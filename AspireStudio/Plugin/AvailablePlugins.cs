using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Aspire.Studio.Plugin;
using Aspire.Utilities;
using Aspire.Utilities.Extensions;

namespace Aspire.Studio.Dialogs
{
	public partial class AvailablePlugins : Form
	{
		private List<PluginInfo> availablePlugins;
		public PluginInfoCollection SelectedPlugins { get; private set; }

		public AvailablePlugins()
		{
			InitializeComponent();
		}

		private void AvailablePlugins_Load(object sender, EventArgs e)
		{
			availablePlugins = PluginHelper.FindPlugins();
			foreach (var info in availablePlugins)
			{
				var node = new TreeNode(info.Name){Checked=false,Tag=info};
				foreach(var type in info.Types)
					node.Nodes.Add(new TreeNode(type.Name) { Tag = type });
				treeView1.Nodes.Add(node);
			}

			var plugins = AppState.ConfigurationFile.Startup.Plugins;

			foreach (TreeNode node in treeView1.Nodes)
			{
				foreach (var pi in plugins)
				{
					if (node.Text == pi.Name)
					{
						foreach (TreeNode tnode in node.Nodes)
						{
							foreach (var type in pi.TypeNames)
								if (tnode.Text == type)
									tnode.Checked = true;
						}
					}
				}
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			SelectedPlugins = new PluginInfoCollection();
			foreach (TreeNode node in treeView1.Nodes )
			{
				if (node.Checked)
				{
					var pi = node.Tag as PluginInfo;
					pi.Types.Clear();
					SelectedPlugins.Add(pi);
					foreach (TreeNode tnode in node.Nodes)
						if (tnode.Checked)
							pi.Types.Add(tnode.Tag as Type);
				}
			}

			var plugins = AppState.ConfigurationFile.Startup.Plugins;
			plugins.Clear();
			if (SelectedPlugins.Count > 0)
			{
				foreach ( var pi in SelectedPlugins)
				{
					var plugin = new PluginInfo()
					{
						Name = pi.Name,
						AssemblyPath = pi.AssemblyPath
					};
					foreach (var type in pi.Types)
						plugin.Types.Add(type);
					plugins.Add(plugin);
				}
			}

			AppState.ConfigurationFile.Save(Path.Combine(AppState.InstallDirectory,StudioSettings.Default.PluginConfigFile));
		}

		private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
		{
			if (checking) return;
			checking = true;

			if (e.Node.Tag is PluginInfo)
			{
				foreach (TreeNode sn in e.Node.Nodes)
					sn.Checked = e.Node.Checked;
			}
			else if (e.Node.Checked) e.Node.Parent.Checked = true;
			else
			{
				bool childChecked = false;
				foreach (TreeNode n in e.Node.Parent.Nodes)
					if (n.Checked) childChecked = true;
				e.Node.Parent.Checked = childChecked;
			}

			checking = false;
		} bool checking;

	}
}
