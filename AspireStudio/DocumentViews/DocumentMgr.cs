using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

using Aspire.Framework;

namespace Aspire.Studio.DocumentViews
{
	public class DocumentMgr
	{
		static List<DocViewDef> docViewDefs = new List<DocViewDef>();

		static DockPanel mDockPanel;
		static DocumentMgr The;
		static MainForm mMainForm;
		static List<StudioDocumentView> mStudioDocViews = new List<StudioDocumentView>();

		Clock mClock;
		Executive mExecutive;


		public DocumentMgr(MainForm form, DockPanel dockPanel)
		{
			mMainForm = form;
			mDockPanel = dockPanel;
			mDockPanel.ContentRemoved += mDockPanel_ContentRemoved;

			DashboardView.Register();
			Monitor.Register();
			StripChart.Register();
			The = this;
		}

		void mDockPanel_ContentRemoved(object sender, DockContentEventArgs e)
		{
			if ( e.Content is StudioDocumentView)
				mStudioDocViews.Remove(e.Content as StudioDocumentView);
		}

		public static void Add(StudioDocumentView studioDocView)
		{
			if (!mStudioDocViews.Contains(studioDocView))
			{
				mStudioDocViews.Add(studioDocView);
				if ( studioDocView.Document == null )
					StudioDocument.Bind(studioDocView);
			}
		}

		public static StudioDocumentView AddDocumentView(StudioDocumentView documentView)
		{
			int count = 1;
			string text = documentView.Name;

			while (FindDocumentView(text) != null)
				text = documentView.Name + ++count;

			if (documentView.Document == null)
			{
				while (StudioDocument.Find(text) != null)
					text = documentView.Name + ++count;
				documentView.AddingToDocManager();
			}

			documentView.Text = text;
			documentView.Name = text;
			documentView.Manager = The;

			if (mDockPanel.DocumentStyle == DocumentStyle.SystemMdi)
			{
				documentView.MdiParent = mMainForm;
				documentView.Show();
			}
			else
				documentView.Show(mDockPanel);

			return documentView;
		}

		public void Clear()
		{
			mStudioDocViews.Clear();
		}

		public Clock Clock { get { return mClock; } }

		public void CloseAllDocuments(DockState dockState = DockState.Unknown)
		{
			if (mDockPanel.DocumentStyle == DocumentStyle.SystemMdi)
			{
				foreach (var form in mMainForm.MdiChildren)
					form.Close();
			}
			else
			{
				for (int index = mDockPanel.Contents.Count - 1; index >= 0; index--)
				{
					var content = mDockPanel.Contents[index];
					if (dockState != DockState.Unknown)
					{
						if (content is IDockContent && content.DockHandler.DockState == dockState)
							content.DockHandler.Close();
					}
					else
					{
						if (content is IDockContent)
							content.DockHandler.Close();
					}
				}
			}
		}

		//public static void AddType(Type type)
		//{
		//	if (!docTypes.Contains(type))
		//		docTypes.Add(type);
		//	DocTypesChanged = true;
		//}

		public static void DefineDocView(Type view, Type doc, Type item=null)
		{
			if (!docTypes.Contains(doc))
				docTypes.Add(doc);
			if (item != null && !docTypes.Contains(item))
				docTypes.Add(item);
			foreach (var dvd in docViewDefs) if (dvd.View == view) return;
			docViewDefs.Add(new DocViewDef(view,doc,item));
			DocViewDefChanged(null, null);
			DocTypesChanged = true;
		} static List<Type> docTypes = new List<Type>();

		public static void UndefineDocView(Type view, Type doc, Type item=null)
		{
			docTypes.Remove(doc);
			if (item != null)
			{
				docTypes.Remove(item);
				DocTypesChanged = true;
			}
			foreach (var dvd in docViewDefs) if (dvd.View == view)
				{
					docViewDefs.Remove(dvd);
					DocViewDefChanged(null, null);
					return;
				}
		}
		public static event EventHandler DocViewDefChanged;

