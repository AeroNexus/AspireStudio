
namespace Aspire.Core.xTEDS
{
	public interface IKnownMarshaler
	{
		VariableMarshaler KnownMarshaler(IVariable iVariable);
	}
}
