namespace Aspire.Studio.Dialogs
{
	partial class TaskEdit
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
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.nameTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.stateComboBox = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.descriptionTextBox = new System.Windows.Forms.TextBox();
			this.taskEditBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.taskEditBindingSource4 = new System.Windows.Forms.BindingSource(this.components);
			this.taskEditBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
			this.taskEditBindingSource3 = new System.Windows.Forms.BindingSource(this.components);
			this.taskEditBindingSource2 = new System.Windows.Forms.BindingSource(this.components);
			((System.ComponentModel.ISupportInitialize)(this.taskEditBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.taskEditBindingSource4)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.taskEditBindingSource1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.taskEditBindingSource3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.taskEditBindingSource2)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Location = new System.Drawing.Point(184, 193);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "OK";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Name";
			// 
			// nameTextBox
			// 
			this.nameTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.taskEditBindingSource4, "TaskName", true));
			this.nameTextBox.Location = new System.Drawing.Point(55, 13);
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.Size = new System.Drawing.Size(100, 20);
			this.nameTextBox.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(32, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "State";
			// 
			// stateComboBox
			// 
			this.stateComboBox.FormattingEnabled = true;
			this.stateComboBox.Location = new System.Drawing.Point(55, 45);
			this.stateComboBox.Name = "stateComboBox";
			this.stateComboBox.Size = new System.Drawing.Size(66, 21);
			this.stateComboBox.TabIndex = 4;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(13, 77);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(60, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Description";
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.taskEditBindingSource, "TaskDescription", true));
			this.descriptionTextBox.Location = new System.Drawing.Point(79, 74);
			this.descriptionTextBox.Multiline = true;
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = new System.Drawing.Size(165, 85);
			this.descriptionTextBox.TabIndex = 6;
			// 
			// taskEditBindingSource
			// 
			this.taskEditBindingSource.DataSource = typeof(Aspire.Studio.Dialogs.TaskEdit);
			// 
			// taskEditBindingSource4
			// 
			this.taskEditBindingSource4.DataSource = typeof(Aspire.Studio.Dialogs.TaskEdit);
			// 
			// taskEditBindingSource1
			// 
			this.taskEditBindingSource1.DataSource = typeof(Aspire.Studio.Dialogs.TaskEdit);
			// 
			// taskEditBindingSource3
			// 
			this.taskEditBindingSource3.DataSource = typeof(Aspire.Studio.Dialogs.TaskEdit);
			// 
			// taskEditBindingSource2
			// 
			this.taskEditBindingSource2.DataSource = typeof(Aspire.Studio.Dialogs.TaskEdit);
			// 
			// TaskEdit
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(268, 227);
			this.Controls.Add(this.descriptionTextBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.stateComboBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.nameTextBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button1);
			this.Name = "TaskEdit";
			this.Text = "TaskEdit";
			this.Activated += new System.EventHandler(this.TaskEdit_Activated);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TaskEdit_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.taskEditBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.taskEditBindingSource4)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.taskEditBindingSource1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.taskEditBindingSource3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.taskEditBindingSource2)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox nameTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox stateComboBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox descriptionTextBox;
		private System.Windows.Forms.BindingSource taskEditBindingSource1;
		private System.Windows.Forms.BindingSource taskEditBindingSource2;
		private System.Windows.Forms.BindingSource taskEditBindingSource;
		private System.Windows.Forms.BindingSource taskEditBindingSource3;
		private System.Windows.Forms.BindingSource taskEditBindingSource4;
	}
}