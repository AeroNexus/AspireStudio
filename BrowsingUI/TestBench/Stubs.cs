using System.Drawing;
using System.Windows.Forms;

using Aspire.Studio.DocumentViews;
using Aspire.Framework.Scripting;

namespace Aspire.BrowsingUI.TestBench
{
  public class StudioDocumentEx : StudioDocument
  {
    public string DocumentId { get; set; }
    public override StudioDocumentView NewView(string name)
    {
      return new StudioDocumentView();
    }
  }

}
