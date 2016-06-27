using System.ComponentModel.Design;
using System.Windows.Forms.Design;

namespace Aspire.Studio.Dashboard
{
	internal class DesignerOptionService4SnapLines : DesignerOptionService
	{
		public DesignerOptionService4SnapLines() : base() { }

		protected override void PopulateOptionCollection(DesignerOptionCollection options)
		{
			if (null != options.Parent) return;

			DesignerOptions ops = new DesignerOptions() { UseSnapLines = true, UseSmartTags = true };
			DesignerOptionCollection wfd = this.CreateOptionCollection(options, "WindowsFormsDesigner", null);
			this.CreateOptionCollection(wfd, "General", ops);
		}
	}

	internal class DesignerOptionService4Grid : DesignerOptionService
	{
		private System.Drawing.Size _gridSize;

		public DesignerOptionService4Grid(System.Drawing.Size gridSize) : base() { _gridSize = gridSize; }

		protected override void PopulateOptionCollection(DesignerOptionCollection options)
		{
			if (null != options.Parent) return;

			DesignerOptions ops = new DesignerOptions()
			{
				GridSize = _gridSize,
				SnapToGrid = true,
				ShowGrid = true,
				UseSnapLines = false,
				UseSmartTags = true
			};
			DesignerOptionCollection wfd = this.CreateOptionCollection(options, "WindowsFormsDesigner", null);
			this.CreateOptionCollection(wfd, "General", ops);
		}
	}

	internal class DesignerOptionService4GridWithoutSnapping : DesignerOptionService
	{
		private System.Drawing.Size _gridSize;

		public DesignerOptionService4GridWithoutSnapping(System.Drawing.Size gridSize) : base() { _gridSize = gridSize; }

		protected override void PopulateOptionCollection(DesignerOptionCollection options)
		{
			if (null != options.Parent) return;

			DesignerOptions ops = new DesignerOptions()
			{
				GridSize = _gridSize,
				SnapToGrid = false,
				ShowGrid = true,
				UseSnapLines = false,
				UseSmartTags = true
			};
			DesignerOptionCollection wfd = this.CreateOptionCollection(options, "WindowsFormsDesigner", null);
			this.CreateOptionCollection(wfd, "General", ops);
		}
	}

	internal class DesignerOptionService4NoGuides : DesignerOptionService
	{
		public DesignerOptionService4NoGuides() : base() { }

		protected override void PopulateOptionCollection(DesignerOptionCollection options)
		{
			if (null != options.Parent) return;

			DesignerOptions ops = new DesignerOptions()
			{
				GridSize = new System.Drawing.Size(8, 8),
				SnapToGrid = false,
				ShowGrid = false,
				UseSnapLines = false,
				UseSmartTags = true
			};
			DesignerOptionCollection wfd = this.CreateOptionCollection(options, "WindowsFormsDesigner", null);
			this.CreateOptionCollection(wfd, "General", ops);
		}
	}

}
