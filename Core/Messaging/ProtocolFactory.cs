using System;
using System.Collections.Generic;
using System.Reflection;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	public class ProtocolBlueprint
	{
		string mName;
		ProtocolId mId;
		internal Type mType;

		public ProtocolBlueprint(string name, ProtocolId id, Type type)
		{
			mName = name;
			mId = id;
			mType = type;
			ProtocolFactory.Add(this);
		}
	}

	public class ProtocolFactory
	{
		static List<ProtocolBlueprint> blueprints = new List<ProtocolBlueprint>();
		static int mNumControlProtocols;
		static Protocol[] mControlProtocols;

		static internal void Add(ProtocolBlueprint blueprint)
		{
			if (blueprint.mType.GetInterface("IControlProtocol")!= null)
				mNumControlProtocols++;
			blueprints.Add(blueprint);
		}


		public static Address CreateAddress(ProtocolId protocolId)
		{
			switch (protocolId)
			{
				case ProtocolId.Monarch:
					return new Address1();
				case ProtocolId.Aspire:
				case ProtocolId.XtedsCommand:
				case ProtocolId.XtedsData:
					return new Address2();
			}
			return null;
		}

		public static Address CreateAddress(MarshaledBuffer buf)
		{
			ProtocolId protocolId;
			if ( buf.Length == 4 )
				protocolId = ProtocolId.Monarch;
			else
				protocolId = ProtocolId.Aspire;
			Address address = CreateAddress(protocolId);
			address.Unmarshal(buf.Bytes,buf.Offset);
			return address;
		}

		public static Message CreateMessage(ProtocolId protocolId)
		{
			switch (protocolId)
			{
				case ProtocolId.Monarch:
					return new Message1();
				case ProtocolId.Aspire:
				case ProtocolId.XtedsCommand:
				case ProtocolId.XtedsData:
					return new Message2();
			}
			return null;
		}
		public static Protocol CreateProtocol(ProtocolId protocolId)
		{
			MsgConsole.WriteLine("");
			MsgConsole.WriteLine("THIS NO LONGER WORKS");
			MsgConsole.WriteLine("");

			//switch (protocolId)
			//{
			//	case ProtocolId.Monarch:
			//		return new MonarchProtocol(mApplication, mTransport, mXtedsString, mCompId, false);
			//	case ProtocolId.Aspire:
			//		return new NativeProtocol();
			//	case ProtocolId.XtedsCommand:
			//	case ProtocolId.XtedsData:
			//		return new XtedsProtocol();
			//}
			return null;
		}

		public static int FindControlProtocols(ref Protocol[] protocols)
		{
			if (!scannedForBlueprints) ScanForBlueprints();

			if (mControlProtocols == null)
				mControlProtocols = new Protocol[mNumControlProtocols];

			int i = 0;
			foreach (var bp in blueprints)
				if (bp.mType.GetInterface("IControlProtocol")!= null)
					mControlProtocols[i++] = Activator.CreateInstance(bp.mType) as Protocol;

			protocols = mControlProtocols;
			return mNumControlProtocols;
		}

		private static void ScanForBlueprints()
		{
			//var types = PlugIn.Manager.Mgr().GetTypesWithInterface("IBlueprint");
			var types = new Type[] { typeof(ApplicationProtocol), typeof(MonarchProtocol) }; 
			//if (types == null || types.Length == 0)
			//	MsgConsole.WriteLine("Can't find any Blueprints in the plugins");
			//types[0].Load(true);
			BindingFlags flags =
				BindingFlags.Public |
				BindingFlags.Static |
				BindingFlags.InvokeMethod;
			foreach (var type in types)
			{
				if (type != null)
				{
					//MsgConsole.WriteLine("{0}.Blueprint", type.Name);
					try
					{
						type.InvokeMember("Blueprint", flags, null, null, null);
					}
					catch (MissingMethodException) { }
				}
			}
			scannedForBlueprints = true;
		} static bool scannedForBlueprints;
	}

	public interface IBlueprint
	{
	}
}
