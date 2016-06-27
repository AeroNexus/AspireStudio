using System.Windows.Forms;

using Aspire.Studio.DocumentViews;
using Aspire.Framework;

namespace Aspire.BowsingUI.Dialogs
{
	public partial class GenerateDashboardDialog : Form
	{
		public GenerateDashboardDialog()
		{
			InitializeComponent();
		}

		public static StudioDocument GenerateDashboard(Blackboard.Item item)
		{
			var doc = new DashboardDoc();
			doc.Name = item.LeafName;
			return doc;
		}
	}
}
