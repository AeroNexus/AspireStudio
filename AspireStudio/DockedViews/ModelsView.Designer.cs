namespace Aspire.Studio.DockedViews
{
	partial class ModelsView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelsView));
			this.mTreeView = new System.Windows.Forms.TreeView();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.SuspendLayout();
			// 
			// mTreeView
			// 
			this.mTreeView.AllowDrop = true;
			this.mTreeView.ContextMenuStrip = this.contextMenuStrip1;
			this.mTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mTreeView.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(23)))), ((int)(((byte)(23)))));
			this.mTreeView.Location = new System.Drawing.Point(0, 0);
			this.mTreeView.Name = "mTreeView";
			this.mTreeView.Size = new System.Drawing.Size(292, 266);
			this.mTreeView.TabIndex = 0;
			this.mTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.mTreeView_ItemDrag);
			this.mTreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.mTreeView_DragDrop);
			this.mTreeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.mTreeView_DragEnter);
			this.mTreeView.DragOver += new System.Windows.Forms.DragEventHandler(this.mTreeView_DragOver);
			this.mTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mTreeView_MouseDown);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
			this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
			// 
			// ModelsView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.mTreeView);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ModelsView";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft;
			this.TabText = "Models";
			this.Text = "";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TreeView mTreeView;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
	}
}
