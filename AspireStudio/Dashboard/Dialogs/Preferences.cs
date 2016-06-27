using System;
using System.Windows.Forms;

using Aspire.Studio.DocumentViews;

namespace Aspire.Studio.Dialogs
{
	public partial class Preferences : Form
	{
		public Preferences()
		{
			InitializeComponent();

			captionComboBox.Items.Clear();
			captionComboBox.Items.AddRange(Enum.GetNames(typeof(StudioDocument.CaptionRule)));
		}

		private void browseDefaultSolutions_Click(object sender, EventArgs e)
		{
			folderBrowserDialog1.SelectedPath = StudioSettings.Default.DefaultSolutionDirectory;
			folderBrowserDialog1.Description = "Default Solutions";
			folderBrowserDialog1.ShowNewFolderButton = true;
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
				defaultSolutionsTextBox.Text = folderBrowserDialog1.SelectedPath;
		}
	}
}
