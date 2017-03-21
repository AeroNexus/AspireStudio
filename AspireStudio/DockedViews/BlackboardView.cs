using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Aspire.Framework;
using Aspire.Utilities;

namespace Aspire.Studio.DockedViews
{
	public partial class BlackboardView : ToolWindow
	{
		delegate TreeNode AddRemoveNodeDelegate(Blackboard.Item item);
		delegate void DisplayPropertiesChangedDelegate(Blackboard.Item item, IBlackboardDisplayProperties displayProperties);

		const int NOIMAGE = -1;

		Dictionary<string, Node> branches = new Dictionary<string, Node>();
		static bool mAlphabetized = true;
		ImageList mImageList = new ImageList();
		Dictionary<int, int> mImageByHash = new Dictionary<int, int>();
		IBlackboardMenuItems mBlackboardMenuItemHost;
		List<string> treeExpansions = new List<string>();
		List<ItemAction> mItemActions = new List<ItemAction>();
		List<ToolStripMenuItem> mBlackboardMenuItems = new List<ToolStripMenuItem>();
		MainForm mMainForm;
		Node mContextMenuNode, mFocusedNode;
		int mImageWidth, mMargin;
		bool mClosed;

		public event PropertiesView.PropertyBrowsableHandler ObjectIsBrowsable;

		public BlackboardView(MainForm mainForm)
		{
			mMainForm = mainForm;
			InitializeComponent();

			mMargin = mTreeView.Margin.Right + 1;
			mTreeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
			mTreeView.DrawNode += mTreeView_DrawNode;

			Blackboard.Cleared += Blackboard_Cleared;
			Blackboard.DisplayPropertiesChanged += Blackboard_DisplayPropertiesChanged;
		}

		Node AddIntermediate(Node parent, string intermediateName)
		{
			IList nodes;
			if (parent == null)
				nodes = mTreeView.Nodes;
			else
			{
				if (parent.HiddenNodes == null)
					parent.HiddenNodes = new List<Node>();
				nodes = parent.HiddenNodes;
			}

			foreach (Node node in nodes)
				if (node.Text == intermediateName)
					return node;

			var newNode = new Node(intermediateName);
			//Log.WriteLine("addI {0}", Tag);

			AddNode(nodes, newNode);

			return newNode;
		}

		private TreeNode AddNode(Blackboard.Item item)
		{
			Node node;
			if ( !item.Path.Contains(".") )
			{
				if (!branches.TryGetValue(item.Path,out node))
				{
					node = new Node(item.Path) { Tag = item };
					AddNode(mTreeView.Nodes, node);

					branches.Add(item.Path, node);
					mTreeView.Refresh();
				}
				else
				{
					node.Tag = item;
				}
			}
			else
			{
				string leafName;
				Node parent;
				FindNode(item.Path, out leafName, out parent);
				node = new Node(leafName) { Tag = item };
				parent.AddChild(node);
				//Log.WriteLine("add {0} {1}[{2}]", node.Text, item.Path, item.tag);
			}
			//Log.WriteLine("add {0}", node.tag);
			return node;
		}

		static void AddNode(IList nodes, Node node)
		{
			if (mAlphabetized)
			{
				int index = 0;
				if (nodes.Count > 0)
				{
					string nodeName = node.Text;
					while (index < nodes.Count && nodeName.CompareTo((nodes[index] as TreeNode).Text) > 0)
						index++;
				}
				//if ( index < nodes.Count) Log.WriteLine("inserting");
				nodes.Insert(index, node);
			}
			else
				nodes.Add(node);
		}

