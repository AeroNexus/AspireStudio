namespace Aspire.BrowsingUI.DocumentViews
{
	partial class HostedPayload
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
			this.greenLbl = new System.Windows.Forms.Label();
			this.greenKeyTb = new System.Windows.Forms.TextBox();
			this.redKeyTb = new System.Windows.Forms.TextBox();
			this.redLbl = new System.Windows.Forms.Label();
			this.connectBtn = new System.Windows.Forms.Button();
			this.brownKeyTb = new System.Windows.Forms.TextBox();
			this.brownLbl = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// greenLbl
			// 
			this.greenLbl.AutoSize = true;
			this.greenLbl.Location = new System.Drawing.Point(18, 15);
			this.greenLbl.Name = "greenLbl";
			this.greenLbl.Size = new System.Drawing.Size(132, 13);
			this.greenLbl.TabIndex = 1;
			this.greenLbl.Text = "System Authentication key";
			// 
			// greenKeyTb
			// 
			this.greenKeyTb.Location = new System.Drawing.Point(21, 32);
			this.greenKeyTb.Name = "greenKeyTb";
			this.greenKeyTb.Size = new System.Drawing.Size(206, 20);
			this.greenKeyTb.TabIndex = 2;
			this.greenKeyTb.TextChanged += new System.EventHandler(this.greenKeyTb_TextChanged);
			// 
			// redKeyTb
			// 
			this.redKeyTb.Location = new System.Drawing.Point(21, 82);
			this.redKeyTb.Name = "redKeyTb";
			this.redKeyTb.Size = new System.Drawing.Size(206, 20);
			this.redKeyTb.TabIndex = 4;
			this.redKeyTb.TextChanged += new System.EventHandler(this.redKeyTb_TextChanged);
			// 
			// redLbl
			// 
			this.redLbl.AutoSize = true;
			this.redLbl.Location = new System.Drawing.Point(18, 65);
			this.redLbl.Name = "redLbl";
			this.redLbl.Size = new System.Drawing.Size(124, 13);
			this.redLbl.TabIndex = 3;
			this.redLbl.Text = "ASIM Authentication key";
			// 
			// connectBtn
			// 
			this.connectBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.connectBtn.Location = new System.Drawing.Point(259, 147);
			this.connectBtn.Name = "connectBtn";
			this.connectBtn.Size = new System.Drawing.Size(75, 23);
			this.connectBtn.TabIndex = 5;
			this.connectBtn.Text = "Connect";
			this.connectBtn.UseVisualStyleBackColor = true;
			this.connectBtn.Click += new System.EventHandler(this.connectBtn_Click);
			// 
			// brownKeyTb
			// 
			this.brownKeyTb.Location = new System.Drawing.Point(21, 131);
			this.brownKeyTb.Name = "brownKeyTb";
			this.brownKeyTb.Size = new System.Drawing.Size(206, 20);
			this.brownKeyTb.TabIndex = 7;
			this.brownKeyTb.TextChanged += new System.EventHandler(this.brownKeyTb_TextChanged);
			// 
			// brownLbl
			// 
			this.brownLbl.AutoSize = true;
			this.brownLbl.Location = new System.Drawing.Point(18, 114);
			this.brownLbl.Name = "brownLbl";
			this.brownLbl.Size = new System.Drawing.Size(121, 13);
			this.brownLbl.TabIndex = 6;
			this.brownLbl.Text = "Data Authentication key";
			// 
			// HostedPayload
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(347, 184);
			this.Controls.Add(this.brownKeyTb);
			this.Controls.Add(this.brownLbl);
			this.Controls.Add(this.connectBtn);
			this.Controls.Add(this.redKeyTb);
			this.Controls.Add(this.redLbl);
			this.Controls.Add(this.greenKeyTb);
			this.Controls.Add(this.greenLbl);
			this.Name = "HostedPayload";
			this.Text = "Hosted Payload";
			this.ToolTipText = "";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label greenLbl;
		private System.Windows.Forms.TextBox greenKeyTb;
		private System.Windows.Forms.TextBox redKeyTb;
		private System.Windows.Forms.Label redLbl;
		private System.Windows.Forms.Button connectBtn;
		private System.Windows.Forms.TextBox brownKeyTb;
		private System.Windows.Forms.Label brownLbl;

	}
}