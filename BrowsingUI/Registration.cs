using Aspire.BrowsingUI.BlackboardActions;
using Aspire.BrowsingUI.DocumentViews;
using Aspire.Studio.DocumentViews;

namespace Aspire.BrowsingUI
{
	public class Registration
	{
		static Registration()
		{
			new CreateDashboardAction();
			new SearchAction();
			new SendMessageAction();
			new AspirePublishNotification();
		  new IssueQueryAction();

		  // For now, don't put calls to DefineDocView in Registration. Leave it to the PluginMgr
		  //DocumentMgr.DefineDocView(typeof(MessageLog), typeof(MessageLogDoc), typeof(MessageLog.MessageLogItem));
		}
	}
}
