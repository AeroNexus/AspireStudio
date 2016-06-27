

namespace Aspire.Framework
{
	/// <summary>
	/// Let the Framework modify tree nodes
	/// </summary>
	public interface ITreeNode
	{
		/// <summary>
		/// Clone an existing TreeNode
		/// </summary>
		/// <returns></returns>
		object Clone();
	}
}
