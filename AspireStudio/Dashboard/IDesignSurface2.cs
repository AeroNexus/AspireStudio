using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;

namespace Aspire.Studio.Dashboard
{
	public interface IDesignSurface2
	{
		//- perform Cut/Copy/Paste/Delete commands
		void DoAction(string command);

		//- de/activate the TabOrder facility
		void SwitchTabOrder();

		//- select the controls alignement mode
		void UseSnapLines();
		void UseGrid(Size gridSize);
		void UseGridWithoutSnapping(Size gridSize);
		void UseNoGuides();

		//- method usefull to create control without the ToolBox facility
		IComponent CreateRootComponent(Type controlType, Size controlSize);
		IComponent CreateRootComponent(DesignerLoader loader, Size controlSize);
		Control CreateControl(Type controlType, Size controlSize, Point controlLocation);

		//- Get the UndoEngine2 object
		UndoEngine2 GetUndoEngine();

		//- Get the IDesignerHost of the .NET 2.0 DesignSurface
		IDesignerHost GetIDesignerHost();

		//- the HostControl of the .NET 2.0 DesignSurface is just a Control
		//- you can manipulate this Control just like any other WinForms Control
		//- (you can dock it and add it to another Control just to display it)
		//- Get the HostControl
		Control GetView();

		//- Get the IDesignerHost of the .NET 2.0 DesignSurface
		ToolboxService GetIToolboxService();
		void EnableDragandDrop();
	}
}
