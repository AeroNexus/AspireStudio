using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire.UiToolbox.SyntaxEditor
{
  public class SyntaxEditor
  {
    public enum LocationToPositionAlgorithm
    {
      BestFit
    }

    public SyntaxEditor()
    {
      Document = new Document();
    }
    public string Text { get; set; }
    public Document Document { get; set; }
    public View SelectedView { get; set; }

    public void Focus()
    {
    }

    public Point PointToClient(Point screenPoint)
    {
      return new Point(screenPoint.X,screenPoint.Y);
    }
  }
  public class Document
  {
    public string HeaderText { get; set; }
    public string FooterText { get; set; }
  }
  public class View
  {
    public Point LocationToOffset(Point clientPoint, SyntaxEditor.LocationToPositionAlgorithm how )
    {
      Point offset = new Point(0,0);
      return offset;
    }
    public Selection Selection { get; set; }
    public string SelectedText { get; set; }
  }
  public class Selection
  {
    public int Length { get; set; }
    public void SelectRange(Point offset, int num )
    {

    }
  }
}
