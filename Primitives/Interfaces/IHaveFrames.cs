
namespace Aspire.Primitives
{
	public interface IHaveFrames
	{
		Frame GetFrame(string name);
		FrameList Frames { get; }
	}
}
