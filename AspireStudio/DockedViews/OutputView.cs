using System;
using System.Drawing;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

using Aspire.Framework;
using Aspire.Utilities;

namespace Aspire.Studio.DockedViews
{
    public partial class OutputView : ToolWindow
    {
        public OutputView()
        {
            InitializeComponent();
			Log.NewText += AddText;
		}

		public void Clear()
		{
			mRichTextBox.Clear();
		}

		delegate void NewTextDelegate(string text, Log.Severity severity);

		void NewText(string text, Log.Severity severity)
		{
			switch (severity)
			{
				case Log.Severity.Info:
					mRichTextBox.SelectionColor = Color.Black;
					break;
				case Log.Severity.Warning:
					mRichTextBox.SelectionColor = Color.Purple;
					break;
				case Log.Severity.Error:
					mRichTextBox.SelectionColor = Color.Red;
					break;
			}
			mRichTextBox.AppendText(text + Environment.NewLine);
		}

		public void AddText(string text, Log.Severity severity)
		{
			if (mRichTextBox.InvokeRequired)
				try
				{
					mRichTextBox.Invoke(new NewTextDelegate(NewText), new object[] { text, severity });
				}
				catch(Exception){}
			else
				NewText(text, severity);
		}

		private void clearToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Log.WriteLine("OutputView.Clear");
			mRichTextBox.Clear();
		}

		private void OutputView_DragDrop(object sender, DragEventArgs e)
		{
			var item = e.Data.GetData(typeof(Blackboard.Item)) as Blackboard.Item;
			if (item != null)
				Log.WriteLine("output dropped:{0},{1},{2},{3}", item.Path, item.Description, item.Units, item.Value);
		}

		private void OutputView_FormClosing(object sender, FormClosingEventArgs e)
		{
			Log.NewText -= AddText;
		}

		private void OutputView_Load(object sender, EventArgs e)
		{
			mRichTextBox.Disposed += mRichTextBox_Disposed;
			//Log.NewText += AddText;
		}

		void mRichTextBox_Disposed(object sender, EventArgs e)
		{
			Log.NewText -= AddText;
		}
	}
}