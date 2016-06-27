using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

using Aspire.Studio.DockedViews;

namespace Aspire.Studio.Dashboard
{
	public partial class PicoDesigner : UserControl, IPicoDesigner
	{
		private const string _Name_ = "PicoDesigner";

		//- the DesignSurfaceManager2 instance must be an OBSERVER
		//- of the UI event which change the active DesignSurface
		//- DesignSurfaceManager is exposed as public getter properties as test facility
		public DesignSurfaceManager2 DesignSurfaceManager { get; private set; }

		#region ctors
		public PicoDesigner()
		{
			InitializeComponent();

            DesignSurfaceManager = new DesignSurfaceManager2();
           // DesignSurfaceManager.PropertyGridHost.Parent = this.splitterpDesigner.Panel2;

            Toolbox = null;
            Dock = DockStyle.Fill;        
		}

        //- usage:
        //-         if (a){
        //-             // do work
        //-         }//end_if
        //-         else{
        //-             // a is not valid
        //-         }//end_else
        public static implicit operator bool ( PicoDesigner d ) {
            bool isValid = true;
            //- the object 'd' must be correctly initialized
            isValid &= ( ( null == d.Toolbox ) ? false : true );
            return isValid;
        }
        #endregion


		#region IpDesigner Members

		//- to get and set the real Toolbox which is provided by the user
		public ListBox Toolbox { get; set; }

		//public TabControl TabControlHostingDesignSurfaces
		//{
		//	get { return this.tbCtrlpDesigner; }
		//}

		public PropertiesView PropertyGridHost
		{
			get { return DesignSurfaceManager.PropertyGridHost; }
		}

		public DesignSurface2 ActiveDesignSurface
		{
			get { return DesignSurfaceManager.ActiveDesignSurface as DesignSurface2; }
		}

		//- Create the DesignSurface and the rootComponent (a .NET Control)
		//- using IDesignSurface.CreateRootComponent() 
		//- if the alignmentMode doesn't use the GRID, then the gridSize param is ignored
		//- Note:
		//-     the generics param is used to know which type of control to use as RootComponent
		//-     TT is requested to be derived from .NET Control class 
		public DesignSurface2 AddDesignSurface<TT>(
														int startingFormWidth, int startingFormHeight,
														AlignmentModeEnum alignmentMode, Size gridSize
													   ) where TT : Control
		{
			const string _signature_ = _Name_ + @"::AddDesignSurface<>()";

			if (!this)
				throw new Exception(_signature_ + " - Exception: " + _Name_ + " is not initialized! Please set the Property: IpDesigner::Toolbox before calling any methods!");

			var surface = DesignSurfaceManager.CreateDesignSurface2();
			DesignSurfaceManager.ActiveDesignSurface = surface;

			switch (alignmentMode)
			{
				case AlignmentModeEnum.SnapLines:
					surface.UseSnapLines();
					break;
				case AlignmentModeEnum.Grid:
					surface.UseGrid(gridSize);
					break;
				case AlignmentModeEnum.GridWithoutSnapping:
					surface.UseGridWithoutSnapping(gridSize);
					break;
				case AlignmentModeEnum.NoGuides:
					surface.UseNoGuides();
					break;
				default:
					surface.UseSnapLines();
					break;
			}

			surface.GetUndoEngine().Enabled = true;

			var tbox = surface.GetIToolboxService() as ToolboxService;
			//- we don't check if Toolbox is null because the very first check: if(!this)...
			if (null != tbox)
				tbox.Toolbox = this.Toolbox;

			var rootComponent = surface.CreateRootComponent(typeof(TT), new Size(startingFormWidth, startingFormHeight)) as Control;
			rootComponent.Site.Name = DesignSurfaceManager.GetValidFormName();

			surface.EnableDragandDrop();

			var componentChangeService = surface.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
			if (null != componentChangeService)
			{
				//- the Type "ComponentEventHandler Delegate" Represents the method that will
				//- handle the ComponentAdding, ComponentAdded, ComponentRemoving, and ComponentRemoved
				//- events raised for component-level events
				componentChangeService.ComponentChanged += (Object sender, ComponentChangedEventArgs e) =>
				{
					// do nothing
				};
				componentChangeService.ComponentAdded += (Object sender, ComponentEventArgs e) =>
				{
					DesignSurfaceManager.UpdatePropertyGridHost(surface);
				};
				componentChangeService.ComponentRemoved += (Object sender, ComponentEventArgs e) =>
				{
					DesignSurfaceManager.UpdatePropertyGridHost(surface);
				};
			}
			//-
			//-
			//- step.7
			//- now set the Form::Text Property
			//- (because it will be an empty string
			//- if we don't set it)
			var view = surface.GetView();
			if (null == view)
				return null;
			var pdc = TypeDescriptor.GetProperties(view);
			//- Sets a PropertyDescriptor to the specific property
			var pdS = pdc.Find("Text", false);
			if (null != pdS)
				pdS.SetValue(rootComponent, rootComponent.Site.Name + " (design mode)");
			//-
			//-
			//- step.8
			//- display the DesignSurface
			var sTabPageText = rootComponent.Site.Name;
			//TabPage newPage = new TabPage(sTabPageText);
			//newPage.Name = sTabPageText;
			//newPage.SuspendLayout(); //----------------------------------------------------
			//view.Dock = DockStyle.Fill;
			//view.Parent = newPage; //- Note this assignment
			//this.tbCtrlpDesigner.TabPages.Add(newPage);
			//newPage.ResumeLayout(); //-----------------------------------------------------
			//- select the TabPage created
			//this.tbCtrlpDesigner.SelectedIndex = this.tbCtrlpDesigner.TabPages.Count - 1;

			return surface;
		}


