using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

using Aspire.UiToolbox.Common;

namespace Aspire.UiToolbox.Controls
{
	public interface INodeCollectionVC
	{
		void Initialize(TreeControl tc);
		void Detaching(TreeControl tc);
		Edges MeasureEdges(TreeControl tc, NodeCollection nc, Graphics g);
		void SetBounds(TreeControl tc, NodeCollection nc, Rectangle bounds);
		bool IntersectsWith(TreeControl tc, NodeCollection nc, Rectangle rectangle);
		void Draw(TreeControl tc, NodeCollection nc, Graphics g, Rectangle clipRectangle, bool preDraw);
		bool MouseDown(TreeControl tc, NodeCollection nc, Node n, MouseButtons button, Point pt);
		bool DoubleClick(TreeControl tc, NodeCollection nc, Node n, Point pt);
		void NodeCollectionClearing(TreeControl tc, NodeCollection nc);
		void SizeChanged(TreeControl tc);
		void PostDrawNodes(TreeControl tc, Graphics g, ArrayList displayNodes);
		void PostCalculateNodes(TreeControl tc, ArrayList displayNodes);
	}

  public interface INodeVC
  {
    void Initialize(TreeControl tc);
    void Detaching(TreeControl tc);
    Size MeasureSize(TreeControl tc, Node n, Graphics g);
    Rectangle SetPosition(TreeControl tc, Node n, Point topLeft);
    void SetChildBounds(TreeControl tc, Node n, Rectangle bounds);
    bool IntersectsWith(TreeControl tc, Node n, Rectangle rectangle, bool recurse);
    void Draw(TreeControl tc, Node n, Graphics g, Rectangle clipRectangle, int leftOffset, int rightOffset);
    bool MouseDown(TreeControl tc, Node n, MouseButtons button, Point pt);
    bool MouseUp(TreeControl tc, Node n, MouseButtons button, Point pt);
    bool DoubleClick(TreeControl tc, Node n, Point pt);
    void NodeExpandedChanged(TreeControl tc, Node n);
    void NodeVisibleChanged(TreeControl tc, Node n);
    void NodeSelectableChanged(TreeControl tc, Node n);
    void NodeCheckStateChanged(TreeControl tc, Node n);
    void NodeRemoving(TreeControl tc, Node n);
    bool CanSelectNode(TreeControl tc, Node n);
    bool CanCollapseNode(TreeControl tc, Node n, bool key, bool mouse);
    bool CanExpandNode(TreeControl tc, Node n, bool key, bool mouse);
    bool CanAutoEdit(TreeControl tc, Node n);
    bool CanToolTip(TreeControl tc, Node n);
    void BeginEditNode(TreeControl tc, Node n);
    void SizeChanged(TreeControl tc);
    Node DragOverNodeFromPoint(TreeControl tc, Point pt);
    void DragEnter(TreeControl tc, Node n, DragEventArgs drgevent);
    void DragOver(TreeControl tc, Node n, DragEventArgs drgevent);
    void DragLeave(TreeControl tc, Node n);
    void DragDrop(TreeControl tc, Node n, DragEventArgs drgevent);
    void DragHover(TreeControl tc, Node n);
    void PostDrawNodes(TreeControl tc, Graphics g, ArrayList displayNodes);
    void PostCalculateNodes(TreeControl tc, ArrayList displayNodes);
    Font GetNodeFont(TreeControl tc, Node n);
    Rectangle GetTextRectangle(TreeControl tc, Node n);
  }

}