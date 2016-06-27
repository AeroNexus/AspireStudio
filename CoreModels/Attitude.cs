using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Utilities;

using Aspire.Core;
using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.CoreModels
{
	public class Attitude : Application
	{
		public Attitude()
			: base("{95EFF4EF-509C-4c65-BC2F-8C5C874DDAB2}")
		{
		}

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			base.Discover(scenario);
		}

		#endregion

		#region Messaging

		enum QueryId { Attitude=1 };

		protected override void Initialize(bool dummy)
		{
			Register();

			Query().ExtraDeliveries = "[up],[up],compUid.";
			Query().ForMessage((int)QueryId.Attitude,
"Notification(DataMsg(Variable(kind=attitude,Qualifier(Representation=quaternion)))),[up],[push],name,[up].");

			var address = new Address2("127.0.0.1", 40001);
			var directory = new CoreAddress(address, "DIRECTORY", "Local", null);
			(ControlProtocol as ApplicationProtocol).OnDirectoryRegistered(directory);
		}

		public override void OnQueryForMessage(int queryId, XtedsMessage msg)
		{
			Write("Found message: {0}",msg.Name);
			switch ((QueryId)queryId)
			{
				case QueryId.Attitude:
					break;
			}
		}

		#endregion

		#region Properties

		#endregion

	}
}
