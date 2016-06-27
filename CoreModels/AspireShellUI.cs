using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Aspire.CoreModels
{
	public partial class AspireShellUI : Form
	{
		public AspireShellUI( string domainName)
		{
			InitializeComponent();
			tbDomain.Text = domainName;
			dlgXtedsBrowse.InitialDirectory = Application.CommonXtedsDirectory;
		}

		public string Domain { get { return tbDomain.Text; } }

		public string XtedsFile { get { return tbXtedsFile.Text; } }

		public string XtedsPath { get; private set; }

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			var result = dlgXtedsBrowse.ShowDialog();
			if (result == DialogResult.OK)
			{
				XtedsPath = dlgXtedsBrowse.FileName;
				tbXtedsFile.Text = System.IO.Path.GetFileName(dlgXtedsBrowse.FileName);
			}
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}

}
