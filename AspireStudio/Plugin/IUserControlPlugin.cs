using System.Windows.Forms;

namespace Aspire.Studio.Plugin
{
	public interface IUserControlPlugin : IPlugin
	{
		UserControl Content { get; }
	}
}