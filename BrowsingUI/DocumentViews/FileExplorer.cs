using System.ComponentModel;
using System.Windows.Forms;
using System.Xml.Serialization;

using Aspire.Studio;
using Aspire.Studio.DocumentViews;

namespace Aspire.BrowsingUI.DocumentViews
{
	public partial class FileExplorer : StudioDocumentView
	{
		FileExplorerDoc myDoc;

		public FileExplorer()
			: base(Solution.ProjectItem.ItemType.Monitor)
		{
			InitializeComponent();
		}

		public override void AddingToDocManager()
		{
			if (Document == null)
			{
				myDoc = new FileExplorerDoc();
				BrowseProperties(myDoc);
				MyDocument(myDoc);
			}
		}

		protected override void OnDocumentBind()
		{
			myDoc = mDocument as FileExplorerDoc;
			BrowseProperties(myDoc);
			splitContainerLeft.Panel1Collapsed = !myDoc.ViewFolders;
			splitContainerRight.Panel1Collapsed = !myDoc.ViewFolders;
			toolStripMenuItem3.Checked = myDoc.ViewFolders;
		}

		public static void Register(bool unregister = false)
		{
			if (unregister)
				DocumentMgr.UndefineDocView(typeof(FileExplorer), typeof(FileExplorerDoc));
			else
				DocumentMgr.DefineDocView(typeof(FileExplorer), typeof(FileExplorerDoc));
		}

		private void FileExplorer_Load(object sender, System.EventArgs e)
		{

		}

		private void FileExplorer_DoubleClick(object sender, System.EventArgs e)
		{
			Aspire.Studio.DockedViews.PropertiesView.The.Browse(myDoc);
		}

		private void toolStripMenuItem3_Click(object sender, System.EventArgs e)
		{
			myDoc.ViewFolders = !myDoc.ViewFolders;
			splitContainerLeft.Panel1Collapsed = !myDoc.ViewFolders;
			splitContainerRight.Panel1Collapsed = !myDoc.ViewFolders;
			(sender as ToolStripMenuItem).Checked = myDoc.ViewFolders;
		}

		private void dataGridView_DoubleClick(object sender, System.EventArgs e)
		{
			Aspire.Studio.DockedViews.PropertiesView.The.Browse(myDoc);
		}

		private void splitContainerRight_SplitterMoved(object sender, SplitterEventArgs e)
		{
			myDoc.RightSplitter = e.SplitX;
		}

		private void splitContainerLeft_SplitterMoved(object sender, SplitterEventArgs e)
		{
			myDoc.LeftSplitter = e.SplitX;
		}

	}

	#region FileExplorerDoc

	public class FileExplorerDoc : StudioDocument
	{
		const string category = "FileExplorer";

		public FileExplorerDoc()
			: base(ItemType.Monitor)
		{
		}

		public override StudioDocumentView NewView(string name)
		{
			var view = new FileExplorer() { Name = name };
			return view;
		}

		[Browsable(false), XmlAttribute("leftSplitter"), DefaultValue(48)]
		public int LeftSplitter { get { return mLeftSplitter; } set { mLeftSplitter = value; IsDirty = true; } } int mLeftSplitter = 48;

		[Browsable(false), XmlAttribute("rightSplitter"), DefaultValue(48)]
		public int RightSplitter { get { return mRightSplitter; } set { mRightSplitter = value; IsDirty = true; } } int mRightSplitter = 48;

		[Category(category), XmlAttribute("viewFolders"), DefaultValue(true), Description("View the Folder panes")]
		public bool ViewFolders { get { return mViewFolders; } set { mViewFolders = value; IsDirty = true; } } bool mViewFolders = true;

	}
	#endregion
}
