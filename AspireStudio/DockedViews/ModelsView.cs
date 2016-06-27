using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Silver.UI;
using WeifenLuo.WinFormsUI.Docking;

using Aspire.Framework;
using Aspire.Utilities;

namespace Aspire.Studio.DockedViews
{
	public partial class ModelsView : ToolWindow
	{
		const int NOIMAGE = -1;
		static Color editColor = Color.Blue;

		public event PropertiesView.PropertyBrowsableHandler ObjectIsBrowsable;

		delegate TreeNode AddDelegate(TreeNodeCollection nodes,Model model);

		TreeNode mContextMenuNode;
		List<TreeNode> modelsToBuild = new List<TreeNode>();
		List<TreeNode> modelsBuilt = new List<TreeNode>();

		int mImageWidth = 0;

		public ModelsView()
		{
			InitializeComponent();

			mTreeView.AfterSelect += treeView_AfterSelect;
			ModelMgr.ModelAdded += ModelMgr_ModelAdded;
			ModelMgr.ModelRemoved += ModelMgr_ModelRemoved;
		}

		void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			ObjectIsBrowsable(e.Node.Tag,e.Node.Text);
		}

		private TreeNode Add(TreeNodeCollection root, Model model)
		{
			var node = new TreeNode(model.Name);
			node.Tag = model;
			root.Add(node);
			if (model.Models.Count > 0)
				Populate(model.Models, node.Nodes);

			return node;
		}

		public void Clear()
		{
			mTreeView.Nodes.Clear();
		}

		public Executive Executive { get; set; }

		bool NeedToBuild { get { return modelsToBuild.Count > 0; } }

		// Returns the bounds of the specified node, including the region  
		// occupied by the node label and any node tag displayed. 
		private Rectangle NodeBounds(TreeNode node)
		{
			if (node.ImageIndex == NOIMAGE)
				// Set the return value to the normal node bounds.
				return Rectangle.Inflate(node.Bounds, 2, 0);

			var bounds = node.Bounds;
			// Retrieve a Graphics object from the TreeView handle 
			// and use it to calculate the display width of the tag.
			var g = mTreeView.CreateGraphics();
			var size = bounds.Size;
			var font = node.NodeFont;
			if (font == null) font = mTreeView.Font;
			size.Width = (int)g.MeasureString(node.Text, font).Width + mImageWidth;
			bounds.Size = size;
			g.Dispose();
			return bounds;
		}

		public void Populate(ModelList models, TreeNodeCollection root = null)
		{
			if (root == null) root = mTreeView.Nodes;
			foreach (var model in models)
				Add(root, model);
		}

		void ModelMgr_ModelAdded(object sender, EventArgs e)
		{
			if (adding) return;

			if (InvokeRequired)
				mTreeView.Invoke(new AddDelegate(Add), new object[] { mTreeView.Nodes, sender as Model });
			else
				Add(mTreeView.Nodes, sender as Model);
		} bool adding;

		void ModelMgr_ModelRemoved(object sender, EventArgs e)
		{
		}
		//	if (removing) return;
		//} bool removing;

		private void mTreeView_DragDrop(object sender, DragEventArgs e)
		{

			var droppedAt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
			var targetNode = ((TreeView)sender).GetNodeAt(droppedAt);
			var root = mTreeView.Nodes;
			Model model;

			var item = e.Data.GetData(typeof(ToolBoxItem)) as ToolBoxItem;
			if (item != null && item.Object is ToolBoxConfig.ToolInfo)
			{
				//Dragged from toolbox
				var toolInfo = item.Object as ToolBoxConfig.ToolInfo;

				model = Activator.CreateInstance(toolInfo.Type) as Model;
				if (targetNode != null && targetNode.Tag is Model)
				{
					model.ParentModel = targetNode.Tag as Model;
					root = targetNode.Nodes;
				}
				model.Name = ModelMgr.GetUniqueName(toolInfo.Name, model.ParentModel);
				var newNode = Add(root, model);
				newNode.ForeColor = editColor;
				modelsToBuild.Add(newNode);
			}
			else
			{
				// Moving
				var node = e.Data.GetData(typeof(TreeNode)) as TreeNode;
				if (node != null)
				{
					model = node.Tag as Model;
					if (node == targetNode) return;
					if (node.Parent == null)
						mTreeView.Nodes.Remove(node);
					else
						node.Parent.Nodes.Remove(node);

					if (node.ForeColor == editColor)
					{
						if (targetNode != null && targetNode.Tag is Model)
						{
							model.ParentModel = targetNode.Tag as Model;
							root = targetNode.Nodes;
						}
						root.Add(node);
					}
					else
					{
						int oldIndex = node.Index;
						if ( targetNode.Parent == null )
							mTreeView.Nodes.Insert(targetNode.Index, node);
						else
							targetNode.Parent.Nodes.Insert(targetNode.Index, node);
						ModelMgr.Move(node.Tag as Model, targetNode.Index);
					}
				}
			}
		}

