//using System;
//using System.IO;
//using System.Text;
using System.Windows.Forms;

using Aspire.Framework;

using Aspire.Core.xTEDS;

namespace Aspire.CoreModels
{
	/// <summary>
	/// Summary description for MessageHelpers.
	/// </summary>
	public class MessageHelper
	{
		static MessageHelper()
		{
			new AspireCommandReceiveAction();
			new AspireDataSubscribeAction();
			new AspireDataUnSubscribeAction();
			new AspireCommandSendNowAction();
			new AspirePublishNotificationNowAction();
		}

		public static short Marshal(byte[] buffer, Xteds.Interface.Message msg)
		{
			if (!msg.marshalerDefined)
			{
				foreach (var varValue in msg.Variables)
					if (varValue.Variable.marshaler == null)
						varValue.Variable.marshaler = new VariableMarshaler(msg.Name, varValue);
				msg.marshalerDefined = true;
			}
			int offset = 0;
			foreach (var varValue in msg.Variables)
				offset += varValue.Variable.marshaler.Serialize(buffer, offset, 65536);
			return (short)offset;
		}

		public static void Unmarshal(byte[] buffer, int length, Xteds.Interface.Message msg)
		{
			if (!msg.marshalerDefined)
			{
				foreach (var varValue in msg.Variables)
					if (varValue.Variable.marshaler == null)
						varValue.Variable.marshaler = new VariableMarshaler(msg.Name, varValue);
				msg.marshalerDefined = true;
			}
			int offset = 0;
			foreach (var varValue in msg.Variables)
				offset += varValue.Variable.marshaler.Deserialize(buffer,offset);
		}
	}

	/// <summary>
	/// Summary description for AspireCommandReceiveAction.
	/// </summary>
	public class AspireCommandReceiveAction: BlackboardAction
	{
		public AspireCommandReceiveAction() : base("Test receive Aspire command") {}

		public override bool Visible(Blackboard.Item item, out bool enabled)
		{
			var cmdMsg = item.Value as Xteds.Interface.CommandMessage;
			enabled = cmdMsg != null && cmdMsg.OwnerInterface.Xteds.Host.Testable;
			return enabled;
		}

		public override void Execute(Blackboard.Item item)
		{
			var cmdMsg = item.Value as Xteds.Interface.CommandMessage;
			var host = cmdMsg.OwnerInterface.Xteds.Host;
			if ( host.CanParse )
			{
				var buf = new byte[256];
				int len = 0;
				host.ParseAspireMessage( buf, len );
			}
		}
	}

	/// <summary>
	/// Summary description for AspireDataSubscribeAction.
	/// </summary>
	public class AspireDataSubscribeAction: BlackboardAction
	{
		public AspireDataSubscribeAction() : base("Subscribe to Aspire Data") {}

		internal static System.Collections.Generic.Dictionary<Blackboard.Item, int> SubscribedCount = new System.Collections.Generic.Dictionary<Blackboard.Item, int>();

		public override bool Visible(Blackboard.Item item, out bool enabled)
		{
			var dataMsg = item.Value as Xteds.Interface.DataMessage;
			if (dataMsg != null)
			{
				enabled = dataMsg.Subscribed == 0;

				var host = dataMsg.OwnerInterface.Xteds.Host;
				return host.HostType == HostType.Consumer && !host.Testable;
			}
			
			enabled = false;
			return false;
		}

		public override void Execute(Blackboard.Item item)
		{
			var dataMsg = item.Value as Xteds.Interface.DataMessage;
			var host = dataMsg.OwnerInterface.Xteds.Host;
			if ( host.Testable )
			{
				//if ( host.CanParse )
				//{
				//    byte[] buf = new byte[128];
				//    int length = subRequest.Marshal(buf);
				//    host.ParseAspireMessage( buf, length );
				//}
			}
			else
			{
				dataMsg.Subscribe();
                int original = 0;
                SubscribedCount.TryGetValue(item, out original);
                SubscribedCount[item] = ++original;
			}
		}
	}
	/// <summary>
	/// Summary description for AspireDataUnSubscribeAction.
	/// </summary>
	public class AspireDataUnSubscribeAction: BlackboardAction
	{
		public AspireDataUnSubscribeAction() : base("UnSubscribe to Aspire data") {}

