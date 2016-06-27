using System.Windows.Forms;

namespace Aspire.Studio.Plugin
{
	public enum ShowAs
	{
		Normal,
		Dialog
	}
	public interface IFormPlugin : IPlugin
	{
		Form Content { get; }
		ShowAs ShowAs { get; }
	}
}
