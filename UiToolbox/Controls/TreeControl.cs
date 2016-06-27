using Microsoft.Win32;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

using Aspire.UiToolbox.Common;
using Aspire.UiToolbox.Win32;

namespace Aspire.UiToolbox.Controls
{
  [DefaultProperty("ViewControllers"), ToolboxBitmap(typeof(TreeControl))]
  public class TreeControl : Control
  {
    private const int WHEEL_DELTA = 120;
    private const int INDICATOR_WIDTH = 13;
    private const int INDICATOR_HEIGHT = 13;
    private const int BUMP_HEIGHT = 15;
    private static ImageList _indicatorImages;
    private bool _borderIs3D;
    private Size _borderSize;
    private Color _borderColor;
    private TreeBorderStyle _borderStyle;
    private Border3DStyle _border3DStyle;
    private IndentPaddingEdges _borderIndent;
    private ButtonBorderStyle _borderButtonStyle;
    private DrawStyle _boxDrawStyle;
    private bool _boxShownAlways;
    private int _columnWidth;
    private int _lineWidth;
    private int _boxLength;
    private Color _lineColor;
    private Color _boxSignColor;
    private Color _boxBorderColor;
    private Color _boxInsideColor;
    private LineDashStyle _lineDashStyle;
    private Pen _cacheLineDashPen;
    private Pen _cacheBoxSignPen;
    private Pen _cacheBoxBorderPen;
    private Brush _cacheBoxInsideBrush;
    private LineBoxVisibility _lineVisibility;
    private LineBoxVisibility _boxVisibility;
    private CheckStates _checkStates;
    private DrawStyle _checkDrawStyle;
    private int _checkLength;
    private int _checkGapLeft;
    private int _checkGapRight;
    private int _checkBorderWidth;
    private Color _checkBorderColor;
    private Color _checkInsideColor;
    private Color _checkInsideHotColor;
    private Color _checkTickColor;
    private Color _checkTickHotColor;
    private Color _checkMixedColor;
    private Color _checkMixedHotColor;
    private Pen _cacheCheckTickPen;
    private Pen _cacheCheckTickHotPen;
    private Brush _cacheCheckTickBrush;
    private Brush _cacheCheckTickHotBrush;
    private Brush _cacheCheckBorderBrush;
    private Brush _cacheCheckInsideBrush;
    private Brush _cacheCheckInsideHotBrush;
    private Brush _cacheCheckMixedBrush;
    private Brush _cacheCheckMixedHotBrush;
    private Point _offset;
    private Panel _corner;
    private VScrollBar _vBar;
    private HScrollBar _hBar;
    private bool _scrollBarsValid;
    private bool _enableMouseWheel;
    private int _displayHeightScroll;
    private int _displayHeightExScroll;
    private ScrollVisibility _vVisibility;
    private ScrollVisibility _hVisibility;
    private VerticalGranularity _granularity;
    private ImageList _imageList;
    private int _imageIndex;
    private int _selectedImageIndex;
    private int _imageGapLeft;
    private int _imageGapRight;
    private Font _groupFont;
    private bool _groupArrows;
    private bool _groupAutoEdit;
    private bool _groupHotTrack;
    private bool _groupGradientBack;
    private bool _groupAutoCollapse;
    private bool _groupAutoAllocate;
    private bool _groupNodesSelectable;
    private bool _groupUseHotFontStyle;
    private bool _groupUseSelectedFontStyle;
    private bool _groupExpandOnDragHover;
    private int _groupIndentLeft;
    private int _groupIndentTop;
    private int _groupIndentBottom;
    private int _groupExtraLeft;
    private int _groupExtraHeight;
    private Color _groupBackColor;
    private Color _groupForeColor;
    private Color _groupLineColor;
    private Color _groupHotBackColor;
    private Color _groupHotForeColor;
    private Color _groupSelectedBackColor;
    private Color _groupSelectedForeColor;
    private Color _groupSelectedNoFocusBackColor;
    private GroupColoring _groupColoring;
    private FontStyle _groupHotFontStyle;
    private FontStyle _groupSelectedFontStyle;
    private GroupBorderStyle _groupBorderStyle;
    private TreeGradientColoring _groupGradientColoring;
    private GradientDirection _groupGradientAngle;
    private ClickExpandAction _groupClickExpand;
    private ClickExpandAction _groupDoubleClickExpand;
    private TextRenderingHint _groupTextRenderingHint;
    private Pen _cacheGroupLinePen;
    private int _groupImageBoxWidth;
    private int _groupImageBoxGap;
    private bool _groupImageBox;
    private bool _groupImageBoxColumn;
    private bool _groupImageBoxGradientBack;
    private bool _groupImageBoxBorder;
    private Color _groupImageBoxLineColor;
    private Color _groupImageBoxBackColor;
    private Color _groupImageBoxSelectedBackColor;
    private Color _groupImageBoxColumnColor;
    private TreeGradientColoring _groupImageBoxGradientColoring;
    private GradientDirection _groupImageBoxGradientAngle;
    private Brush _cacheGroupImageBoxColumnBrush;
    private Pen _cacheGroupImageBoxLinePen;
    private Node _hotNode;
    private Node _tooltipNode;
    private Node _focusNode;
    private Point _hotPoint;
    private bool _tooltips;
    private bool _infotips;
    private bool _instantUpdate;
    private bool _autoCollapse;
    private bool _nodesSelectable;
    private bool _extendToRight;
    private bool _useHotFontStyle;
    private bool _useSelectedFontStyle;
    private bool _canUserExpandCollapse;
    private bool _expandOnDragHover;
    private int _verticalNodeGap;
    private int _minimumNodeHeight;
    private int _maximumNodeHeight;
    private string _pathSeparator;
    private Color _hotBackColor;
    private Color _hotForeColor;
    private Color _selectedBackColor;
    private Color _selectedForeColor;
    private Color _selectedNoFocusBackColor;
    private ContextMenu _contextMenuNode;
    private ContextMenu _contextMenuSpace;
    private PopupTooltipSingle _tooltip;
    private FontStyle _hotFontStyle;
    private FontStyle _selectedFontStyle;
    private Indicators _indicators;
    private ClickExpandAction _clickExpand;
    private ClickExpandAction _doubleClickExpand;
    private TextRenderingHint _textRenderingHint;
    private Brush _cacheHotBackBrush;
    private Brush _cacheSelectedBackBrush;
    private Brush _cacheSelectedNoFocusBackBrush;
    private Timer _infotipTimer;
    private bool _autoEdit;
    private bool _labelEdit;
    private Node _labelEditNode;
    private Node _autoEditNode;
    private TextBox _labelEditBox;
    private Timer _autoEditTimer;
    private Node _lastSelectedNode;
    private SelectMode _selectMode;
    private Hashtable _selected;
    private Node _dragNode;
    private bool _bumpUpwards;
    private Timer _dragBumpTimer;
    private Timer _dragHoverTimer;
    private Hashtable _cachedSelected;
    private DragDropEffects _dragEffects;
    private Node _mouseDownNode;
    private Point _mouseDownPt;
    private int _fontHeight;
    private int _groupFontHeight;
    private Font _fontBoldItalic;
    private Font _groupFontBoldItalic;
    private bool _invalidated;
    private bool _nodeSizesDirty;
    private bool _nodeDrawingValid;
    private bool _innerRectangleValid;
    private bool _drawRectangleValid;
    private Rectangle _innerRectangle;
    private Rectangle _drawRectangle;
    private Rectangle _clipRectangle;
    private ArrayList _displayNodes;
    private Hashtable _nodeKeys;
    private Size _glyphThemeSize;
    private ThemeHelper _themeTreeView;
    private ThemeHelper _themeCheckbox;
    private ColorDetails _colorDetails;
    private NodeCollection _rootNodes;
    private INodeVC _nodeVC;
    private INodeVC _defaultNodeVC;
    private INodeVC _groupNodeVC;
    private INodeCollectionVC _collectionVC;
    private INodeCollectionVC _defaultCollectionVC;
    private INodeCollectionVC _groupCollectionVC;
    private ViewControllers _viewControllers;

    [Category("Property Changed")]
    public event EventHandler BorderIndentChanged;

    [Category("Property Changed")]
    public event EventHandler BorderStyleChanged;

    [Category("Property Changed")]
    public event EventHandler BorderColorChanged;

    [Category("Property Changed")]
    public event EventHandler LineWidthChanged;

    [Category("Property Changed")]
    public event EventHandler LineColorChanged;

    [Category("Property Changed")]
    public event EventHandler LineDashStyleChanged;

    [Category("Property Changed")]
    public event EventHandler BoxShownAlwaysChanged;

    [Category("Property Changed")]
    public event EventHandler BoxSignColorChanged;

    [Category("Property Changed")]
    public event EventHandler BoxBorderColorChanged;

    [Category("Property Changed")]
    public event EventHandler BoxInsideColorChanged;

    [Category("Property Changed")]
    public event EventHandler BoxLengthChanged;

    [Category("Property Changed")]
    public event EventHandler BoxDrawStyleChanged;

    [Category("Property Changed")]
    public event EventHandler BoxVisibilityChanged;

    [Category("Property Changed")]
    public event EventHandler LineVisibilityChanged;

    [Category("Property Changed")]
    public event EventHandler ColumnWidthChanged;

    [Category("Property Changed")]
    public event EventHandler CheckStatesChanged;

    [Category("Property Changed")]
    public event EventHandler CheckDrawStyleChanged;

    [Category("Property Changed")]
    public event EventHandler CheckLengthChanged;

    [Category("Property Changed")]
    public event EventHandler CheckGapLeftChanged;

    [Category("Property Changed")]
    public event EventHandler CheckGapRightChanged;

    [Category("Property Changed")]
    public event EventHandler CheckBorderWidthChanged;

    [Category("Property Changed")]
    public event EventHandler CheckBorderColorChanged;

    [Category("Property Changed")]
    public event EventHandler CheckInsideColorChanged;

    [Category("Property Changed")]
    public event EventHandler CheckInsideHotColorChanged;

    [Category("Property Changed")]
    public event EventHandler CheckTickColorChanged;

    [Category("Property Changed")]
    public event EventHandler CheckTickHotColorChanged;

    [Category("Property Changed")]
    public event EventHandler CheckMixedColorChanged;

    [Category("Property Changed")]
    public event EventHandler CheckMixedHotColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupAutoEditChanged;

    [Category("Property Changed")]
    public event EventHandler GroupFontChanged;

    [Category("Property Changed")]
    public event EventHandler GroupArrowsChanged;

    [Category("Property Changed")]
    public event EventHandler GroupIndentLeftChanged;

    [Category("Property Changed")]
    public event EventHandler GroupIndentTopChanged;

    [Category("Property Changed")]
    public event EventHandler GroupIndentBottomChanged;

    [Category("Property Changed")]
    public event EventHandler GroupColoringChanged;

    [Category("Property Changed")]
    public event EventHandler GroupBackColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupForeColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupLineColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupHotBackColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupHotForeColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupSelectedBackColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupSelectedNoFocusBackColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupSelectedForeColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupBorderStyleChanged;

    [Category("Property Changed")]
    public event EventHandler GroupHotFontStyleChanged;

    [Category("Property Changed")]
    public event EventHandler GroupUseHotFontStyleChanged;

    [Category("Property Changed")]
    public event EventHandler GroupSelectedFontStyleChanged;

    [Category("Property Changed")]
    public event EventHandler GroupUseSelectedFontStyleChanged;

    [Category("Property Changed")]
    public event EventHandler GroupGradientAngleChanged;

    [Category("Property Changed")]
    public event EventHandler GroupGradientColoringChanged;

    [Category("Property Changed")]
    public event EventHandler GroupGradientBackChanged;

    [Category("Property Changed")]
    public event EventHandler GroupExtraLeftChanged;

    [Category("Property Changed")]
    public event EventHandler GroupClickExpandChanged;

    [Category("Property Changed")]
    public event EventHandler GroupDoubleClickExpandChanged;

    [Category("Property Changed")]
    public event EventHandler GroupAutoCollapseChanged;

    [Category("Property Changed")]
    public event EventHandler GroupNodesSelectableChanged;

    [Category("Property Changed")]
    public event EventHandler GroupAutoAllocateChanged;

    [Category("Property Changed")]
    public event EventHandler GroupExtraHeightChanged;

    [Category("Property Changed")]
    public event EventHandler GroupHotTrackChanged;

    [Category("Property Changed")]
    public event EventHandler GroupImageBoxChanged;

    [Category("Property Changed")]
    public event EventHandler GroupImageBoxBorderChanged;

    [Category("Property Changed")]
    public event EventHandler GroupImageBoxWidthChanged;

    [Category("Property Changed")]
    public event EventHandler GroupImageBoxGapChanged;

    [Category("Property Changed")]
    public event EventHandler GroupImageBoxLineColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupImageBoxBackColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupImageBoxSelectedBackColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupImageBoxColumnColorChanged;

    [Category("Property Changed")]
    public event EventHandler GroupImageBoxGradientBackChanged;

    [Category("Property Changed")]
    public event EventHandler GroupImageBoxGradientColoringChanged;

    [Category("Property Changed")]
    public event EventHandler GroupImageBoxGradientAngleChanged;

    [Category("Property Changed")]
    public event EventHandler GroupTextRenderingHintChanged;

    [Category("Property Changed")]
    public event EventHandler GroupExpandOnDragHoverChanged;

    [Category("Property Changed")]
    public event EventHandler VerticalScrollbarChanged;

    [Category("Property Changed")]
    public event EventHandler HorizontalScrollbarChanged;

    [Category("Property Changed")]
    public event EventHandler VerticalGranularityChanged;

    [Category("Property Changed")]
    public event EventHandler EnableMouseWheelChanged;

    [Category("Property Changed")]
    public event EventHandler ImageListChanged;

    [Category("Property Changed")]
    public event EventHandler ImageIndexChanged;

    [Category("Property Changed")]
    public event EventHandler ImageGapLeftChanged;

    [Category("Property Changed")]
    public event EventHandler ImageGapRightChanged;

    [Category("Property Changed")]
    public event EventHandler SelectedImageIndexChanged;

    [Category("Property Changed")]
    public event EventHandler ContextMenuNodeChanged;

    [Category("Property Changed")]
    public event EventHandler ContextMenuSpaceChanged;

    [Category("Property Changed")]
    public event EventHandler ClickExpandChanged;

    [Category("Property Changed")]
    public event EventHandler DoubleClickExpandChanged;

    [Category("Property Changed")]
    public event EventHandler AutoCollapseChanged;

    [Category("Property Changed")]
    public event EventHandler ExtendToRightChanged;

    [Category("Property Changed")]
    public event EventHandler AutoEditChanged;

    [Category("Property Changed")]
    public event EventHandler LabelEditChanged;

    [Category("Property Changed")]
    public event EventHandler NodesSelectableChanged;

    [Category("Property Changed")]
    public event EventHandler CanUserExpandCollapseChanged;

    [Category("Property Changed")]
    public event EventHandler ExpandOnDragHoverChanged;

    [Category("Property Changed")]
    public event EventHandler InstantUpdateChanged;

    [Category("Property Changed")]
    public event EventHandler TooltipsChanged;

    [Category("Property Changed")]
    public event EventHandler InfotipsChanged;

    [Category("Property Changed")]
    public event EventHandler IndicatorsChanged;

    [Category("Property Changed")]
    public event EventHandler ViewControllersChanged;

    [Category("Property Changed")]
    public event EventHandler HotNodeChanged;

    [Category("Property Changed")]
    public event EventHandler VerticalNodeGapChanged;

    [Category("Property Changed")]
    public event EventHandler MinimumNodeHeightChanged;

    [Category("Property Changed")]
    public event EventHandler MaximumNodeHeightChanged;

    [Category("Property Changed")]
    public event EventHandler TextRenderingHintChanged;

    [Category("Property Changed")]
    public event EventHandler HotBackColorChanged;

    [Category("Property Changed")]
    public event EventHandler HotForeColorChanged;

    [Category("Property Changed")]
    public event EventHandler SelectedBackColorChanged;

    [Category("Property Changed")]
    public event EventHandler SelectedNoFocusBackColorChanged;

    [Category("Property Changed")]
    public event EventHandler SelectedForeColorChanged;

    [Category("Property Changed")]
    public event EventHandler HotFontStyleChanged;

    [Category("Property Changed")]
    public event EventHandler UseHotFontStyleChanged;

    [Category("Property Changed")]
    public event EventHandler SelectedFontStyleChanged;

    [Category("Property Changed")]
    public event EventHandler UseSelectedFontStyleChanged;

    [Category("Property Changed")]
    public event EventHandler SelectModeChanged;

    [Category("Property Changed")]
    public event EventHandler PathSeparatorChanged;

    [Category("Nodes")]
    public event DragEventHandler ClientDragEnter;

    [Category("Nodes")]
    public event EventHandler ClientDragLeave;

    [Category("Nodes")]
    public event DragEventHandler ClientDragOver;

    [Category("Nodes")]
    public event DragEventHandler ClientDragDrop;

    [Category("Nodes")]
    public event NodeDragDropEventHandler NodeDragEnter;

    [Category("Nodes")]
    public event NodeEventHandler NodeDragLeave;

    [Category("Nodes")]
    public event NodeDragDropEventHandler NodeDragOver;

    [Category("Nodes")]
    public event NodeDragDropEventHandler NodeDragDrop;

    [Category("Nodes")]
    public event StartDragEventHandler NodeDrag;

    [Category("Nodes")]
    public event CancelNodeEventHandler BeforeSelect;

    [Category("Nodes")]
    public event NodeEventHandler AfterSelect;

    [Category("Nodes")]
    public event NodeEventHandler AfterDeselect;

    [Category("Nodes")]
    public event CancelNodeEventHandler BeforeCheck;

    [Category("Nodes")]
    public event NodeEventHandler AfterCheck;

    [Category("Nodes")]
    public event CancelNodeEventHandler BeforeExpand;

    [Category("Nodes")]
    public event NodeEventHandler AfterExpand;

    [Category("Nodes")]
    public event CancelNodeEventHandler BeforeCollapse;

    [Category("Nodes")]
    public event NodeEventHandler AfterCollapse;

    [Category("Nodes")]
    public event LabelEditEventHandler BeforeLabelEdit;

    [Category("Nodes")]
    public event LabelEditEventHandler AfterLabelEdit;

    [Category("Nodes")]
    public event LabelControlEventHandler LabelControlCreated;

    [Category("Nodes")]
    public event CancelNodeEventHandler ShowContextMenuNode;

    [Category("Nodes")]
    public event CancelEventHandler ShowContextMenuSpace;

    [Category("Group")]
    public event NodeDragDropEventHandler GroupDragEnter;

    [Category("Group")]
    public event NodeEventHandler GroupDragLeave;

    [Category("Group")]
    public event NodeDragDropEventHandler GroupDragOver;

