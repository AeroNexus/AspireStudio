namespace Aspire.CoreModels
{
	partial class AspireShellUI
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
			this.label1 = new System.Windows.Forms.Label();
			this.tbXtedsFile = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.tbDomain = new System.Windows.Forms.TextBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.dlgXtedsBrowse = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(0, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(60, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "xTEDS File";
			// 
			// tbXtedsFile
			// 
			this.tbXtedsFile.Location = new System.Drawing.Point(66, 8);
			this.tbXtedsFile.Name = "tbXtedsFile";
			this.tbXtedsFile.Size = new System.Drawing.Size(205, 20);
			this.tbXtedsFile.TabIndex = 1;
			// 
			// btnBrowse
			// 
			this.btnBrowse.Location = new System.Drawing.Point(277, 5);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(30, 23);
			this.btnBrowse.TabIndex = 2;
			this.btnBrowse.Text = "...";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(0, 43);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Domain";
			// 
			// tbDomain
			// 
			this.tbDomain.Location = new System.Drawing.Point(66, 40);
			this.tbDomain.Name = "tbDomain";
			this.tbDomain.Size = new System.Drawing.Size(91, 20);
			this.tbDomain.TabIndex = 4;
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(261, 38);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(46, 23);
			this.btnOK.TabIndex = 5;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// dlgXtedsBrowse
			// 
			this.dlgXtedsBrowse.DefaultExt = "xteds";
			this.dlgXtedsBrowse.ReadOnlyChecked = true;
			this.dlgXtedsBrowse.Title = "Browse for xTEDS";
			// 
			// AspireShellUI
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(311, 67);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.tbDomain);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.tbXtedsFile);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AspireShellUI";
			this.Text = "New Aspire Shell";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbXtedsFile;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbDomain;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.OpenFileDialog dlgXtedsBrowse;
	}
}