		private void mTreeView_DragEnter(object sender, DragEventArgs e)
		{
			if (Executive.Mode != ExecutiveMode.Stop)
			{
				Log.WriteLine("Must be stopped to edit the Model list");
				return;
			}

			var tbItem = e.Data.GetData(typeof(ToolBoxItem)) as ToolBoxItem;
			if (tbItem != null && tbItem.Object is ToolBoxConfig.ToolInfo)
				e.Effect = e.AllowedEffect;
			else
			{
				var node = e.Data.GetData(typeof(TreeNode)) as TreeNode;
				if (node != null)
				{
					var droppedAt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
					var targetNode = ((TreeView)sender).GetNodeAt(droppedAt);
					if ( targetNode.Parent == node.Parent)
						e.Effect = e.AllowedEffect;
				}
			}
		}

		private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			contextMenuStrip1.Items.Clear();
			if (NeedToBuild)
				contextMenuStrip1.Items.Add(new ToolStripMenuItem("Build", null, new EventHandler(OnBuild)));
			if ( mContextMenuNode != null )
				contextMenuStrip1.Items.Add(new ToolStripMenuItem("Remove",null,new EventHandler(OnRemove)));
			if ( ModelMgr.IsDirty )
				contextMenuStrip1.Items.Add(new ToolStripMenuItem("Save", null, new EventHandler(OnSave)));
		}

		void OnBuild(object sender, EventArgs e)
		{
			try
			{
				foreach (var node in modelsToBuild)
				{
					adding = true;
					if (ModelMgr.Add(node.Tag as Model))
					{
						modelsBuilt.Add(node);
						node.ForeColor = Color.Black;
					}
					adding = false;
				}
				foreach ( var n in modelsBuilt)
					modelsToBuild.Remove(n);
				modelsBuilt.Clear();
			}
			finally
			{
			}
		}

		void OnRemove(object sender, EventArgs e)
		{
			mTreeView.Nodes.Remove(mContextMenuNode);
			if (mContextMenuNode.ForeColor != editColor)
				ModelMgr.Remove(mContextMenuNode.Tag as Model);
		}

		void OnSave(object sender, EventArgs e)
		{
			ModelMgr.Save();
		}

		private void mTreeView_MouseDown(object sender, MouseEventArgs e)
		{
			var clickedNode = mTreeView.GetNodeAt(e.X, e.Y) as TreeNode;
			if (e.Button == MouseButtons.Right && clickedNode != null && NodeBounds(clickedNode).Contains(e.X, e.Y))
				mContextMenuNode = clickedNode;
			else
				mContextMenuNode = null;
		}

		private void mTreeView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			DoDragDrop(e.Item, DragDropEffects.Move);
		}

		private void mTreeView_DragOver(object sender, DragEventArgs e)
		{
			var droppedAt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
			var targetNode = ((TreeView)sender).GetNodeAt(droppedAt);
			var item = e.Data.GetData(typeof(ToolBoxItem)) as ToolBoxItem;
			// Moving
			var node = e.Data.GetData(typeof(TreeNode)) as TreeNode;
			if (node != null && node.ForeColor != editColor)
			{
				if (node != targetNode && targetNode != null && node.Parent == targetNode.Parent)
					e.Effect = e.AllowedEffect;
				else
					e.Effect = DragDropEffects.None;
			}
		}

	}
}