		public static Type[] DocTypes { get { return docTypes.ToArray(); } }

		public static bool DocTypesChanged { get; set; }

		public static string[] DocViews
		{
			get
			{
				var views = new List<string>();
				foreach (var viewDef in docViewDefs)
					views.Add(viewDef.View.Name);
				return views.ToArray();
			}
		}

		public static StudioDocumentView DocViewFactory(string viewFullName)
		{
			foreach ( var viewDef in docViewDefs )
			{
				if (viewDef.View.FullName == viewFullName)
				{
					var docView = viewDef.View.InvokeMember("",
							BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.Public,
							null, null, null) as StudioDocumentView;
					docView.Manager = The;
					return docView;
				}
			}
			return null;
		}

		void mExecutive_ModeChanged(ExecutiveMode previousMode, ExecutiveMode mode)
		{
			if ( mode == ExecutiveMode.Reset )
				foreach (var studioDocView in mStudioDocViews)
					studioDocView.OnResetTime();
		}

		public static IDockContent FindDocumentView(string text)
		{
			if (mDockPanel.DocumentStyle == DocumentStyle.SystemMdi)
			{
				foreach (var form in mMainForm.MdiChildren)
					if (form.Text == text)
						return form as IDockContent;

				return null;
			}
			else
			{
				foreach (IDockContent content in mDockPanel.Documents)
					if (content.DockHandler.TabText == text)
						return content;

				return null;
			}
		}

		public static void NewDocumentView(string viewName)
		{
			foreach (var viewDef in docViewDefs)
			{
				if (viewDef.View.Name == viewName)
				{
					var docView = viewDef.View.InvokeMember("",
							BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.Public,
							null, null, null) as StudioDocumentView;
					AddDocumentView(docView);
					Aspire.Studio.DockedViews.PropertiesView.The.Browse(docView.Document);
				}
			}
		}

		public void OnNewScenario(Clock clock, Executive executive)
		{
			if (mExecutive != null)
			{
				mExecutive.ModeChanged -= mExecutive_ModeChanged;
				mExecutive.Scheduler.FrameEnd -= mScheduler_FrameEnd;
			}
			mClock = clock;
			mExecutive = executive;
			mExecutive.ModeChanged += mExecutive_ModeChanged;
			mExecutive.Scheduler.FrameEnd += mScheduler_FrameEnd;
		}

		void mScheduler_FrameEnd(object sender, EventArgs e)
		{
			UpdateViews();
		}

		public static void Remove(StudioDocument document)
		{
			if (document.View != null)
			{
				if (mDockPanel.DocumentStyle == DocumentStyle.SystemMdi)
				{
					foreach (var form in mMainForm.MdiChildren)
						if (form == document.View)
						{
							mStudioDocViews.Remove(document.View);
							form.Close();
						}
				}
				else
				{
					mStudioDocViews.Remove(document.View);
					document.View.DockHandler.Close();
				}
			}
		}

		public void UpdateViews()
		{
			if (mClock == null) return;

			double time = mClock.ElapsedSeconds;

			foreach (var studioDocView in mStudioDocViews)
				if (studioDocView.Document != null && studioDocView.Document.Enabled &&
					(mExecutive.Mode != ExecutiveMode.Executing || time - studioDocView.LastDisplayed >= studioDocView.Document.Period)
					)
				{
					studioDocView.UpdateDisplay(mClock);
					studioDocView.LastDisplayed = time - 0.00001;
				}
		}

		class DocViewDef
		{
			internal DocViewDef(Type view, Type document, Type item)
			{
				View = view;
				Document = document;
				Item = item;
			}
			internal Type Document { get; private set; }
			internal Type Item { get; private set; }
			internal Type View { get; private set; }
			public override string ToString()
			{
				return Document.ToString();
			}
		}
	}
}
