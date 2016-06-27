namespace Aspire.Studio.DocumentViews
{
    partial class StudioDocumentView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StudioDocumentView));
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.SuspendLayout();
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Name = "contextMenuStrip1";
			this.contextMenuStrip.Size = new System.Drawing.Size(153, 26);
			// 
			// StudioDocumentView
			// 
			this.ClientSize = new System.Drawing.Size(448, 393);
			this.ContextMenuStrip = this.contextMenuStrip;
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "StudioDocumentView";
			this.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.StudioDoc_DragEnter);
			this.DoubleClick += new System.EventHandler(this.StudioDocumentView_DoubleClick);
			this.ResumeLayout(false);

		}
		#endregion

		protected System.Windows.Forms.ToolTip toolTip;
		protected System.Windows.Forms.ContextMenuStrip contextMenuStrip;
    }
}