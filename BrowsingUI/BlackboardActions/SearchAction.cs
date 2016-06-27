using System.Windows.Forms;

using Aspire.BrowsingUI.Dialogs;
using Aspire.CoreModels;
using Aspire.Framework;

namespace Aspire.BrowsingUI.BlackboardActions
{
	public class SearchAction : BlackboardAction
	{
		public SearchAction() : base("Search for Aspire Messages") { }

		public override bool Visible(Blackboard.Item item, out bool enabled)
		{
			enabled = item.Value is AspireBrowser;
			return enabled;
		}

		public override void Execute(Blackboard.Item item)
		{
			var dlg = new SearchDialog(item.Value as AspireBrowser);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				(item.Value as Xteds.Interface.CommandMessage).Send();
			}
		}
	}

}
