using System;
//using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
//using System.Data;
using System.Drawing;
using System.Drawing.Design;
//using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Aspire.Studio.Dashboard;
using Aspire.Framework;
using Aspire.Utilities;

namespace Aspire.Studio.DocumentViews
{
	public partial class DashboardView : StudioDocumentView
	{
		DashboardDoc myDoc;
		DesignSurfaceManager2 manager;
		DesignSurface2 mSurface;
		IPropertiesView mPropertiesView;
		ISelectionService mSelectionService;

		public DashboardView() : base(Solution.ProjectItem.ItemType.Dashboard)
		{
			InitializeComponent();
			Name = "Dashboard"; // simplify
			LogDrop = true;

			manager = DashboardManager.SurfaceManager;

			var menuItem = MainMenuStrip.Items.Add("Tab order");
			menuItem.Click += TabOrder_Click;

			newFormUseSnapLinesMenuItem_Click(this, EventArgs.Empty);
		}

		private DesignSurface2 CreateDesignSurface()
		{
			//- step.0
			//- create a DesignSurface and put it inside a Form in DesignTime
			var surface = manager.CreateDesignSurface2();
			//- step.1
			//- enable the UndoEngines
			surface.GetUndoEngine().Enabled = true;
			//- step.2
			//- try to get a ptr to ISelectionService interface
			//- if we obtain it then hook the SelectionChanged event
			mSelectionService = surface.GetIDesignerHost().GetService(typeof(ISelectionService)) as ISelectionService;
			if (null != mSelectionService)
				mSelectionService.SelectionChanged += new System.EventHandler(OnSelectionChanged);
			//- step.3
			//- Find the IPropertiesView service
			mPropertiesView = surface.GetIDesignerHost().GetService(typeof(IPropertiesView)) as IPropertiesView;
			//-
			//- step.4
			//- Select the service IToolboxService
			//- and hook it to our ListBox
			//var tbox = surface.GetIToolboxService() as ToolboxService;
			//if (null != tbox)
			//	tbox.Toolbox = listBox1;
			//-
			//- finally return the Designsurface
			return surface;
		}

		private IComponent CreateRootComponent(Form parent, DesignSurface2 surface, Type controlType, Size controlSize)
		{
			try
			{
				IComponent rootComponent = surface.CreateRootComponent(controlType, controlSize);
				//this.tabControl1.SelectedIndex = this.tabControl1.TabPages.Count - 1;
				rootComponent.Site.Name = manager.GetValidFormName();
				//- display the DesignSurface
				Control view = surface.GetView();
				if (null == view)
					return null;
				//- change some properties
				view.Text = rootComponent.Site.Name;
				view.Dock = DockStyle.Fill;
				//- Note these assignments
				view.Parent = parent;
				//- finally enable the Drag&Drop on RootComponent
				surface.EnableDragandDrop();
				return rootComponent;
			}
			catch (Exception ex)
			{
				Console.WriteLine(Name + "CreateRootComponent() has generated errors during loading!Exception: " + ex.Message);
				throw;
			}
		}

		protected override void NewItem(Blackboard.Item bbItem, DragEventArgs e)
		{
			if (myDoc == null) myDoc = MyDocument(new DashboardDoc()) as DashboardDoc;

			var item = new DashboardItem();
			myDoc.AddItem(item, bbItem);
		}

		protected override void OnDocumentBind()
		{
			myDoc = mDocument as DashboardDoc;
		}

		//- When the selection changes this sets the PropertyGrid's selected component
		private void OnSelectionChanged(object sender, EventArgs e)
		{
			if (mPropertiesView == null) return;
			ISelectionService selectionService = selectionService = mSurface.GetIDesignerHost().GetService(typeof(ISelectionService)) as ISelectionService;
			try
			{
				mPropertiesView.SelectedObject = (sender as ISelectionService).PrimarySelection;
			}
			catch (Exception ex)
			{
				Log.ReportException(ex, "OnSelectionChanged");
			}
		}

		public static void Register(bool unregister = false)
		{
			if (unregister)
				DocumentMgr.UndefineDocView(typeof(DashboardView), typeof(DashboardDoc), typeof(DashboardView.DashboardItem));
			else
				DocumentMgr.DefineDocView(typeof(DashboardView), typeof(DashboardDoc), typeof(DashboardView.DashboardItem));
		}

		public override void UpdateDisplay(Clock clock)
		{
		}

		public class DashboardItem : StudioDocument.Item
		{
		}

		private void newFormUseSnapLinesMenuItem_Click(object sender, EventArgs e)
		{
			//- step.0
			//TabPage tp = CreateNewTabPage("Use SnapLines");
			//- step.1
			mSurface = CreateDesignSurface();
			//- step.2
			//- choose an alignment mode...
			mSurface.UseSnapLines();
			//- step.3
			//- create the Root compoment, in these case a Form
			var rootComponent = CreateRootComponent(this, mSurface, typeof(Form), new Size(400, 400)) as Control;
			var control = mSurface.View;
			rootComponent.Text = "DesignSurface";
			rootComponent.Dock = DockStyle.Fill;
			var form = rootComponent as Form;
			form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			form.ShowIcon = false;
			//rootComponent.AllowDrop = true;
			//rootComponent.DragEnter += rootComponent_DragEnter;
			//rootComponent.DragDrop += rootComponent_DragDrop;
			//rootComponent.BackColor = Color.LightGray;
		}

		//void rootComponent_DragDrop(object sender, DragEventArgs e)
		//{
		//	Log.WriteLine("rootComponent_DragDrop");
		//}

		//void rootComponent_DragEnter(object sender, DragEventArgs e)
		//{
		//	Log.WriteLine("rootComponent_DragEnter");
		//}

		void TabOrder_Click(object sender, System.EventArgs e)
		{
			mSurface.SwitchTabOrder();
		}
	}

	public class DashboardDoc : StudioDocument
	{
		const string category = "Dashboard";

		public DashboardDoc() : base(ItemType.Dashboard)
		{
		}

		public override StudioDocumentView NewView(string name)
		{
			var view = new DashboardView() { Name = name };
			return view;
		}

	}

}
