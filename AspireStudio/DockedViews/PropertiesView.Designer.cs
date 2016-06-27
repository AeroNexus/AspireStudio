namespace Aspire.Studio.DockedViews
{
    partial class PropertiesView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertiesView));
			this.mComboBox = new System.Windows.Forms.ComboBox();
			this.mPropertyGrid = new System.Windows.Forms.PropertyGrid();
			this.SuspendLayout();
			// 
			// mComboBox
			// 
			this.mComboBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.mComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.mComboBox.FormattingEnabled = true;
			this.mComboBox.Location = new System.Drawing.Point(0, 0);
			this.mComboBox.Margin = new System.Windows.Forms.Padding(2);
			this.mComboBox.Name = "mComboBox";
			this.mComboBox.Size = new System.Drawing.Size(207, 21);
			this.mComboBox.TabIndex = 2;
			// 
			// mPropertyGrid
			// 
			this.mPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mPropertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.mPropertyGrid.Location = new System.Drawing.Point(0, 21);
			this.mPropertyGrid.Margin = new System.Windows.Forms.Padding(2);
			this.mPropertyGrid.Name = "mPropertyGrid";
			this.mPropertyGrid.Size = new System.Drawing.Size(207, 281);
			this.mPropertyGrid.TabIndex = 1;
			this.mPropertyGrid.SelectedObjectsChanged += new System.EventHandler(this.mPropertyGrid_SelectedObjectsChanged);
			this.mPropertyGrid.Enter += new System.EventHandler(this.mPropertyGrid_Enter);
			this.mPropertyGrid.Leave += new System.EventHandler(this.mPropertyGrid_Leave);
			// 
			// PropertiesView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(207, 302);
			this.Controls.Add(this.mPropertyGrid);
			this.Controls.Add(this.mComboBox);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "PropertiesView";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
			this.TabText = "Properties";
			this.Text = "Properties";
			this.ResumeLayout(false);

		}
		#endregion

		private System.Windows.Forms.ComboBox mComboBox;
		private System.Windows.Forms.PropertyGrid mPropertyGrid;
    }
}