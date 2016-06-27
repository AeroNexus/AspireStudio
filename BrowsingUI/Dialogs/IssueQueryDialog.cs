using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Aspire.Core;
using Aspire.Core.Utilities;
using Aspire.CoreModels;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.BrowsingUI.Dialogs
{
  public partial class IssueQueryDialog : Form
  {
    private string domainName = string.Empty;

    public IssueQueryDialog(string domainName)
    {
      this.domainName = domainName;
      InitializeComponent();
    }

    public QueryMgr.When When { get; private set; }

    public string SearchSpec { get { return txtQuerySpec.Text; } }

    private void IssueQueryDialog_Load(object sender, EventArgs e)
    {
      Text = "Issue Query to " + domainName;
    }

    private void rbExisting_CheckedChanged(object sender, EventArgs e) { When = QueryMgr.When.Existing; }

    private void rbFuture_CheckedChanged(object sender, EventArgs e) { When = QueryMgr.When.Future; }

    private void btnIssue_Click(object sender, EventArgs e) { btnIssue.DialogResult = DialogResult.OK; }
  }
}