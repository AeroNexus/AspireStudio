using System;
using System.Windows.Forms;

namespace Aspire.UiToolbox.Controls
{
  public delegate void CancelNodeEventHandler(TreeControl tc, CancelNodeEventArgs e);

  public delegate void LabelControlEventHandler(TreeControl tc, LabelControlEventArgs e);

  public delegate void NodeDragDropEventHandler(TreeControl tc, Node n, DragEventArgs e);

  public delegate void NodeEventHandler(TreeControl tc, NodeEventArgs e);

  public delegate void StartDragEventHandler(TreeControl tc, StartDragEventArgs e);


}
