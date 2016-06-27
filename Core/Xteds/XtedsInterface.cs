
namespace Aspire.Core.xTEDS
{
	class XtedsInterface
	{
	}
	public interface IMessageType
	{
		//string ToString();
	}
	public interface IXtedsRequest : IMessageType
	{
		XtedsMessage CommandXtedsMsg { get; }
		XtedsMessage DataReplyXtedsMsg { get; }
		//{
		//    get { return mReplyMessage; }
		//} XtedsMessage mReplyMessage;
	}
}
