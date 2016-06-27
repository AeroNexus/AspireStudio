using System.Xml.Linq;

namespace Aspire.Studio.Plugin
{
    public interface IPlugin
    {
        string Title { get; }
        string Description { get; }
        string Group { get; }
        string SubGroup { get; }
        XElement Configuration { get; set; }
        string Icon { get; }

        void Dispose();
    }
}