		public override bool Visible(Blackboard.Item item, out bool enabled)
		{
			var dataMsg = item.Value as Xteds.Interface.DataMessage;
			if (dataMsg != null)
			{
				enabled = dataMsg.Subscribed > 0;

				var host = dataMsg.OwnerInterface.Xteds.Host;
				return host.HostType == HostType.Consumer && !host.Testable;
			}

			enabled = false;
			return false;
		}

		public override void Execute(Blackboard.Item item)
		{
			var dataMsg = item.Value as Xteds.Interface.DataMessage;
            
            int subscribedInternally = 0;
            AspireDataSubscribeAction.SubscribedCount.TryGetValue(item, out subscribedInternally);

            if (dataMsg.Subscribed > subscribedInternally)
            {
                if (MessageBox.Show(//AspireStudio.Form1.ApplicationWindow,
                    Properties.Resources.UnsubscribeConfirmation, Properties.Resources.Confirmation, 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    == DialogResult.Yes)
                {
                    dataMsg.ForceUnsubscribe();
                    AspireDataSubscribeAction.SubscribedCount[item] = 0;
                }
            }
            else
            {
                dataMsg.Unsubscribe();
                AspireDataSubscribeAction.SubscribedCount[item] = subscribedInternally > 0 ? --subscribedInternally : 0;
            }
		}
	}

	/// <summary>
	/// Summary description for AspireCommandSendNowAction.
	/// </summary>
	public class AspireCommandSendNowAction : BlackboardAction
	{
		public AspireCommandSendNowAction() : base("Send Aspire command now") { }
		public AspireCommandSendNowAction(string name) : base(name) { }

		public override bool ExecuteOnDoubleClick { get { return true; } }

		/// <summary>
		/// Is the context menu item visible and enabled
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool Visible(Blackboard.Item item, out bool enabled)
		{
			var msg = item.Value as Xteds.Interface.Message;

			enabled = false;
			if (msg == null) return false;

			var host = msg.OwnerInterface.Xteds.Host;

			bool visible;

			if (host.HostType == HostType.Consumer)
				visible = msg is Xteds.Interface.CommandMessage && !host.Testable;
			else // Provider
				visible = msg is Xteds.Interface.DataReplyMessage && !host.Testable;

			if ( visible )
			{
				var reply = msg as Xteds.Interface.DataReplyMessage;
				if (reply != null)
					enabled = (reply.RequestMessage as Xteds.Interface.Message).Received > 0;
				else
					enabled = true;
			}

			return visible;
		}

		/// <summary>
		/// Execute the command
		/// </summary>
		/// <param name="item"></param>
		public override void Execute(Blackboard.Item item)
		{
			var msg = item.Value as Xteds.Interface.Message;
			msg.OwnerInterface.Xteds.Host.Send(msg);
		}
	}

		/// <summary>
		/// Summary description for AspirePublishNotificationActionNow.
		/// </summary>
		public class AspirePublishNotificationNowAction : BlackboardAction
		{
			public AspirePublishNotificationNowAction() : base("Publish Aspire notification now") { }
			public AspirePublishNotificationNowAction(string name) : base(name) { }

			/// <summary>
			/// Is the context menu item visible and enabled
			/// </summary>
			/// <param name="item"></param>
			/// <returns></returns>
			public override bool Visible(Blackboard.Item item, out bool enabled )
			{
				var dataMsg = item.Value as Xteds.Interface.DataMessage;

				bool visible = dataMsg != null &&
				    dataMsg.MessageType is Xteds.Interface.Notification &&
				    dataMsg.OwnerInterface.Xteds.Host.HostType == HostType.Provider;

				enabled = false;

				if (visible && dataMsg != null && dataMsg.Publication != null &&
						dataMsg.Publication.Subscriptions.Count > 0)
				{
					foreach (var sub in dataMsg.Publication.Subscriptions)
						if (sub.State == Subscription.OpState.Active)
						{
							enabled = true;
							break;
						}
				}

				return visible;
			}

			/// <summary>
			/// Execute the command
			/// </summary>
			/// <param name="item"></param>
			public override void Execute(Blackboard.Item item)
			{
				(item.Value as Xteds.Interface.DataMessage).Publish();
			}
		}
}

