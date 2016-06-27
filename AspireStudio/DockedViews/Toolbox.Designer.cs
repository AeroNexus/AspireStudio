namespace Aspire.Studio.DockedViews
{
    partial class Toolbox
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Toolbox));
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.toolBox1 = new Silver.UI.ToolBox();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.addItemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.addCategoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.contextMenuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "Mouse.bmp");
			// 
			// toolBox1
			// 
			this.toolBox1.AllowDrop = true;
			this.toolBox1.AllowSwappingByDragDrop = true;
			this.toolBox1.ContextMenuStrip = this.contextMenuStrip1;
			this.toolBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolBox1.InitialScrollDelay = 500;
			this.toolBox1.ItemBackgroundColor = System.Drawing.Color.Empty;
			this.toolBox1.ItemBorderColor = System.Drawing.Color.Empty;
			this.toolBox1.ItemHeight = 20;
			this.toolBox1.ItemHoverColor = System.Drawing.Color.BurlyWood;
			this.toolBox1.ItemHoverTextColor = System.Drawing.SystemColors.ControlText;
			this.toolBox1.ItemNormalColor = System.Drawing.SystemColors.Control;
			this.toolBox1.ItemNormalTextColor = System.Drawing.SystemColors.ControlText;
			this.toolBox1.ItemSelectedColor = System.Drawing.Color.Linen;
			this.toolBox1.ItemSelectedTextColor = System.Drawing.SystemColors.ControlText;
			this.toolBox1.ItemSpacing = 1;
			this.toolBox1.LargeItemSize = new System.Drawing.Size(64, 64);
			this.toolBox1.LayoutDelay = 10;
			this.toolBox1.Location = new System.Drawing.Point(0, 0);
			this.toolBox1.Name = "toolBox1";
			this.toolBox1.ScrollDelay = 60;
			this.toolBox1.SelectAllTextWhileRenaming = true;
			this.toolBox1.SelectedTabIndex = -1;
			this.toolBox1.ShowOnlyOneItemPerRow = false;
			this.toolBox1.Size = new System.Drawing.Size(74, 245);
			this.toolBox1.SmallImageList = this.imageList;
			this.toolBox1.SmallItemSize = new System.Drawing.Size(32, 32);
			this.toolBox1.TabHeight = 18;
			this.toolBox1.TabHoverTextColor = System.Drawing.SystemColors.ControlText;
			this.toolBox1.TabIndex = 1;
			this.toolBox1.TabNormalTextColor = System.Drawing.SystemColors.ControlText;
			this.toolBox1.TabSelectedTextColor = System.Drawing.SystemColors.ControlText;
			this.toolBox1.TabSpacing = 1;
			this.toolBox1.UseItemColorInRename = false;
			this.toolBox1.TabSelectionChanged += new Silver.UI.TabSelectionChangedHandler(this.toolBox1_TabSelectionChanged);
			this.toolBox1.ItemSelectionChanged += new Silver.UI.ItemSelectionChangedHandler(this.toolBox1_ItemSelectionChanged);
			this.toolBox1.TabMouseUp += new Silver.UI.TabMouseEventHandler(this.toolBox1_TabMouseUp);
			this.toolBox1.ItemMouseUp += new Silver.UI.ItemMouseEventHandler(this.toolBox1_ItemMouseUp);
			this.toolBox1.ItemKeyPress += new Silver.UI.ItemKeyPressEventHandler(this.toolBox1_ItemKeyPress);
			this.toolBox1.DragDropFinished += new Silver.UI.DragDropFinishedHandler(this.toolBox1_DragDropFinished);
			this.toolBox1.RenameFinished += new Silver.UI.RenameFinishedHandler(this.toolBox1_RenameFinished);
			this.toolBox1.OnSerializeObject += new Silver.UI.XmlSerializerHandler(this.toolBox1_OnSerializeObject);
			this.toolBox1.OnDeSerializeObject += new Silver.UI.XmlSerializerHandler(this.toolBox1_OnDeSerializeObject);
			this.toolBox1.OnBeginDragDrop += new Silver.UI.PreDragDropHandler(this.toolBox1_OnBeginDragDrop);
			this.toolBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolBox1_MouseUp);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addItemsToolStripMenuItem,
            this.addCategoryToolStripMenuItem,
            this.renameToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(127, 70);
			// 
			// addItemsToolStripMenuItem
			// 
			this.addItemsToolStripMenuItem.Name = "addItemsToolStripMenuItem";
			this.addItemsToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
			this.addItemsToolStripMenuItem.Text = "Add items";
			this.addItemsToolStripMenuItem.Click += new System.EventHandler(this.addItemsToolStripMenuItem_Click);
			// 
			// addCategoryToolStripMenuItem
			// 
			this.addCategoryToolStripMenuItem.Name = "addCategoryToolStripMenuItem";
			this.addCategoryToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
			this.addCategoryToolStripMenuItem.Text = "Add category";
			this.addCategoryToolStripMenuItem.Click += new System.EventHandler(this.addCategoryToolStripMenuItem_Click);
			// 
			// renameToolStripMenuItem
			// 
			this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
			this.renameToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
			this.renameToolStripMenuItem.Text = "Rename";
			this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
			// 
			// listBox1
			// 
			this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBox1.FormattingEnabled = true;
			this.listBox1.Location = new System.Drawing.Point(0, 0);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(90, 245);
			this.listBox1.TabIndex = 2;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 2);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.toolBox1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.listBox1);
			this.splitContainer1.Size = new System.Drawing.Size(168, 245);
			this.splitContainer1.SplitterDistance = 74;
			this.splitContainer1.TabIndex = 3;
			// 
			// Toolbox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(168, 249);
			this.Controls.Add(this.splitContainer1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Toolbox";
			this.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockLeftAutoHide;
			this.TabText = "Toolbox";
			this.Text = "Toolbox";
			this.contextMenuStrip1.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private System.Windows.Forms.ImageList imageList;
		private Silver.UI.ToolBox toolBox1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem addItemsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addCategoryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.SplitContainer splitContainer1;
    }
}