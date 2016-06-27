using System;

namespace Aspire.Framework.Scripting
{
	/// <summary>
	/// A ui type editor for browsing script files.
	/// </summary>
	public class ScriptFileBrowser: System.Windows.Forms.Design.FileNameEditor
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public ScriptFileBrowser()
		{
		}

		/// <summary>
		/// <see cref="System.Windows.Forms.Design.FileNameEditor.InitializeDialog"/>
		/// </summary>
		/// <param name="openFileDialog"></param>
		protected override void InitializeDialog(System.Windows.Forms.OpenFileDialog openFileDialog)
		{
			base.InitializeDialog (openFileDialog);
			openFileDialog.CheckFileExists = true;
			openFileDialog.Filter = ScriptHelperFactory.GetFileDialogFilter();
		}
	}
}
