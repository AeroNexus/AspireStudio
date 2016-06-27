namespace Aspire.Studio.DockedViews
{
    partial class TaskListView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaskListView));
			this.listView = new System.Windows.Forms.ListView();
			this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.stateColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.descriptionColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.newTaskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// listView
			// 
			this.listView.AllowColumnReorder = true;
			this.listView.AutoArrange = false;
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.stateColumn,
            this.descriptionColumn});
			this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView.GridLines = true;
			this.listView.HideSelection = false;
			this.listView.LabelEdit = true;
			this.listView.Location = new System.Drawing.Point(0, 3);
			this.listView.MultiSelect = false;
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(337, 370);
			this.listView.TabIndex = 0;
			this.listView.UseCompatibleStateImageBehavior = false;
			this.listView.View = System.Windows.Forms.View.Details;
			this.listView.ItemActivate += new System.EventHandler(this.listView_ItemActivate);
			// 
			// nameColumn
			// 
			this.nameColumn.Text = "Task";
			this.nameColumn.Width = 40;
			// 
			// stateColumn
			// 
			this.stateColumn.Text = "!";
			this.stateColumn.Width = 16;
			// 
			// descriptionColumn
			// 
			this.descriptionColumn.Text = "Description";
			this.descriptionColumn.Width = 300;
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newTaskToolStripMenuItem});
			this.contextMenuStrip.Name = "contextMenuStrip";
			this.contextMenuStrip.Size = new System.Drawing.Size(114, 26);
			// 
			// newTaskToolStripMenuItem
			// 
			this.newTaskToolStripMenuItem.Name = "newTaskToolStripMenuItem";
			this.newTaskToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
			this.newTaskToolStripMenuItem.Text = "New Task";
			this.newTaskToolStripMenuItem.Click += new System.EventHandler(this.newTaskToolStripMenuItem_Click);
			// 
			// TaskListView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(337, 376);
			this.Controls.Add(this.listView);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "TaskListView";
			this.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottomAutoHide;
			this.TabPageContextMenuStrip = this.contextMenuStrip;
			this.TabText = "Task List";
			this.Text = "Task List";
			this.contextMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private System.Windows.Forms.ListView listView;
		private System.Windows.Forms.ColumnHeader nameColumn;
		private System.Windows.Forms.ColumnHeader stateColumn;
		private System.Windows.Forms.ColumnHeader descriptionColumn;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem newTaskToolStripMenuItem;
    }
}