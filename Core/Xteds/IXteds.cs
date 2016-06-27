using System;
using System.Collections.Generic;
using System.Text;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;

namespace Aspire.Core.xTEDS
{
	public interface IXtedsComponent
	{
		string Name { get; }
	}

	public enum HostType { Provider, Consumer };

	public interface IHostXtedsLite
	{
		// Amount of lease to grant to subscribers
		int AllowableLeasePeriod{ get; }

		// Is this host a producer or consumer of data
		HostType HostType { get; }
	}

	public delegate void ChangePublisherState(IPublisher publisher, bool active);

	public abstract class IXteds
	{
		public delegate IXteds ParseHandler(string text, string filePath, IHostXtedsLite host);
		static ParseHandler mParseHandler;
		public static void SetParseHandler(ParseHandler handler)
		{
			mParseHandler = handler;
		}
		public abstract IXtedsComponent XtedsComponent{get;}
		public abstract void AddMessage(XtedsMessage msg);
		public abstract void ChangeAllCommandProviders(Address newAddress);
		public abstract IDataMessage FindDataMessage(string interfaceName, string messageName);
		public abstract XtedsMessage FindMessage(int selector, bool nag = true);
		public abstract XtedsMessage FindMessage(MarshaledBuffer messageSpec);
		public abstract XtedsMessage FindXtedsMessage(string interfaceName, string messageName);
		public abstract List<IDataMessage> NotificationMessages();
		public static IXteds Parse(IHostXtedsLite host, string text, string filePath)
		{
			if (mParseHandler == null) throw new System.NullReferenceException("Aspire.Core.Xteds");
			return mParseHandler(text,filePath,host);
		}
		public abstract void VisitAllPublishers(ChangePublisherState OnChangePublisherState,bool active,Address newAddress=null);
	}

	#region IVariableMarshaler

	public interface IVariableMarshaler
	{
		int Deserialize(byte[] buffer, int offset);
		int Serialize(byte[] buffer, int offset, int size);
		IVariable IVariable { get; }
	}

	#endregion

  public class XtedsHelper
  {
    public static string ConstructXteds(string name)
    {
      return string.Format(
    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
    "<xTEDS xmlns=\"http://www.PnPInnovations.com/Aspire/xTEDS\"\n" +
    " name=\"{0}_xTEDS\" version=\"1.0\">\n" +
      " <Application name=\"{0}\" kind=\"Software\" componentKey=\"{0}\"" +
    " architecture=\"Intel\" operatingSystem=\"dotNet\" version=\"1.0\"/>\n" +
      "</xTEDS>\n\0", name);
    }
  }
}
