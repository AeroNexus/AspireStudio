using System;
using System.Collections.Generic;

using Aspire.Core.Messaging;
using Aspire.Studio;
using Aspire.Studio.DocumentViews;
using Aspire.Utilities;

namespace Aspire.BrowsingUI.DocumentViews
{
	public partial class DirectoryConsole : StudioDocumentView
	{
		Dictionary<string, CoreAddress> mDirectoryByName = new Dictionary<string, CoreAddress>();
		DirectoryConsoleDoc myDoc;
		string mDomain = "???";

		public DirectoryConsole()
			: base(Solution.ProjectItem.ItemType.Monitor)
		{
			InitializeComponent();
			AddressServer.DirectoryAdded += AddressServer_DirectoryAdded;
		}

		public override void AddingToDocManager()
		{
			if (Document == null)
			{
				myDoc = new DirectoryConsoleDoc();
				//BrowseProperties(myDoc);
				MyDocument(myDoc);
			}
		}

		void AddressServer_DirectoryAdded(object sender, EventArgs e)
		{
			var directory = sender as CoreAddress;
			CoreAddress existing;
			if (mDirectoryByName.TryGetValue(directory.DomainName, out existing))
				mDirectoryByName.Remove(directory.DomainName);
			mDirectoryByName.Add(directory.DomainName, directory);
		}

		protected override void OnDocumentBind()
		{
			myDoc = mDocument as DirectoryConsoleDoc;
			//BrowseProperties(myDoc);
		}

		public static void Register(bool unregister = false)
		{
			if (unregister)
				DocumentMgr.UndefineDocView(typeof(DirectoryConsole), typeof(DirectoryConsoleDoc));
			else
				DocumentMgr.DefineDocView(typeof(DirectoryConsole), typeof(DirectoryConsoleDoc));
		}

		private void commandTb_TextChanged(object sender, EventArgs e)
		{
			int nl = commandTb.Text.IndexOf(Environment.NewLine);
			if (nl>=0)
			{
				var text = commandTb.Text.Remove(nl);
				Log.WriteLine("sending {0} to {1}",text,mDomain);
				commandTb.Text = string.Empty;
			}
		}
	}

	public class DirectoryConsoleDoc : StudioDocument
	{
		public override StudioDocumentView NewView(string name)
		{
			return new DirectoryConsole();
		}
	}
}