    [Category("Group")]
    public event NodeDragDropEventHandler GroupDragDrop;

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public INodeVC NodeVC
    {
      get
      {
        return _nodeVC;
      }
      set
      {
        if (_nodeVC != null)
        {
          _nodeVC.Detaching(this);
        }
        if (value == null)
        {
          switch (ViewControllers)
          {
            case ViewControllers.Default:
              _nodeVC = _defaultNodeVC;
              break;
            case ViewControllers.Group:
              _nodeVC = _groupNodeVC;
              break;
          }
        }
        else
        {
          _nodeVC = value;
        }
        _nodeVC.Initialize(this);
        MarkAllNodeSizesDirty();
        InvalidateNodeDrawing();
      }
    }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public INodeCollectionVC CollectionVC
    {
      get
      {
        return _collectionVC;
      }
      set
      {
        if (_collectionVC != null)
        {
          _collectionVC.Detaching(this);
        }
        if (value == null)
        {
          switch (ViewControllers)
          {
            case ViewControllers.Default:
              _collectionVC = _groupCollectionVC;
              break;
            case ViewControllers.Group:
              _collectionVC = _defaultCollectionVC;
              break;
          }
        }
        else
        {
          _collectionVC = value;
        }
        _collectionVC.Initialize(this);
        MarkAllNodeSizesDirty();
        InvalidateNodeDrawing();
      }
    }
    [Category("Nodes"), DefaultValue(typeof(ViewControllers), "Default"), Description("Which view controllers should be used.")]
    public ViewControllers ViewControllers
    {
      get
      {
        return _viewControllers;
      }
      set
      {
        _viewControllers = value;
        switch (_viewControllers)
        {
          case ViewControllers.Default:
            NodeVC = _defaultNodeVC;
            CollectionVC = _defaultCollectionVC;
            break;
          case ViewControllers.Group:
            NodeVC = _groupNodeVC;
            CollectionVC = _groupCollectionVC;
            break;
        }
        OnViewControllersChanged();
        MarkAllNodeSizesDirty();
        InvalidateNodeDrawing();
      }
    }
    [Browsable(false)]
    public SelectedNodeCollection SelectedNodes
    {
      get
      {
        return new SelectedNodeCollection(_selected);
      }
    }
    [Browsable(false)]
    public Node SelectedNode
    {
      get
      {
        if (_focusNode != null && _focusNode.IsSelected)
        {
          return _focusNode;
        }
        if (_selected.Count != 0)
        {
          IEnumerator enumerator = _selected.Keys.GetEnumerator();
          try
          {
            if (enumerator.MoveNext())
            {
              return (Node)enumerator.Current;
            }
          }
          finally
          {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable != null)
            {
              disposable.Dispose();
            }
          }
        }
        return null;
      }
      set
      {
        if (value != null && SelectMode != SelectMode.None && value.VC.CanSelectNode(this, value))
        {
          SingleSelect(value);
        }
      }
    }
    [Browsable(false)]
    public Node FocusNode
    {
      get
      {
        return _focusNode;
      }
      set
      {
        SetFocusNode(value);
      }
    }
    [Category("Nodes"), Description("The collection of root nodes."), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Editor("Crownwood.DotNetMagic.Controls.NodeCollectionEditor", typeof(UITypeEditor))]
    public NodeCollection Nodes
    {
      get
      {
        return _rootNodes;
      }
    }
    [Category("Nodes"), DefaultValue(null), Description("ContextMenu to show on right clicking a Node.")]
    public ContextMenu ContextMenuNode
    {
      get
      {
        return _contextMenuNode;
      }
      set
      {
        if (_contextMenuNode != value)
        {
          _contextMenuNode = value;
          OnContextMenuNodeChanged();
        }
      }
    }
    [Category("Nodes"), DefaultValue(null), Description("ContextMenu to show on right clicking outside of any Node.")]
    public ContextMenu ContextMenuSpace
    {
      get
      {
        return _contextMenuSpace;
      }
      set
      {
        if (_contextMenuSpace != value)
        {
          _contextMenuSpace = value;
          OnContextMenuSpaceChanged();
        }
      }
    }
    [Category("Nodes"), DefaultValue("\\"), Description("Delimiter string that the node path uses.")]
    public string PathSeparator
    {
      get
      {
        return _pathSeparator;
      }
      set
      {
        if (_pathSeparator != value)
        {
          _pathSeparator = value;
          OnPathSeparatorChanged();
        }
      }
    }
    [DefaultValue(typeof(Color), "Window")]
    public override Color BackColor
    {
      get
      {
        return base.BackColor;
      }
      set
      {
        base.BackColor = value;
      }
    }
    [Category("Border"), Description("Specifies indentation between border and inside drawing"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public IndentPaddingEdges BorderIndent
    {
      get
      {
        return _borderIndent;
      }
    }
    [Category("Border"), DefaultValue(typeof(TreeBorderStyle), "Theme"), Description("Indicates whether or not the tree control should have a border.")]
    public TreeBorderStyle BorderStyle
    {
      get
      {
        return _borderStyle;
      }
      set
      {
        if (_borderStyle != value)
        {
          _borderStyle = value;
          using (Graphics graphics = base.CreateGraphics())
          {
            UpdateBorderCache(graphics);
          }
          OnBorderStyleChanged();
          InvalidateInnerRectangle();
        }
      }
    }
    [Category("Border"), DefaultValue(typeof(Color), "WindowText"), Description("The color used to draw the non-3D border styles.")]
    public Color BorderColor
    {
      get
      {
        return _borderColor;
      }
      set
      {
        if (_borderColor != value)
        {
          _borderColor = value;
          OnBorderColorChanged();
          InvalidateAll();
        }
      }
    }
    [Browsable(false)]
    public Panel Corner
    {
      get
      {
        return _corner;
      }
    }
    [Category("Lines and Boxes"), DefaultValue(1), Description("Defines the width of lines.")]
    public int LineWidth
    {
      get
      {
        return _lineWidth;
      }
      set
      {
        if (_lineWidth != value)
        {
          _lineWidth = value;
          ClearLineBoxCache();
          OnLineWidthChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Lines and Boxes"), DefaultValue(typeof(Color), "GrayText"), Description("Defines the color of lines.")]
    public Color LineColor
    {
      get
      {
        return _lineColor;
      }
      set
      {
        if (_lineColor != value)
        {
          _lineColor = value;
          ClearLineBoxCache();
          OnLineColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Lines and Boxes"), DefaultValue(typeof(LineDashStyle), "Dot"), Description("Defines the style used for lines.")]
    public LineDashStyle LineDashStyle
    {
      get
      {
        return _lineDashStyle;
      }
      set
      {
        if (_lineDashStyle != value)
        {
          _lineDashStyle = value;
          ClearLineBoxCache();
          OnLineDashStyleChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Lines and Boxes"), DefaultValue(9), Description("Defines length used for drawing box.")]
    public int BoxLength
    {
      get
      {
        return _boxLength;
      }
      set
      {
        if (_boxLength != value)
        {
          _boxLength = value;
          if (_boxLength < 1)
          {
            _boxLength = 1;
          }
          OnBoxLengthChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Lines and Boxes"), DefaultValue(typeof(DrawStyle), "Themed"), Description("Determines how Boxes are drawn.")]
    public DrawStyle BoxDrawStyle
    {
      get
      {
        return _boxDrawStyle;
      }
      set
      {
        if (_boxDrawStyle != value)
        {
          _boxDrawStyle = value;
          OnBoxDrawStyleChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Lines and Boxes"), DefaultValue(false), Description("Should box be drawn when node has no children.")]
    public bool BoxShownAlways
    {
      get
      {
        return _boxShownAlways;
      }
      set
      {
        if (_boxShownAlways != value)
        {
          _boxShownAlways = value;
          OnBoxShownAlwaysChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Lines and Boxes"), DefaultValue(typeof(Color), "WindowText"), Description("Defines the color of box signs.")]
    public Color BoxSignColor
    {
      get
      {
        return _boxSignColor;
      }
      set
      {
        if (_boxSignColor != value)
        {
          _boxSignColor = value;
          ClearLineBoxCache();
          OnBoxSignColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Lines and Boxes"), DefaultValue(typeof(Color), "GrayText"), Description("Defines the color of box borders.")]
    public Color BoxBorderColor
    {
      get
      {
        return _boxBorderColor;
      }
      set
      {
        if (_boxBorderColor != value)
        {
          _boxBorderColor = value;
          ClearLineBoxCache();
          OnBoxBorderColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Lines and Boxes"), DefaultValue(typeof(Color), "Window"), Description("Defines the color of box borders.")]
    public Color BoxInsideColor
    {
      get
      {
        return _boxInsideColor;
      }
      set
      {
        if (_boxInsideColor != value)
        {
          _boxInsideColor = value;
          ClearLineBoxCache();
          OnBoxInsideColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Lines and Boxes"), DefaultValue(typeof(LineBoxVisibility), "Everywhere"), Description("Determine when Boxes are displayed.")]
    public LineBoxVisibility BoxVisibility
    {
      get
      {
        return _boxVisibility;
      }
      set
      {
        if (_boxVisibility != value)
        {
          _boxVisibility = value;
          OnBoxVisibilityChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Lines and Boxes"), DefaultValue(typeof(LineBoxVisibility), "Everywhere"), Description("Determine when Lines are displayed.")]
    public LineBoxVisibility LineVisibility
    {
      get
      {
        return _lineVisibility;
      }
      set
      {
        if (_lineVisibility != value)
        {
          _lineVisibility = value;
          OnLineVisibilityChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Lines and Boxes"), DefaultValue(19), Description("Defines the width of the lines column.")]
    public int ColumnWidth
    {
      get
      {
        return _columnWidth;
      }
      set
      {
        if (_columnWidth != value)
        {
          _columnWidth = value;
          OnColumnWidthChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Check"), DefaultValue(typeof(CheckStates), "None"), Description("Define the style of checkboxes.")]
    public CheckStates CheckStates
    {
      get
      {
        return _checkStates;
      }
      set
      {
        if (_checkStates != value)
        {
          _checkStates = value;
          OnCheckStatesChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Check"), DefaultValue(13), Description("Define size for drawing a checkbox.")]
    public int CheckLength
    {
      get
      {
        return _checkLength;
      }
      set
      {
        if (_checkLength != value)
        {
          _checkLength = value;
          if (_checkLength < 9)
          {
            _checkLength = 9;
          }
          OnCheckLengthChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Check"), DefaultValue(1), Description("Pixel gap between start of node and start of check.")]
    public int CheckGapLeft
    {
      get
      {
        return _checkGapLeft;
      }
      set
      {
        if (_checkGapLeft != value)
        {
          _checkGapLeft = value;
          if (_checkGapLeft < 0)
          {
            _checkGapLeft = 0;
          }
          OnCheckGapLeftChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Check"), DefaultValue(3), Description("Pixel gap between end of check and start of image.")]
    public int CheckGapRight
    {
      get
      {
        return _checkGapRight;
      }
      set
      {
        if (_checkGapRight != value)
        {
          _checkGapRight = value;
          if (_checkGapRight < 0)
          {
            _checkGapRight = 0;
          }
          OnCheckGapRightChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Check"), DefaultValue(typeof(DrawStyle), "Themed"), Description("Define how checkboxes are drawn.")]
    public DrawStyle CheckDrawStyle
    {
      get
      {
        return _checkDrawStyle;
      }
      set
      {
        if (_checkDrawStyle != value)
        {
          _checkDrawStyle = value;
          OnCheckDrawStyleChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Check"), DefaultValue(1), Description("Width of the check border.")]
    public int CheckBorderWidth
    {
      get
      {
        return _checkBorderWidth;
      }
      set
      {
        if (_checkBorderWidth != value)
        {
          _checkBorderWidth = value;
          OnCheckBorderWidthChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Check"), DefaultValue(typeof(Color), "ControlDarkDark"), Description("Defines the color of check borders.")]
    public Color CheckBorderColor
    {
      get
      {
        return _checkBorderColor;
      }
      set
      {
        if (_checkBorderColor != value)
        {
          _checkBorderColor = value;
          ClearCheckCache();
          OnCheckBorderColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Check"), DefaultValue(typeof(Color), "Window"), Description("Defines the color inside the check box.")]
    public Color CheckInsideColor
    {
      get
      {
        return _checkInsideColor;
      }
      set
      {
        if (_checkInsideColor != value)
        {
          _checkInsideColor = value;
          ClearCheckCache();
          OnCheckInsideColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Check"), DefaultValue(typeof(Color), "Window"), Description("Defines the color inside the check box when hot tracked.")]
    public Color CheckInsideHotColor
    {
      get
      {
        return _checkInsideHotColor;
      }
      set
      {
        if (_checkInsideHotColor != value)
        {
          _checkInsideHotColor = value;
          ClearCheckCache();
          OnCheckInsideHotColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Check"), DefaultValue(typeof(Color), "ControlText"), Description("Defines the ticked color inside the check box.")]
    public Color CheckTickColor
    {
      get
      {
        return _checkTickColor;
      }
      set
      {
        if (_checkTickColor != value)
        {
          _checkTickColor = value;
          ClearCheckCache();
          OnCheckTickColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Check"), DefaultValue(typeof(Color), "ControlText"), Description("Defines the ticked color inside the check box when hot tracking.")]
    public Color CheckTickHotColor
    {
      get
      {
        return _checkTickHotColor;
      }
      set
      {
        if (_checkTickHotColor != value)
        {
          _checkTickHotColor = value;
          ClearCheckCache();
          OnCheckTickHotColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Check"), DefaultValue(typeof(Color), "ControlText"), Description("Defines the mixed color inside the check box.")]
    public Color CheckMixedColor
    {
      get
      {
        return _checkMixedColor;
      }
      set
      {
        if (_checkMixedColor != value)
        {
          _checkMixedColor = value;
          ClearCheckCache();
          OnCheckMixedColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Check"), DefaultValue(typeof(Color), "ControlText"), Description("Defines the mixed color inside the check box when hot tracking.")]
    public Color CheckMixedHotColor
    {
      get
      {
        return _checkMixedHotColor;
      }
      set
      {
        if (_checkMixedHotColor != value)
        {
          _checkMixedHotColor = value;
          ClearCheckCache();
          OnCheckMixedHotColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Scrolling"), DefaultValue(typeof(VerticalGranularity), "Node"), Description("Defines the vertical scrolling granularity.")]
    public VerticalGranularity VerticalGranularity
    {
      get
      {
        return _granularity;
      }
      set
      {
        if (_granularity != value)
        {
          _granularity = value;
          _offset = Point.Empty;
          OnVerticalGranularityChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Scrolling"), DefaultValue(typeof(ScrollVisibility), "WhenNeeded"), Description("Decide when the vertical scrollbar is shown.")]
    public ScrollVisibility VerticalScrollbar
    {
      get
      {
        return _vVisibility;
      }
      set
      {
        if (_vVisibility != value)
        {
          _vVisibility = value;
          OnVerticalScrollbarChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Scrolling"), DefaultValue(typeof(ScrollVisibility), "WhenNeeded"), Description("Decide when the horizontal scrollbar is shown.")]
    public ScrollVisibility HorizontalScrollbar
    {
      get
      {
        return _hVisibility;
      }
      set
      {
        if (_hVisibility != value)
        {
          _hVisibility = value;
          OnHorizontalScrollbarChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Scrolling"), DefaultValue(true), Description("Should mouse wheel be used to scroll vertically.")]
    public bool EnableMouseWheel
    {
      get
      {
        return _enableMouseWheel;
      }
      set
      {
        if (_enableMouseWheel != value)
        {
          _enableMouseWheel = value;
          OnEnableMouseWheelChanged();
        }
      }
    }
    [Category("Group"), DefaultValue(true), Description("Can group level nodes be selected.")]
    public bool GroupNodesSelectable
    {
      get
      {
        return _groupNodesSelectable;
      }
      set
      {
        if (_groupNodesSelectable != value)
        {
          _groupNodesSelectable = value;
          OnGroupNodesSelectableChanged();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(TextRenderingHint), "AntiAlias"), Description("Control how group text is rendered.")]
    public TextRenderingHint GroupTextRenderingHint
    {
      get
      {
        return _groupTextRenderingHint;
      }
      set
      {
        if (_groupTextRenderingHint != value)
        {
          _groupTextRenderingHint = value;
          OnGroupTextRenderingHintChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), Description("Defines the font for drawing group text.")]
    public Font GroupFont
    {
      get
      {
        return _groupFont;
      }
      set
      {
        if (_groupFont != value)
        {
          _groupFont = value;
          OnGroupFontChanged();
          InternalResetGroupFontHeight();
          InternalResetGroupFontBoldItalic();
          MarkAllNodeSizesDirty();
        }
      }
    }
    [Category("Group"), DefaultValue(false), Description("Do group nodes enter editing after a delayed second click.")]
    public bool GroupAutoEdit
    {
      get
      {
        return _groupAutoEdit;
      }
      set
      {
        if (_groupAutoEdit != value)
        {
          _groupAutoEdit = value;
          OnGroupAutoEditChanged();
        }
      }
    }
    [Category("Group"), DefaultValue(false), Description("Should arrows be shown on the group nodes.")]
    public bool GroupArrows
    {
      get
      {
        return _groupArrows;
      }
      set
      {
        if (_groupArrows != value)
        {
          _groupArrows = value;
          OnGroupArrowsChanged();
          MarkAllNodeSizesDirty();
        }
      }
    }
    [Category("Group"), DefaultValue(2), Description("Defines extra space on left of group children.")]
    public int GroupIndentLeft
    {
      get
      {
        return _groupIndentLeft;
      }
      set
      {
        if (_groupIndentLeft != value)
        {
          _groupIndentLeft = value;
          OnGroupIndentLeftChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Group"), DefaultValue(5), Description("Defines extra space at top of group children.")]
    public int GroupIndentTop
    {
      get
      {
        return _groupIndentTop;
      }
      set
      {
        if (_groupIndentTop != value)
        {
          _groupIndentTop = value;
          OnGroupIndentTopChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Group"), DefaultValue(5), Description("Defines extra space at bottom of group children.")]
    public int GroupIndentBottom
    {
      get
      {
        return _groupIndentBottom;
      }
      set
      {
        if (_groupIndentBottom != value)
        {
          _groupIndentBottom = value;
          OnGroupIndentBottomChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(GroupColoring), "ControlProperties"), Description("Defines how the groups are colored.")]
    public GroupColoring GroupColoring
    {
      get
      {
        return _groupColoring;
      }
      set
      {
        if (_groupColoring != value)
        {
          _groupColoring = value;
          OnGroupColoringChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(Color), "ControlDark"), Description("Defines the group background color.")]
    public Color GroupBackColor
    {
      get
      {
        return _groupBackColor;
      }
      set
      {
        if (_groupBackColor != value)
        {
          _groupBackColor = value;
          OnGroupBackColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(Color), "ControlText"), Description("Defines the group foreground color.")]
    public Color GroupForeColor
    {
      get
      {
        return _groupForeColor;
      }
      set
      {
        if (_groupForeColor != value)
        {
          _groupForeColor = value;
          OnGroupForeColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(Color), "ControlText"), Description("Defines the group line color.")]
    public Color GroupLineColor
    {
      get
      {
        return _groupLineColor;
      }
      set
      {
        if (_groupLineColor != value)
        {
          _groupLineColor = value;
          ClearGroupCache();
          OnGroupLineColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(Color), "HotTrack"), Description("Defines the group background color when hot tracking.")]
    public Color GroupHotBackColor
    {
      get
      {
        return _groupHotBackColor;
      }
      set
      {
        if (_groupHotBackColor != value)
        {
          _groupHotBackColor = value;
          OnGroupHotBackColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(Color), "Highlight"), Description("Defines the group background color when selected.")]
    public Color GroupSelectedBackColor
    {
      get
      {
        return _groupSelectedBackColor;
      }
      set
      {
        if (_groupSelectedBackColor != value)
        {
          _groupSelectedBackColor = value;
          OnGroupSelectedBackColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(Color), "Highlight"), Description("Defines the group background color when selected but without focus.")]
    public Color GroupSelectedNoFocusBackColor
    {
      get
      {
        return _groupSelectedNoFocusBackColor;
      }
      set
      {
        if (_groupSelectedNoFocusBackColor != value)
        {
          _groupSelectedNoFocusBackColor = value;
          OnGroupSelectedNoFocusBackColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(Color), "HighlightText"), Description("Defines the group foreground color when hot tracking.")]
    public Color GroupHotForeColor
    {
      get
      {
        return _groupHotForeColor;
      }
      set
      {
        if (_groupHotForeColor != value)
        {
          _groupHotForeColor = value;
          OnGroupHotForeColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(Color), "HighlightText"), Description("Defines the group foreground color when selected.")]
    public Color GroupSelectedForeColor
    {
      get
      {
        return _groupSelectedForeColor;
      }
      set
      {
        if (_groupSelectedForeColor != value)
        {
          _groupSelectedForeColor = value;
          OnGroupSelectedForeColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Nodes"), DefaultValue(true), Description("Should group nodes expand when dragging hovers over them.")]
    public bool GroupExpandOnDragHover
    {
      get
      {
        return _groupExpandOnDragHover;
      }
      set
      {
        if (_groupExpandOnDragHover != value)
        {
          _groupExpandOnDragHover = value;
          OnGroupExpandOnDragHoverChanged();
        }
      }
    }
    [Category("Group - ImageBox"), DefaultValue(typeof(Color), "InfoText"), Description("Defines the image box line color.")]
    public Color GroupImageBoxLineColor
    {
      get
      {
        return _groupImageBoxLineColor;
      }
      set
      {
        if (_groupImageBoxLineColor != value)
        {
          _groupImageBoxLineColor = value;
          ClearGroupCache();
          OnGroupImageBoxLineColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group - ImageBox"), DefaultValue(typeof(Color), "Info"), Description("Defines the image box background color.")]
    public Color GroupImageBoxBackColor
    {
      get
      {
        return _groupImageBoxBackColor;
      }
      set
      {
        if (_groupImageBoxBackColor != value)
        {
          _groupImageBoxBackColor = value;
          ClearGroupCache();
          OnGroupImageBoxBackColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group - ImageBox"), DefaultValue(typeof(Color), "Info"), Description("Defines the image box background color when selected.")]
    public Color GroupImageBoxSelectedBackColor
    {
      get
      {
        return _groupImageBoxSelectedBackColor;
      }
      set
      {
        if (_groupImageBoxSelectedBackColor != value)
        {
          _groupImageBoxSelectedBackColor = value;
          ClearGroupCache();
          OnGroupImageBoxSelectedBackColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group - ImageBox"), DefaultValue(typeof(Color), "Control"), Description("Defines the image box column background color.")]
    public Color GroupImageBoxColumnColor
    {
      get
      {
        return _groupImageBoxColumnColor;
      }
      set
      {
        if (_groupImageBoxColumnColor != value)
        {
          _groupImageBoxColumnColor = value;
          ClearGroupCache();
          OnGroupImageBoxColumnColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group - ImageBox"), DefaultValue(false), Description("Should group image box background be drawn with gradient.")]
    public bool GroupImageBoxGradientBack
    {
      get
      {
        return _groupImageBoxGradientBack;
      }
      set
      {
        if (_groupImageBoxGradientBack != value)
        {
          _groupImageBoxGradientBack = value;
          OnGroupImageBoxGradientBackChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group - ImageBox"), DefaultValue(typeof(GradientDirection), "TopToBottom"), Description("Direction of image box gradient when drawing group in gradient effect.")]
    public GradientDirection GroupImageBoxGradientAngle
    {
      get
      {
        return _groupImageBoxGradientAngle;
      }
      set
      {
        if (_groupImageBoxGradientAngle != value)
        {
          _groupImageBoxGradientAngle = value;
          OnGroupImageBoxGradientAngleChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group - ImageBox"), DefaultValue(typeof(TreeGradientColoring), "VeryLightToColor"), Description("Define how the image box gradient colour is calculated.")]
    public TreeGradientColoring GroupImageBoxGradientColoring
    {
      get
      {
        return _groupImageBoxGradientColoring;
      }
      set
      {
        if (_groupImageBoxGradientColoring != value)
        {
          _groupImageBoxGradientColoring = value;
          OnGroupImageBoxGradientColoringChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(GroupBorderStyle), "AllEdges"), Description("Defines how group borders are drawn.")]
    public GroupBorderStyle GroupBorderStyle
    {
      get
      {
        return _groupBorderStyle;
      }
      set
      {
        if (_groupBorderStyle != value)
        {
          _groupBorderStyle = value;
          OnGroupBorderStyleChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(FontStyle), "Regular"), Description("Group font style to apply when hot tracking.")]
    public FontStyle GroupHotFontStyle
    {
      get
      {
        return _groupHotFontStyle;
      }
      set
      {
        if (_groupHotFontStyle != value)
        {
          _groupHotFontStyle = value;
          OnGroupHotFontStyleChanged();
          MarkAllNodeSizesDirty();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(false), Description("Should the GroupHotFontStyle be used.")]
    public bool GroupUseHotFontStyle
    {
      get
      {
        return _groupUseHotFontStyle;
      }
      set
      {
        if (_groupUseHotFontStyle != value)
        {
          _groupUseHotFontStyle = value;
          OnGroupUseHotFontStyleChanged();
          MarkAllNodeSizesDirty();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(FontStyle), "Regular"), Description("Group font style to apply when selected.")]
    public FontStyle GroupSelectedFontStyle
    {
      get
      {
        return _groupSelectedFontStyle;
      }
      set
      {
        if (_groupSelectedFontStyle != value)
        {
          _groupSelectedFontStyle = value;
          OnGroupSelectedFontStyleChanged();
          MarkAllNodeSizesDirty();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(false), Description("Should the GroupSelectedFontStyle be used.")]
    public bool GroupUseSelectedFontStyle
    {
      get
      {
        return _groupUseSelectedFontStyle;
      }
      set
      {
        if (_groupUseSelectedFontStyle != value)
        {
          _groupUseSelectedFontStyle = value;
          OnGroupUseSelectedFontStyleChanged();
          MarkAllNodeSizesDirty();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(GradientDirection), "TopToBottom"), Description("Direction of gradient when drawing group in gradient effect.")]
    public GradientDirection GroupGradientAngle
    {
      get
      {
        return _groupGradientAngle;
      }
      set
      {
        if (_groupGradientAngle != value)
        {
          _groupGradientAngle = value;
          OnGroupGradientAngleChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(TreeGradientColoring), "VeryLightToColor"), Description("Define how the gradient colour is calculated.")]
    public TreeGradientColoring GroupGradientColoring
    {
      get
      {
        return _groupGradientColoring;
      }
      set
      {
        if (_groupGradientColoring != value)
        {
          _groupGradientColoring = value;
          OnGroupGradientColoringChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(true), Description("Should group background be drawn with gradient.")]
    public bool GroupGradientBack
    {
      get
      {
        return _groupGradientBack;
      }
      set
      {
        if (_groupGradientBack != value)
        {
          _groupGradientBack = value;
          OnGroupGradientBackChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(3), Description("Extra padding on left of group.")]
    public int GroupExtraLeft
    {
      get
      {
        return _groupExtraLeft;
      }
      set
      {
        if (_groupExtraLeft != value)
        {
          if (value < 0)
          {
            value = 0;
          }
          _groupExtraLeft = value;
          OnGroupExtraLeftChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Group"), DefaultValue(5), Description("Extra padding added to height of group.")]
    public int GroupExtraHeight
    {
      get
      {
        return _groupExtraHeight;
      }
      set
      {
        if (_groupExtraHeight != value)
        {
          if (value < 0)
          {
            value = 0;
          }
          _groupExtraHeight = value;
          OnGroupExtraHeightChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Group"), DefaultValue(true), Description("Determine if group should be hot tracked.")]
    public bool GroupHotTrack
    {
      get
      {
        return _groupHotTrack;
      }
      set
      {
        if (_groupHotTrack != value)
        {
          _groupHotTrack = value;
          OnGroupHotTrackChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(ClickExpandAction), "Toggle"), Description("Expand action to perform when clicking a group node.")]
    public ClickExpandAction GroupClickExpand
    {
      get
      {
        return _groupClickExpand;
      }
      set
      {
        if (_groupClickExpand != value)
        {
          _groupClickExpand = value;
          OnGroupClickExpandChanged();
        }
      }
    }
    [Category("Group"), DefaultValue(typeof(ClickExpandAction), "Expand"), Description("Expand action to perform when double clicking a group node.")]
    public ClickExpandAction GroupDoubleClickExpand
    {
      get
      {
        return _groupDoubleClickExpand;
      }
      set
      {
        if (_groupDoubleClickExpand != value)
        {
          _groupDoubleClickExpand = value;
          OnGroupDoubleClickExpandChanged();
        }
      }
    }
    [Category("Group"), DefaultValue(false), Description("Should other nodes at same level collapse when group node is expanded.")]
    public bool GroupAutoCollapse
    {
      get
      {
        return _groupAutoCollapse;
      }
      set
      {
        if (_groupAutoCollapse != value)
        {
          _groupAutoCollapse = value;
          if (_groupAutoCollapse)
          {
            Node node = null;
            int i;
            for (i = 0; i < Nodes.Count; i++)
            {
              Node node2 = Nodes[i];
              if (node2.Visible)
              {
                if (node2.Expanded)
                {
                  break;
                }
                if (node2.Nodes.VisibleCount > 0 && node == null)
                {
                  node = node2;
                }
              }
            }
            if (i == Nodes.Count && node != null)
            {
              node.Expand();
            }
          }
          OnGroupAutoCollapseChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Group"), DefaultValue(false), Description("Allocate any spare space between last node and end of control to expanded group.")]
    public bool GroupAutoAllocate
    {
      get
      {
        return _groupAutoAllocate;
      }
      set
      {
        if (_groupAutoAllocate != value)
        {
          _groupAutoAllocate = value;
          OnGroupAutoAllocateChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Group - ImageBox"), DefaultValue(false), Description("Show an image box at start of group nodes.")]
    public bool GroupImageBox
    {
      get
      {
        return _groupImageBox;
      }
      set
      {
        if (_groupImageBox != value)
        {
          _groupImageBox = value;
          OnGroupImageBoxChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Group - ImageBox"), DefaultValue(true), Description("Should the image box column be drawn.")]
    public bool GroupImageBoxColumn
    {
      get
      {
        return _groupImageBoxColumn;
      }
      set
      {
        if (_groupImageBoxColumn != value)
        {
          _groupImageBoxColumn = value;
          OnGroupImageBoxColumnChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Group - ImageBox"), DefaultValue(true), Description("Should the image box border be drawn.")]
    public bool GroupImageBoxBorder
    {
      get
      {
        return _groupImageBoxBorder;
      }
      set
      {
        if (_groupImageBoxBorder != value)
        {
          _groupImageBoxBorder = value;
          OnGroupImageBoxBorderChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Group - ImageBox"), DefaultValue(20), Description("Width of image box area.")]
    public int GroupImageBoxWidth
    {
      get
      {
        return _groupImageBoxWidth;
      }
      set
      {
        if (value < 1)
        {
          value = 1;
        }
        if (_groupImageBoxWidth != value)
        {
          _groupImageBoxWidth = value;
          OnGroupImageBoxWidthChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Group - ImageBox"), DefaultValue(6), Description("Spacing gap between the image box and group text.")]
    public int GroupImageBoxGap
    {
      get
      {
        return _groupImageBoxGap;
      }
      set
      {
        if (value < 1)
        {
          value = 1;
        }
        if (_groupImageBoxGap != value)
        {
          _groupImageBoxGap = value;
          OnGroupImageBoxGapChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Nodes"), DefaultValue(typeof(ClickExpandAction), "None"), Description("Expand action to take when node is clicked.")]
    public ClickExpandAction ClickExpand
    {
      get
      {
        return _clickExpand;
      }
      set
      {
        if (_clickExpand != value)
        {
          _clickExpand = value;
          OnClickExpandChanged();
        }
      }
    }
    [Category("Nodes"), DefaultValue(typeof(ClickExpandAction), "Toggle"), Description("Expand action to take when node is double clicked.")]
    public ClickExpandAction DoubleClickExpand
    {
      get
      {
        return _doubleClickExpand;
      }
      set
      {
        if (_doubleClickExpand != value)
        {
          _doubleClickExpand = value;
          OnDoubleClickExpandChanged();
        }
      }
    }
    [Category("Nodes"), DefaultValue(false), Description("Should other nodes at same level collapse when node is expanded.")]
    public bool AutoCollapse
    {
      get
      {
        return _autoCollapse;
      }
      set
      {
        if (_autoCollapse != value)
        {
          _autoCollapse = value;
          OnAutoCollapseChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Nodes"), DefaultValue(false), Description("Should a node be selectable and drawn to right extent.")]
    public bool ExtendToRight
    {
      get
      {
        return _extendToRight;
      }
      set
      {
        if (_extendToRight != value)
        {
          _extendToRight = value;
          OnExtendToRightChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Nodes"), DefaultValue(false), Description("Should each change be shown immediately.")]
    public bool InstantUpdate
    {
      get
      {
        return _instantUpdate;
      }
      set
      {
        if (_instantUpdate != value)
        {
          _instantUpdate = value;
          OnInstantUpdateChanged();
        }
      }
    }
    [Category("Nodes"), DefaultValue(true), Description("Should tooltips be shown when node cannot be fully seen.")]
    public bool Tooltips
    {
      get
      {
        return _tooltips;
      }
      set
      {
        if (_tooltips != value)
        {
          _tooltips = value;
          OnTooltipsChanged();
        }
      }
    }
    [Category("Nodes"), DefaultValue(true), Description("Should node specific tooltips be used if defined for the individual node.")]
    public bool Infotips
    {
      get
      {
        return _infotips;
      }
      set
      {
        if (_infotips != value)
        {
          _infotips = value;
          OnInfotipsChanged();
        }
      }
    }
    [Category("Nodes"), DefaultValue(true), Description("Can nodes be selected.")]
    public bool NodesSelectable
    {
      get
      {
        return _nodesSelectable;
      }
      set
      {
        if (_nodesSelectable != value)
        {
          _nodesSelectable = value;
          OnNodesSelectableChanged();
        }
      }
    }
    [Category("Nodes"), DefaultValue(true), Description("Do nodes enter editing after a delayed second click.")]
    public bool AutoEdit
    {
      get
      {
        return _autoEdit;
      }
      set
      {
        if (_autoEdit != value)
        {
          _autoEdit = value;
          OnAutoEditChanged();
        }
      }
    }
    [Category("Nodes"), DefaultValue(true), Description("Can nodes labels be edited.")]
    public bool LabelEdit
    {
      get
      {
        return _labelEdit;
      }
      set
      {
        if (_labelEdit != value)
        {
          _labelEdit = value;
          OnLabelEditChanged();
        }
      }
    }
    [Category("Nodes"), DefaultValue(true), Description("Can user expand or collapse nodes.")]
    public bool CanUserExpandCollapse
    {
      get
      {
        return _canUserExpandCollapse;
      }
      set
      {
        if (_canUserExpandCollapse != value)
        {
          _canUserExpandCollapse = value;
          OnCanUserExpandCollapseChanged();
        }
      }
    }
    [Category("Nodes"), DefaultValue(true), Description("Should nodes expand when dragging hovers over them.")]
    public bool ExpandOnDragHover
    {
      get
      {
        return _expandOnDragHover;
      }
      set
      {
        if (_expandOnDragHover != value)
        {
          _expandOnDragHover = value;
          OnExpandOnDragHoverChanged();
        }
      }
    }
    [Category("Nodes"), DefaultValue(typeof(Indicators), "None"), Description("How should indicators be shown.")]
    public Indicators Indicators
    {
      get
      {
        return _indicators;
      }
      set
      {
        if (_indicators != value)
        {
          _indicators = value;
          OnIndicatorsChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Nodes"), DefaultValue(0), Description("Defines the vertical pixel gap between nodes.")]
    public int VerticalNodeGap
    {
      get
      {
        return _verticalNodeGap;
      }
      set
      {
        if (_verticalNodeGap != value)
        {
          _verticalNodeGap = value;
          OnVerticalNodeGapChanged();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Nodes"), Description("Defines the minimum height for a node.")]
    public int MinimumNodeHeight
    {
      get
      {
        return _minimumNodeHeight;
      }
      set
      {
        if (_minimumNodeHeight != value)
        {
          _minimumNodeHeight = value;
          OnMinimumNodeHeightChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Nodes"), DefaultValue(9999), Description("Defines the maximum height for a node.")]
    public int MaximumNodeHeight
    {
      get
      {
        return _maximumNodeHeight;
      }
      set
      {
        if (_maximumNodeHeight != value)
        {
          _maximumNodeHeight = value;
          OnMaximumNodeHeightChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Images"), DefaultValue(null), Description("The image list from which node images are taken.")]
    public ImageList ImageList
    {
      get
      {
        return _imageList;
      }
      set
      {
        if (_imageList != value)
        {
          _imageList = value;
          OnImageListChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Images"), DefaultValue(-1), Description("The default image index for nodes.")]
    public int ImageIndex
    {
      get
      {
        return _imageIndex;
      }
      set
      {
        if (_imageIndex != value)
        {
          _imageIndex = value;
          if (_imageIndex < -1)
          {
            _imageIndex = -1;
          }
          OnImageIndexChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Images"), DefaultValue(-1), Description("The default selected image index for nodes.")]
    public int SelectedImageIndex
    {
      get
      {
        return _selectedImageIndex;
      }
      set
      {
        if (_selectedImageIndex != value)
        {
          _selectedImageIndex = value;
          if (_selectedImageIndex < -1)
          {
            _selectedImageIndex = -1;
          }
          OnSelectedImageIndexChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Images"), DefaultValue(0), Description("Pixel gap between start of node and start of image.")]
    public int ImageGapLeft
    {
      get
      {
        return _imageGapLeft;
      }
      set
      {
        if (_imageGapLeft != value)
        {
          _imageGapLeft = value;
          if (_imageGapLeft < 0)
          {
            _imageGapLeft = 0;
          }
          OnImageGapLeftChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Images"), DefaultValue(3), Description("Pixel gap between end of image and start of text.")]
    public int ImageGapRight
    {
      get
      {
        return _imageGapRight;
      }
      set
      {
        if (_imageGapRight != value)
        {
          _imageGapRight = value;
          if (_imageGapRight < 0)
          {
            _imageGapRight = 0;
          }
          OnImageGapRightChanged();
          MarkAllNodeSizesDirty();
          InvalidateNodeDrawing();
        }
      }
    }
    [Category("Nodes"), DefaultValue(typeof(TextRenderingHint), "SystemDefault"), Description("Control how text is rendered.")]
    public TextRenderingHint TextRenderingHint
    {
      get
      {
        return _textRenderingHint;
      }
      set
      {
        if (_textRenderingHint != value)
        {
          _textRenderingHint = value;
          OnTextRenderingHintChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Nodes"), DefaultValue(typeof(Color), "Empty"), Description("Node background color for hot tracking.")]
    public Color HotBackColor
    {
      get
      {
        return _hotBackColor;
      }
      set
      {
        if (_hotBackColor != value)
        {
          _hotBackColor = value;
          ClearNodeCache();
          OnHotBackColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Nodes"), DefaultValue(typeof(Color), "Empty"), Description("Node foreground color for hot tracking.")]
    public Color HotForeColor
    {
      get
      {
        return _hotForeColor;
      }
      set
      {
        if (_hotForeColor != value)
        {
          _hotForeColor = value;
          OnHotForeColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Nodes"), DefaultValue(typeof(Color), "Highlight"), Description("Node background color for selected nodes.")]
    public Color SelectedBackColor
    {
      get
      {
        return _selectedBackColor;
      }
      set
      {
        if (_selectedBackColor != value)
        {
          _selectedBackColor = value;
          ClearNodeCache();
          OnSelectedBackColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Nodes"), DefaultValue(typeof(Color), "Color"), Description("Node background color for selected nodes without focus.")]
    public Color SelectedNoFocusBackColor
    {
      get
      {
        return _selectedNoFocusBackColor;
      }
      set
      {
        if (_selectedNoFocusBackColor != value)
        {
          _selectedNoFocusBackColor = value;
          ClearNodeCache();
          OnSelectedNoFocusBackColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Nodes"), DefaultValue(typeof(Color), "HighlightText"), Description("Node foreground color for selected nodes.")]
    public Color SelectedForeColor
    {
      get
      {
        return _selectedForeColor;
      }
      set
      {
        if (_selectedForeColor != value)
        {
          _selectedForeColor = value;
          OnSelectedForeColorChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Nodes"), DefaultValue(typeof(FontStyle), "Regular"), Description("Node font style to apply when hot tracking.")]
    public FontStyle HotFontStyle
    {
      get
      {
        return _hotFontStyle;
      }
      set
      {
        if (_hotFontStyle != value)
        {
          _hotFontStyle = value;
          OnHotFontStyleChanged();
          MarkAllNodeSizesDirty();
          InvalidateAll();
        }
      }
    }
    [Category("Nodes"), DefaultValue(false), Description("Should the HotFontStyle be used.")]
    public bool UseHotFontStyle
    {
      get
      {
        return _useHotFontStyle;
      }
      set
      {
        if (_useHotFontStyle != value)
        {
          _useHotFontStyle = value;
          OnUseHotFontStyleChanged();
          MarkAllNodeSizesDirty();
          InvalidateAll();
        }
      }
    }
    [Category("Nodes"), DefaultValue(typeof(SelectMode), "Multiple"), Description("Specify how selection operates.")]
    public SelectMode SelectMode
    {
      get
      {
        return _selectMode;
      }
      set
      {
        if (_selectMode != value)
        {
          _selectMode = value;
          switch (_selectMode)
          {
            case SelectMode.None:
              {
                SelectedNodeCollection selectedNodes = SelectedNodes;
                IEnumerator enumerator = selectedNodes.GetEnumerator();
                try
                {
                  while (enumerator.MoveNext())
                  {
                    Node n = (Node)enumerator.Current;
                    DeselectNode(n, false);
                  }
                  goto IL_A6;
                }
                finally
                {
                  IDisposable disposable = enumerator as IDisposable;
                  if (disposable != null)
                  {
                    disposable.Dispose();
                  }
                }
              }
            case SelectMode.Single:
              break;
            default:
              goto IL_A6;
          }
          if (_selected.Count > 1)
          {
            SelectedNodeCollection selectedNodes2 = SelectedNodes;
            for (int i = 1; i < selectedNodes2.Count; i++)
            {
              DeselectNode(selectedNodes2[i], false);
            }
          }
        IL_A6:
          OnSelectModeChanged();
          InvalidateAll();
        }
      }
    }
    [Category("Nodes"), DefaultValue(typeof(FontStyle), "Regular"), Description("Node font style to apply for selected nodes.")]
    public FontStyle SelectedFontStyle
    {
      get
      {
        return _selectedFontStyle;
      }
      set
      {
        if (_selectedFontStyle != value)
        {
          _selectedFontStyle = value;
          OnSelectedFontStyleChanged();
          MarkAllNodeSizesDirty();
          InvalidateAll();
        }
      }
    }
    [Category("Nodes"), DefaultValue(false), Description("Should the SelectedFontStyle be used.")]
    public bool UseSelectedFontStyle
    {
      get
      {
        return _useSelectedFontStyle;
      }
      set
      {
        if (_useSelectedFontStyle != value)
        {
          _useSelectedFontStyle = value;
          OnUseSelectedFontStyleChanged();
          MarkAllNodeSizesDirty();
          InvalidateAll();
        }
      }
    }
    internal Node LabelEditNode
    {
      get
      {
        return _labelEditNode;
      }
    }
    internal Node DragOverNode
    {
      get
      {
        if (_dragEffects != DragDropEffects.None)
        {
          return _dragNode;
        }
        return null;
      }
    }
    internal int SelectedCount
    {
      get
      {
        return _selected.Count;
      }
    }
    internal Size IndicatorSize
    {
      get
      {
        return new Size(13, 13);
      }
    }
    internal Rectangle DrawRectangle
    {
      get
      {
        return _drawRectangle;
      }
    }
    internal Rectangle InnerRectangle
    {
      get
      {
        return _innerRectangle;
      }
    }
    internal Node HotNode
    {
      get
      {
        return _hotNode;
      }
    }
    internal Node TooltipNode
    {
      get
      {
        return _tooltipNode;
      }
    }
    internal Point HotPoint
    {
      get
      {
        return _hotPoint;
      }
      set
      {
        _hotPoint = value;
      }
    }
    internal bool IsControlThemed
    {
      get
      {
        return _themeTreeView.IsControlThemed && _themeCheckbox.IsControlThemed;
      }
    }
    internal ColorDetails ColorDetails
    {
      get
      {
        return _colorDetails;
      }
    }
    internal Size GlyphThemeSize
    {
      get
      {
        return _glyphThemeSize;
      }
    }
    static TreeControl()
    {
      TreeControl._indicatorImages = ResourceHelper.LoadBitmapStrip(typeof(TreeControl), "Crownwood.DotNetMagic.Resources.Indicators.bmp", new Size(13, 13), new Point(0, 0));
    }
    public TreeControl()
    {
      //NAG.NAG_Start();
      base.SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
      base.SetStyle(ControlStyles.StandardDoubleClick, true);
      _themeTreeView = new ThemeHelper(this, "TreeView");
      _themeCheckbox = new ThemeHelper(this, "Button");
      _colorDetails = new ColorDetails();
      _rootNodes = new NodeCollection(this);
      _borderIndent = new IndentPaddingEdges();
      _borderIndent.IndentChanged += new EventHandler(OnBorderIndentChanged);
      _themeTreeView.ThemeOpened += new EventHandler(OnThemeChanged);
      _themeTreeView.ThemeClosed += new EventHandler(OnThemeChanged);
      _themeCheckbox.ThemeOpened += new EventHandler(OnThemeChanged);
      _themeCheckbox.ThemeClosed += new EventHandler(OnThemeChanged);
      _defaultNodeVC = new DefaultNodeVC();
      _defaultCollectionVC = new DefaultCollectionVC();
      _groupNodeVC = new GroupNodeVC();
      _groupCollectionVC = new GroupCollectionVC();
      _dragHoverTimer = new Timer();
      _dragHoverTimer.Interval= 1000;
      _dragHoverTimer.Tick += OnDragHoverTimeout;
      _dragBumpTimer = new Timer();
      _dragBumpTimer.Interval = 250;
      _dragBumpTimer.Tick += OnDragBumpTimeout;
      _autoEditTimer = new Timer();
      _autoEditTimer.Interval = 500;
      _autoEditTimer.Tick += OnAutoEditTimeout;
      _infotipTimer = new Timer();
      _infotipTimer.Interval = 500;
      _infotipTimer.Tick += OnInfoTipTimeout;
      CreateChildControls();
      _offset = Point.Empty;
      _displayNodes = new ArrayList();
      _selected = new Hashtable();
      _nodeKeys = new Hashtable();
      InternalResetFontHeight();
      InternalResetFontBoldItalic();
      InternalResetGroupFontHeight();
      InternalResetGroupFontBoldItalic();
      InternalResetHotPoint();
      InvalidateNodeDrawing();
      InvalidateInnerRectangle();
      ResetAllProperties();
      ResetImageList();
      ResetImageIndex();
      ResetSelectedImageIndex();
      ResetContextMenuNode();
      ResetContextMenuSpace();
      ResetGroupColoring();
      ResetPathSeparator();
      ResetAutoEdit();
      ResetGroupAutoEdit();
      ResetInstantUpdate();
      ResetTooltips();
      ResetInfotips();
      SystemEvents.UserPreferenceChanged += OnPreferenceChanged;
    }
    public void ResetNodeVC()
    {
      NodeVC = null;
    }
    public void ResetCollectionVC()
    {
      CollectionVC = null;
    }
    public void ResetViewControllers()
    {
      ViewControllers = ViewControllers.Default;
    }
    public void DeselectAll()
    {
      IEnumerator enumerator = _selected.Keys.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node n = (Node)enumerator.Current;
          OnAfterDeselect(n);
        }
      }
      finally
      {
        IDisposable disposable = enumerator as IDisposable;
        if (disposable != null)
        {
          disposable.Dispose();
        }
      }
      ClearSelection();
    }
    public void ResetContextMenuNode()
    {
      ContextMenuNode = null;
    }
    public void ResetContextMenuSpace()
    {
      ContextMenuSpace = null;
    }
    public void ResetPathSeparator()
    {
      PathSeparator = "\\";
    }
    public override void ResetBackColor()
    {
      BackColor = SystemColors.Window;
    }
    public void ResetBorderIndent()
    {
      _borderIndent.ResetTop();
      _borderIndent.ResetBottom();
      _borderIndent.ResetLeft();
      _borderIndent.ResetRight();
    }
    public void ResetBorderStyle()
    {
      BorderStyle = TreeBorderStyle.Theme;
    }
    public void ResetBorderColor()
    {
      BorderColor = SystemColors.WindowText;
    }
    public void ResetLineWidth()
    {
      LineWidth = 1;
    }
    public void ResetLineColor()
    {
      LineColor = SystemColors.GrayText;
    }
    public void ResetLineDashStyle()
    {
      LineDashStyle = LineDashStyle.Dot;
    }
    public void ResetBoxLength()
    {
      BoxLength = 9;
    }
    public void ResetBoxDrawStyle()
    {
      BoxDrawStyle = DrawStyle.Themed;
    }
    public void ResetBoxShownAlways()
    {
      BoxShownAlways = false;
    }
    public void ResetBoxSignColor()
    {
      BoxSignColor = SystemColors.WindowText;
    }
    public void ResetBoxBorderColor()
    {
      BoxBorderColor = SystemColors.GrayText;
    }
    public void ResetBoxInsideColor()
    {
      BoxInsideColor = SystemColors.Window;
    }
    public void ResetBoxVisibility()
    {
      BoxVisibility = LineBoxVisibility.Everywhere;
    }
    public void ResetLineVisibility()
    {
      LineVisibility = LineBoxVisibility.Everywhere;
    }
    public void ResetColumnWidth()
    {
      ColumnWidth = 19;
    }
    public void ResetCheckStates()
    {
      CheckStates = CheckStates.None;
    }
    public void ResetCheckLength()
    {
      CheckLength = 13;
    }
    public void ResetCheckGapLeft()
    {
      CheckGapLeft = 1;
    }
    public void ResetCheckGapRight()
    {
      CheckGapRight = 3;
    }
    public void ResetCheckDrawStyle()
    {
      CheckDrawStyle = DrawStyle.Themed;
    }
    public void ResetCheckBorderWidth()
    {
      CheckBorderWidth = 1;
    }
    public void ResetCheckBorderColor()
    {
      CheckBorderColor = SystemColors.ControlDarkDark;
    }
    public void ResetCheckInsideColor()
    {
      CheckInsideColor = SystemColors.Window;
    }
    public void ResetCheckInsideHotColor()
    {
      CheckInsideHotColor = SystemColors.Window;
    }
    public void ResetCheckTickColor()
    {
      CheckTickColor = SystemColors.ControlText;
    }
    public void ResetCheckTickHotColor()
    {
      CheckTickHotColor = SystemColors.ControlText;
    }
    public void ResetCheckMixedColor()
    {
      CheckMixedColor = SystemColors.ControlText;
    }
    public void ResetCheckMixedHotColor()
    {
      CheckMixedHotColor = SystemColors.ControlText;
    }
    public void ResetVerticalGranularity()
    {
      VerticalGranularity = VerticalGranularity.Node;
    }
    public void ResetVerticalScrollbar()
    {
      VerticalScrollbar = ScrollVisibility.WhenNeeded;
    }
    public void ResetHorizontalScrollbar()
    {
      HorizontalScrollbar = ScrollVisibility.WhenNeeded;
    }
    public void ResetEnableMouseWheel()
    {
      EnableMouseWheel = true;
    }
    public void ResetGroupNodesSelectable()
    {
      GroupNodesSelectable = true;
    }
    public void ResetGroupTextRenderingHint()
    {
      GroupTextRenderingHint = TextRenderingHint.AntiAlias;
    }
    private bool ShouldSerializeGroupFont()
    {
      return !GroupFont.Equals(new Font(SystemInformation.MenuFont, 0));
    }
    public void ResetGroupFont()
    {
      GroupFont = new Font(SystemInformation.MenuFont, 0);
    }
    public void ResetGroupAutoEdit()
    {
      GroupAutoEdit = false;
    }
    public void ResetGroupArrows()
    {
      GroupArrows = false;
    }
    public void ResetGroupIndentLeft()
    {
      GroupIndentLeft = 2;
    }
    public void ResetGroupIndentTop()
    {
      GroupIndentTop = 5;
    }
    public void ResetGroupIndentBottom()
    {
      GroupIndentBottom = 5;
    }
    public void ResetGroupColoring()
    {
      GroupColoring = GroupColoring.ControlProperties;
    }
    public void ResetGroupBackColor()
    {
      GroupBackColor = SystemColors.ControlDark;
    }
    public void ResetGroupForeColor()
    {
      GroupForeColor = SystemColors.ControlText;
    }
    public void ResetGroupLineColor()
    {
      GroupLineColor = SystemColors.ControlText;
    }
    public void ResetGroupHotBackColor()
    {
      GroupHotBackColor = SystemColors.HotTrack;
    }
    public void ResetGroupSelectedBackColor()
    {
      GroupSelectedBackColor = SystemColors.Highlight;
    }
    public void ResetGroupSelectedNoFocusBackColor()
    {
      GroupSelectedNoFocusBackColor = SystemColors.Highlight;
    }
    public void ResetGroupHotForeColor()
    {
      GroupHotForeColor = SystemColors.HighlightText;
    }
    public void ResetGroupSelectedForeColor()
    {
      GroupSelectedForeColor = SystemColors.HighlightText;
    }
    public void ResetGroupExpandOnDragHover()
    {
      GroupExpandOnDragHover = true;
    }
    public void ResetGroupImageBoxLineColor()
    {
      GroupImageBoxLineColor = SystemColors.InfoText;
    }
    public void ResetGroupImageBoxBackColor()
    {
      GroupImageBoxBackColor = SystemColors.Info;
    }
    public void ResetGroupImageBoxSelectedBackColor()
    {
      GroupImageBoxSelectedBackColor = SystemColors.Info;
    }
    public void ResetGroupImageBoxColumnColor()
    {
      GroupImageBoxColumnColor = SystemColors.Control;
    }
    public void ResetGroupImageBoxGradientBack()
    {
      GroupImageBoxGradientBack = false;
    }
    public void ResetGroupImageBoxGradientAngle()
    {
      GroupImageBoxGradientAngle = GradientDirection.TopToBottom;
    }
    public void ResetGroupImageBoxGradientColoring()
    {
      GroupImageBoxGradientColoring = TreeGradientColoring.VeryLightToColor;
    }
    public void ResetGroupBorderStyle()
    {
      GroupBorderStyle = GroupBorderStyle.AllEdges;
    }
    public void ResetGroupHotFontStyle()
    {
      GroupHotFontStyle = 0;
    }
    public void ResetGroupUseHotFontStyle()
    {
      GroupUseHotFontStyle = false;
    }
    public void ResetGroupSelectedFontStyle()
    {
      GroupSelectedFontStyle = 0;
    }
    public void ResetGroupUseSelectedFontStyle()
    {
      GroupUseSelectedFontStyle = false;
    }
    public void ResetGroupGradientAngle()
    {
      GroupGradientAngle = GradientDirection.TopToBottom;
    }
    public void ResetGroupGradientColoring()
    {
      GroupGradientColoring = TreeGradientColoring.VeryLightToColor;
    }
    public void ResetGroupGradientBack()
    {
      GroupGradientBack = true;
    }
    public void ResetGroupExtraLeft()
    {
      GroupExtraLeft = 3;
    }
    public void ResetGroupExtraHeight()
    {
      GroupExtraHeight = 5;
    }
    public void ResetGroupHotTrack()
    {
      GroupHotTrack = true;
    }
    public void ResetGroupClickExpand()
    {
      GroupClickExpand = ClickExpandAction.Toggle;
    }
    public void ResetGroupDoubleClickExpand()
    {
      GroupDoubleClickExpand = ClickExpandAction.Expand;
    }
    public void ResetGroupAutoCollapse()
    {
      GroupAutoCollapse = false;
    }
    public void ResetGroupAutoAllocate()
    {
      GroupAutoAllocate = false;
    }
    public void ResetGroupImageBox()
    {
      GroupImageBox = false;
    }
    public void ResetGroupImageBoxColumn()
    {
      GroupImageBoxColumn = true;
    }
    public void ResetGroupImageBoxBorder()
    {
      GroupImageBoxBorder = true;
    }
    public void ResetGroupImageBoxWidth()
    {
      GroupImageBoxWidth = 20;
    }
    public void ResetGroupImageBoxGap()
    {
      GroupImageBoxGap = 6;
    }
    public void ResetClickExpand()
    {
      ClickExpand = ClickExpandAction.None;
    }
    public void ResetDoubleClickExpand()
    {
      DoubleClickExpand = ClickExpandAction.Toggle;
    }
    public void ResetAutoCollapse()
    {
      AutoCollapse = false;
    }
    public void ResetExtendToRight()
    {
      ExtendToRight = false;
    }
    public void ResetInstantUpdate()
    {
      InstantUpdate = false;
    }
    public void ResetTooltips()
    {
      Tooltips = true;
    }
    public void ResetInfotips()
    {
      Infotips = true;
    }
    public void ResetNodesSelectable()
    {
      NodesSelectable = true;
    }
    public void ResetAutoEdit()
    {
      AutoEdit = true;
    }
    public void ResetLabelEdit()
    {
      LabelEdit = true;
    }
    public void ResetCanUserExpandCollapse()
    {
      CanUserExpandCollapse = true;
    }
    public void ResetExpandOnDragHover()
    {
      ExpandOnDragHover = true;
    }
    public void ResetIndicators()
    {
      Indicators = Indicators.None;
    }
    public void ResetVerticalNodeGap()
    {
      VerticalNodeGap = 0;
    }
    private bool ShouldSerializeMinimumNodeHeight()
    {
      return MinimumNodeHeight != Font.Height;
    }
    public void ResetMinimumNodeHeight()
    {
      MinimumNodeHeight = Font.Height;
    }
    public void ResetMaximumNodeHeight()
    {
      MaximumNodeHeight = 9999;
    }
    public void ResetImageList()
    {
      ImageList = null;
    }
    public void ResetImageIndex()
    {
      ImageIndex = -1;
    }
    public void ResetSelectedImageIndex()
    {
      SelectedImageIndex = -1;
    }
    public void ResetImageGapLeft()
    {
      ImageGapLeft = 0;
    }
    public void ResetImageGapRight()
    {
      ImageGapRight = 3;
    }
    public void ResetTextRenderingHint()
    {
      TextRenderingHint = 0;
    }
    public void ResetHotBackColor()
    {
      HotBackColor = Color.Empty;
    }
    public void ResetHotForeColor()
    {
      HotForeColor = Color.Empty;
    }
    public void ResetSelectedBackColor()
    {
      SelectedBackColor = SystemColors.Highlight;
    }
    public void ResetSelectedNoFocusBackColor()
    {
      SelectedNoFocusBackColor = SystemColors.Control;
    }
    public void ResetSelectedForeColor()
    {
      SelectedForeColor = SystemColors.HighlightText;
    }
    public void ResetHotFontStyle()
    {
      HotFontStyle = 0;
    }
    public void ResetUseHotFontStyle()
    {
      UseHotFontStyle = false;
    }
    public void ResetSelectMode()
    {
      SelectMode = SelectMode.Multiple;
    }
    public void ResetSelectedFontStyle()
    {
      SelectedFontStyle = 0;
    }
    public void ResetUseSelectedFontStyle()
    {
      UseSelectedFontStyle = false;
    }
    public void Expand()
    {
      IEnumerator enumerator = Nodes.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node = (Node)enumerator.Current;
          node.Expand();
        }
      }
      finally
      {
        IDisposable disposable = enumerator as IDisposable;
        if (disposable != null)
        {
          disposable.Dispose();
        }
      }
      InvalidateNodeDrawing();
    }
    public void ExpandAll()
    {
      IEnumerator enumerator = Nodes.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node = (Node)enumerator.Current;
          node.ExpandAll();
        }
      }
      finally
      {
        IDisposable disposable = enumerator as IDisposable;
        if (disposable != null)
        {
          disposable.Dispose();
        }
      }
      InvalidateNodeDrawing();
    }
    public void Collapse()
    {
      IEnumerator enumerator = Nodes.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node = (Node)enumerator.Current;
          node.Collapse();
        }
      }
      finally
      {
        IDisposable disposable = enumerator as IDisposable;
        if (disposable != null)
        {
          disposable.Dispose();
        }
      }
      InvalidateNodeDrawing();
    }
    public void CollapseAll()
    {
      IEnumerator enumerator = Nodes.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node = (Node)enumerator.Current;
          node.CollapseAll();
        }
      }
      finally
      {
        IDisposable disposable = enumerator as IDisposable;
        if (disposable != null)
        {
          disposable.Dispose();
        }
      }
      InvalidateNodeDrawing();
    }
    public void SetTreeControlStyle(TreeControlStyles style)
    {
      switch (style)
      {
        case TreeControlStyles.StandardPlain:
          ResetAllProperties();
          BoxDrawStyle = DrawStyle.Plain;
          CheckDrawStyle = DrawStyle.Plain;
          BorderStyle = TreeBorderStyle.Solid;
          BorderIndent.Top = 1;
          BorderIndent.Left = 1;
          BorderIndent.Bottom = 1;
          BorderIndent.Right = 1;
          return;
        case TreeControlStyles.StandardThemed:
          ResetAllProperties();
          return;
        case TreeControlStyles.Explorer:
          ResetAllProperties();
          DoubleClickExpand = ClickExpandAction.Expand;
          HotFontStyle = FontStyle.Underline;
          UseHotFontStyle = true;
          HotForeColor = SystemColors.HotTrack;
          ClickExpand = ClickExpandAction.Expand;
          LineVisibility = LineBoxVisibility.Nowhere;
          SelectMode = SelectMode.Single;
          return;
        case TreeControlStyles.Navigator:
          ResetAllProperties();
          ViewControllers = ViewControllers.Group;
          BorderStyle = TreeBorderStyle.None;
          BoxDrawStyle = DrawStyle.Plain;
          VerticalGranularity = VerticalGranularity.Pixel;
          GroupClickExpand = ClickExpandAction.Expand;
          GroupAutoCollapse = true;
          GroupAutoAllocate = true;
          GroupNodesSelectable = false;
          GroupFont = new Font(Font.Name, Font.Size + 3f, FontStyle.Bold);
          GroupGradientColoring = TreeGradientColoring.VeryLightToColor;
          GroupBackColor = SystemColors.ActiveCaption;
          GroupForeColor = SystemColors.ActiveCaptionText;
          GroupSelectedBackColor = SystemColors.Info;
          GroupSelectedNoFocusBackColor = SystemColors.Info;
          GroupSelectedForeColor = SystemColors.InfoText;
          GroupSelectedFontStyle = FontStyle.Bold;
          GroupHotFontStyle = FontStyle.Bold;
          GroupImageBox = true;
          GroupImageBoxBackColor = SystemColors.InactiveCaption;
          GroupImageBoxSelectedBackColor = SystemColors.InactiveCaption;
          GroupImageBoxWidth = (int)((double)GroupFont.Height * 1.5);
          GroupImageBoxLineColor = SystemColors.ControlText;
          GroupImageBoxGradientBack = true;
          return;
        case TreeControlStyles.Group:
          ResetAllProperties();
          BorderStyle = TreeBorderStyle.Solid;
          BoxDrawStyle = DrawStyle.Themed;
          ViewControllers = ViewControllers.Group;
          VerticalGranularity = VerticalGranularity.Pixel;
          GroupFont = new Font(Font.Name, Font.Size + 1f, FontStyle.Bold);
          GroupBorderStyle = GroupBorderStyle.BottomEdge;
          GroupSelectedFontStyle = FontStyle.Bold;
          GroupHotFontStyle = FontStyle.Bold;
          GroupArrows = true;
          return;
        case TreeControlStyles.GroupOfficeLight:
          ResetAllProperties();
          BorderStyle = TreeBorderStyle.Solid;
          BoxDrawStyle = DrawStyle.Themed;
          ViewControllers = ViewControllers.Group;
          VerticalGranularity = VerticalGranularity.Pixel;
          GroupFont = new Font(SystemInformation.MenuFont, 0);
          GroupForeColor = Color.Black;
          GroupBorderStyle = GroupBorderStyle.VerticalEdges;
          GroupSelectedForeColor = Color.Black;
          GroupSelectedFontStyle = FontStyle.Bold;
          GroupHotFontStyle = FontStyle.Bold;
          GroupColoring = GroupColoring.Office2003Light;
          GroupTextRenderingHint = 0;
          GroupHotTrack = false;
          GroupArrows = true;
          return;
        case TreeControlStyles.GroupOfficeDark:
          ResetAllProperties();
          BorderStyle = TreeBorderStyle.Solid;
          BoxDrawStyle = DrawStyle.Themed;
          ViewControllers = ViewControllers.Group;
          VerticalGranularity = VerticalGranularity.Pixel;
          GroupFont = new Font("Tahoma", 11f, FontStyle.Bold);
          if (ThemeHelper.IsAppThemed)
          {
            GroupForeColor = Color.White;
          }
          else
          {
            GroupForeColor = SystemColors.ControlText;
          }
          GroupSelectedForeColor = Color.Black;
          GroupBorderStyle = GroupBorderStyle.VerticalEdges;
          GroupSelectedFontStyle = FontStyle.Bold;
          GroupHotFontStyle = FontStyle.Bold;
          GroupColoring = GroupColoring.Office2003Dark;
          GroupHotTrack = false;
          GroupArrows = true;
          return;
        case TreeControlStyles.List:
          ResetAllProperties();
          ColumnWidth = 0;
          BorderIndent.Top = 1;
          BorderIndent.Left = 1;
          BorderIndent.Bottom = 1;
          BorderIndent.Right = 1;
          ClickExpand = ClickExpandAction.None;
          ExtendToRight = true;
          CanUserExpandCollapse = false;
          return;
        default:
          return;
      }
    }
    public void InvalidateNodeDrawing()
    {
      _nodeDrawingValid = false;
      _scrollBarsValid = false;
      if (InstantUpdate)
      {
        Refresh();
      }
      InvalidateAll();
    }
    public void InvalidateNode(Node n)
    {
      InvalidateNode(n, false);
    }
    public void InvalidateNode(Node n, bool recurse)
    {
      if (n.Visible)
      {
        Rectangle rectangle = recurse ? n.Cache.ChildBounds : n.Cache.Bounds;
        rectangle = NodeSpaceToClient(rectangle);
        rectangle.Inflate(2, 2);
        rectangle.Width = base.Width - rectangle.Left;
        base.Invalidate(rectangle);
      }
    }
    public void InvalidateNodeLine(Node n, bool recurse)
    {
      if (n.Visible)
      {
        Rectangle rectangle;
        if (recurse)
        {
          rectangle = NodeSpaceToClient(n.Cache.ChildBounds);
        }
        else
        {
          rectangle = NodeSpaceToClient(n.Cache.Bounds);
        }
        rectangle.X = 0;
        rectangle.Width = base.Width;
        base.Invalidate(rectangle);
      }
    }
    public void InvalidateNodeCollection(NodeCollection nc)
    {
      base.Invalidate(NodeSpaceToClient(nc.Cache.Bounds));
    }
    public Point ClientPointToNode(Point point)
    {
      return ClientToNodeSpace(point);
    }
    public Rectangle ClientRectToNode(Rectangle rect)
    {
      return ClientToNodeSpace(rect);
    }
    public Point NodePointToClient(Point point)
    {
      return NodeSpaceToClient(point);
    }
    public Rectangle NodeRectToClient(Rectangle rect)
    {
      return NodeSpaceToClient(rect);
    }
    public bool EnsureDisplayed(Node n)
    {
      return EnsureDisplayed(n, true, true);
    }
    public bool EnsureDisplayed(Node n, bool expand, bool visible)
    {
      if (!EnsureParentChain(n, expand, visible))
      {
        return false;
      }
      if (FindDisplayIndex(n) == -1)
      {
        using (Graphics graphics = base.CreateGraphics())
        {
          CalculationCycle(graphics);
        }
      }
      Rectangle rectangle = NodeSpaceToClient(n.Cache.Bounds);
      bool flag = false;
      bool flag2 = rectangle.Left < _drawRectangle.Left;
      bool flag3 = rectangle.Right > _drawRectangle.Right;
      bool flag4 = rectangle.Top < _drawRectangle.Top;
      bool flag5 = rectangle.Bottom > _drawRectangle.Bottom;
      if (flag2 || flag3)
      {
        if (rectangle.Width > _drawRectangle.Width)
        {
          flag2 = true;
        }
        if (flag2)
        {
          if (n.Cache.Bounds.Right < _drawRectangle.Width)
          {
            _offset.X = 0;
          }
          else
          {
            _offset.X = _offset.X - (_drawRectangle.Left - rectangle.Left);
          }
          flag = true;
        }
        else if (flag3)
        {
          _offset.X = _offset.X + (rectangle.Right - _drawRectangle.Right);
          flag = true;
        }
      }
      else if (n.Cache.Bounds.Right < _drawRectangle.Width)
      {
        _offset.X = 0;
        flag = true;
      }
      if (flag4 || flag5)
      {
        if (rectangle.Height > _drawRectangle.Height)
        {
          flag4 = true;
        }
        if (flag4)
        {
          if (VerticalGranularity == VerticalGranularity.Pixel)
          {
            _offset.Y = _offset.Y - (_drawRectangle.Top - rectangle.Top);
          }
          else
          {
            _offset.Y = FindDisplayIndex(n);
          }
          flag = true;
        }
        else if (flag5)
        {
          if (VerticalGranularity == VerticalGranularity.Pixel)
          {
            _offset.Y = _offset.Y + (rectangle.Bottom - _drawRectangle.Bottom);
          }
          else
          {
            int index = FindDisplayIndex(n);
            _offset.Y = InsidePageUpIndex(index, _drawRectangle);
          }
          flag = true;
        }
      }
      if (flag)
      {
        SetTooltipNode(null);
        bool scrollBarsValid = _scrollBarsValid;
        _scrollBarsValid = false;
        UpdateScrollBars();
        _scrollBarsValid = scrollBarsValid;
      }
      return true;
    }
    public void SetTopNode(Node n, bool expand, bool visible)
    {
      if (!EnsureParentChain(n, expand, visible))
      {
        return;
      }
      if (FindDisplayIndex(n) == -1)
      {
        using (Graphics graphics = base.CreateGraphics())
        {
          CalculationCycle(graphics);
        }
      }
      Rectangle rectangle = NodeSpaceToClient(n.Cache.Bounds);
      bool flag = false;
      bool flag2 = rectangle.Left < _drawRectangle.Left;
      bool flag3 = rectangle.Right > _drawRectangle.Right;
      int arg_76_0 = rectangle.Top;
      int arg_82_0 = _drawRectangle.Top;
      int arg_8A_0 = rectangle.Bottom;
      int arg_96_0 = _drawRectangle.Bottom;
      if (flag2 || flag3)
      {
        if (rectangle.Width > _drawRectangle.Width)
        {
          flag2 = true;
        }
        if (flag2)
        {
          if (n.Cache.Bounds.Right < _drawRectangle.Width)
          {
            _offset.X = 0;
          }
          else
          {
            _offset.X = _offset.X - (_drawRectangle.Left - rectangle.Left);
          }
          flag = true;
        }
        else if (flag3)
        {
          _offset.X = _offset.X + (rectangle.Right - _drawRectangle.Right);
          flag = true;
        }
      }
      else if (n.Cache.Bounds.Right < _drawRectangle.Width)
      {
        _offset.X = 0;
        flag = true;
      }
      if (VerticalGranularity == VerticalGranularity.Pixel)
      {
        if (_offset.Y != rectangle.Top)
        {
          _offset.Y = rectangle.Top;
          flag = true;
        }
      }
      else
      {
        int num = FindDisplayIndex(n);
        if (_offset.Y != num)
        {
          _offset.Y = num;
          flag = true;
        }
      }
      if (flag)
      {
        SetTooltipNode(null);
        bool scrollBarsValid = _scrollBarsValid;
        _scrollBarsValid = false;
        UpdateScrollBars();
        _scrollBarsValid = scrollBarsValid;
      }
    }
    public Node GetNodeAt(int y)
    {
      return FindDisplayNodeFromY(y);
    }
    public Node GetNodeAt(int x, int y)
    {
      return GetNodeAt(new Point(x, y));
    }
    public Node GetNodeAt(Point pt)
    {
      return FindDisplayNodeFromPoint(pt);
    }
    public Node GetFirstNode()
    {
      return Nodes.GetFirstNode();
    }
    public Node GetFirstDisplayedNode()
    {
      return Nodes.GetFirstDisplayedNode();
    }
    public Node GetLastNode()
    {
      return Nodes.GetLastNode();
    }
    public Node GetLastDisplayedNode()
    {
      return Nodes.GetLastDisplayedNode();
    }
    public Node GetNodeFromKey(object key)
    {
      if (key == null)
      {
        return null;
      }
      return _nodeKeys[key] as Node;
    }
    public virtual bool OnShowContextMenuNode(Node n)
    {
      CancelNodeEventArgs cancelNodeEventArgs = new CancelNodeEventArgs(n);
      if (ShowContextMenuNode != null)
      {
        ShowContextMenuNode(this, cancelNodeEventArgs);
      }
      return !cancelNodeEventArgs.Cancel;
    }
    public virtual void OnClientDragEnter(DragEventArgs e)
    {
      if (ClientDragEnter != null)
      {
        ClientDragEnter.Invoke(this, e);
      }
    }
    public virtual void OnClientDragLeave(EventArgs e)
    {
      if (ClientDragLeave != null)
      {
        ClientDragLeave.Invoke(this, e);
      }
    }
    public virtual void OnClientDragOver(DragEventArgs e)
    {
      if (ClientDragOver != null)
      {
        ClientDragOver.Invoke(this, e);
      }
    }
    public virtual void OnClientDragDrop(DragEventArgs e)
    {
      if (ClientDragDrop != null)
      {
        ClientDragDrop.Invoke(this, e);
      }
    }
    public virtual void OnNodeDragEnter(Node n, DragEventArgs e)
    {
      if (NodeDragEnter != null)
      {
        NodeDragEnter(this, n, e);
      }
    }
    public virtual void OnNodeDragLeave(Node n)
    {
      if (NodeDragLeave != null)
      {
        NodeDragLeave(this, new NodeEventArgs(n));
      }
    }
    public virtual void OnNodeDragOver(Node n, DragEventArgs e)
    {
      if (NodeDragOver != null)
      {
        NodeDragOver(this, n, e);
      }
    }
    public virtual void OnNodeDragDrop(Node n, DragEventArgs e)
    {
      if (NodeDragDrop != null)
      {
        NodeDragDrop(this, n, e);
      }
    }
    public virtual void OnNodeDrag(StartDragEventArgs e)
    {
      if (NodeDrag != null)
      {
        NodeDrag(this, e);
      }
    }
    public virtual void OnGroupDragEnter(Node n, DragEventArgs e)
    {
      if (GroupDragEnter != null)
      {
        GroupDragEnter(this, n, e);
      }
    }
    public virtual void OnGroupDragLeave(Node n)
    {
      if (GroupDragLeave != null)
      {
        GroupDragLeave(this, new NodeEventArgs(n));
      }
    }
    public virtual void OnGroupDragOver(Node n, DragEventArgs e)
    {
      if (GroupDragOver != null)
      {
        GroupDragOver(this, n, e);
      }
    }
    public virtual void OnGroupDragDrop(Node n, DragEventArgs e)
    {
      if (GroupDragDrop != null)
      {
        GroupDragDrop(this, n, e);
      }
    }
    public virtual bool OnBeforeSelect(Node n)
    {
      CancelNodeEventArgs cancelNodeEventArgs = new CancelNodeEventArgs(n);
      if (BeforeSelect != null)
      {
        BeforeSelect(this, cancelNodeEventArgs);
      }
      return !cancelNodeEventArgs.Cancel;
    }
    public virtual void OnAfterSelect(Node n)
    {
      NodeEventArgs e = new NodeEventArgs(n);
      if (AfterSelect != null)
      {
        AfterSelect(this, e);
      }
    }
    public virtual void OnAfterDeselect(Node n)
    {
      NodeEventArgs e = new NodeEventArgs(n);
      if (AfterDeselect != null)
      {
        AfterDeselect(this, e);
      }
    }
    public virtual bool OnBeforeCheck(Node n)
    {
      CancelNodeEventArgs cancelNodeEventArgs = new CancelNodeEventArgs(n);
      if (BeforeCheck != null)
      {
        BeforeCheck(this, cancelNodeEventArgs);
      }
      return !cancelNodeEventArgs.Cancel;
    }
    public virtual void OnAfterCheck(Node n)
    {
      NodeEventArgs e = new NodeEventArgs(n);
      if (AfterCheck != null)
      {
        AfterCheck(this, e);
      }
    }
    public virtual bool OnBeforeExpand(Node n)
    {
      CancelNodeEventArgs cancelNodeEventArgs = new CancelNodeEventArgs(n);
      if (BeforeExpand != null)
      {
        BeforeExpand(this, cancelNodeEventArgs);
      }
      return !cancelNodeEventArgs.Cancel;
    }
    public virtual void OnAfterExpand(Node n)
    {
      NodeEventArgs e = new NodeEventArgs(n);
      if (AfterExpand != null)
      {
        AfterExpand(this, e);
      }
    }
    public virtual bool OnBeforeCollapse(Node n)
    {
      CancelNodeEventArgs cancelNodeEventArgs = new CancelNodeEventArgs(n);
      if (BeforeCollapse != null)
      {
        BeforeCollapse(this, cancelNodeEventArgs);
      }
      return !cancelNodeEventArgs.Cancel;
    }
    public virtual void OnAfterCollapse(Node n)
    {
      NodeEventArgs e = new NodeEventArgs(n);
      if (AfterCollapse != null)
      {
        AfterCollapse(this, e);
      }
    }
    public virtual bool OnBeforeLabelEdit(Node n, ref string label)
    {
      LabelEditEventArgs labelEditEventArgs = new LabelEditEventArgs(n.Index, label);
      if (BeforeLabelEdit != null)
      {
        BeforeLabelEdit(this, labelEditEventArgs);
      }
      label = labelEditEventArgs.Label;
      return !labelEditEventArgs.CancelEdit;
    }
    public virtual bool OnAfterLabelEdit(Node n, ref string label)
    {
      LabelEditEventArgs labelEditEventArgs = new LabelEditEventArgs(n.Index, label);
      if (AfterLabelEdit != null)
      {
        AfterLabelEdit(this, labelEditEventArgs);
      }
      label = labelEditEventArgs.Label;
      return !labelEditEventArgs.CancelEdit;
    }
    public virtual bool OnShowContextMenuSpace()
    {
      CancelEventArgs cancelEventArgs = new CancelEventArgs();
      if (ShowContextMenuSpace != null)
      {
        ShowContextMenuSpace.Invoke(this, cancelEventArgs);
      }
      return !cancelEventArgs.Cancel;
    }
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        ClearLineBoxCache();
        RemoveAnyToolTip();
        _dragBumpTimer.Stop();
        _dragHoverTimer.Stop();
        _autoEditTimer.Stop();
        _infotipTimer.Stop();
        _dragBumpTimer.Dispose();
        _dragHoverTimer.Dispose();
        _autoEditTimer.Dispose();
        _infotipTimer.Dispose();
        _vBar.ValueChanged -= OnVertValueChanged;
        _hBar.ValueChanged -= OnHorzValueChanged;
        _themeTreeView.ThemeOpened -= OnThemeChanged;
        _themeTreeView.ThemeClosed -= OnThemeChanged;
        _themeCheckbox.ThemeOpened -= OnThemeChanged;
        _themeCheckbox.ThemeClosed -= OnThemeChanged;
        _borderIndent.IndentChanged -= OnBorderIndentChanged;
        base.Controls.Remove(_corner);
        base.Controls.Remove(_hBar);
        base.Controls.Remove(_vBar);
        SystemEvents.UserPreferenceChanged -= OnPreferenceChanged;
        _colorDetails.Dispose();
        _themeTreeView.Dispose();
        _themeCheckbox.Dispose();
      }
      base.Dispose(disposing);
    }
    protected override void OnSizeChanged(EventArgs e)
    {
      NodeVC.SizeChanged(this);
      CollectionVC.SizeChanged(this);
      InvalidateInnerRectangle();
      base.OnSizeChanged(e);
    }
    protected override void OnFontChanged(EventArgs e)
    {
      InternalResetFontHeight();
      InternalResetFontBoldItalic();
      ResetMinimumNodeHeight();
      MarkAllNodeSizesDirty();
      base.OnFontChanged(e);
    }
    protected override void OnGotFocus(EventArgs e)
    {
      if (_focusNode == null)
      {
        Node firstDisplayedNode = Nodes.GetFirstDisplayedNode();
        if (firstDisplayedNode != null)
        {
          if (firstDisplayedNode.VC.CanSelectNode(this, firstDisplayedNode))
          {
            SingleSelect(firstDisplayedNode);
          }
          else
          {
            SetFocusNode(firstDisplayedNode);
          }
        }
      }
      InvalidateSelection();
      InvalidateFocus();
      base.OnGotFocus(e);
    }
    protected override void OnLostFocus(EventArgs e)
    {
      InvalidateSelection();
      InvalidateFocus();
      base.OnLostFocus(e);
    }
    protected override void OnEnabledChanged(EventArgs e)
    {
      InvalidateAll();
      base.OnEnabledChanged(e);
    }
    protected override bool ProcessDialogKey(Keys keyData)
    {
      bool flag = false;
      Keys keys = keyData & ~(Keys.Shift|Keys.Control);
      bool shiftKey = (keyData & Keys.Shift) == Keys.Shift;
      bool controlKey = (keyData & Keys.Control) == Keys.Control;
      if (keys == Keys.Up)
      {
        flag = ProcessUpKey(shiftKey, controlKey);
      }
      else if (keys == Keys.Down)
      {
        flag = ProcessDownKey(shiftKey, controlKey);
      }
      else if (keys == Keys.Left)
      {
        flag = ProcessLeftKey(shiftKey, controlKey);
      }
      else if (keys == Keys.Right)
      {
        flag = ProcessRightKey(shiftKey, controlKey);
      }
      else if (keys == Keys.Home)
      {
        flag = ProcessHomeKey(shiftKey, controlKey);
      }
      else if (keys == Keys.End)
      {
        flag = ProcessEndKey(shiftKey, controlKey);
      }
      else if (keys == Keys.PageUp)
      {
        flag = ProcessPageUpKey(shiftKey, controlKey);
      }
      else if (keys == Keys.PageDown)
      {
        flag = ProcessPageDownKey(shiftKey, controlKey);
      }
      else if (keys == Keys.Add)
      {
        flag = ProcessPlusKey();
      }
      else if (keys == Keys.Multiply)
      {
        flag = ProcessPlusKey();
      }
      else if (keys == Keys.Subtract)
      {
        flag = ProcessMinusKey();
      }
      if (!flag)
      {
        flag = base.ProcessDialogKey(keyData);
      }
      return flag;
    }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
      if (EnableMouseWheel && _vBar.Enabled && _vBar.Visible)
      {
        int num = e.Delta / 120;
        int num2 = _vBar.Value - num;
        int num3 = _vBar.Maximum - _vBar.LargeChange + 1;
        if (num2 > num3)
        {
          num2 = num3;
        }
        if (num2 < _vBar.Minimum)
        {
          num2 = _vBar.Minimum;
        }
        if (_vBar.Value != num2)
        {
          _vBar.Value = num2;
        }
      }
      base.OnMouseWheel(e);
    }
    protected override void OnMouseEnter(EventArgs e)
    {
      base.OnMouseEnter(e);
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {
      base.Focus();
      if (base.ContainsFocus)
      {
        Point point = new Point(e.X, e.Y);
        _mouseDownNode = null;
        _mouseDownPt = Point.Empty;
        HotPoint = ClientToNodeSpace(point);
        Node node = FindDisplayNodeFromY(point.Y);
        bool flag = false;
        if (node != null && node.VC != null)
        {
          if (!node.VC.MouseDown(this, node, e.Button, HotPoint))
          {
            NodeCollection parentNodes = node.ParentNodes;
            while (parentNodes.VC == null || !parentNodes.VC.MouseDown(this, parentNodes, node, e.Button, HotPoint))
            {
              if (parentNodes.ParentNode == null)
              {
                goto IL_E1;
              }
              parentNodes = parentNodes.ParentNode.ParentNodes;
              node = null;
            }
            flag = true;
          }
          else
          {
            flag = true;
            if (e.Button == MouseButtons.Left)
            {
              _mouseDownNode = node;
              _mouseDownPt = point;
            }
          }
        }
      IL_E1:
        if (!flag && e.Button == MouseButtons.Right && OnShowContextMenuSpace() && ContextMenuSpace != null)
        {
          ContextMenuSpace.Show(this, point);
        }
      }
      base.OnMouseDown(e);
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
      Point point = new Point(e.X, e.Y);
      _mouseDownNode = null;
      _mouseDownPt = Point.Empty;
      Node node = FindDisplayNodeFromY(point.Y);
      if (node != null && node.VC != null && !node.VC.MouseUp(this, node, e.Button, HotPoint) && e.Button == MouseButtons.Right && OnShowContextMenuSpace() && ContextMenuSpace != null)
      {
        ContextMenuSpace.Show(this, point);
      }
      base.OnMouseUp(e);
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
      Point point = new Point(e.X, e.Y);
      bool flag = false;
      if (_mouseDownNode != null)
      {
        Rectangle rectangle = new Rectangle(_mouseDownPt, Size.Empty);
        rectangle.Inflate(SystemInformation.DragSize);
        if (!rectangle.Contains(point))
        {
          StartDragEventArgs startDragEventArgs = new StartDragEventArgs(_mouseDownNode, DragDropEffects.All);
          OnNodeDrag(startDragEventArgs);
          if (!startDragEventArgs.Cancel)
          {
            RemoveAnyToolTip();
            flag = true;
            base.DoDragDrop(startDragEventArgs.Object, startDragEventArgs.Effect);
          }
          _mouseDownNode = null;
          _mouseDownPt = Point.Empty;
        }
      }
      HotPoint = ClientToNodeSpace(point);
      Form form = base.FindForm();
      if (form != null && form.ContainsFocus)
      {
        SetHotNode(FindDisplayNodeFromY(point.Y));
        if (!flag)
        {
          SetTooltipNode(FindDisplayNodeFromPoint(point));
        }
      }
      else
      {
        SetHotNode(null);
        SetTooltipNode(null);
      }
      base.OnMouseMove(e);
    }
    protected override void OnMouseLeave(EventArgs e)
    {
      InternalResetHotPoint();
      SetHotNode(null);
      SetTooltipNode(null);
      base.OnMouseLeave(e);
    }
    protected override void OnDoubleClick(EventArgs e)
    {
      Point point = base.PointToClient(Control.MousePosition);
      HotPoint = ClientToNodeSpace(point);
      Node node = FindDisplayNodeFromY(point.Y);
      if (node != null && node.VC != null && !node.VC.DoubleClick(this, node, HotPoint))
      {
        NodeCollection parentNodes = node.ParentNodes;
        while ((parentNodes.VC == null || !parentNodes.VC.DoubleClick(this, parentNodes, node, HotPoint)) && parentNodes.ParentNode != null)
        {
          parentNodes = parentNodes.ParentNode.ParentNodes;
          node = null;
        }
      }
      base.OnDoubleClick(e);
    }
    protected override void OnDragEnter(DragEventArgs drgevent)
    {
      _cachedSelected = _selected;
      _selected = new Hashtable();
      InvalidateDrawRectangle();
      Point point = base.PointToClient(new Point(drgevent.X, drgevent.Y));
      HotPoint = ClientToNodeSpace(point);
      Node node = NodeVC.DragOverNodeFromPoint(this, point);
      if (point.X > _drawRectangle.Right)
      {
        node = null;
      }
      else if (point.Y > _drawRectangle.Bottom)
      {
        node = null;
      }
      drgevent.Effect = 0;
      if (node != null && node.VC != null)
      {
        node.VC.DragEnter(this, node, drgevent);
        _dragHoverTimer.Start();
      }
      else
      {
        OnClientDragEnter(drgevent);
      }
      _dragNode = node;
      _dragEffects = drgevent.Effect;
      base.OnDragEnter(drgevent);
    }
    protected override void OnDragLeave(EventArgs e)
    {
      _dragBumpTimer.Stop();
      if (_dragNode != null)
      {
        _dragHoverTimer.Stop();
        _dragNode.VC.DragLeave(this, _dragNode);
        _dragNode = null;
      }
      else
      {
        OnClientDragLeave(e);
      }
      _selected = _cachedSelected;
      _cachedSelected = null;
      InvalidateDrawRectangle();
      base.OnDragLeave(e);
    }
    protected override void OnDragOver(DragEventArgs drgevent)
    {
      Point point = base.PointToClient(new Point(drgevent.X, drgevent.Y));
      HotPoint = ClientToNodeSpace(point);
      Node node = NodeVC.DragOverNodeFromPoint(this, point);
      if (point.X > _drawRectangle.Right)
      {
        node = null;
      }
      else if (point.Y > _drawRectangle.Bottom)
      {
        node = null;
      }
      drgevent.Effect = 0;
      if (node == _dragNode)
      {
        if (_dragNode != null)
        {
          _dragNode.VC.DragOver(this, _dragNode, drgevent);
        }
        else
        {
          OnClientDragOver(drgevent);
        }
      }
      else
      {
        if (_dragNode == null)
        {
          OnClientDragLeave(EventArgs.Empty);
          node.VC.DragEnter(this, node, drgevent);
          _dragHoverTimer.Start();
        }
        else if (node == null)
        {
          _dragHoverTimer.Stop();
          _dragNode.VC.DragLeave(this, _dragNode);
          OnClientDragEnter(drgevent);
        }
        else
        {
          _dragHoverTimer.Stop();
          _dragNode.VC.DragLeave(this, _dragNode);
          node.VC.DragEnter(this, node, drgevent);
          _dragHoverTimer.Start();
        }
        _dragNode = node;
      }
      int num = _drawRectangle.Top + 15;
      int num2 = _drawRectangle.Bottom - 15;
      if (point.Y < num)
      {
        _bumpUpwards = true;
        _dragBumpTimer.Start();
      }
      else if (point.Y > num2 && point.Y < _drawRectangle.Bottom)
      {
        _bumpUpwards = false;
        _dragBumpTimer.Start();
      }
      else
      {
        _dragBumpTimer.Stop();
      }
      _dragEffects = drgevent.Effect;
      InvalidateDrawRectangle();
      base.OnDragOver(drgevent);
    }
    protected override void OnDragDrop(DragEventArgs drgevent)
    {
      _dragBumpTimer.Stop();
      _selected = _cachedSelected;
      _cachedSelected = null;
      if (_dragNode != null)
      {
        _dragHoverTimer.Stop();
        _dragNode.VC.DragDrop(this, _dragNode, drgevent);
        _dragNode = null;
      }
      else
      {
        OnClientDragDrop(drgevent);
      }
      InvalidateDrawRectangle();
      base.OnDragDrop(drgevent);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);
      e.Graphics.TextRenderingHint = TextRenderingHint;
      CalculationCycle(e.Graphics);
      DrawControlBorder(e);
      using (Region region = new Region(_drawRectangle))
      {
        e.Graphics.SetClip(region, CombineMode.Intersect);
        Point point = NodeSpaceToClient(Point.Empty);
        _clipRectangle = ClientToNodeSpace(_drawRectangle);
        e.Graphics.TranslateTransform((float)point.X, (float)point.Y);
        DrawAllNodes(e);
        NodeVC.PostDrawNodes(this, e.Graphics, _displayNodes);
        CollectionVC.PostDrawNodes(this, e.Graphics, _displayNodes);
        e.Graphics.TranslateTransform((float)(-(float)point.X), (float)(-(float)point.Y));
        _invalidated = false;
      }
    }
    protected override void WndProc(ref Message m)
    {
      _themeTreeView.WndProc(ref m);
      _themeCheckbox.WndProc(ref m);
      base.WndProc(ref m);
    }
    protected virtual void OnViewControllersChanged()
    {
      if (ViewControllersChanged != null)
      {
        ViewControllersChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnContextMenuNodeChanged()
    {
      if (ContextMenuNodeChanged != null)
      {
        ContextMenuNodeChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnContextMenuSpaceChanged()
    {
      if (ContextMenuSpaceChanged != null)
      {
        ContextMenuSpaceChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnPathSeparatorChanged()
    {
      if (PathSeparatorChanged != null)
      {
        PathSeparatorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnBorderIndentChanged()
    {
      if (BorderIndentChanged != null)
      {
        BorderIndentChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnBorderStyleChanged()
    {
      if (BorderStyleChanged != null)
      {
        BorderStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnBorderColorChanged()
    {
      if (BorderColorChanged != null)
      {
        BorderColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnLineWidthChanged()
    {
      if (LineWidthChanged != null)
      {
        LineWidthChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnLineColorChanged()
    {
      if (LineColorChanged != null)
      {
        LineColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnLineDashStyleChanged()
    {
      if (LineDashStyleChanged != null)
      {
        LineDashStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnBoxLengthChanged()
    {
      if (BoxLengthChanged != null)
      {
        BoxLengthChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnBoxDrawStyleChanged()
    {
      if (BoxDrawStyleChanged != null)
      {
        BoxDrawStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnBoxShownAlwaysChanged()
    {
      if (BoxShownAlwaysChanged != null)
      {
        BoxShownAlwaysChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnBoxSignColorChanged()
    {
      if (BoxSignColorChanged != null)
      {
        BoxSignColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnBoxBorderColorChanged()
    {
      if (BoxBorderColorChanged != null)
      {
        BoxBorderColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnBoxInsideColorChanged()
    {
      if (BoxInsideColorChanged != null)
      {
        BoxInsideColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnBoxVisibilityChanged()
    {
      if (BoxVisibilityChanged != null)
      {
        BoxVisibilityChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnLineVisibilityChanged()
    {
      if (LineVisibilityChanged != null)
      {
        LineVisibilityChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnColumnWidthChanged()
    {
      if (ColumnWidthChanged != null)
      {
        ColumnWidthChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckStatesChanged()
    {
      if (CheckStatesChanged != null)
      {
        CheckStatesChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckLengthChanged()
    {
      if (CheckLengthChanged != null)
      {
        CheckLengthChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckGapLeftChanged()
    {
      if (CheckGapLeftChanged != null)
      {
        CheckGapLeftChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckGapRightChanged()
    {
      if (CheckGapRightChanged != null)
      {
        CheckGapRightChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckDrawStyleChanged()
    {
      if (CheckDrawStyleChanged != null)
      {
        CheckDrawStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckBorderColorChanged()
    {
      if (CheckBorderColorChanged != null)
      {
        CheckBorderColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckInsideColorChanged()
    {
      if (CheckInsideColorChanged != null)
      {
        CheckInsideColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckInsideHotColorChanged()
    {
      if (CheckInsideHotColorChanged != null)
      {
        CheckInsideHotColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckTickColorChanged()
    {
      if (CheckTickColorChanged != null)
      {
        CheckTickColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckTickHotColorChanged()
    {
      if (CheckTickHotColorChanged != null)
      {
        CheckTickHotColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckMixedColorChanged()
    {
      if (CheckMixedColorChanged != null)
      {
        CheckMixedColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckMixedHotColorChanged()
    {
      if (CheckMixedHotColorChanged != null)
      {
        CheckMixedHotColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckBorderWidthChanged()
    {
      if (CheckBorderWidthChanged != null)
      {
        CheckBorderWidthChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnVerticalScrollbarChanged()
    {
      if (VerticalScrollbarChanged != null)
      {
        VerticalScrollbarChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnHorizontalScrollbarChanged()
    {
      if (HorizontalScrollbarChanged != null)
      {
        HorizontalScrollbarChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnVerticalGranularityChanged()
    {
      if (VerticalGranularityChanged != null)
      {
        VerticalGranularityChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnEnableMouseWheelChanged()
    {
      if (EnableMouseWheelChanged != null)
      {
        EnableMouseWheelChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupFontChanged()
    {
      if (GroupFontChanged != null)
      {
        GroupFontChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupArrowsChanged()
    {
      if (GroupArrowsChanged != null)
      {
        GroupArrowsChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupHotFontStyleChanged()
    {
      if (GroupHotFontStyleChanged != null)
      {
        GroupHotFontStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupUseHotFontStyleChanged()
    {
      if (GroupUseHotFontStyleChanged != null)
      {
        GroupUseHotFontStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupSelectedFontStyleChanged()
    {
      if (GroupSelectedFontStyleChanged != null)
      {
        GroupSelectedFontStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupUseSelectedFontStyleChanged()
    {
      if (GroupUseSelectedFontStyleChanged != null)
      {
        GroupUseSelectedFontStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupBorderStyleChanged()
    {
      if (GroupBorderStyleChanged != null)
      {
        GroupBorderStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupGradientAngleChanged()
    {
      if (GroupGradientAngleChanged != null)
      {
        GroupGradientAngleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupGradientColoringChanged()
    {
      if (GroupGradientColoringChanged != null)
      {
        GroupGradientColoringChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupGradientBackChanged()
    {
      if (GroupGradientBackChanged != null)
      {
        GroupGradientBackChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupIndentLeftChanged()
    {
      if (GroupIndentLeftChanged != null)
      {
        GroupIndentLeftChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupIndentTopChanged()
    {
      if (GroupIndentTopChanged != null)
      {
        GroupIndentTopChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupIndentBottomChanged()
    {
      if (GroupIndentBottomChanged != null)
      {
        GroupIndentBottomChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupColoringChanged()
    {
      if (GroupColoringChanged != null)
      {
        GroupColoringChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupBackColorChanged()
    {
      if (GroupBackColorChanged != null)
      {
        GroupBackColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupForeColorChanged()
    {
      if (GroupForeColorChanged != null)
      {
        GroupForeColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupLineColorChanged()
    {
      if (GroupLineColorChanged != null)
      {
        GroupLineColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupHotBackColorChanged()
    {
      if (GroupHotBackColorChanged != null)
      {
        GroupHotBackColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupHotForeColorChanged()
    {
      if (GroupHotForeColorChanged != null)
      {
        GroupHotForeColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupSelectedBackColorChanged()
    {
      if (GroupSelectedBackColorChanged != null)
      {
        GroupSelectedBackColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupSelectedNoFocusBackColorChanged()
    {
      if (GroupSelectedNoFocusBackColorChanged != null)
      {
        GroupSelectedNoFocusBackColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupSelectedForeColorChanged()
    {
      if (GroupSelectedForeColorChanged != null)
      {
        GroupSelectedForeColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupExtraLeftChanged()
    {
      if (GroupExtraLeftChanged != null)
      {
        GroupExtraLeftChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupExtraHeightChanged()
    {
      if (GroupExtraHeightChanged != null)
      {
        GroupExtraHeightChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupHotTrackChanged()
    {
      if (GroupHotTrackChanged != null)
      {
        GroupHotTrackChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupImageBoxChanged()
    {
      if (GroupImageBoxChanged != null)
      {
        GroupImageBoxChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupImageBoxColumnChanged()
    {
      if (GroupImageBoxChanged != null)
      {
        GroupImageBoxChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupImageBoxWidthChanged()
    {
      if (GroupImageBoxWidthChanged != null)
      {
        GroupImageBoxWidthChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupImageBoxGapChanged()
    {
      if (GroupImageBoxGapChanged != null)
      {
        GroupImageBoxGapChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupImageBoxBorderChanged()
    {
      if (GroupImageBoxBorderChanged != null)
      {
        GroupImageBoxBorderChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupImageBoxLineColorChanged()
    {
      if (GroupImageBoxLineColorChanged != null)
      {
        GroupImageBoxLineColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupImageBoxBackColorChanged()
    {
      if (GroupImageBoxBackColorChanged != null)
      {
        GroupImageBoxBackColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupImageBoxSelectedBackColorChanged()
    {
      if (GroupImageBoxSelectedBackColorChanged != null)
      {
        GroupImageBoxSelectedBackColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupImageBoxColumnColorChanged()
    {
      if (GroupImageBoxColumnColorChanged != null)
      {
        GroupImageBoxColumnColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupImageBoxGradientBackChanged()
    {
      if (GroupImageBoxGradientBackChanged != null)
      {
        GroupImageBoxGradientBackChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupImageBoxGradientAngleChanged()
    {
      if (GroupImageBoxGradientAngleChanged != null)
      {
        GroupImageBoxGradientAngleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupImageBoxGradientColoringChanged()
    {
      if (GroupImageBoxGradientColoringChanged != null)
      {
        GroupImageBoxGradientColoringChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupClickExpandChanged()
    {
      if (GroupClickExpandChanged != null)
      {
        GroupClickExpandChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupAutoEditChanged()
    {
      if (GroupAutoEditChanged != null)
      {
        GroupAutoEditChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupNodesSelectableChanged()
    {
      if (GroupNodesSelectableChanged != null)
      {
        GroupNodesSelectableChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupDoubleClickExpandChanged()
    {
      if (GroupDoubleClickExpandChanged != null)
      {
        GroupDoubleClickExpandChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupAutoCollapseChanged()
    {
      if (GroupAutoCollapseChanged != null)
      {
        GroupAutoCollapseChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupAutoAllocateChanged()
    {
      if (GroupAutoAllocateChanged != null)
      {
        GroupAutoAllocateChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupExpandOnDragHoverChanged()
    {
      if (GroupExpandOnDragHoverChanged != null)
      {
        GroupExpandOnDragHoverChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnGroupTextRenderingHintChanged()
    {
      if (GroupTextRenderingHintChanged != null)
      {
        GroupTextRenderingHintChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnClickExpandChanged()
    {
      if (ClickExpandChanged != null)
      {
        ClickExpandChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnDoubleClickExpandChanged()
    {
      if (DoubleClickExpandChanged != null)
      {
        DoubleClickExpandChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnAutoCollapseChanged()
    {
      if (AutoCollapseChanged != null)
      {
        AutoCollapseChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnExtendToRightChanged()
    {
      if (ExtendToRightChanged != null)
      {
        ExtendToRightChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnInstantUpdateChanged()
    {
      if (InstantUpdateChanged != null)
      {
        InstantUpdateChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnTooltipsChanged()
    {
      if (TooltipsChanged != null)
      {
        TooltipsChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnInfotipsChanged()
    {
      if (InfotipsChanged != null)
      {
        InfotipsChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnNodesSelectableChanged()
    {
      if (NodesSelectableChanged != null)
      {
        NodesSelectableChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnAutoEditChanged()
    {
      if (AutoEditChanged != null)
      {
        AutoEditChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnLabelEditChanged()
    {
      if (LabelEditChanged != null)
      {
        LabelEditChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCanUserExpandCollapseChanged()
    {
      if (CanUserExpandCollapseChanged != null)
      {
        CanUserExpandCollapseChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnExpandOnDragHoverChanged()
    {
      if (ExpandOnDragHoverChanged != null)
      {
        ExpandOnDragHoverChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnIndicatorsChanged()
    {
      if (IndicatorsChanged != null)
      {
        IndicatorsChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnHotNodeChanged()
    {
      if (HotNodeChanged != null)
      {
        HotNodeChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnVerticalNodeGapChanged()
    {
      if (VerticalNodeGapChanged != null)
      {
        VerticalNodeGapChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnMinimumNodeHeightChanged()
    {
      if (MinimumNodeHeightChanged != null)
      {
        MinimumNodeHeightChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnMaximumNodeHeightChanged()
    {
      if (MaximumNodeHeightChanged != null)
      {
        MaximumNodeHeightChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnTextRenderingHintChanged()
    {
      if (TextRenderingHintChanged != null)
      {
        TextRenderingHintChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnHotBackColorChanged()
    {
      if (HotBackColorChanged != null)
      {
        HotBackColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnHotForeColorChanged()
    {
      if (HotForeColorChanged != null)
      {
        HotForeColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnSelectedBackColorChanged()
    {
      if (SelectedBackColorChanged != null)
      {
        SelectedBackColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnSelectedNoFocusBackColorChanged()
    {
      if (SelectedNoFocusBackColorChanged != null)
      {
        SelectedNoFocusBackColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnSelectedForeColorChanged()
    {
      if (SelectedForeColorChanged != null)
      {
        SelectedForeColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnSelectModeChanged()
    {
      if (SelectModeChanged != null)
      {
        SelectModeChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnHotFontStyleChanged()
    {
      if (HotFontStyleChanged != null)
      {
        HotFontStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnUseHotFontStyleChanged()
    {
      if (UseHotFontStyleChanged != null)
      {
        UseHotFontStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnSelectedFontStyleChanged()
    {
      if (SelectedFontStyleChanged != null)
      {
        SelectedFontStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnUseSelectedFontStyleChanged()
    {
      if (UseSelectedFontStyleChanged != null)
      {
        UseSelectedFontStyleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnImageListChanged()
    {
      if (ImageListChanged != null)
      {
        ImageListChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnImageIndexChanged()
    {
      if (ImageIndexChanged != null)
      {
        ImageIndexChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnSelectedImageIndexChanged()
    {
      if (SelectedImageIndexChanged != null)
      {
        SelectedImageIndexChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnImageGapLeftChanged()
    {
      if (ImageGapLeftChanged != null)
      {
        ImageGapLeftChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnImageGapRightChanged()
    {
      if (ImageGapRightChanged != null)
      {
        ImageGapRightChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnLabelControlCreated(TextBox textBox)
    {
      if (LabelControlCreated != null)
      {
        LabelControlCreated(this, new LabelControlEventArgs(textBox));
      }
    }
    internal void BeginAutoEdit(Node n)
    {
      _autoEditTimer.Stop();
      _autoEditNode = n;
      _autoEditTimer.Start();
    }
    internal void CancelAutoEdit()
    {
      _autoEditTimer.Stop();
      _autoEditNode = null;
    }
    internal void BeginEditLabel(Node n, Rectangle textRect)
    {
      if (_labelEditNode == null)
      {
        string text = n.Text;
        if (OnBeforeLabelEdit(n, ref text))
        {
          RemoveAnyToolTip();
          _labelEditNode = n;
          _labelEditBox = new TextBox();
          _labelEditBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
          _labelEditBox.Font = n.VC.GetNodeFont(this, n);
          _labelEditBox.Text = text;
          textRect = NodeSpaceToClient(textRect);
          if (textRect.Right > _innerRectangle.Right)
          {
            textRect.Width = textRect.Width - (textRect.Right - _innerRectangle.Right);
          }
          _labelEditBox.SetBounds(textRect.X, textRect.Y, textRect.Width, textRect.Height);
          if (_labelEditBox.Height != textRect.Height)
          {
            int num = (_labelEditBox.Height - textRect.Height) / 2;
            TextBox expr_FD = _labelEditBox;
            expr_FD.Top = expr_FD.Top - num;
          }
          _labelEditBox.KeyDown += OnLabelEditBoxKeyDown;
          _labelEditBox.LostFocus += OnLabelEditBoxLostFocus;
          _labelEditBox.TextChanged += OnLabelEditTextChanged;
          base.Controls.Add(_labelEditBox);
          OnLabelControlCreated(_labelEditBox);
          _labelEditBox.Focus();
        }
      }
    }
    internal void EndEditLabel(bool quit)
    {
      string text = _labelEditBox.Text;
      _labelEditBox.TextChanged -= OnLabelEditTextChanged;
      _labelEditBox.LostFocus -= OnLabelEditBoxLostFocus;
      _labelEditBox.KeyDown -= OnLabelEditBoxKeyDown;
      base.Controls.Remove(_labelEditBox);
      _labelEditBox.Dispose();
      _labelEditBox = null;
      Node labelEditNode = _labelEditNode;
      _labelEditNode = null;
      if (!quit && OnAfterLabelEdit(labelEditNode, ref text))
      {
        labelEditNode.Text = text;
      }
    }
    internal void NodeAdded(Node n)
    {
      if (n.Key != null && !_nodeKeys.Contains(n.Key))
      {
        _nodeKeys.Add(n.Key, n);
      }
    }
    internal void NodeRemoved(Node n)
    {
      if (n.Key != null && _nodeKeys.Contains(n.Key))
      {
        _nodeKeys.Remove(n.Key);
      }
    }
    internal void NodeKeyChanged(Node n, object oldKey, object newKey)
    {
      if (oldKey != null && _nodeKeys.Contains(oldKey))
      {
        _nodeKeys.Remove(oldKey);
      }
      if (newKey != null && !_nodeKeys.Contains(newKey))
      {
        _nodeKeys.Add(newKey, n);
      }
    }
    internal void SetFocusNode(Node n)
    {
      if (_focusNode != n)
      {
        if (_focusNode != null)
        {
          InvalidateNode(_focusNode);
        }
        _focusNode = n;
        if (_focusNode != null)
        {
          InvalidateNode(n);
          EnsureDisplayed(n);
        }
      }
    }
    internal bool IsNodeSelected(Node n)
    {
      return _selected.ContainsKey(n);
    }
    internal void SelectNode(Node n)
    {
      SelectNode(n, KeyHelper.SHIFTPressed, KeyHelper.CTRLPressed);
    }
    internal void SelectNode(Node n, bool shift, bool ctrl)
    {
      switch (SelectMode)
      {
        case SelectMode.None:
          SetFocusNode(n);
          return;
        case SelectMode.Single:
          SingleSelect(n);
          return;
        case SelectMode.Multiple:
          if (ctrl)
          {
            CtrlSelect(n);
            return;
          }
          if (shift)
          {
            ShiftSelect(n);
            return;
          }
          SingleSelect(n);
          return;
        default:
          return;
      }
    }
    internal void DeselectNode(Node n, bool recurse)
    {
      if (_lastSelectedNode == n)
      {
        _lastSelectedNode = null;
      }
      if (_selected.ContainsKey(n))
      {
        _selected.Remove(n);
        OnAfterDeselect(n);
        InvalidateNode(n);
      }
      if (recurse)
      {
        IEnumerator enumerator = n.Nodes.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            Node n2 = (Node)enumerator.Current;
            DeselectNode(n2, true);
          }
        }
        finally
        {
          IDisposable disposable = enumerator as IDisposable;
          if (disposable != null)
          {
            disposable.Dispose();
          }
        }
      }
    }
    internal void ClearSelection()
    {
      if (_selected.Count > 0)
      {
        IEnumerator enumerator = _selected.Keys.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            Node n = (Node)enumerator.Current;
            InvalidateNode(n);
          }
        }
        finally
        {
          IDisposable disposable = enumerator as IDisposable;
          if (disposable != null)
          {
            disposable.Dispose();
          }
        }
        _selected.Clear();
      }
    }
    internal void InvalidateSelection()
    {
      if (_selected.Count > 0)
      {
        IEnumerator enumerator = _selected.Keys.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            Node n = (Node)enumerator.Current;
            InvalidateNode(n);
          }
        }
        finally
        {
          IDisposable disposable = enumerator as IDisposable;
          if (disposable != null)
          {
            disposable.Dispose();
          }
        }
      }
    }
    internal void NodeExpandedChanged(Node n)
    {
      if (!n.Expanded)
      {
        EnsureFocusAfterExpand();
      }
      InvalidateNodeDrawing();
    }
    internal void NodeVisibleChanged(Node n, bool makeSelected)
    {
      if (!n.Visible)
      {
        EnsureFocusAfterHidden(makeSelected);
      }
      InvalidateNodeDrawing();
    }
    internal void NodeContentRemoved(bool makeSelected)
    {
      EnsureFocusAfterHidden(makeSelected);
    }
    internal void NodeContentCleared(bool makeSelected)
    {
      EnsureFocusAfterClear(makeSelected);
      InvalidateAll();
    }
    internal Image GetIndicatorImage(Indicator ind)
    {
      return TreeControl._indicatorImages.Images[(int)ind];
    }
    internal void InternalResetHotPoint()
    {
      HotPoint = new Point(-1, -1);
    }
    internal int GetFontHeight()
    {
      if (_fontHeight == -1)
      {
        _fontHeight = Font.Height;
      }
      return _fontHeight;
    }
    internal int GetGroupFontHeight()
    {
      if (_groupFontHeight == -1)
      {
        _groupFontHeight = GroupFont.Height;
      }
      return _groupFontHeight;
    }
    internal Font GetFontBoldItalic()
    {
      if (_fontBoldItalic == null)
      {
        _fontBoldItalic = new Font(Font, FontStyle.Bold|FontStyle.Italic);
      }
      return _fontBoldItalic;
    }
    internal Font GetGroupFontBoldItalic()
    {
      if (_groupFontBoldItalic == null)
      {
        _groupFontBoldItalic = new Font(GroupFont, FontStyle.Bold|FontStyle.Italic);
      }
      return _groupFontBoldItalic;
    }
    internal void InternalResetFontHeight()
    {
      _fontHeight = -1;
    }
    internal void InternalResetGroupFontHeight()
    {
      _groupFontHeight = -1;
    }
    internal void InternalResetFontBoldItalic()
    {
      if (_fontBoldItalic != null)
      {
        _fontBoldItalic.Dispose();
        _fontBoldItalic = null;
      }
    }
    internal void InternalResetGroupFontBoldItalic()
    {
      if (_groupFontBoldItalic != null)
      {
        _groupFontBoldItalic.Dispose();
        _groupFontBoldItalic = null;
      }
    }
    internal Pen GetCacheLineDashPen()
    {
      if (_cacheLineDashPen == null)
      {
        _cacheLineDashPen = new Pen(LineColor, (float)LineWidth);
        switch (LineDashStyle)
        {
          case LineDashStyle.Dot:
            _cacheLineDashPen.DashStyle = DashStyle.Dot;
            break;
          case LineDashStyle.Dash:
            _cacheLineDashPen.DashStyle = DashStyle.Dash;
            break;
        }
      }
      return _cacheLineDashPen;
    }
    internal Pen GetCacheBoxSignPen()
    {
      if (_cacheBoxSignPen == null)
      {
        _cacheBoxSignPen = new Pen(BoxSignColor, 1f);
      }
      return _cacheBoxSignPen;
    }
    internal Pen GetCacheBoxBorderPen()
    {
      if (_cacheBoxBorderPen == null)
      {
        _cacheBoxBorderPen = new Pen(BoxBorderColor, 1f);
      }
      return _cacheBoxBorderPen;
    }
    internal Brush GetCacheBoxInsideBrush()
    {
      if (_cacheBoxInsideBrush == null)
      {
        _cacheBoxInsideBrush = new SolidBrush(BoxInsideColor);
      }
      return _cacheBoxInsideBrush;
    }
    internal Pen GetCacheCheckTickPen()
    {
      if (_cacheCheckTickPen == null)
      {
        _cacheCheckTickPen = new Pen(CheckTickColor, 1f);
      }
      return _cacheCheckTickPen;
    }
    internal Brush GetCacheCheckTickBrush()
    {
      if (_cacheCheckTickBrush == null)
      {
        _cacheCheckTickBrush = new SolidBrush(CheckTickColor);
      }
      return _cacheCheckTickBrush;
    }
    internal Pen GetCacheCheckTickHotPen()
    {
      if (_cacheCheckTickHotPen == null)
      {
        _cacheCheckTickHotPen = new Pen(CheckTickHotColor, 1f);
      }
      return _cacheCheckTickHotPen;
    }
    internal Brush GetCacheCheckTickHotBrush()
    {
      if (_cacheCheckTickHotBrush == null)
      {
        _cacheCheckTickHotBrush = new SolidBrush(CheckTickHotColor);
      }
      return _cacheCheckTickHotBrush;
    }
    internal Brush GetCacheCheckBorderBrush()
    {
      if (_cacheCheckBorderBrush == null)
      {
        _cacheCheckBorderBrush = new SolidBrush(CheckBorderColor);
      }
      return _cacheCheckBorderBrush;
    }
    internal Brush GetCacheCheckInsideBrush()
    {
      if (_cacheCheckInsideBrush == null)
      {
        _cacheCheckInsideBrush = new SolidBrush(CheckInsideColor);
      }
      return _cacheCheckInsideBrush;
    }
    internal Brush GetCacheCheckInsideHotBrush()
    {
      if (_cacheCheckInsideHotBrush == null)
      {
        _cacheCheckInsideHotBrush = new SolidBrush(CheckInsideHotColor);
      }
      return _cacheCheckInsideHotBrush;
    }
    internal Brush GetCacheCheckMixedBrush()
    {
      if (_cacheCheckMixedBrush == null)
      {
        _cacheCheckMixedBrush = new SolidBrush(CheckMixedColor);
      }
      return _cacheCheckMixedBrush;
    }
    internal Brush GetCacheCheckMixedHotBrush()
    {
      if (_cacheCheckMixedHotBrush == null)
      {
        _cacheCheckMixedHotBrush = new SolidBrush(CheckMixedHotColor);
      }
      return _cacheCheckMixedHotBrush;
    }
    internal Pen GetCacheGroupLinePen()
    {
      if (_cacheGroupLinePen == null)
      {
        _cacheGroupLinePen = new Pen(GroupLineColor);
      }
      return _cacheGroupLinePen;
    }
    internal Pen GetCacheGroupImageBoxLinePen()
    {
      if (_cacheGroupImageBoxLinePen == null)
      {
        _cacheGroupImageBoxLinePen = new Pen(GroupImageBoxLineColor);
      }
      return _cacheGroupImageBoxLinePen;
    }
    internal Brush GetCacheGroupImageBoxColumnBrush()
    {
      if (_cacheGroupImageBoxColumnBrush == null)
      {
        _cacheGroupImageBoxColumnBrush = new SolidBrush(GroupImageBoxColumnColor);
      }
      return _cacheGroupImageBoxColumnBrush;
    }
    internal Brush GetCacheHotBackBrush()
    {
      if (_cacheHotBackBrush == null)
      {
        _cacheHotBackBrush = new SolidBrush(HotBackColor);
      }
      return _cacheHotBackBrush;
    }
    internal Brush GetCacheSelectedBackBrush()
    {
      if (_cacheSelectedBackBrush == null)
      {
        _cacheSelectedBackBrush = new SolidBrush(SelectedBackColor);
      }
      return _cacheSelectedBackBrush;
    }
    internal Brush GetCacheSelectedNoFocusBackBrush()
    {
      if (_cacheSelectedNoFocusBackBrush == null)
      {
        _cacheSelectedNoFocusBackBrush = new SolidBrush(SelectedNoFocusBackColor);
      }
      return _cacheSelectedNoFocusBackBrush;
    }
    internal void GradientColors(TreeGradientColoring scheme, Color back, ref Color start, ref Color end)
    {
      switch (scheme)
      {
        case TreeGradientColoring.VeryLightToColor:
          start = ControlPaint.LightLight(back);
          end = back;
          return;
        case TreeGradientColoring.LightToColor:
          start = ControlPaint.Light(back);
          end = back;
          return;
        case TreeGradientColoring.LightToDark:
          start = ControlPaint.Light(back);
          end = ControlPaint.Dark(back);
          return;
        case TreeGradientColoring.LightToVeryDark:
          start = ControlPaint.Light(back);
          end = ControlPaint.DarkDark(back);
          return;
        case TreeGradientColoring.VeryLightToVeryDark:
          start = ControlPaint.LightLight(back);
          end = ControlPaint.DarkDark(back);
          return;
        default:
          return;
      }
    }
    internal void DrawThemedBox(Graphics g, Rectangle rect, bool expanded)
    {
      if (IsControlThemed)
      {
        rect = NodeSpaceToClient(rect);
        ThemeTreeViewGlyphState state;
        if (expanded)
        {
          state = ThemeTreeViewGlyphState.Open;
        }
        else
        {
          state = ThemeTreeViewGlyphState.Closed;
        }
        _themeTreeView.DrawThemeBackground(g, rect, 2, (int)state);
      }
    }
    internal void DrawThemedCheckbox(Graphics g, Rectangle rect, CheckState state, CheckStates states, bool hotTrack)
    {
      if (IsControlThemed)
      {
        rect = NodeSpaceToClient(rect);
        ThemeButtonPart part;
        if (states == CheckStates.Radio)
        {
          part = ThemeButtonPart.RadioButton;
        }
        else
        {
          part = ThemeButtonPart.Checkbox;
        }
        ThemeButtonCheckboxState state2;
        switch (state)
        {
          case CheckState.Mixed:
            if (hotTrack)
            {
              state2 = ThemeButtonCheckboxState.MixedHot;
              goto IL_56;
            }
            state2 = ThemeButtonCheckboxState.MixedNormal;
            goto IL_56;
          case CheckState.Checked:
            if (hotTrack)
            {
              state2 = ThemeButtonCheckboxState.CheckedHot;
              goto IL_56;
            }
            state2 = ThemeButtonCheckboxState.CheckedNormal;
            goto IL_56;
        }
        if (hotTrack)
        {
          state2 = ThemeButtonCheckboxState.UncheckedHot;
        }
        else
        {
          state2 = ThemeButtonCheckboxState.UncheckedNormal;
        }
      IL_56:
        _themeCheckbox.DrawThemeBackground(g, rect, (int)part, (int)state2);
      }
    }
    internal bool IsFirstDisplayedNode(Node n)
    {
      return _displayNodes.Count != 0 && _displayNodes[0] as Node == n;
    }
    internal bool IsLastDisplayedNode(Node n)
    {
      return _displayNodes.Count != 0 && _displayNodes[_displayNodes.Count - 1] as Node == n;
    }
    internal bool IsNodeDisplayed(Node n)
    {
      if (n == null)
      {
        return false;
      }
      do
      {
        n = n.Parent;
        if (n == null)
        {
          return true;
        }
      }
      while (n.Visible && n.Expanded);
      return false;
    }
    internal Point NodeSpaceToClient(Point point)
    {
      point.Offset(_drawRectangle.X, _drawRectangle.Y);
      if (VerticalGranularity == VerticalGranularity.Pixel)
      {
        point.Offset(-_offset.X, -_offset.Y);
      }
      else if (_displayNodes.Count > 0)
      {
        Node node = _displayNodes[_offset.Y] as Node;
        point.Offset(-_offset.X, -node.Cache.Bounds.Top);
      }
      return point;
    }
    internal Point ClientToNodeSpace(Point point)
    {
      if (VerticalGranularity == VerticalGranularity.Pixel)
      {
        point.Offset(_offset.X, _offset.Y);
      }
      else if (_displayNodes.Count > 0)
      {
        Node node = _displayNodes[_offset.Y] as Node;
        point.Offset(_offset.X, node.Cache.Bounds.Top);
      }
      point.Offset(-_drawRectangle.X, -_drawRectangle.Y);
      return point;
    }
    internal Rectangle NodeSpaceToClient(Rectangle bounds)
    {
      bounds.Offset(_drawRectangle.X, _drawRectangle.Y);
      if (VerticalGranularity == VerticalGranularity.Pixel)
      {
        bounds.Offset(-_offset.X, -_offset.Y);
      }
      else if (_displayNodes.Count > 0)
      {
        Node node = _displayNodes[_offset.Y] as Node;
        bounds.Offset(-_offset.X, -node.Cache.Bounds.Top);
      }
      return bounds;
    }
    internal Rectangle ClientToNodeSpace(Rectangle bounds)
    {
      if (VerticalGranularity == VerticalGranularity.Pixel)
      {
        bounds.Offset(_offset.X, _offset.Y);
      }
      else if (_displayNodes.Count > 0)
      {
        Node node = _displayNodes[_offset.Y] as Node;
        bounds.Offset(_offset.X, node.Cache.Bounds.Top);
      }
      bounds.Offset(-_drawRectangle.X, -_drawRectangle.Y);
      return bounds;
    }
    internal Node FindDisplayNodeFromPoint(Point clientPt)
    {
      Node node = FindDisplayNodeFromY(clientPt.Y);
      if (node != null)
      {
        Point point = ClientToNodeSpace(clientPt);
        if (point.X < node.Cache.Bounds.Left || point.X > node.Cache.Bounds.Right)
        {
          node = null;
        }
      }
      return node;
    }
    internal int FindDisplayFromY(int y)
    {
      if (_displayNodes.Count == 0)
      {
        return 0;
      }
      if (y < (_displayNodes[0] as Node).Cache.Bounds.Top)
      {
        return -1;
      }
      int num = 0;
      int num2 = _displayNodes.Count - 1;
      int bottom;
      while (true)
      {
        int num3 = num + (num2 - num) / 2;
        bottom = (_displayNodes[num3] as Node).Cache.Bounds.Bottom;
        if (num == num2)
        {
          break;
        }
        if (bottom < y)
        {
          num = num3 + 1;
        }
        else
        {
          num2 = num3;
        }
      }
      if (bottom < y)
      {
        return -1;
      }
      return num2;
    }
    internal int FindDisplayIndex(Node n)
    {
      return _displayNodes.IndexOf(n);
    }
    internal Node FindDisplayNodeFromY(int Y)
    {
      if (Y < _innerRectangle.Top || Y > _innerRectangle.Bottom)
      {
        return null;
      }
      Point point = ClientToNodeSpace(new Point(0, Y));
      int num = FindDisplayFromY(point.Y);
      if (num < 0 || num >= _displayNodes.Count)
      {
        return null;
      }
      Node node = _displayNodes[num] as Node;
      if (point.Y >= node.Cache.Bounds.Top && point.Y <= node.Cache.Bounds.Bottom)
      {
        return node;
      }
      return null;
    }
    internal void MarkAllNodeSizesDirty()
    {
      _nodeSizesDirty = true;
      InvalidateNodeDrawing();
    }
    private void ResetAllProperties()
    {
      ResetNodeVC();
      ResetCollectionVC();
      ResetViewControllers();
      ResetBackColor();
      ResetBorderStyle();
      ResetBorderColor();
      ResetBorderIndent();
      ResetLineColor();
      ResetLineWidth();
      ResetLineDashStyle();
      ResetBoxLength();
      ResetBoxDrawStyle();
      ResetBoxSignColor();
      ResetBoxBorderColor();
      ResetBoxInsideColor();
      ResetBoxVisibility();
      ResetBoxShownAlways();
      ResetLineVisibility();
      ResetColumnWidth();
      ResetCheckStates();
      ResetCheckLength();
      ResetCheckGapLeft();
      ResetCheckGapRight();
      ResetCheckBorderWidth();
      ResetCheckDrawStyle();
      ResetCheckBorderColor();
      ResetCheckInsideColor();
      ResetCheckInsideHotColor();
      ResetCheckTickColor();
      ResetCheckTickHotColor();
      ResetCheckMixedColor();
      ResetCheckMixedHotColor();
      ResetVerticalScrollbar();
      ResetHorizontalScrollbar();
      ResetVerticalGranularity();
      ResetEnableMouseWheel();
      ResetImageGapLeft();
      ResetImageGapRight();
      ResetGroupFont();
      ResetGroupArrows();
      ResetGroupIndentLeft();
      ResetGroupIndentTop();
      ResetGroupIndentBottom();
      ResetGroupBackColor();
      ResetGroupForeColor();
      ResetGroupLineColor();
      ResetGroupColoring();
      ResetGroupHotBackColor();
      ResetGroupHotForeColor();
      ResetGroupHotFontStyle();
      ResetGroupUseHotFontStyle();
      ResetGroupSelectedBackColor();
      ResetGroupSelectedNoFocusBackColor();
      ResetGroupSelectedForeColor();
      ResetGroupSelectedFontStyle();
      ResetGroupUseSelectedFontStyle();
      ResetGroupGradientColoring();
      ResetGroupGradientAngle();
      ResetGroupGradientBack();
      ResetGroupExtraLeft();
      ResetGroupExtraHeight();
      ResetGroupHotTrack();
      ResetGroupBorderStyle();
      ResetGroupClickExpand();
      ResetGroupDoubleClickExpand();
      ResetGroupAutoCollapse();
      ResetGroupAutoAllocate();
      ResetGroupImageBox();
      ResetGroupImageBoxColumn();
      ResetGroupImageBoxWidth();
      ResetGroupImageBoxGap();
      ResetGroupImageBoxBorder();
      ResetGroupImageBoxLineColor();
      ResetGroupImageBoxBackColor();
      ResetGroupImageBoxSelectedBackColor();
      ResetGroupImageBoxColumnColor();
      ResetGroupImageBoxGradientBack();
      ResetGroupImageBoxGradientAngle();
      ResetGroupImageBoxGradientColoring();
      ResetGroupTextRenderingHint();
      ResetGroupNodesSelectable();
      ResetGroupExpandOnDragHover();
      ResetIndicators();
      ResetVerticalNodeGap();
      ResetMinimumNodeHeight();
      ResetMaximumNodeHeight();
      ResetTextRenderingHint();
      ResetHotBackColor();
      ResetHotForeColor();
      ResetHotFontStyle();
      ResetUseHotFontStyle();
      ResetSelectMode();
      ResetSelectedBackColor();
      ResetSelectedNoFocusBackColor();
      ResetSelectedForeColor();
      ResetSelectedFontStyle();
      ResetUseSelectedFontStyle();
      ResetClickExpand();
      ResetDoubleClickExpand();
      ResetAutoCollapse();
      ResetExtendToRight();
      ResetNodesSelectable();
      ResetCanUserExpandCollapse();
      ResetExpandOnDragHover();
      ResetLabelEdit();
    }
    private void EnsureFocusAfterExpand()
    {
      if (_focusNode != null)
      {
        int[] nodeLocation = GetNodeLocation(_focusNode);
        Node node = null;
        NodeCollection nodes = Nodes;
        int[] array = nodeLocation;
        for (int i = 0; i < array.Length; i++)
        {
          int num = array[i];
          if (num == -1)
          {
            break;
          }
          Node node2 = nodes[num];
          if (!node2.Visible)
          {
            break;
          }
          node = node2;
          if (!node.Expanded)
          {
            break;
          }
          nodes = node.Nodes;
        }
        if (node != null)
        {
          if (node != _focusNode)
          {
            SetFocusNode(node);
            return;
          }
        }
        else
        {
          Node firstDisplayedNode = Nodes.GetFirstDisplayedNode();
          if (firstDisplayedNode != null)
          {
            SetFocusNode(firstDisplayedNode);
          }
        }
      }
    }
    private void EnsureFocusAfterHidden(bool makeSelected)
    {
      if (_focusNode != null)
      {
        if (Nodes.Count == 0)
        {
          _focusNode = null;
          return;
        }
        int[] nodeLocation = GetNodeLocation(_focusNode);
        Node node = null;
        NodeCollection nodes = Nodes;
        int[] array = nodeLocation;
        int i = 0;
        while (i < array.Length)
        {
          int num = array[i];
          Node node2 = nodes[num];
          if (node2.Visible && !node2.Removing)
          {
            node = node2;
            if (!node.Expanded)
            {
              break;
            }
            nodes = node.Nodes;
            i++;
          }
          else
          {
            bool flag = false;
            for (int j = num + 1; j < nodes.Count; j++)
            {
              if (nodes[j].Visible && !nodes[j].Removing)
              {
                node = nodes[j];
                flag = true;
                break;
              }
            }
            if (!flag)
            {
              for (int k = num - 1; k >= 0; k--)
              {
                if (nodes[k].Visible && !nodes[k].Removing)
                {
                  node = nodes[k];
                  break;
                }
              }
              break;
            }
            break;
          }
        }
        if (node != null)
        {
          if (node != _focusNode)
          {
            if (makeSelected)
            {
              SelectNode(node);
            }
            SetFocusNode(node);
            return;
          }
        }
        else
        {
          Node node3 = Nodes.GetFirstDisplayedNode();
          if (node3 != null)
          {
            if (node3.Removing)
            {
              node3 = node3.NextDisplayedNode;
            }
            if (node3 != null)
            {
              if (makeSelected)
              {
                SelectNode(node3);
              }
              SetFocusNode(node3);
            }
          }
        }
      }
    }
    private void EnsureFocusAfterClear(bool makeSelected)
    {
      if (_focusNode != null)
      {
        if (Nodes.Count == 0)
        {
          _focusNode = null;
          return;
        }
        int[] nodeLocation = GetNodeLocation(_focusNode);
        Node node = null;
        NodeCollection nodes = Nodes;
        int[] array = nodeLocation;
        int i = 0;
        while (i < array.Length)
        {
          int num = array[i];
          if (num == -1)
          {
            _focusNode = null;
            node = null;
            break;
          }
          Node node2 = nodes[num];
          if (node2.Visible && !node2.Removing)
          {
            node = node2;
            if (!node.Expanded)
            {
              break;
            }
            nodes = node.Nodes;
            i++;
          }
          else
          {
            bool flag = false;
            for (int j = num; j < nodes.Count; j++)
            {
              if (nodes[j].Visible && !nodes[j].Removing)
              {
                node = nodes[j];
                flag = true;
                break;
              }
            }
            if (!flag)
            {
              for (int k = num - 1; k >= 0; k--)
              {
                if (nodes[k].Visible && !nodes[k].Removing)
                {
                  node = nodes[k];
                  break;
                }
              }
              break;
            }
            break;
          }
        }
        if (node != null)
        {
          if (node != _focusNode)
          {
            if (makeSelected)
            {
              SelectNode(node);
            }
            SetFocusNode(node);
            return;
          }
        }
        else
        {
          Node node3 = Nodes.GetFirstDisplayedNode();
          if (node3 != null)
          {
            if (node3.Removing)
            {
              node3 = node3.NextDisplayedNode;
            }
            if (node3 != null)
            {
              if (makeSelected)
              {
                SelectNode(node3);
              }
              SetFocusNode(node3);
            }
          }
        }
      }
    }
    private bool EnsureParentChain(Node n, bool expand, bool visible)
    {
      if (expand || visible)
      {
        Node node = n;
        bool flag = false;
        while (true)
        {
          if (!node.Visible)
          {
            if (!visible)
            {
              break;
            }
            node.Visible = true;
            flag = true;
          }
          if (!node.Expanded && node != n)
          {
            if (!expand)
            {
              return false;
            }
            node.Expand();
            flag = true;
          }
          if (node.Parent == null)
          {
            goto IL_4A;
          }
          node = node.Parent;
        }
        return false;
      IL_4A:
        if (flag)
        {
          _nodeDrawingValid = false;
          using (Graphics graphics = base.CreateGraphics())
          {
            CalculationCycle(graphics);
          }
          InvalidateAll();
        }
      }
      return true;
    }
    private int FindFirstFullDisplayedNode(Rectangle bounds)
    {
      if (VerticalGranularity == VerticalGranularity.Node)
      {
        return _offset.Y;
      }
      int num = FindDisplayFromY(_offset.Y);
      if (num < 0)
      {
        return -1;
      }
      Node node = _displayNodes[num] as Node;
      if (node.Cache.Bounds.Top < _offset.Y && num < _displayNodes.Count - 1)
      {
        return num + 1;
      }
      return num;
    }
    private int FindLastFullDisplayedNode(Rectangle bounds)
    {
      if (VerticalGranularity == VerticalGranularity.Node)
      {
        Node node = _displayNodes[_offset.Y] as Node;
        int num = bounds.Height + node.Cache.Bounds.Top;
        int num2 = FindDisplayFromY(num);
        if (num2 < 0)
        {
          return _displayNodes.Count - 1;
        }
        Node node2 = _displayNodes[num2] as Node;
        if (node2.Cache.Bounds.Bottom >= num && num2 > 0)
        {
          return num2 - 1;
        }
        return num2;
      }
      else
      {
        int num3 = bounds.Height + _offset.Y;
        int num4 = FindDisplayFromY(num3);
        if (num4 < 0)
        {
          return _displayNodes.Count - 1;
        }
        Node node3 = _displayNodes[num4] as Node;
        if (node3.Cache.Bounds.Bottom >= num3 && num4 > 0)
        {
          return num4 - 1;
        }
        return num4;
      }
    }
    private int InsidePageUpIndex(int index, Rectangle bounds)
    {
      int num = bounds.Height;
      Node node = _displayNodes[index] as Node;
      num -= node.Cache.Bounds.Height;
      while (num > 0 && index > 0)
      {
        node = (_displayNodes[index - 1] as Node);
        num -= node.Cache.Bounds.Height;
        if (num > 0)
        {
          index--;
        }
      }
      return index;
    }
    private Node OutsidePageUpNode(Node node, Rectangle bounds)
    {
      int num = _displayNodes.IndexOf(node);
      if (num == -1)
      {
        return null;
      }
      int num2 = bounds.Height;
      while (num2 > 0 && num > 0)
      {
        Node node2 = _displayNodes[num - 1] as Node;
        num2 -= node2.Cache.Bounds.Height;
        if (num2 > 0)
        {
          num--;
        }
      }
      return _displayNodes[num] as Node;
    }
    private Node OutsidePageDownNode(Node node, Rectangle bounds)
    {
      int num = _displayNodes.IndexOf(node);
      if (num == -1)
      {
        return null;
      }
      int num2 = bounds.Height;
      while (num2 > 0 && num < _displayNodes.Count - 1)
      {
        Node node2 = _displayNodes[num + 1] as Node;
        num2 -= node2.Cache.Bounds.Height;
        if (num2 > 0)
        {
          num++;
        }
      }
      return _displayNodes[num] as Node;
    }
    private bool ProcessUpKey(bool shiftKey, bool controlKey)
    {
      Node node;
      if (_focusNode != null)
      {
        node = _focusNode.PreviousDisplayedNode;
      }
      else
      {
        node = Nodes.GetFirstDisplayedNode();
      }
      if (node != null)
      {
        if (controlKey)
        {
          SetFocusNode(node);
        }
        else
        {
          SelectNode(node, shiftKey, controlKey);
        }
      }
      return true;
    }
    private bool ProcessDownKey(bool shiftKey, bool controlKey)
    {
      Node node;
      if (_focusNode != null)
      {
        node = _focusNode.NextDisplayedNode;
      }
      else
      {
        node = Nodes.GetFirstDisplayedNode();
      }
      if (node != null)
      {
        if (controlKey)
        {
          SetFocusNode(node);
        }
        else
        {
          SelectNode(node, shiftKey, controlKey);
        }
      }
      return true;
    }
    private bool ProcessLeftKey(bool shiftKey, bool controlKey)
    {
      if (!shiftKey && !controlKey && _focusNode != null)
      {
        if (_focusNode.Expanded)
        {
          if (_focusNode.VC.CanCollapseNode(this, _focusNode, true, false))
          {
            _focusNode.Collapse();
          }
        }
        else
        {
          Node parent = _focusNode.Parent;
          if (parent != null)
          {
            if (SelectMode == SelectMode.None)
            {
              SetFocusNode(parent);
            }
            else
            {
              SingleSelect(parent);
            }
          }
        }
      }
      return true;
    }
    private bool ProcessRightKey(bool shiftKey, bool controlKey)
    {
      if (!shiftKey && !controlKey && _focusNode != null)
      {
        if (!_focusNode.Expanded)
        {
          if (_focusNode.VC.CanExpandNode(this, _focusNode, true, false))
          {
            _focusNode.Expand();
          }
        }
        else
        {
          Node firstDisplayedNode = _focusNode.Nodes.GetFirstDisplayedNode();
          if (firstDisplayedNode != null)
          {
            if (SelectMode == SelectMode.None)
            {
              SetFocusNode(firstDisplayedNode);
            }
            else
            {
              SingleSelect(firstDisplayedNode);
            }
          }
        }
      }
      return true;
    }
    private bool ProcessHomeKey(bool shiftKey, bool controlKey)
    {
      Node firstDisplayedNode = Nodes.GetFirstDisplayedNode();
      if (firstDisplayedNode != null)
      {
        if (controlKey || SelectMode == SelectMode.None)
        {
          SetFocusNode(firstDisplayedNode);
        }
        else
        {
          SelectNode(firstDisplayedNode, shiftKey, controlKey);
        }
      }
      return true;
    }
    private bool ProcessEndKey(bool shiftKey, bool controlKey)
    {
      Node lastDisplayedNode = Nodes.GetLastDisplayedNode();
      if (lastDisplayedNode != null)
      {
        if (controlKey || SelectMode == SelectMode.None)
        {
          SetFocusNode(lastDisplayedNode);
        }
        else
        {
          SelectNode(lastDisplayedNode, shiftKey, controlKey);
        }
      }
      return true;
    }
    private bool ProcessPageUpKey(bool shiftKey, bool controlKey)
    {
      Node node = null;
      if (_focusNode != null)
      {
        int num = FindFirstFullDisplayedNode(_drawRectangle);
        if (num >= 0)
        {
          if (_displayNodes.IndexOf(_focusNode) > num)
          {
            node = (_displayNodes[num] as Node);
          }
          else
          {
            node = OutsidePageUpNode(_focusNode, _drawRectangle);
          }
        }
      }
      else
      {
        node = Nodes.GetFirstDisplayedNode();
      }
      if (node != null)
      {
        if (controlKey || SelectMode == SelectMode.None)
        {
          SetFocusNode(node);
        }
        else
        {
          SelectNode(node, shiftKey, controlKey);
        }
      }
      return true;
    }
    private bool ProcessPageDownKey(bool shiftKey, bool controlKey)
    {
      Node node = null;
      if (_focusNode != null)
      {
        int num = FindLastFullDisplayedNode(_drawRectangle);
        if (num >= 0)
        {
          if (_displayNodes.IndexOf(_focusNode) < num)
          {
            node = (_displayNodes[num] as Node);
          }
          else
          {
            node = OutsidePageDownNode(_focusNode, _drawRectangle);
          }
        }
      }
      else
      {
        node = Nodes.GetLastDisplayedNode();
      }
      if (node != null)
      {
        if (controlKey || SelectMode == SelectMode.None)
        {
          SetFocusNode(node);
        }
        else
        {
          SelectNode(node, shiftKey, controlKey);
        }
      }
      return true;
    }
    private bool ProcessPlusKey()
    {
      if (_focusNode != null && !_focusNode.Expanded && _focusNode.VC.CanExpandNode(this, _focusNode, true, false))
      {
        _focusNode.Expand();
      }
      return true;
    }
    private bool ProcessMinusKey()
    {
      if (_focusNode != null && _focusNode.Expanded && _focusNode.VC.CanCollapseNode(this, _focusNode, true, false))
      {
        _focusNode.Collapse();
      }
      return true;
    }
    private void CtrlSelect(Node n)
    {
      bool flag = false;
      bool flag2 = false;
      if (n.VC.CanSelectNode(this, n))
      {
        if (!_selected.ContainsKey(n))
        {
          if (OnBeforeSelect(n))
          {
            _selected.Add(n, n);
            flag = true;
          }
        }
        else
        {
          _selected.Remove(n);
          flag2 = true;
        }
        _lastSelectedNode = n;
        InvalidateNode(n);
      }
      SetFocusNode(n);
      if (flag)
      {
        OnAfterSelect(n);
      }
      if (flag2)
      {
        OnAfterDeselect(n);
      }
    }
    private void ShiftSelect(Node n)
    {
      if (_lastSelectedNode != null)
      {
        if (IsNodeDisplayed(_lastSelectedNode))
        {
          if (_lastSelectedNode == n)
          {
            SingleSelect(n);
            SetFocusNode(n);
            goto IL_18B;
          }
          Hashtable hashtable = (Hashtable)_selected.Clone();
          ClearSelection();
          int[] nodeLocation = GetNodeLocation(_lastSelectedNode);
          int[] nodeLocation2 = GetNodeLocation(n);
          Node node = n;
          Node node2 = _lastSelectedNode;
          if (LocationIsBefore(nodeLocation, nodeLocation2))
          {
            node = _lastSelectedNode;
            node2 = n;
          }
          bool flag = false;
          ArrayList arrayList = new ArrayList();
          do
          {
            bool flag2 = hashtable.ContainsKey(node);
            if (node.VC.CanSelectNode(this, node) && (flag2 || OnBeforeSelect(node)))
            {
              _selected.Add(node, node);
              InvalidateNode(node);
              if (!flag2)
              {
                arrayList.Add(node);
              }
            }
            if (flag)
            {
              break;
            }
            node = node.NextDisplayedNode;
            if (node == node2)
            {
              flag = true;
            }
          }
          while (node != null);
          SetFocusNode(n);
          IEnumerator enumerator = hashtable.Keys.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              Node node3 = (Node)enumerator.Current;
              if (!_selected.Contains(node3))
              {
                OnAfterDeselect(node3);
              }
            }
          }
          finally
          {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable != null)
            {
              disposable.Dispose();
            }
          }
          enumerator = arrayList.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              Node n2 = (Node)enumerator.Current;
              OnAfterSelect(n2);
            }
            goto IL_18B;
          }
          finally
          {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable != null)
            {
              disposable.Dispose();
            }
          }
        }
        _lastSelectedNode = null;
      }
    IL_18B:
      if (_lastSelectedNode == null)
      {
        CtrlSelect(n);
      }
    }
    private void SingleSelect(Node n)
    {
      bool flag = false;
      bool flag2 = _selected.ContainsKey(n);
      Hashtable hashtable = (Hashtable)_selected.Clone();
      ClearSelection();
      if (n.VC.CanSelectNode(this, n) && (flag2 || OnBeforeSelect(n)))
      {
        _selected.Add(n, n);
        _lastSelectedNode = n;
        InvalidateNode(n);
        if (!flag2)
        {
          flag = true;
        }
        else if (_focusNode != n)
        {
          flag = true;
        }
      }
      if (flag)
      {
        SetFocusNode(n);
      }
      IEnumerator enumerator = _selected.Keys.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node = (Node)enumerator.Current;
          hashtable.Remove(node);
        }
      }
      finally
      {
        IDisposable disposable = enumerator as IDisposable;
        if (disposable != null)
        {
          disposable.Dispose();
        }
      }
      if (hashtable.Count > 0)
      {
        enumerator = hashtable.Keys.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            Node n2 = (Node)enumerator.Current;
            OnAfterDeselect(n2);
          }
        }
        finally
        {
          IDisposable disposable = enumerator as IDisposable;
          if (disposable != null)
          {
            disposable.Dispose();
          }
        }
      }
      if (flag)
      {
        OnAfterSelect(n);
      }
    }
    private int[] GetNodeLocation(Node n)
    {
      ArrayList arrayList = new ArrayList();
      while (n != null)
      {
        arrayList.Insert(0, n.Index);
        n = n.Parent;
      }
      int[] array = new int[arrayList.Count];
      for (int i = 0; i < arrayList.Count; i++)
      {
        array[i] = (int)arrayList[i];
      }
      return array;
    }
    private bool LocationIsBefore(int[] first, int[] second)
    {
      int num = (first.Length < second.Length) ? first.Length : second.Length;
      for (int i = 0; i < num; i++)
      {
        if (first[i] < second[i])
        {
          return true;
        }
        if (first[i] > second[i])
        {
          return false;
        }
      }
      return first.Length < second.Length;
    }
    private void InvalidateFocus()
    {
      if (_focusNode != null)
      {
        InvalidateNode(_focusNode);
      }
    }
    private void ClearLineBoxCache()
    {
      if (_cacheLineDashPen != null)
      {
        _cacheLineDashPen.Dispose();
        _cacheLineDashPen = null;
      }
      if (_cacheBoxSignPen != null)
      {
        _cacheBoxSignPen.Dispose();
        _cacheBoxSignPen = null;
      }
      if (_cacheBoxBorderPen != null)
      {
        _cacheBoxBorderPen.Dispose();
        _cacheBoxBorderPen = null;
      }
      if (_cacheBoxInsideBrush != null)
      {
        _cacheBoxInsideBrush.Dispose();
        _cacheBoxInsideBrush = null;
      }
    }
    private void ClearCheckCache()
    {
      if (_cacheCheckTickPen != null)
      {
        _cacheCheckTickPen.Dispose();
        _cacheCheckTickPen = null;
      }
      if (_cacheCheckTickBrush != null)
      {
        _cacheCheckTickBrush.Dispose();
        _cacheCheckTickBrush = null;
      }
      if (_cacheCheckTickHotPen != null)
      {
        _cacheCheckTickHotPen.Dispose();
        _cacheCheckTickHotPen = null;
      }
      if (_cacheCheckTickHotBrush != null)
      {
        _cacheCheckTickHotBrush.Dispose();
        _cacheCheckTickHotBrush = null;
      }
      if (_cacheCheckBorderBrush != null)
      {
        _cacheCheckBorderBrush.Dispose();
        _cacheCheckBorderBrush = null;
      }
      if (_cacheCheckMixedBrush != null)
      {
        _cacheCheckMixedBrush.Dispose();
        _cacheCheckMixedBrush = null;
      }
      if (_cacheCheckMixedHotBrush != null)
      {
        _cacheCheckMixedHotBrush.Dispose();
        _cacheCheckMixedHotBrush = null;
      }
      if (_cacheCheckInsideBrush != null)
      {
        _cacheCheckInsideBrush.Dispose();
        _cacheCheckInsideBrush = null;
      }
      if (_cacheCheckInsideHotBrush != null)
      {
        _cacheCheckInsideHotBrush.Dispose();
        _cacheCheckInsideHotBrush = null;
      }
    }
    private void ClearGroupCache()
    {
      if (_cacheGroupLinePen != null)
      {
        _cacheGroupLinePen.Dispose();
        _cacheGroupLinePen = null;
      }
      if (_cacheGroupImageBoxLinePen != null)
      {
        _cacheGroupImageBoxLinePen.Dispose();
        _cacheGroupImageBoxLinePen = null;
      }
      if (_cacheGroupImageBoxColumnBrush != null)
      {
        _cacheGroupImageBoxColumnBrush.Dispose();
        _cacheGroupImageBoxColumnBrush = null;
      }
    }
    private void ClearNodeCache()
    {
      if (_cacheHotBackBrush != null)
      {
        _cacheHotBackBrush.Dispose();
        _cacheHotBackBrush = null;
      }
      if (_cacheSelectedBackBrush != null)
      {
        _cacheSelectedBackBrush.Dispose();
        _cacheSelectedBackBrush = null;
      }
      if (_cacheSelectedNoFocusBackBrush != null)
      {
        _cacheSelectedNoFocusBackBrush.Dispose();
        _cacheSelectedNoFocusBackBrush = null;
      }
    }
    private void CreateChildControls()
    {
      _vBar = new VScrollBar();
      _hBar = new HScrollBar();
      _vBar.ValueChanged += OnVertValueChanged;
      _hBar.ValueChanged += OnHorzValueChanged;
      _corner = new Panel();
      _corner.BackColor = SystemColors.Control;
      _vBar.Visible = false;
      _hBar.Visible = false;
      _corner.Visible = false;
      base.Controls.AddRange(new Control[]
			{
				_vBar,
				_hBar,
				_corner
			});
    }
    private void InvalidateInnerRectangle()
    {
      _innerRectangleValid = false;
      _scrollBarsValid = false;
      InvalidateDrawRectangle();
    }
    private void InvalidateDrawRectangle()
    {
      _drawRectangleValid = false;
      InvalidateAll();
    }
    private void SetAllNodeSizesDirty(NodeCollection nc)
    {
      IEnumerator enumerator = nc.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node = (Node)enumerator.Current;
          node.Cache.InvalidateSize();
          SetAllNodeSizesDirty(node.Nodes);
        }
      }
      finally
      {
        IDisposable disposable = enumerator as IDisposable;
        if (disposable != null)
        {
          disposable.Dispose();
        }
      }
    }
    private void CalculationCycle(Graphics g)
    {
      CalculateAllNodes(g);
      CalculateInnerRectangle();
      UpdateScrollBars();
      CalculateDrawRectangle();
      PostCalculateNodes();
    }
    private void CalculateAllNodes(Graphics g)
    {
      if (!_nodeDrawingValid)
      {
        _displayNodes.Clear();
        if (_nodeSizesDirty)
        {
          SetAllNodeSizesDirty(Nodes);
          _nodeSizesDirty = false;
        }
        CalculateNodeCollection(g, Nodes, Point.Empty);
      }
    }
    private Rectangle CalculateNodeCollection(Graphics g, NodeCollection nc, Point topLeft)
    {
      Edges edges = nc.VC.MeasureEdges(this, nc, g);
      Point point = new Point(topLeft.X, topLeft.Y);
      point.X = point.X + edges.Left;
      point.Y = point.Y + edges.Top;
      if (_displayNodes.Count > 0 && nc.VisibleCount > 0)
      {
        point.Y = point.Y + VerticalNodeGap;
      }
      int num = 0;
      IEnumerator enumerator = nc.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node = (Node)enumerator.Current;
          if (node.Visible)
          {
            _displayNodes.Add(node);
            Size size = CalculateNode(g, node, point);
            point.Y = point.Y + size.Height;
            point.Y = point.Y + VerticalNodeGap;
            if (size.Width > num)
            {
              num = size.Width;
            }
          }
          else
          {
            node.Cache.Size = Size.Empty;
            node.VC.SetPosition(this, node, point);
            node.Cache.ChildBounds = node.Cache.Bounds;
          }
        }
      }
      finally
      {
        IDisposable disposable = enumerator as IDisposable;
        if (disposable != null)
        {
          disposable.Dispose();
        }
      }
      Rectangle rectangle = new Rectangle(topLeft.X, topLeft.Y, num + edges.Left + edges.Right, point.Y - topLeft.Y + edges.Bottom);
      nc.VC.SetBounds(this, nc, rectangle);
      return rectangle;
    }
    private Size CalculateNode(Graphics g, Node n, Point nodeTopLeft)
    {
      INodeVC vC = n.VC;
      Size size = vC.MeasureSize(this, n, g);
      n.Cache.Size = size;
      Rectangle bounds = vC.SetPosition(this, n, nodeTopLeft);
      if (n.IsExpanded)
      {
        Rectangle rectangle = CalculateNodeCollection(g, n.Nodes, new Point(bounds.Left, bounds.Bottom));
        bounds.Height = bounds.Height + rectangle.Height;
        if (bounds.Width < rectangle.Width)
        {
          bounds.Width = rectangle.Width;
        }
      }
      vC.SetChildBounds(this, n, bounds);
      return bounds.Size;
    }
    private void CalculateInnerRectangle()
    {
      if (!_innerRectangleValid)
      {
        _innerRectangle = base.ClientRectangle;
        _innerRectangle.Inflate(-_borderSize.Width, -_borderSize.Height);
        _innerRectangle.Y = _innerRectangle.Y + _borderIndent.Top;
        _innerRectangle.X = _innerRectangle.X + _borderIndent.Left;
        _innerRectangle.Width = _innerRectangle.Width - (_borderIndent.Left + _borderIndent.Right);
        _innerRectangle.Height = _innerRectangle.Height - (_borderIndent.Top + _borderIndent.Bottom);
        _innerRectangleValid = true;
      }
    }
    private void UpdateScrollBars()
    {
      if (!_scrollBarsValid)
      {
        Size size;
        Size size2;
        if (VerticalGranularity == VerticalGranularity.Node)
        {
          CalculateVerticalPageSizes();
          size = new Size(Nodes.Cache.Bounds.Size.Width, _displayNodes.Count);
          size2 = new Size(_innerRectangle.Size.Width, _displayHeightExScroll);
        }
        else
        {
          size = Nodes.Cache.Bounds.Size;
          size2 = _innerRectangle.Size;
        }
        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        bool flag4 = false;
        bool flag5 = VerticalScrollbar == ScrollVisibility.Always;
        bool flag6 = VerticalScrollbar == ScrollVisibility.Never;
        bool flag7 = HorizontalScrollbar == ScrollVisibility.Always;
        bool flag8 = HorizontalScrollbar == ScrollVisibility.Never;
        if (flag5)
        {
          size2.Width = size2.Width - SystemInformation.VerticalScrollBarWidth;
        }
        if (flag7)
        {
          if (VerticalGranularity == VerticalGranularity.Node)
          {
            size2.Height = _displayHeightScroll;
          }
          else
          {
            size2.Height = size2.Height - SystemInformation.HorizontalScrollBarHeight;
          }
        }
        Point empty = Point.Empty;
        if (size.Height > size2.Height)
        {
          flag = true;
          flag3 = true;
          if (!flag5 && !flag6)
          {
            size2.Width = size2.Width - SystemInformation.VerticalScrollBarWidth;
          }
        }
        if (size.Width > size2.Width)
        {
          flag2 = true;
          flag4 = true;
          if (!flag7 && !flag8)
          {
            if (VerticalGranularity == VerticalGranularity.Node)
            {
              size2.Height = _displayHeightScroll;
            }
            else
            {
              size2.Height = size2.Height - SystemInformation.HorizontalScrollBarHeight;
            }
          }
          if (!flag && size.Height > size2.Height)
          {
            flag = true;
            flag3 = true;
            if (!flag5 && !flag5)
            {
              size2.Width = size2.Width - SystemInformation.VerticalScrollBarWidth;
            }
          }
        }
        if (flag)
        {
          empty.Y = size.Height - size2.Height;
          if (_offset.Y > empty.Y)
          {
            _offset.Y = empty.Y;
          }
        }
        else
        {
          _offset.Y = 0;
          empty.Y = 0;
        }
        if (flag2)
        {
          empty.X = size.Width - size2.Width;
          if (_offset.X > empty.X)
          {
            _offset.X = empty.X;
          }
        }
        else
        {
          _offset.X = 0;
          empty.X = 0;
        }
        Size empty2 = Size.Empty;
        Size empty3 = Size.Empty;
        if (flag)
        {
          if (size.Height > 1 && size2.Height > 1)
          {
            _vBar.Minimum = 0;
            _vBar.Maximum = size.Height - 1;
            _vBar.SmallChange = 1;
            _vBar.LargeChange = size2.Height;
            _vBar.Value = _offset.Y;
          }
          else
          {
            _vBar.Minimum = 0;
            _vBar.Maximum = 0;
            _vBar.Value = 0;
            flag = false;
          }
        }
        if (flag2)
        {
          if (size.Width > 1 && size2.Width > 1)
          {
            _hBar.Minimum = 0;
            _hBar.Maximum = size.Width - 1;
            _hBar.SmallChange = 1;
            _hBar.LargeChange = size2.Width;
            _hBar.Value = _offset.X;
          }
          else
          {
            _hBar.Minimum = 0;
            _hBar.Maximum = 0;
            _hBar.Value = 0;
            flag2 = false;
          }
        }
        switch (VerticalScrollbar)
        {
          case ScrollVisibility.Never:
            flag = false;
            break;
          case ScrollVisibility.Always:
            flag = true;
            break;
        }
        switch (HorizontalScrollbar)
        {
          case ScrollVisibility.Never:
            flag2 = false;
            break;
          case ScrollVisibility.Always:
            flag2 = true;
            break;
        }
        if (flag)
        {
          _vBar.Location = new Point(base.Width - _vBar.Width - _borderSize.Width, _borderSize.Height);
          empty2 = new Size(_vBar.Width, base.Height - _borderSize.Height * 2);
        }
        if (flag2)
        {
          _hBar.Location = new Point(_borderSize.Width, base.Height - _hBar.Height - _borderSize.Height);
          empty3 = new Size(base.Width - _borderSize.Width * 2, _hBar.Height);
          if (flag)
          {
            empty2.Height = empty2.Height - _hBar.Height;
            empty3.Width = empty3.Width - _vBar.Width;
          }
        }
        if (empty2 != Size.Empty)
        {
          _vBar.Size = empty2;
        }
        if (empty3 != Size.Empty)
        {
          _hBar.Size = empty3;
        }
        if (_vBar.Enabled != flag3)
        {
          _vBar.Enabled = flag3;
        }
        if (_hBar.Enabled != flag4)
        {
          _hBar.Enabled = flag4;
        }
        if (flag && flag2)
        {
          _corner.Size = new Size(_vBar.Width, _hBar.Height);
          _corner.Location = new Point(_vBar.Left, _hBar.Top);
        }
        _corner.Visible = flag && flag2;
        if (_vBar.Visible != flag)
        {
          _vBar.Visible = flag;
          InvalidateDrawRectangle();
        }
        if (_hBar.Visible != flag2)
        {
          _hBar.Visible = flag2;
          InvalidateDrawRectangle();
        }
        _scrollBarsValid = true;
      }
    }
    private void CalculateDrawRectangle()
    {
      if (!_drawRectangleValid)
      {
        _drawRectangle = _innerRectangle;
        if (_vBar.Visible)
        {
          _drawRectangle.Width = _drawRectangle.Width - _vBar.Width;
        }
        if (_hBar.Visible)
        {
          _drawRectangle.Height = _drawRectangle.Height - _hBar.Height;
        }
        _drawRectangleValid = true;
      }
    }
    private void PostCalculateNodes()
    {
      if (!_nodeDrawingValid)
      {
        NodeVC.PostCalculateNodes(this, _displayNodes);
        CollectionVC.PostCalculateNodes(this, _displayNodes);
        _nodeDrawingValid = true;
      }
    }
    private void DrawControlBorder(PaintEventArgs e)
    {
      if (BorderStyle != TreeBorderStyle.None)
      {
        if (BorderStyle == TreeBorderStyle.Theme && IsControlThemed)
        {
          ThemeTreeViewItemState state;
          if (base.Enabled)
          {
            state = ThemeTreeViewItemState.Normal;
          }
          else
          {
            state = ThemeTreeViewItemState.Disabled;
          }
          _themeTreeView.DrawThemeBackground(e.Graphics, base.ClientRectangle, _drawRectangle, 1, (int)state);
          return;
        }
        if (_borderIs3D)
        {
          ControlPaint.DrawBorder3D(e.Graphics, base.ClientRectangle, _border3DStyle);
          return;
        }
        ControlPaint.DrawBorder(e.Graphics, base.ClientRectangle, BorderColor, _borderButtonStyle);
      }
    }
    private void DrawAllNodes(PaintEventArgs e)
    {
      DrawNodeCollection(e.Graphics, Nodes);
    }
    private void DrawNodeCollection(Graphics g, NodeCollection nc)
    {
      INodeCollectionVC vC = nc.VC;
      if (vC.IntersectsWith(this, nc, _clipRectangle))
      {
        vC.Draw(this, nc, g, _clipRectangle, true);
        int num = nc.ChildFromY(_clipRectangle.Top);
        while (num < nc.Count && nc[num].Cache.ChildBounds.Top <= _clipRectangle.Bottom)
        {
          if (nc[num].Visible)
          {
            DrawNode(g, nc[num]);
          }
          num++;
        }
        vC.Draw(this, nc, g, _clipRectangle, false);
      }
    }
    private void DrawNode(Graphics g, Node n)
    {
      if (n.Visible)
      {
        INodeVC vC = n.VC;
        if (vC.IntersectsWith(this, n, _clipRectangle, false))
        {
          vC.Draw(this, n, g, _clipRectangle, 0, 0);
        }
        if (vC.IntersectsWith(this, n, _clipRectangle, true) && n.IsExpanded)
        {
          DrawNodeCollection(g, n.Nodes);
        }
      }
    }
    private void SetTooltipNode(Node n)
    {
      if (n != _tooltipNode)
      {
        if (_tooltipNode != null)
        {
          RemoveAnyToolTip();
        }
        if (n != null && n.VC != null && !n.VC.CanToolTip(this, n))
        {
          n = null;
        }
        _tooltipNode = n;
        if (_tooltipNode != null)
        {
          TestForNeedingToolTip();
        }
      }
    }
    private void SetHotNode(Node n)
    {
      if (n != _hotNode)
      {
        if (_hotNode != null)
        {
          InvalidateNodeLine(_hotNode, false);
        }
        _hotNode = n;
        if (_hotNode != null)
        {
          InvalidateNodeLine(_hotNode, false);
        }
        OnHotNodeChanged();
        return;
      }
      if (_hotNode != null)
      {
        InvalidateNode(_hotNode, false);
      }
    }
    private void RemoveAnyToolTip()
    {
      if (_infotipTimer.Enabled)
      {
        _infotipTimer.Stop();
      }
      if (_tooltip != null)
      {
        _tooltip.Hide();
        _tooltip.Dispose();
        _tooltip = null;
        _tooltipNode = null;
      }
    }
    private void TestForNeedingToolTip()
    {
      if (Tooltips || Infotips)
      {
        Form form = base.FindForm();
        if (form == null || (form != null && form.ContainsFocus))
        {
          Node tooltipNode = TooltipNode;
          if (tooltipNode != null)
          {
            if (Infotips && tooltipNode.Tooltip != null && tooltipNode.Tooltip.Length > 0)
            {
              _infotipTimer.Start();
              return;
            }
            if (Tooltips && tooltipNode.Cache != null && tooltipNode.VC != null)
            {
              Rectangle rectangle = NodeSpaceToClient(tooltipNode.Cache.Bounds);
              if (rectangle.Left < _drawRectangle.Left || rectangle.Right > _drawRectangle.Right)
              {
                _tooltip = new PopupTooltipSingle(VisualStyle.Plain, tooltipNode.GetNodeFont());
                Rectangle rectangle2 = tooltipNode.VC.GetTextRectangle(this, tooltipNode);
                rectangle2 = NodeSpaceToClient(rectangle2);
                rectangle2 = base.RectangleToScreen(rectangle2);
                _tooltip.TextHeight = rectangle2.Height;
                _tooltip.ToolText = tooltipNode.Text;
                _tooltip.ShowWithoutFocus(new Point(rectangle2.X - 3, rectangle2.Y));
              }
            }
          }
        }
      }
    }
    private void CalculateVerticalPageSizes()
    {
      if (VerticalGranularity == VerticalGranularity.Node)
      {
        int height = _innerRectangle.Size.Height;
        int num = height - SystemInformation.HorizontalScrollBarHeight;
        _displayHeightExScroll = 0;
        _displayHeightScroll = 0;
        if (_displayNodes.Count > 0)
        {
          int bottom = Nodes.Cache.Bounds.Bottom;
          for (int i = _displayNodes.Count - 1; i >= 0; i--)
          {
            int num2 = bottom - (_displayNodes[i] as Node).Cache.Bounds.Top;
            if (num >= num2)
            {
              _displayHeightScroll++;
              _displayHeightExScroll++;
            }
            else
            {
              if (height < num2)
              {
                break;
              }
              _displayHeightExScroll++;
            }
          }
        }
        if (_displayHeightExScroll == 0)
        {
          _displayHeightExScroll = 1;
        }
        if (_displayHeightScroll == 0)
        {
          _displayHeightScroll = 1;
        }
      }
    }
    private void OnDragHoverTimeout(object sender, EventArgs e)
    {
      _dragHoverTimer.Stop();
      if (_dragNode != null)
      {
        _dragNode.VC.DragHover(this, _dragNode);
      }
    }
    private void OnDragBumpTimeout(object sender, EventArgs e)
    {
      if (!_bumpUpwards)
      {
        _offset.Y = _offset.Y + 1;
        InvalidateNodeDrawing();
        return;
      }
      if (_offset.Y == 0)
      {
        _dragBumpTimer.Stop();
        return;
      }
      _offset.Y = _offset.Y - 1;
      InvalidateNodeDrawing();
    }
    private void OnAutoEditTimeout(object sender, EventArgs e)
    {
      _autoEditTimer.Stop();
      if (_autoEditNode != null)
      {
        if (SelectedCount == 1 && _autoEditNode.IsSelected)
        {
          _autoEditNode.BeginEdit();
        }
        _autoEditNode = null;
      }
    }
    private void OnInfoTipTimeout(object sender, EventArgs e)
    {
      _infotipTimer.Stop();
      _tooltip = new PopupTooltipSingle(VisualStyle.IDE);
      _tooltip.ToolText = TooltipNode.Tooltip;
      _tooltip.ShowWithoutFocus(new Point(Control.MousePosition.X, Control.MousePosition.Y + 24));
    }
    private void OnLabelEditBoxLostFocus(object sender, EventArgs e)
    {
      EndEditLabel(false);
    }
    private void OnLabelEditBoxKeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Escape)
      {
        EndEditLabel(true);
      }
      if (e.KeyCode == Keys.Enter)
      {
        EndEditLabel(false);
      }
    }
    private void OnLabelEditTextChanged(object sender, EventArgs e)
    {
      using (Graphics graphics = _labelEditBox.CreateGraphics())
      {
        SizeF sizeF = graphics.MeasureString(_labelEditBox.Text + "W", _labelEditBox.Font);
        SizeF sizeF2 = graphics.MeasureString("01234", _labelEditBox.Font);
        if (sizeF2.Width > sizeF.Width)
        {
          sizeF.Width = sizeF2.Width;
        }
        Rectangle rectangle = new Rectangle(_labelEditBox.Location, _labelEditBox.Size);
        rectangle.Width = (int)sizeF.Width + SystemInformation.BorderSize.Width * 2;
        if (rectangle.Right > _innerRectangle.Right)
        {
          rectangle.Width = rectangle.Width - (rectangle.Right - _innerRectangle.Right);
        }
        _labelEditBox.SetBounds(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
      }
    }
    private void OnVertValueChanged(object sender, EventArgs e)
    {
      _offset.Y = _vBar.Value;
      if (LabelEditNode != null)
      {
        EndEditLabel(false);
      }
      Refresh();
    }
    private void OnHorzValueChanged(object sender, EventArgs e)
    {
      _offset.X = _hBar.Value;
      if (LabelEditNode != null)
      {
        EndEditLabel(false);
      }
      Refresh();
    }
    private void UpdateBorderCache(Graphics g)
    {
      if (BorderStyle == TreeBorderStyle.None)
      {
        _borderSize = Size.Empty;
        return;
      }
      bool flag = true;
      switch (BorderStyle)
      {
        case TreeBorderStyle.Theme:
          if (IsControlThemed)
          {
            Size themePartSize = _themeTreeView.GetThemePartSize(g, 1, 1, THEMESIZE.TS_TRUE);
            _borderSize = new Size(themePartSize.Width / 2 + 1, themePartSize.Height / 2 + 1);
            flag = false;
          }
          else
          {
            _borderIs3D = true;
            _border3DStyle = Border3DStyle.Sunken;
          }
          break;
        case TreeBorderStyle.Solid:
          _borderIs3D = false;
          _borderButtonStyle = ButtonBorderStyle.Solid;
          break;
        case TreeBorderStyle.Dashed:
          _borderIs3D = false;
          _borderButtonStyle = ButtonBorderStyle.Dashed;
          break;
        case TreeBorderStyle.Dotted:
          _borderIs3D = false;
          _borderButtonStyle = ButtonBorderStyle.Dotted;
          break;
        case TreeBorderStyle.Flat3D:
          _borderIs3D = true;
          _border3DStyle = Border3DStyle.Flat;
          break;
        case TreeBorderStyle.Bump3D:
          _borderIs3D = true;
          _border3DStyle = Border3DStyle.Bump;
          break;
        case TreeBorderStyle.Etched3D:
          _borderIs3D = true;
          _border3DStyle = Border3DStyle.Etched;
          break;
        case TreeBorderStyle.Adjust3D:
          _borderIs3D = true;
          _border3DStyle = Border3DStyle.Adjust;
          break;
        case TreeBorderStyle.Raised3D:
          _borderIs3D = true;
          _border3DStyle = Border3DStyle.Raised;
          break;
        case TreeBorderStyle.RaisedInner3D:
          _borderIs3D = true;
          _border3DStyle = Border3DStyle.RaisedInner;
          break;
        case TreeBorderStyle.RaisedOuter3D:
          _borderIs3D = true;
          _border3DStyle = Border3DStyle.RaisedOuter;
          break;
        case TreeBorderStyle.Sunken3D:
          _borderIs3D = true;
          _border3DStyle = Border3DStyle.Sunken;
          break;
        case TreeBorderStyle.SunkenInner3D:
          _borderIs3D = true;
          _border3DStyle = Border3DStyle.SunkenInner;
          break;
        case TreeBorderStyle.SunkenOuter3D:
          _borderIs3D = true;
          _border3DStyle = Border3DStyle.SunkenOuter;
          break;
      }
      if (flag)
      {
        if (_borderIs3D)
        {
          _borderSize = SystemInformation.Border3DSize;
          return;
        }
        _borderSize = SystemInformation.BorderSize;
      }
    }
    private void OnBorderIndentChanged(object sender, EventArgs e)
    {
      InvalidateInnerRectangle();
    }
    private void OnPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
      _colorDetails.Reset();
      InvalidateAll();
    }
    private void InvalidateAll()
    {
      if (!_invalidated)
      {
        _invalidated = true;
        base.Invalidate();
      }
    }
    private void OnThemeChanged(object sender, EventArgs e)
    {
      using (Graphics graphics = base.CreateGraphics())
      {
        UpdateBorderCache(graphics);
        if (IsControlThemed)
        {
          _glyphThemeSize = _themeTreeView.GetThemePartSize(graphics, 2, 1, THEMESIZE.TS_TRUE);
        }
      }
      ClearLineBoxCache();
      ClearCheckCache();
      ClearGroupCache();
      ClearNodeCache();
      InvalidateInnerRectangle();
      InvalidateNodeDrawing();
    }
  }
}
