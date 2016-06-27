namespace Aspire.BrowsingUI.Dialogs
{
  partial class IssueQueryDialog
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
      this.txtQuerySpec = new System.Windows.Forms.TextBox();
      this.btnIssue = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.rbFuture = new System.Windows.Forms.RadioButton();
      this.rbExisting = new System.Windows.Forms.RadioButton();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(10, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(63, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Query Spec";
      // 
      // txtQuerySpec
      // 
      this.txtQuerySpec.Location = new System.Drawing.Point(82, 10);
      this.txtQuerySpec.Name = "txtQuerySpec";
      this.txtQuerySpec.Size = new System.Drawing.Size(344, 20);
      this.txtQuerySpec.TabIndex = 1;
      // 
      // btnIssue
      // 
      this.btnIssue.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnIssue.Location = new System.Drawing.Point(350, 49);
      this.btnIssue.Name = "btnIssue";
      this.btnIssue.Size = new System.Drawing.Size(75, 23);
      this.btnIssue.TabIndex = 3;
      this.btnIssue.Text = "Issue";
      this.btnIssue.UseVisualStyleBackColor = true;
      this.btnIssue.Click += new System.EventHandler(this.btnIssue_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.rbFuture);
      this.groupBox1.Controls.Add(this.rbExisting);
      this.groupBox1.Location = new System.Drawing.Point(12, 36);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(135, 36);
      this.groupBox1.TabIndex = 4;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "When:";
      // 
      // rbFuture
      // 
      this.rbFuture.AutoSize = true;
      this.rbFuture.Location = new System.Drawing.Point(74, 13);
      this.rbFuture.Name = "rbFuture";
      this.rbFuture.Size = new System.Drawing.Size(55, 17);
      this.rbFuture.TabIndex = 1;
      this.rbFuture.Text = "Future";
      this.rbFuture.UseVisualStyleBackColor = true;
      this.rbFuture.CheckedChanged += new System.EventHandler(this.rbFuture_CheckedChanged);
      // 
      // rbExisting
      // 
      this.rbExisting.AutoSize = true;
      this.rbExisting.Checked = true;
      this.rbExisting.Location = new System.Drawing.Point(7, 13);
      this.rbExisting.Name = "rbExisting";
      this.rbExisting.Size = new System.Drawing.Size(61, 17);
      this.rbExisting.TabIndex = 0;
      this.rbExisting.TabStop = true;
      this.rbExisting.Text = "Existing";
      this.rbExisting.UseVisualStyleBackColor = true;
      this.rbExisting.CheckedChanged += new System.EventHandler(this.rbExisting_CheckedChanged);
      // 
      // IssueQueryDialog
      // 
      this.AcceptButton = this.btnIssue;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(437, 84);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.btnIssue);
      this.Controls.Add(this.txtQuerySpec);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "IssueQueryDialog";
      this.Text = "Issue Query";
      this.Load += new System.EventHandler(this.IssueQueryDialog_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox txtQuerySpec;
    private System.Windows.Forms.Button btnIssue;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton rbFuture;
    private System.Windows.Forms.RadioButton rbExisting;
  }
}