		private void BuildItemActions(TreeNode node)
		{
			// Clear out item specific actions
			foreach (var itemAction in mItemActions)
				contextMenuStrip1.Items.Remove(itemAction.MenuItem);
			mItemActions.Clear();

			foreach ( var bmi in mBlackboardMenuItems )
				contextMenuStrip1.Items.Remove(bmi);
			mBlackboardMenuItems.Clear();

			var item = node.Tag as Blackboard.Item;
      if (item == null)
        // i have no idea what BuilItemActions is supposed to do. i am seeing crashes with null tags
        // here, though, on the Blackboard node.
        return;

			var type = item.Type;
			bindToToolStripMenuItem.Visible =
				type.IsValueType || type.IsArray || type.IsEnum || type == typeof(string) ||
				(type == typeof(object) && item.Value.GetType().IsPrimitive);


			if (item.Value is IBlackboardMenuItems)
			{
				mBlackboardMenuItemHost = item.Value as IBlackboardMenuItems;
				foreach (var bmi in mBlackboardMenuItemHost.MenuItems)
				{
					var mi = new ToolStripMenuItem(bmi);
					mi.Click += BlackboardMenuItem_Click;
					mBlackboardMenuItems.Add(mi);
					contextMenuStrip1.Items.Add(mi);
				}
			}

			// Create and add an ItemAction for the Action
			foreach (var action in BlackboardAction.Actions)
			{
				bool enabled;
				if (action.Visible(item, out enabled))
				{
					var itemAction = new ItemAction(action, item);
					itemAction.MenuItem.Enabled = enabled;
					contextMenuStrip1.Items.Add(itemAction.MenuItem);
					mItemActions.Add(itemAction);
				}
			}
		}

		public void Clear()
		{
			mTreeView.Nodes.Clear();
			branches.Clear();
			treeExpansions.Clear();
		}

		void DisplayPropertiesChanged(Blackboard.Item item, IBlackboardDisplayProperties displayProperties)
		{
			Node parent;
			string leafName;
			var node = FindNode(item.Path, out leafName, out parent);
			if (node == null) return;
			var dp = displayProperties;

			if (dp.Image == null)
				node.ImageIndex = -1;
			else
			{
				int hash = dp.Image.GetHashCode();
				int index = -1;
				if (mImageByHash.TryGetValue(hash, out index))
				{
					node.ImageIndex = index;
					node.SelectedImageIndex = index;
				}
				else
				{
					mImageList.Images.Add(dp.Image);
					int n = mImageList.Images.Count;
					mImageWidth = Math.Max(mImageWidth, dp.Image.Width + mTreeView.Margin.Left);

					node.ImageIndex = n - 1;
					node.SelectedImageIndex = n - 1;
					mImageByHash.Add(hash, node.ImageIndex);
				}
			}

			if (dp.ForeColor.HasValue) node.ForeColor = dp.ForeColor.Value;
			if (dp.BackColor.HasValue) node.BackColor = dp.BackColor.Value;

			if (dp.FontItalic.HasValue || dp.FontBold.HasValue)
			{
				var fontStyle = FontStyle.Regular;
				if (dp.FontItalic.HasValue && dp.FontItalic.Value) fontStyle |= FontStyle.Italic;
				if (dp.FontBold.HasValue && dp.FontBold.Value) fontStyle |= FontStyle.Bold;

				var nodeFont = node.NodeFont;
				if (nodeFont == null) nodeFont = mTreeView.Font;

				node.NodeFont = new Font(nodeFont, fontStyle);
			}
		}

		Node FindNode(string path, out string leafName, out Node parent)
		{
			IList nodes;
			int dot = path.LastIndexOf('.');
			if (dot > 0)
			{
				leafName = path.Substring(dot + 1);
				var branchName = path.Substring(0, dot);
				if (!branches.TryGetValue(branchName, out parent))
				{
					string iName;
					FindNode(branchName, out iName, out parent);
					parent = AddIntermediate(parent, iName);
					branches.Add(branchName, parent);
				}
				nodes = parent.HiddenNodes;
				if (nodes == null) return null;
			}
			else
			{
				parent = null;
				nodes = mTreeView.Nodes;
				leafName = path;
			}

			foreach (Node node in nodes)
				if (node.Text == leafName)
					return node;

			return null;
		}

