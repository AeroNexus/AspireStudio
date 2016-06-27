namespace Aspire.BrowsingUI.DocumentViews
{
	partial class DirectoryConsole
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
			this.commandTb = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// commandTb
			// 
			this.commandTb.AcceptsReturn = true;
			this.commandTb.Location = new System.Drawing.Point(76, 10);
			this.commandTb.Multiline = true;
			this.commandTb.Name = "commandTb";
			this.commandTb.Size = new System.Drawing.Size(100, 20);
			this.commandTb.TabIndex = 0;
			this.commandTb.TextChanged += new System.EventHandler(this.commandTb_TextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Command:";
			// 
			// DirectoryConsole
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(290, 188);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.commandTb);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "DirectoryConsole";
			this.Text = "DirectoryConsole";
			this.ToolTipText = "";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox commandTb;
		private System.Windows.Forms.Label label1;
	}
}