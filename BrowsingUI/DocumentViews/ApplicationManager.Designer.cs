using Aspire.BrowsingUI.DocumentViews.AppMgr;

namespace Aspire.BrowsingUI.DocumentViews
{
	partial class ApplicationManager : Aspire.Studio.DocumentViews.StudioDocumentView, IAppInfoClient
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.applicationsTab = new System.Windows.Forms.TabPage();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.NameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PidColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.StateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PriorityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.IdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
			this.configurationsTab = new System.Windows.Forms.TabPage();
			this.configsLb = new System.Windows.Forms.CheckedListBox();
			this.configsLbContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.addAppToConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.placeholderToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new System.Windows.Forms.Panel();
			this.cloneBtn = new System.Windows.Forms.Button();
			this.activateBtn = new System.Windows.Forms.Button();
			this.removeBtn = new System.Windows.Forms.Button();
			this.newBtn = new System.Windows.Forms.Button();
			this.aspireTab = new System.Windows.Forms.TabPage();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.appMgrToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.startAspireToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.stopAspireToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.addAppToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.placeholderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.heartbeatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tabControl.SuspendLayout();
			this.applicationsTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
			this.configurationsTab.SuspendLayout();
			this.configsLbContextMenu.SuspendLayout();
			this.panel1.SuspendLayout();
			this.menuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.applicationsTab);
			this.tabControl.Controls.Add(this.configurationsTab);
			this.tabControl.Controls.Add(this.aspireTab);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = new System.Drawing.Point(0, 4);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(269, 189);
			this.tabControl.TabIndex = 3;
			// 
			// applicationsTab
			// 
			this.applicationsTab.BackColor = System.Drawing.Color.Transparent;
			this.applicationsTab.Controls.Add(this.dataGridView1);
			this.applicationsTab.Location = new System.Drawing.Point(4, 20);
			this.applicationsTab.Name = "applicationsTab";
			this.applicationsTab.Padding = new System.Windows.Forms.Padding(3);
			this.applicationsTab.Size = new System.Drawing.Size(261, 165);
			this.applicationsTab.TabIndex = 0;
			this.applicationsTab.Text = "Applications";
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			this.dataGridView1.AllowUserToOrderColumns = true;
			this.dataGridView1.AllowUserToResizeRows = false;
			this.dataGridView1.AutoGenerateColumns = false;
			this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NameColumn,
            this.PidColumn,
            this.StateColumn,
            this.PriorityColumn,
            this.IdColumn});
			this.dataGridView1.DataSource = this.bindingSource1;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
			this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView1.Location = new System.Drawing.Point(3, 3);
			this.dataGridView1.MultiSelect = false;
			this.dataGridView1.Name = "dataGridView1";
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.dataGridView1.RowHeadersVisible = false;
			this.dataGridView1.RowTemplate.ContextMenuStrip = this.contextMenuStrip;
			this.dataGridView1.RowTemplate.Height = 20;
			this.dataGridView1.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridView1.Size = new System.Drawing.Size(255, 159);
			this.dataGridView1.TabIndex = 0;
			this.dataGridView1.VirtualMode = true;
			this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
			this.dataGridView1.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellMouseEnter);
			this.dataGridView1.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellMouseLeave);
			this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
			this.dataGridView1.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridView1_DataError);
			this.dataGridView1.RowContextMenuStripNeeded += new System.Windows.Forms.DataGridViewRowContextMenuStripNeededEventHandler(this.dataGridView1_RowContextMenuStripNeeded);
			this.dataGridView1.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.dataGridView1_RowsAdded);
			this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
			this.dataGridView1.DoubleClick += new System.EventHandler(this.dataGridView1_DoubleClick);
			// 
			// NameColumn
			// 
			this.NameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.NameColumn.HeaderText = "Name";
			this.NameColumn.Name = "NameColumn";
			this.NameColumn.ReadOnly = true;
			this.NameColumn.Width = 60;
			// 
			// PidColumn
			// 
			this.PidColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.PidColumn.HeaderText = "Pid";
			this.PidColumn.Name = "PidColumn";
			this.PidColumn.ReadOnly = true;
			this.PidColumn.Width = 47;
			// 
			// StateColumn
			// 
			this.StateColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.StateColumn.HeaderText = "State";
			this.StateColumn.Name = "StateColumn";
			this.StateColumn.ReadOnly = true;
			this.StateColumn.Width = 57;
			// 
			// PriorityColumn
			// 
			this.PriorityColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.PriorityColumn.HeaderText = "Priority";
			this.PriorityColumn.Name = "PriorityColumn";
			// 
			// IdColumn
			// 
			this.IdColumn.HeaderText = "Id";
			this.IdColumn.Name = "IdColumn";
			this.IdColumn.ReadOnly = true;
			this.IdColumn.Visible = false;
			// 
			// configurationsTab
			// 
			this.configurationsTab.Controls.Add(this.configsLb);
			this.configurationsTab.Controls.Add(this.panel1);
			this.configurationsTab.Location = new System.Drawing.Point(4, 20);
			this.configurationsTab.Name = "configurationsTab";
			this.configurationsTab.Padding = new System.Windows.Forms.Padding(3);
			this.configurationsTab.Size = new System.Drawing.Size(261, 165);
			this.configurationsTab.TabIndex = 1;
			this.configurationsTab.Text = "Configurations";
			this.configurationsTab.UseVisualStyleBackColor = true;
			this.configurationsTab.DoubleClick += new System.EventHandler(this.configurationsTab_DoubleClick);
			// 
			// configsLb
			// 
			this.configsLb.ContextMenuStrip = this.configsLbContextMenu;
			this.configsLb.Dock = System.Windows.Forms.DockStyle.Fill;
			this.configsLb.FormattingEnabled = true;
			this.configsLb.Location = new System.Drawing.Point(3, 3);
			this.configsLb.Name = "configsLb";
			this.configsLb.Size = new System.Drawing.Size(173, 159);
			this.configsLb.Sorted = true;
			this.configsLb.TabIndex = 0;
			this.configsLb.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.configsLb_ItemCheck);
			this.configsLb.SelectedIndexChanged += new System.EventHandler(this.configsLb_SelectedIndexChanged);
			this.configsLb.DoubleClick += new System.EventHandler(this.configsLb_DoubleClick);
			this.configsLb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.configsLb_MouseDown);
			// 
			// configsLbContextMenu
			// 
			this.configsLbContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addAppToConfigToolStripMenuItem});
			this.configsLbContextMenu.Name = "configsLbContextMenu";
			this.configsLbContextMenu.Size = new System.Drawing.Size(108, 26);
			// 
			// addAppToConfigToolStripMenuItem
			// 
			this.addAppToConfigToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.placeholderToolStripMenuItem2});
			this.addAppToConfigToolStripMenuItem.Name = "addAppToConfigToolStripMenuItem";
			this.addAppToConfigToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			this.addAppToConfigToolStripMenuItem.Text = "Add App";
			this.addAppToConfigToolStripMenuItem.DropDownOpening += new System.EventHandler(this.addAppToConfig_DropDownOpening);
			// 
			// placeholderToolStripMenuItem2
			// 
			this.placeholderToolStripMenuItem2.Name = "placeholderToolStripMenuItem2";
			this.placeholderToolStripMenuItem2.Size = new System.Drawing.Size(152, 22);
			this.placeholderToolStripMenuItem2.Text = "placeholder";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cloneBtn);
			this.panel1.Controls.Add(this.activateBtn);
			this.panel1.Controls.Add(this.removeBtn);
			this.panel1.Controls.Add(this.newBtn);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel1.Location = new System.Drawing.Point(176, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(82, 159);
			this.panel1.TabIndex = 6;
			// 
			// cloneBtn
			// 
			this.cloneBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cloneBtn.Location = new System.Drawing.Point(13, 33);
			this.cloneBtn.Name = "cloneBtn";
			this.cloneBtn.Size = new System.Drawing.Size(57, 23);
			this.cloneBtn.TabIndex = 6;
			this.cloneBtn.Text = "Clone";
			this.cloneBtn.UseVisualStyleBackColor = true;
			this.cloneBtn.Click += new System.EventHandler(this.cloneBtn_Click);
			// 
			// activateBtn
			// 
			this.activateBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.activateBtn.Location = new System.Drawing.Point(13, 91);
			this.activateBtn.Name = "activateBtn";
			this.activateBtn.Size = new System.Drawing.Size(57, 23);
			this.activateBtn.TabIndex = 5;
			this.activateBtn.Text = "Activate";
			this.activateBtn.UseVisualStyleBackColor = true;
			this.activateBtn.Click += new System.EventHandler(this.activateBtn_Click);
			// 
			// removeBtn
			// 
			this.removeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.removeBtn.Location = new System.Drawing.Point(13, 62);
			this.removeBtn.Name = "removeBtn";
			this.removeBtn.Size = new System.Drawing.Size(57, 23);
			this.removeBtn.TabIndex = 4;
			this.removeBtn.Text = "Remove";
			this.removeBtn.UseVisualStyleBackColor = true;
			this.removeBtn.Click += new System.EventHandler(this.removeBtn_Click);
			// 
			// newBtn
			// 
			this.newBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.newBtn.Location = new System.Drawing.Point(13, 3);
			this.newBtn.Name = "newBtn";
			this.newBtn.Size = new System.Drawing.Size(57, 23);
			this.newBtn.TabIndex = 2;
			this.newBtn.Text = "New";
			this.newBtn.UseVisualStyleBackColor = true;
			this.newBtn.Click += new System.EventHandler(this.newBtn_Click);
			// 
			// aspireTab
			// 
			this.aspireTab.Location = new System.Drawing.Point(4, 20);
			this.aspireTab.Name = "aspireTab";
			this.aspireTab.Padding = new System.Windows.Forms.Padding(3);
			this.aspireTab.Size = new System.Drawing.Size(261, 165);
			this.aspireTab.TabIndex = 2;
			this.aspireTab.Text = "Aspire Down";
			this.aspireTab.UseVisualStyleBackColor = true;
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.appMgrToolStripMenuItem});
			this.menuStrip.Location = new System.Drawing.Point(0, 4);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(269, 24);
			this.menuStrip.TabIndex = 4;
			this.menuStrip.Visible = false;
			// 
			// appMgrToolStripMenuItem
			// 
			this.appMgrToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startAspireToolStripMenuItem,
            this.stopAspireToolStripMenuItem,
            this.addAppToolStripMenuItem,
            this.heartbeatToolStripMenuItem});
			this.appMgrToolStripMenuItem.Name = "appMgrToolStripMenuItem";
			this.appMgrToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
			this.appMgrToolStripMenuItem.Text = "AppMgr";
			// 
			// startAspireToolStripMenuItem
			// 
			this.startAspireToolStripMenuItem.Name = "startAspireToolStripMenuItem";
			this.startAspireToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
			this.startAspireToolStripMenuItem.Text = "Start Aspire";
			this.startAspireToolStripMenuItem.Click += new System.EventHandler(this.startAspireToolStripMenuItem_Click);
			// 
			// stopAspireToolStripMenuItem
			// 
			this.stopAspireToolStripMenuItem.Name = "stopAspireToolStripMenuItem";
			this.stopAspireToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
			this.stopAspireToolStripMenuItem.Text = "Stop Aspire";
			this.stopAspireToolStripMenuItem.Click += new System.EventHandler(this.stopAspireToolStripMenuItem_Click);
			// 
			// addAppToolStripMenuItem
			// 
			this.addAppToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.placeholderToolStripMenuItem});
			this.addAppToolStripMenuItem.Name = "addAppToolStripMenuItem";
			this.addAppToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
			this.addAppToolStripMenuItem.Text = "Add App";
			this.addAppToolStripMenuItem.DropDownOpening += new System.EventHandler(this.addAppToolStripMenuItem_DropDownOpening);
			// 
			// placeholderToolStripMenuItem
			// 
			this.placeholderToolStripMenuItem.Name = "placeholderToolStripMenuItem";
			this.placeholderToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
			this.placeholderToolStripMenuItem.Text = "placeholder";
			// 
			// heartbeatToolStripMenuItem
			// 
			this.heartbeatToolStripMenuItem.CheckOnClick = true;
			this.heartbeatToolStripMenuItem.Name = "heartbeatToolStripMenuItem";
			this.heartbeatToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
			this.heartbeatToolStripMenuItem.Text = "Heartbeat";
			this.heartbeatToolStripMenuItem.CheckedChanged += new System.EventHandler(this.heartbeatToolStripMenuItem_CheckedChanged);
			// 
			// ApplicationManager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(269, 193);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.menuStrip);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MainMenuStrip = this.menuStrip;
			this.Name = "ApplicationManager";
			this.Text = "ApplicationManager";
			this.ToolTipText = "";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ApplicationManager_FormClosing);
			this.Load += new System.EventHandler(this.ApplicationManager_Load);
			this.tabControl.ResumeLayout(false);
			this.applicationsTab.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
			this.configurationsTab.ResumeLayout(false);
			this.configsLbContextMenu.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage applicationsTab;
		private System.Windows.Forms.TabPage configurationsTab;
		private System.Windows.Forms.Button newBtn;
		private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.DataGridViewTextBoxColumn NameColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn PidColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn StateColumn;
		private System.Windows.Forms.Button removeBtn;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.CheckedListBox configsLb;
		private System.Windows.Forms.Button activateBtn;
		private System.Windows.Forms.Button cloneBtn;
		private System.Windows.Forms.ToolStripMenuItem appMgrToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem startAspireToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem stopAspireToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addAppToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem placeholderToolStripMenuItem;
		private System.Windows.Forms.DataGridViewTextBoxColumn PriorityColumn;
		private System.Windows.Forms.TabPage aspireTab;
		private System.Windows.Forms.BindingSource bindingSource1;
		private System.Windows.Forms.DataGridViewTextBoxColumn IdColumn;
		private System.Windows.Forms.ToolStripMenuItem heartbeatToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip configsLbContextMenu;
		private System.Windows.Forms.ToolStripMenuItem addAppToConfigToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem placeholderToolStripMenuItem2;
	}
}