using System.Windows.Forms;

using Aspire.BowsingUI.Dialogs;
using Aspire.CoreModels;
using Aspire.Framework;

namespace Aspire.BrowsingUI.BlackboardActions
{
	public class SendMessageAction : AspireCommandSendNowAction
	{
		public SendMessageAction() : base("Send Aspire command") { }

		public override void Execute(Blackboard.Item item)
		{
			var msg = item.Value as Xteds.Interface.Message;

			//var msg = item.Value as Xteds.Interface.CommandMessage;
			if (msg.Variables.Length == 0)
				msg.OwnerInterface.Xteds.Host.Send(msg);
			else
			{
				var form = new SendMessageDialog(msg);
				if (form.ShowDialog() == DialogResult.OK)
				{
					msg.OwnerInterface.Xteds.Host.Send(msg);
				}
			}
		}
	}

	public class AspirePublishNotification : AspirePublishNotificationNowAction
	{
		public AspirePublishNotification() : base("Publish Aspire notification") { }

		public override void Execute(Blackboard.Item item)
		{
			var msg = item.Value as Xteds.Interface.Message;

			//var msg = item.Value as Xteds.Interface.CommandMessage;
			if (msg.Variables.Length == 0)
				(item.Value as Xteds.Interface.DataMessage).Publish();
			else
			{
				var form = new SendMessageDialog(msg);
				if (form.ShowDialog() == DialogResult.OK)
					(item.Value as Xteds.Interface.DataMessage).Publish();
			}
		}
	}
}
