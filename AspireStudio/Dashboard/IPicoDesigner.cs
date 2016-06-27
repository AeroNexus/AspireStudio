using System.Drawing;
using System.Windows.Forms;

using Aspire.Studio.DockedViews;

namespace Aspire.Studio.Dashboard
{
	public enum AlignmentModeEnum : int { SnapLines = 0, Grid, GridWithoutSnapping, NoGuides };

	public interface IPicoDesigner
	{
		//- controls accessing section  -----------------------------------------------------------
		//-     +-------------+-----------------------------+-----------+
		//-     |toolboxItem1 | ____                        |           |
		//-     |toolboxItem2 ||____|_____________________  +-----------+
		//-     |toolboxItem3 ||                          | |     |     |
		//-     |             ||                          | |     |     |
		//-     |  TOOLBOX    ||      DESIGNSURFACE       | | PROPERTY  |
		//-     |             ||                          | |   GRID    |
		//-     |             ||__________________________| |     |     |
		//-     +-------------+-----------------------------+-----------+
		ListBox Toolbox { get; set; }                       //- TOOLBOX
		//TabControl TabControlHostingDesignSurfaces { get; } //- DESIGNSURFACES HOST
		PropertiesView PropertyGridHost { get; }          //- PROPERTYGRID

		//- DesignSurfaces management section -----------------------------------------------------
		DesignSurface2 ActiveDesignSurface { get; }
		//- Create the DesignSurface and the rootComponent (a .NET Control)
		//- using IDesignSurface.CreateRootComponent() 
		//- if the alignmentMode doesn't use the GRID, then the gridSize param is ignored
		//- Note:
		//-     the generics param is used to know which type of control to use as RootComponent
		//-     TT is requested to be derived from .NET Control class 
		DesignSurface2 AddDesignSurface<TT>(
											   int startingFormWidth, int startingFormHeight,
											   AlignmentModeEnum alignmentMode, Size gridSize
											  ) where TT : Control;
		void RemoveDesignSurface(DesignSurface2 activeSurface);

		//- Editing section  ----------------------------------------------------------------------
		void UndoOnDesignSurface();
		void RedoOnDesignSurface();
		void CutOnDesignSurface();
		void CopyOnDesignSurface();
		void PasteOnDesignSurface();
		void DeleteOnDesignSurface();
		//void SwitchTabOrder();
	}
}
