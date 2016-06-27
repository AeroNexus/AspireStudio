using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace Aspire.Studio.Plugin
{
	public class PluginTreeView : TreeView
	{
		ImageList imageList = new ImageList();

		public Size ImageListImageSize
		{
			get { return imageList.ImageSize; }
			set { imageList.ImageSize = value; }
		}

		public PluginTreeView()
		{
			this.ImageList = imageList;
			imageList.Images.Add(Resources.TreeImage);
		}

		public void AddPlugin(PluginInfo pluginInfo)
		{
			#region Main Plugin Node

			var pluginItem = new TreeNode(pluginInfo.Name);
			pluginItem.Tag = pluginInfo;

			try
			{
				//if (!string.IsNullOrEmpty(pluginInfo.Plugin.Icon))
				//{
				//	imageList.Images.Add(Image.FromFile(pluginInfo.Plugin.Icon));
				//	pluginItem.ImageIndex = imageList.Images.Count - 1;
				//	pluginItem.SelectedImageIndex = imageList.Images.Count - 1;
				//}
			}
			catch (NotImplementedException) { } // No icon

			#endregion

			#region SubGroup

			TreeNode subGroup = null;
			try
			{
				//if (!string.IsNullOrEmpty(pluginInfo.Plugin.SubGroup))
				//{
				//	subGroup = new TreeNode(pluginInfo.Plugin.SubGroup);
				//	subGroup.Nodes.Add(pluginItem);
				//}
			}
			catch (NotImplementedException)
			{
				// Do nothing (check for main group next)
			}

			#endregion

			#region Group

			TreeNode group = null;
			try
			{
				//if (!string.IsNullOrEmpty(pluginInfo.Plugin.Group))
				//{
				//	group = new TreeNode(pluginInfo.Plugin.Group);

				//	if (subGroup != null)
				//		group.Nodes.Add(subGroup);
				//	else
				//		group.Nodes.Add(pluginItem);
				//	//this.Nodes.Add(group);
				//	EnsureNode(group);
				//}
			}
			catch (NotImplementedException)
			{
				if (subGroup != null)
					this.Nodes.Add(subGroup);
				else
					this.Nodes.Add(pluginItem);
				return;
			}

			#endregion

			if (group == null && subGroup == null)
				this.Nodes.Add(pluginItem);
		}
		public void AddSettingsPlugin(PluginInfo pluginInfo, ISettingsPlugin settingsPlugin)
		{
			#region Main Plugin Node

			var pluginItem = new TreeNode(pluginInfo.Name);
			pluginItem.Tag = settingsPlugin;

			try
			{
				//if (!string.IsNullOrEmpty(pluginInfo.Plugin.Icon))
				//{
				//	imageList.Images.Add(Image.FromFile(pluginInfo.Plugin.Icon));
				//	pluginItem.ImageIndex = imageList.Images.Count - 1;
				//	pluginItem.SelectedImageIndex = imageList.Images.Count - 1;
				//}
			}
			catch (NotImplementedException) { } // No icon

			#endregion

			#region SubGroup

			TreeNode subGroup = null;
			try
			{
				//if (!string.IsNullOrEmpty(pluginInfo.Plugin.SubGroup))
				//{
				//	subGroup = new TreeNode(pluginInfo.Plugin.SubGroup);
				//	subGroup.Nodes.Add(pluginItem);
				//}
			}
			catch (NotImplementedException)
			{
				// Do nothing (check for main group next)
			}

			#endregion

			#region Group

			TreeNode group = null;
			try
			{
				//if (!string.IsNullOrEmpty(pluginInfo.Plugin.Group))
				//{
				//	group = new TreeNode(pluginInfo.Plugin.Group);

				//	if (subGroup != null)
				//		group.Nodes.Add(subGroup);
				//	else
				//		group.Nodes.Add(pluginItem);
				//	//this.Nodes.Add(group);
				//	EnsureNode(group);
				//}
			}
			catch (NotImplementedException)
			{
				if (subGroup != null)
					this.Nodes.Add(subGroup);
				else
					this.Nodes.Add(pluginItem);
				return;
			}

			#endregion

			if (group == null && subGroup == null)
				this.Nodes.Add(pluginItem);
		}
		private TreeNode EnsureNode(TreeNode treeNode)
		{
			TreeNode node = (from x in this.Nodes.Cast<TreeNode>()
							 where x.Text == treeNode.Text
							 select x).SingleOrDefault();

			if (node == null)
			{
				this.Nodes.Add(treeNode);
				return treeNode;
			}
			else
			{
				foreach (TreeNode subNode in treeNode.Nodes)
					node.Nodes.Add(subNode);
				return node;
			}
		}
	}
}
