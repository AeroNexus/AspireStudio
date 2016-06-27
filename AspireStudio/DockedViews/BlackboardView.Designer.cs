namespace Aspire.Studio.DockedViews
{
	partial class BlackboardView
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlackboardView));
			this.mTreeView = new System.Windows.Forms.TreeView();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.copyPathToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bindToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// mTreeView
			// 
			this.mTreeView.ContextMenuStrip = this.contextMenuStrip1;
			this.mTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mTreeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.mTreeView.Indent = 19;
			this.mTreeView.ItemHeight = 16;
			this.mTreeView.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(23)))), ((int)(((byte)(23)))));
			this.mTreeView.Location = new System.Drawing.Point(0, 0);
			this.mTreeView.Name = "mTreeView";
			this.mTreeView.PathSeparator = ".";
			this.mTreeView.ShowNodeToolTips = true;
			this.mTreeView.Size = new System.Drawing.Size(292, 266);
			this.mTreeView.TabIndex = 0;
			this.mTreeView.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.mTreeView_AfterCollapse);
			this.mTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.mTreeView_BeforeExpand);
			this.mTreeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.mTreeView_AfterExpand);
			this.mTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.mTreeView_ItemDrag);
			this.mTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.mTreeView_AfterSelect);
			this.mTreeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.mTreeView_NodeMouseDoubleClick);
			this.mTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mTreeView_MouseDown);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Font = new System.Drawing.Font("Tahoma", 7F);
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyPathToClipboardToolStripMenuItem,
            this.bindToToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(176, 48);
			this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
			// 
			// copyPathToClipboardToolStripMenuItem
			// 
			this.copyPathToClipboardToolStripMenuItem.Name = "copyPathToClipboardToolStripMenuItem";
			this.copyPathToClipboardToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			this.copyPathToClipboardToolStripMenuItem.Text = "Copy path to clipboard";
			this.copyPathToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyPathToClipboardToolStripMenuItem_Click);
			// 
			// bindToToolStripMenuItem
			// 
			this.bindToToolStripMenuItem.Name = "bindToToolStripMenuItem";
			this.bindToToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
			this.bindToToolStripMenuItem.Text = "Bind to source item";
			this.bindToToolStripMenuItem.Click += new System.EventHandler(this.bindToToolStripMenuItem_Click);
			// 
			// BlackboardView
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.mTreeView);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "BlackboardView";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft;
			this.TabText = "Blackboard";
			this.Text = "";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BlackboardView_FormClosing);
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TreeView mTreeView;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem copyPathToClipboardToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem bindToToolStripMenuItem;
	}
}
