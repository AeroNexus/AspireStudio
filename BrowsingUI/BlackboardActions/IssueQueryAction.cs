using System.Windows.Forms;

using Aspire.BrowsingUI.Dialogs;
using Aspire.Core;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;
using Aspire.CoreModels;
using Aspire.Framework;

namespace Aspire.BrowsingUI.BlackboardActions
{
  /// <summary>
  /// Summary description for Manifest BlackboardAction.
  /// </summary>
  public class IssueQueryAction : BlackboardAction, IQueryClient
  {
    private AspireBrowser browser;

    public IssueQueryAction() : base("Issue Query") { }

    public override bool Visible(Blackboard.Item item, out bool enabled)
    {
      enabled = item.Value is AspireBrowser;
      return enabled;
    }

    public override void Execute(Blackboard.Item item)
    {
      browser = item.Value as AspireBrowser;
      var form = new IssueQueryDialog(browser.DomainName);
      if (form.ShowDialog() == DialogResult.OK && form.SearchSpec.Length > 0 &&
        browser.Query().Validate(form.SearchSpec))
      {
        int id = form.When == QueryMgr.When.Existing ? browser.AdHocQueryId : browser.RegisterQueryClient(this);
        browser.Query().ForMessage(id, form.SearchSpec,form.When);
      }
    }

    public void OnQueryForMessage(int queryId, XtedsMessage xmsg, StructuredResponse mResponse, int responseId)
    {
      MsgConsole.WriteLine("{4}: Query {0} found message {1}.{2} provided by {3}",
        queryId, xmsg.InterfaceName, xmsg.Name, xmsg.Provider.ToString(), browser.Name);
      mResponse.PrintResponses(responseId);
    }
  }

}