		// Returns the bounds of the specified node, including the region  
		// occupied by the node label and any node tag displayed. 
		private Rectangle NodeBounds(TreeNode node)
		{
			if (node.ImageIndex == NOIMAGE)
			// Set the return value to the normal node bounds.
				return Rectangle.Inflate(node.Bounds,2,0);

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

		public void Populate()
		{
			TreeNode node;
			lock (Blackboard.ItemsByFullPath)
				lock (Blackboard.Items)
					foreach (var item in Blackboard.Items)
					node = AddNode(item);

			mTreeView.Sort();

			Blackboard.ItemPublished += Blackboard_ItemPublished;
			Blackboard.ItemUnpublished += Blackboard_ItemUnpublished;

			// One day, revisit this. Use actual node names instead of indexes. Handle late comers
			//TreeNodeCollection tnodes;
			//if (StudioSettings.Default.BlackboardExpand.Length > 0)
			//{
			//	var paths = StudioSettings.Default.BlackboardExpand.Split(',');
			//	foreach (var nodePath in paths)
			//	{
			//		tnodes = mTreeView.Nodes;
			//		var nodes = nodePath.Split('.');
			//		for (int j = nodes.Length - 1; j >= 0; j--)
			//		{
			//			int nodeIndex = int.Parse(nodes[j]);
			//			if (nodeIndex < tnodes.Count)
			//			{
			//				var n = tnodes[nodeIndex];
			//				n.Expand();
			//				tnodes = n.Nodes;
			//			}
			//		}
			//	}
			//}

			BlackboardSelection.SourceTreeNodes = mTreeView.Nodes;

			Blackboard.RaiseViewPopulated();
		}

		TreeNode RemoveNode(Blackboard.Item item)
		{
			Node parent;
			string leafName;
			var node = FindNode(item.Path, out leafName, out parent);
			if (node != null)
			{
				//Log.WriteLine("remove {0}", node.tag);
				if (parent == null)
				{
					mTreeView.Nodes.Remove(node);
					branches.Remove(node.Text);
				}
				else
				{
					if (node.Nodes.Count > 0)
						branches.Remove(node.Text);
					parent.RemoveChild(node);
				}
				//Log.WriteLine("remove {0} {1}[{2}]", node.Text, item.Path, item.tag);
			}
			return node;
		}

		public void SelectFirst()
		{
			var node = mTreeView.Nodes[0];
			//mFocusedNode = node as Node;
			var loc = mTreeView.ClientRectangle.Location;
			loc = mTreeView.PointToScreen(loc);
			OnMouseDown(new MouseEventArgs(MouseButtons.Left,1,loc.X+8,loc.Y+20,0));
			mTreeView.SelectedNode = node;
		}

		public string TreeExpansionsText
		{
			get
			{
				var sb = new StringBuilder();
				foreach (var s in treeExpansions)
				{
					if (sb.Length > 0)
						sb.Append(',');
					sb.Append(s);
				}
				return sb.ToString();
			}
		}

		StringBuilder sb = new StringBuilder();
		string TreeNodeString(TreeNode node)
		{
			TreeNode parent = node.Parent;
			sb.Length = 0;
			sb.Append(node.Index);
			while (parent != null)
			{
				sb.Append('.');
				sb.Append(parent.Index);
				parent = parent.Parent;
			}
			return sb.ToString();
		}

		void Blackboard_Cleared(object sender, EventArgs e)
		{
			Clear();
			Blackboard.ItemPublished -= Blackboard_ItemPublished;
			Blackboard.ItemUnpublished -= Blackboard_ItemUnpublished;
			//Blackboard.DisplayPropertiesChanged -= Blackboard_DisplayPropertiesChanged;
		}

		void BlackboardMenuItem_Click(object sender, EventArgs e)
		{
			var tsi = sender as ToolStripItem;
			for(int i=0; i<mBlackboardMenuItemHost.MenuItems.Length; i++)
				if (tsi.Text == mBlackboardMenuItemHost.MenuItems[i])
				{
					mBlackboardMenuItemHost.OnMenu(i);
					return;
				}
		}

		void Blackboard_DisplayPropertiesChanged(Blackboard.Item item, IBlackboardDisplayProperties displayProperties)
		{
			if (mClosed) return;
			if (mTreeView.InvokeRequired)
				mTreeView.Invoke(new DisplayPropertiesChangedDelegate(DisplayPropertiesChanged), new object[] { item, displayProperties });
			else
				DisplayPropertiesChanged(item, displayProperties);
		}

		void Blackboard_ItemPublished(object sender, EventArgs e)
		{
			//Log.WriteLine("pub {0}", (sender as Blackboard.Item).Path);
			if (mTreeView.InvokeRequired)
				mTreeView.Invoke(new AddRemoveNodeDelegate(AddNode), new object[] { sender as Blackboard.Item });
			else
				AddNode(sender as Blackboard.Item);
		}

		void Blackboard_ItemUnpublished(object sender, EventArgs e)
		{
			//Log.WriteLine("unpub {0}", (sender as Blackboard.Item).Path);
			if (mTreeView.InvokeRequired)
				mTreeView.Invoke(new AddRemoveNodeDelegate(RemoveNode), new object[] { sender as Blackboard.Item });
			else
				RemoveNode(sender as Blackboard.Item);
		}

		private void mTreeView_AfterCollapse(object sender, TreeViewEventArgs e)
		{
			var str = TreeNodeString(e.Node);
			if (treeExpansions.Contains(str))
				treeExpansions.Remove(str);
			mMainForm.SettingsDirty = true;

			var node = e.Node as TreeNode;
			if (node.ImageIndex == NOIMAGE)
				base.Invalidate(node.Bounds);
			else
			{
				Rectangle rect = node.Bounds;
				var loc = node.Bounds.Location;
				loc.X += mImageWidth;
				rect.Location = loc;
				base.Invalidate(rect);
			}
		}

		private void mTreeView_AfterExpand(object sender, TreeViewEventArgs e)
		{
			var str = TreeNodeString(e.Node);
			if (!treeExpansions.Contains(str))
				treeExpansions.Add(str);
			mMainForm.SettingsDirty = true;
		}

		private void mTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			object obj;
			if (e.Node.Tag is Blackboard.Item)
			{
				var item = e.Node.Tag as Blackboard.Item;
				var type = item.Type;
				if (type.IsValueType || type.IsArray || type.IsEnum || type == typeof(string))
				{
					obj = item;
					if (mBindDest != null)
					{
						this.Cursor = Cursors.Default;
						Blackboard.AddBinding(item, mBindDest);
					}
				}
				else
					obj = item.Value;
			}
			else
				obj = e.Node.Tag;

			mMainForm.SettingsDirty = true;

			ObjectIsBrowsable(obj, e.Node.Text);
		}