		public void RemoveDesignSurface(DesignSurface2 surfaceToErase)
		{
			try
			{

				//- remove the TabPage which has the same name of
				//- the RootComponent host by DesignSurface "surfaceToErase"
				//- Note:
				//-     DesignSurfaceManager continues to reference the DesignSurface erased
				//-     that Designsurface continue to exist but it is no more reachable
				//-     this fact is usefull when generate new names for Designsurfaces just created
				//-     avoiding name clashing
				string dsRootComponentName = surfaceToErase.GetIDesignerHost().RootComponent.Site.Name;
				//TabPage tpToRemove = null;
				//foreach (TabPage tp in this.tbCtrlpDesigner.TabPages)
				//{
				//	if (tp.Name == dsRootComponentName)
				//	{
				//		tpToRemove = tp;
				//		break;
				//	}
				//}
				//if (null != tpToRemove)
				//	this.tbCtrlpDesigner.TabPages.Remove(tpToRemove);


				//- now remove the DesignSurface
				this.DesignSurfaceManager.DeleteDesignSurface(surfaceToErase);


				//- finally the DesignSurfaceManager remove the DesignSurface
				//- AND set as active DesignSurface the last one
				//- therefore we set as active the last TabPage
				//this.tbCtrlpDesigner.SelectedIndex = this.tbCtrlpDesigner.TabPages.Count - 1;
			}
			catch (Exception exx)
			{
				Debug.WriteLine(exx.Message);
				if (null != exx.InnerException)
					Debug.WriteLine(exx.InnerException.Message);

				throw;
			}
		}

		public void UndoOnDesignSurface()
		{
			IDesignSurface2 isurf = DesignSurfaceManager.ActiveDesignSurface;
			if (null != isurf)
				isurf.GetUndoEngine().Undo();
		}

		public void RedoOnDesignSurface()
		{
			IDesignSurface2 isurf = DesignSurfaceManager.ActiveDesignSurface;
			if (null != isurf)
				isurf.GetUndoEngine().Redo();
		}

		public void CutOnDesignSurface()
		{
			IDesignSurface2 isurf = DesignSurfaceManager.ActiveDesignSurface;
			if (null != isurf)
				isurf.DoAction("Cut");
		}

		public void CopyOnDesignSurface()
		{
			IDesignSurface2 isurf = DesignSurfaceManager.ActiveDesignSurface;
			if (null != isurf)
				isurf.DoAction("Copy");
		}

		public void PasteOnDesignSurface()
		{
			IDesignSurface2 isurf = DesignSurfaceManager.ActiveDesignSurface;
			if (null != isurf)
				isurf.DoAction("Paste");
		}

		public void DeleteOnDesignSurface()
		{
			IDesignSurface2 isurf = DesignSurfaceManager.ActiveDesignSurface;
			if (null != isurf)
				isurf.DoAction("Delete");
		}

		//public void SwitchTabOrder()
		//{
		//	IDesignSurface2 isurf = DesignSurfaceManager.ActiveDesignSurface;
		//	if (null != isurf)
		//		isurf.SwitchTabOrder();
		//}

		#endregion

	}
}
