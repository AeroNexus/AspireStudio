using System.ComponentModel;
using System.Windows.Forms;

namespace Aspire.UiToolbox.Forms
{
  /// <summary>
  /// InputBox is a simple dialog window that has a single text box.
  /// </summary>
  public class InputBox : System.Windows.Forms.Form
  {
    private TextBox textBox1;
    private Panel panel1;
    private Button cmdOk;
    private Panel panel2;
    private Button cmdCancel;
    private Panel panel3;
    private Label label;
    private Container components = null;

    private InputBox()
    {
      InitializeComponent();
      Icon = null;
    }

    /// <summary>
    /// Dispose the form.
    /// </summary>
    /// <param name="disposing">Bool</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      textBox1 = new System.Windows.Forms.TextBox();
      panel1 = new System.Windows.Forms.Panel();
      panel2 = new System.Windows.Forms.Panel();
      cmdCancel = new System.Windows.Forms.Button();
      cmdOk = new System.Windows.Forms.Button();
      panel3 = new System.Windows.Forms.Panel();
      label = new System.Windows.Forms.Label();
      panel1.SuspendLayout();
      panel2.SuspendLayout();
      panel3.SuspendLayout();
      SuspendLayout();
      // 
      // textBox1
      // 
      textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      textBox1.Location = new System.Drawing.Point(10, 33);
      textBox1.Multiline = true;
      textBox1.Name = "textBox1";
      textBox1.Size = new System.Drawing.Size(272, 33);
      textBox1.TabIndex = 0;
      textBox1.Text = "";
      // 
      // panel1
      // 
      panel1.Controls.Add(panel2);
      panel1.Controls.Add(cmdOk);
      panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      panel1.DockPadding.All = 10;
      panel1.Location = new System.Drawing.Point(0, 76);
      panel1.Name = "panel1";
      panel1.Size = new System.Drawing.Size(292, 42);
      panel1.TabIndex = 1;
      // 
      // panel2
      // 
      panel2.Controls.Add(cmdCancel);
      panel2.Dock = System.Windows.Forms.DockStyle.Right;
      panel2.DockPadding.Right = 10;
      panel2.Location = new System.Drawing.Point(58, 10);
      panel2.Name = "panel2";
      panel2.Size = new System.Drawing.Size(152, 22);
      panel2.TabIndex = 1;
      // 
      // cmdCancel
      // 
      cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      cmdCancel.Dock = System.Windows.Forms.DockStyle.Right;
      cmdCancel.Location = new System.Drawing.Point(70, 0);
      cmdCancel.Name = "cmdCancel";
      cmdCancel.Size = new System.Drawing.Size(72, 22);
      cmdCancel.TabIndex = 0;
      cmdCancel.Text = "Cancel";
      cmdCancel.Click += new System.EventHandler(cmdCancel_Click);
      // 
      // cmdOk
      // 
      cmdOk.Dock = System.Windows.Forms.DockStyle.Right;
      cmdOk.Location = new System.Drawing.Point(210, 10);
      cmdOk.Name = "cmdOk";
      cmdOk.Size = new System.Drawing.Size(72, 22);
      cmdOk.TabIndex = 0;
      cmdOk.Text = "OK";
      cmdOk.Click += new System.EventHandler(cmdOk_Click);
      // 
      // panel3
      // 
      panel3.Controls.Add(textBox1);
      panel3.Controls.Add(label);
      panel3.Dock = System.Windows.Forms.DockStyle.Fill;
      panel3.DockPadding.All = 10;
      panel3.Location = new System.Drawing.Point(0, 0);
      panel3.Name = "panel3";
      panel3.Size = new System.Drawing.Size(292, 76);
      panel3.TabIndex = 0;
      // 
      // label
      // 
      label.Dock = System.Windows.Forms.DockStyle.Top;
      label.Location = new System.Drawing.Point(10, 10);
      label.Name = "label";
      label.Size = new System.Drawing.Size(272, 23);
      label.TabIndex = 1;
      label.Text = "label";
      // 
      // InputBox
      // 
      AcceptButton = cmdOk;
      AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      CancelButton = cmdCancel;
      ClientSize = new System.Drawing.Size(292, 118);
      Controls.Add(panel3);
      Controls.Add(panel1);
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "InputBox";
      StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      Text = "0";
      panel1.ResumeLayout(false);
      panel2.ResumeLayout(false);
      panel3.ResumeLayout(false);
      ResumeLayout(false);

    }

    /// <summary>
    /// Display the input box.
    /// </summary>
    /// <param name="caption">The caption of the window.</param>
    /// <param name="labelText">The text to display in the label. If empty, the label will not be shown.</param>
    /// <returns>The value entered by the user.</returns>
    public static string ShowInputBox(string caption, string labelText)
    {
      return ShowInputBox(caption, labelText, string.Empty, true);
    }

    /// <summary>
    /// Display the input box.
    /// </summary>
    /// <param name="caption">The caption of the window.</param>
    /// <param name="labelText">The text to display in the label. If empty, the label will not be shown.</param>
    /// <param name="originalValue">The original value to display.</param>
    /// <param name="multiLine">Specifies whether the text box will be multilin.e</param>
    /// <returns>The value entered by the user.</returns>
    public static string ShowInputBox(string caption, string labelText, string originalValue, bool multiLine)
    {
      InputBox box = new InputBox();
      box.Text = caption;
      box.DialogResult = DialogResult.None;
      if (labelText != null && labelText.Length != 0)
      {
        box.label.Text = labelText;
        box.label.Visible = true;
      }
      else
      {
        box.label.Visible = false;
      }

      box.textBox1.Multiline = multiLine;
      box.textBox1.Text = originalValue;
      box.textBox1.Focus();

      if (box.ShowDialog() == DialogResult.OK)
      {
        return box.textBox1.Text;
      }
      else
      {
        return string.Empty;
      }
    }

    private void cmdOk_Click(object sender, System.EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void cmdCancel_Click(object sender, System.EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }
  }
}
