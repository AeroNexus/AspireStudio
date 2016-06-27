namespace Aspire.Studio.Dialogs
{
	partial class Preferences
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
			this.label2 = new System.Windows.Forms.Label();
			this.browseDefaultSolutions = new System.Windows.Forms.Button();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.button1 = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.cbMessageLogResumes = new System.Windows.Forms.CheckBox();
			this.defaultSolutionsTextBox = new System.Windows.Forms.TextBox();
			this.captionComboBox = new System.Windows.Forms.ComboBox();
			this.cbBlackboardTooltips = new System.Windows.Forms.CheckBox();
			this.cbLoadLastSolution = new System.Windows.Forms.CheckBox();
			this.tbMonitorCaptionRules = new System.Windows.Forms.TextBox();
			this.cbFindComponentWithWholeAddress = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 90);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(178, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Blackboard Generated Caption Rule";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 116);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(114, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Default solutions folder";
			// 
			// browseDefaultSolutions
			// 
			this.browseDefaultSolutions.Location = new System.Drawing.Point(242, 132);
			this.browseDefaultSolutions.Name = "browseDefaultSolutions";
			this.browseDefaultSolutions.Size = new System.Drawing.Size(24, 20);
			this.browseDefaultSolutions.TabIndex = 7;
			this.browseDefaultSolutions.Text = "...";
			this.browseDefaultSolutions.UseVisualStyleBackColor = true;
			this.browseDefaultSolutions.Click += new System.EventHandler(this.browseDefaultSolutions_Click);
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Location = new System.Drawing.Point(267, 245);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(50, 23);
			this.button1.TabIndex = 9;
			this.button1.Text = "Accept";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(13, 165);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(105, 13);
			this.label3.TabIndex = 12;
			this.label3.Text = "Monitor caption rules";
			// 
			// cbMessageLogResumes
			// 
			this.cbMessageLogResumes.AutoSize = true;
			this.cbMessageLogResumes.Checked = global::Aspire.Studio.StudioSettings.Default.MessageLogResumes;
			this.cbMessageLogResumes.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbMessageLogResumes.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Aspire.Studio.StudioSettings.Default, "MessageLogResumes", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.cbMessageLogResumes.Location = new System.Drawing.Point(12, 58);
			this.cbMessageLogResumes.Name = "cbMessageLogResumes";
			this.cbMessageLogResumes.Size = new System.Drawing.Size(239, 17);
			this.cbMessageLogResumes.TabIndex = 10;
			this.cbMessageLogResumes.Text = "Message log resumes when scrollbar bottoms";
			this.cbMessageLogResumes.UseVisualStyleBackColor = true;
			// 
			// defaultSolutionsTextBox
			// 
			this.defaultSolutionsTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Aspire.Studio.StudioSettings.Default, "DefaultSolutionDirectory", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.defaultSolutionsTextBox.Location = new System.Drawing.Point(12, 132);
			this.defaultSolutionsTextBox.Name = "defaultSolutionsTextBox";
			this.defaultSolutionsTextBox.Size = new System.Drawing.Size(241, 20);
			this.defaultSolutionsTextBox.TabIndex = 6;
			this.defaultSolutionsTextBox.Text = global::Aspire.Studio.StudioSettings.Default.DefaultSolutionDirectory;
			// 
			// captionComboBox
			// 
			this.captionComboBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Aspire.Studio.StudioSettings.Default, "BlackboardGeneratedCaptionRule", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.captionComboBox.FormattingEnabled = true;
			this.captionComboBox.Items.AddRange(new object[] {
            "Leaf"});
			this.captionComboBox.Location = new System.Drawing.Point(196, 87);
			this.captionComboBox.Name = "captionComboBox";
			this.captionComboBox.Size = new System.Drawing.Size(57, 21);
			this.captionComboBox.TabIndex = 4;
			this.captionComboBox.Text = global::Aspire.Studio.StudioSettings.Default.BlackboardGeneratedCaptionRule;
			// 
			// cbBlackboardTooltips
			// 
			this.cbBlackboardTooltips.AutoSize = true;
			this.cbBlackboardTooltips.Checked = global::Aspire.Studio.StudioSettings.Default.BlackboardToolTips;
			this.cbBlackboardTooltips.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbBlackboardTooltips.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Aspire.Studio.StudioSettings.Default, "BlackboardToolTips", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.cbBlackboardTooltips.Location = new System.Drawing.Point(12, 35);
			this.cbBlackboardTooltips.Name = "cbBlackboardTooltips";
			this.cbBlackboardTooltips.Size = new System.Drawing.Size(116, 17);
			this.cbBlackboardTooltips.TabIndex = 2;
			this.cbBlackboardTooltips.Text = "Blackboard tooltips";
			this.cbBlackboardTooltips.UseVisualStyleBackColor = true;
			// 
			// cbLoadLastSolution
			// 
			this.cbLoadLastSolution.AutoSize = true;
			this.cbLoadLastSolution.Checked = global::Aspire.Studio.StudioSettings.Default.LoadLastSolution;
			this.cbLoadLastSolution.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbLoadLastSolution.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Aspire.Studio.StudioSettings.Default, "LoadLastSolution", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.cbLoadLastSolution.Location = new System.Drawing.Point(12, 12);
			this.cbLoadLastSolution.Name = "cbLoadLastSolution";
			this.cbLoadLastSolution.Size = new System.Drawing.Size(108, 17);
			this.cbLoadLastSolution.TabIndex = 1;
			this.cbLoadLastSolution.Text = "Load last solution";
			this.cbLoadLastSolution.UseVisualStyleBackColor = true;
			// 
			// tbMonitorCaptionRules
			// 
			this.tbMonitorCaptionRules.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Aspire.Studio.StudioSettings.Default, "MonitorCaptionRules", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.tbMonitorCaptionRules.Location = new System.Drawing.Point(12, 182);
			this.tbMonitorCaptionRules.Multiline = true;
			this.tbMonitorCaptionRules.Name = "tbMonitorCaptionRules";
			this.tbMonitorCaptionRules.Size = new System.Drawing.Size(202, 36);
			this.tbMonitorCaptionRules.TabIndex = 13;
			this.tbMonitorCaptionRules.Text = global::Aspire.Studio.StudioSettings.Default.MonitorCaptionRules;
			// 
			// cbFindComponentWithWholeAddress
			// 
			this.cbFindComponentWithWholeAddress.AutoSize = true;
			this.cbFindComponentWithWholeAddress.Checked = global::Aspire.Studio.StudioSettings.Default.FindComponentWithWholeAddress;
			this.cbFindComponentWithWholeAddress.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbFindComponentWithWholeAddress.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Aspire.Studio.StudioSettings.Default, "FindComponentWithWholeAddress", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.cbFindComponentWithWholeAddress.Location = new System.Drawing.Point(12, 245);
			this.cbFindComponentWithWholeAddress.Name = "cbFindComponentWithWholeAddress";
			this.cbFindComponentWithWholeAddress.Size = new System.Drawing.Size(221, 17);
			this.cbFindComponentWithWholeAddress.TabIndex = 14;
			this.cbFindComponentWithWholeAddress.Text = "Find Aspire component w/ whole address";
			this.cbFindComponentWithWholeAddress.UseVisualStyleBackColor = true;
			// 
			// Preferences
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(329, 280);
			this.Controls.Add(this.cbFindComponentWithWholeAddress);
			this.Controls.Add(this.tbMonitorCaptionRules);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.cbMessageLogResumes);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.browseDefaultSolutions);
			this.Controls.Add(this.defaultSolutionsTextBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.captionComboBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cbBlackboardTooltips);
			this.Controls.Add(this.cbLoadLastSolution);
			this.Name = "Preferences";
			this.Text = "Leaf";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox cbLoadLastSolution;
		private System.Windows.Forms.CheckBox cbBlackboardTooltips;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox captionComboBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox defaultSolutionsTextBox;
		private System.Windows.Forms.Button browseDefaultSolutions;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.CheckBox cbMessageLogResumes;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbMonitorCaptionRules;
		private System.Windows.Forms.CheckBox cbFindComponentWithWholeAddress;

	}
}