		private void mTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			var item = e.Node.Tag as Blackboard.Item;
			if ( item != null ) item.RaiseExpanded();
			(e.Node as Node).BeforeExpand();
		}

		void mTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			if (e.Bounds.X < 0) return;
			var node = e.Node as Node;

			//Log.WriteLine("{0},{1},{2}", e.Node.Text, e.State, e.Bounds);

			var treeView = sender as TreeView;

			var nodeFont = node.NodeFont;
			if (nodeFont == null) nodeFont = treeView.Font;

			// Draw the node text.
			var rect = node.Bounds;
			if (node.ImageIndex != NOIMAGE && node.ImageIndex < mImageList.Images.Count)
			{
				var image = mImageList.Images[node.ImageIndex];
				e.Graphics.DrawImage(image, node.Bounds.Location);
				var loc = node.Bounds.Location;
				loc.X += mImageWidth;
				rect.Location = loc;
			}
			if ((e.State & TreeNodeStates.Selected) != 0 || (e.State & TreeNodeStates.Focused) != 0)
			{
				e.Graphics.FillRectangle(new SolidBrush(treeView.ForeColor), Rectangle.Inflate(rect, mMargin, 0));
				e.Graphics.DrawString(node.Text, nodeFont, Brushes.White, Rectangle.Inflate(rect, mMargin, 0));
			}
			else
				e.Graphics.DrawString(node.Text, nodeFont, new SolidBrush(ForeColor), Rectangle.Inflate(rect, mMargin, 0));
		}

		private void mTreeView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			if (!(e.Item is TreeNode)) return;

			if (e.Button == MouseButtons.Left)
				DoDragDrop((e.Item as TreeNode).Tag, DragDropEffects.All);
		}

		private void mTreeView_MouseDown(object sender, MouseEventArgs e)
		{
			var clickedNode = mTreeView.GetNodeAt(e.X, e.Y) as Node;
			Cursor = Cursors.Default;
			switch (e.Button)
			{
				case MouseButtons.Left:
					if (clickedNode != null && NodeBounds(clickedNode).Contains(e.X, e.Y))
						if (mTreeView.SelectedNode == clickedNode) // in case we are returning from elsewhere
							mTreeView.SelectedNode = null;
						mTreeView.SelectedNode = clickedNode;
					break;
				case System.Windows.Forms.MouseButtons.Right:
					if (clickedNode != null && NodeBounds(clickedNode).Contains(e.X, e.Y))
						mFocusedNode = clickedNode;
					break;
			}
		}

		private void copyPathToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (mContextMenuNode != null)
				Clipboard.SetDataObject((mContextMenuNode.Tag as Blackboard.Item).Path, true);
		}

		private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (mFocusedNode != null)
			{
				mContextMenuNode = mFocusedNode;

				BuildItemActions(mContextMenuNode);
			}
		}

		private void mTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			BuildItemActions(e.Node);
			foreach (var itemAction in mItemActions)
				if (itemAction.Action.ExecuteOnDoubleClick)
				{
					bool enabled;
					if (itemAction.Action.Visible(itemAction.Item, out enabled) && enabled)
					{
						itemAction.Action.Execute(itemAction.Item);
						return;
					}
				}
		}

		class ItemAction
		{
			internal BlackboardAction Action { get; set; }
			internal Blackboard.Item Item { get; set; }
			internal ToolStripMenuItem MenuItem { get; set; }

			internal ItemAction(BlackboardAction action, Blackboard.Item item)
			{
				Action = action;
				Item = item;
				MenuItem = new ToolStripMenuItem(Action.Name);

				MenuItem.Click += menuItem_Click;
			}

			void menuItem_Click(object sender, EventArgs e)
			{
				try
				{
					Action.Execute(Item);
				}
				catch (Exception ex)
				{
					Log.ReportException(ex,"{0} clicked",Action.Name);
				}
			}
		}

		class Node : TreeNode, ITreeNode
		{
			//static int Tags;
			//internal int tag = ++Tags;
			internal List<Node> HiddenNodes;

			internal Node() { }

			internal Node(string text) : base(text)
			{
				//Log.WriteLine("n{0}:{1}", tag, text);
			}
			internal void AddChild(Node child)
			{
				if (HiddenNodes == null) HiddenNodes = new List<Node>();
				AddNode(HiddenNodes, child);

				if (IsExpanded)
					AddNode(Nodes, child);
				else if (Nodes.Count == 0)
					Nodes.Add(child);
			}

			internal void BeforeExpand()
			{
				if (HiddenNodes.Count == Nodes.Count) return;
				//Log.WriteLine("clearing {0}'s nodes", Text);
				Nodes.Clear();
				var nodes = HiddenNodes.ToArray();
				Nodes.AddRange(nodes);
			}

			public override object Clone()
			{
				var clone = new TreeNode(Text);
				if (HiddenNodes != null)
					foreach (var node in HiddenNodes)
						clone.Nodes.Add(node.Clone() as TreeNode);
				else
					foreach (TreeNode node in Nodes)
						clone.Nodes.Add(node.Clone() as TreeNode);
				clone.Tag = Tag;
				if (IsExpanded)
					clone.Expand();
				return clone;
			}

			internal void RemoveChild(Node child)
			{
				HiddenNodes.Remove(child);
				Nodes.Remove(child);
				if ( !IsExpanded )
					BeforeExpand();
			}
		}

		private void bindToToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.SizeAll;
			mBindDest = mContextMenuNode.Tag as Blackboard.Item;
		}
		Blackboard.Item mBindDest;

		private void BlackboardView_FormClosing(object sender, FormClosingEventArgs e)
		{
			mClosed = true;
		}

	}
}
