using Aspire.BowsingUI.Dialogs;

using Aspire.Studio.DocumentViews;

using Aspire.CoreModels;
using Aspire.Framework;

namespace Aspire.BrowsingUI.BlackboardActions
{
	public class CreateDashboardAction : BlackboardAction
	{
		public CreateDashboardAction() : base("Create Dashboard") { }

		public override bool Visible(Blackboard.Item item, out bool enabled)
		{
			enabled = (item.Value is Xteds || 
				item.Value is AspireComponent ||
				item.Value is Xteds.Interface ||
				item.Value is Xteds.Interface.Message);
			return false; // enabled;
		}

		public override void Execute(Blackboard.Item item)
		{
			var doc = GenerateDashboardDialog.GenerateDashboard(item);
			if (doc != null)
			{
				StudioDocument.Add(doc);
				doc.Display();
			}
		}
	}
}
