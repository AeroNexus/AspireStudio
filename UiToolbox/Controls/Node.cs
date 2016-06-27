using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Aspire.UiToolbox.Controls
{
  [DefaultEvent("TextChanged"), DefaultProperty("Text"), ToolboxItem(false)]
  public class Node : Component, ICloneable
  {
    [Flags]
    private enum Flags
    {
      Expanded = 1,
      Visible = 2,
      Selectable = 4,
      Removing = 8
    }
    private const string _textDefault = "Node";
    private NodeCache _cache;
    private NodeCollection _parentNodes;
    private string _text;
    private string _tooltip;
    private Font _nodeFont;
    private Font _nodeFontBoldItalic;
    private int _nodeFontHeight;
    private Color _backColor;
    private Color _foreColor;
    private int _imageIndex;
    private int _selectedImageIndex;
    private Icon _icon;
    private Icon _selectedIcon;
    private Image _image;
    private Image _selectedImage;
    private CheckState _checkState;
    private NodeCheckStates _checkStates;
    private Indicator _indicator;
    private Node.Flags _flags;
    private object _key;
    private object _tag;
    private NodeCollection _nodes;
    private INodeVC _vc;
    private Node _original;

    [Category("Appearance")]
    public event EventHandler TextChanged;

    [Category("Appearance")]
    public event EventHandler TooltipChanged;

    [Category("Appearance")]
    public event EventHandler NodeFontChanged;

    [Category("Appearance")]
    public event EventHandler BackColorChanged;

    [Category("Appearance")]
    public event EventHandler ForeColorChanged;

    [Category("Appearance")]
    public event EventHandler ImageIndexChanged;

    [Category("Appearance")]
    public event EventHandler SelectedImageIndexChanged;

    [Category("Appearance")]
    public event EventHandler IconChanged;

    [Category("Appearance")]
    public event EventHandler SelectedIconChanged;

    [Category("Appearance")]
    public event EventHandler ImageChanged;

    [Category("Appearance")]
    public event EventHandler SelectedImageChanged;

    [Category("Appearance")]
    public event EventHandler IndicatorChanged;

    [Category("Appearance")]
    public event EventHandler CheckStateChanged;

    [Category("Appearance")]
    public event EventHandler CheckStatesChanged;

    [Category("Behavior")]
    public event EventHandler VisibleChanged;

    [Category("Behavior")]
    public event EventHandler SelectableChanged;

    [Category("Behavior")]
    public event EventHandler ExpandedChanged;

    [Category("Data")]
    public event EventHandler KeyChanged;

    [Category("Data")]
    public event EventHandler TagChanged;

    [Browsable(false)]
    public event EventHandler VCChanged;

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public Node Parent
    {
      get
      {
        return Cache.ParentNode;
      }
    }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public NodeCollection ParentNodes
    {
      get
      {
        return _parentNodes;
      }
    }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public TreeControl TreeControl
    {
      get
      {
        return Cache.TreeControl;
      }
    }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public INodeVC VC
    {
      get
      {
        if (_vc != null)
        {
          return _vc;
        }
        if (Cache.TreeControl != null)
        {
          return Cache.TreeControl.NodeVC;
        }
        return null;
      }
      set
      {
        if (_vc != value)
        {
          _vc = value;
          this.OnVCChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Browsable(false), Category("Data"), Description("The collection of child nodes in the node."),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public NodeCollection Nodes
    {
      get
      {
        return _nodes;
      }
    }
    [Category("Appearance"), Description("The text contained in the node."), Localizable(true)]
    public string Text
    {
      get
      {
        return _text;
      }
      set
      {
        if (_text != value)
        {
          _text = value;
          Cache.InvalidateSize();
          this.OnTextChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Category("Appearance"), DefaultValue(""), Description("The tooltip for the node."), Localizable(true)]
    public string Tooltip
    {
      get
      {
        return _tooltip;
      }
      set
      {
        if (_tooltip != value)
        {
          _tooltip = value;
          this.OnTooltipChanged();
        }
      }
    }
    [Category("Appearance"), Description("The font used to display Node text.")]
    public Font NodeFont
    {
      get
      {
        if (_nodeFont != null)
        {
          return _nodeFont;
        }
        if (Cache.TreeControl != null)
        {
          return Cache.TreeControl.Font;
        }
        return null;
      }
      set
      {
        if (_nodeFont != value)
        {
          _nodeFont = value;
          _nodeFontBoldItalic = null;
          _nodeFontHeight = -1;
          Cache.InvalidateSize();
          this.OnNodeFontChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Category("Appearance"), Description("The foreground color used to draw text and graphics.")]
    public Color ForeColor
    {
      get
      {
        if (_foreColor != Color.Empty)
        {
          return _foreColor;
        }
        if (Cache.TreeControl != null)
        {
          return Cache.TreeControl.ForeColor;
        }
        return Color.Empty;
      }
      set
      {
        if (_foreColor != value)
        {
          _foreColor = value;
          Cache.InvalidateSize();
          OnForeColorChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Category("Appearance"), Description("The background color used to draw text and graphics.")]
    public Color BackColor
    {
      get
      {
        if (_backColor != Color.Empty)
        {
          return _backColor;
        }
        if (Cache.TreeControl != null)
        {
          return Cache.TreeControl.BackColor;
        }
        return Color.Empty;
      }
      set
      {
        if (_backColor != value)
        {
          _backColor = value;
          Cache.InvalidateSize();
          this.OnBackColorChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Category("Appearance"), DefaultValue(-1), Description("The image index for node.")]
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
          this.OnImageIndexChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Category("Appearance"), DefaultValue(-1), Description("The selected image index for node.")]
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
          this.OnSelectedImageIndexChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Category("Appearance"), DefaultValue(null), Description("The icon for node.")]
    public Icon Icon
    {
      get
      {
        return _icon;
      }
      set
      {
        if (_icon != value)
        {
          _icon = value;
          Cache.InvalidateSize();
          this.OnIconChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Category("Appearance"), DefaultValue(null), Description("The selected icon for node.")]
    public Icon SelectedIcon
    {
      get
      {
        return _selectedIcon;
      }
      set
      {
        if (_selectedIcon != value)
        {
          _selectedIcon = value;
          Cache.InvalidateSize();
          this.OnSelectedIconChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Category("Appearance"), DefaultValue(null), Description("The image for node.")]
    public Image Image
    {
      get
      {
        return _image;
      }
      set
      {
        if (_image != value)
        {
          _image = value;
          Cache.InvalidateSize();
          this.OnImageChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Category("Appearance"), DefaultValue(null), Description("The selected image for node.")]
    public Image SelectedImage
    {
      get
      {
        return _selectedImage;
      }
      set
      {
        if (_selectedImage != value)
        {
          _selectedImage = value;
          Cache.InvalidateSize();
          this.OnSelectedImageChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Category("Appearance"), DefaultValue(typeof(Indicator), "None"), Description("Indicator symbol for this node.")]
    public Indicator Indicator
    {
      get
      {
        return _indicator;
      }
      set
      {
        if (_indicator != value)
        {
          _indicator = value;
          this.OnIndicatorChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Category("Appearance"), DefaultValue(typeof(NodeCheckStates), "Inherit"), Description("Define the style of checkboxes.")]
    public NodeCheckStates CheckStates
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
          Cache.InvalidateSize();
          this.OnCheckStatesChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Category("Appearance"), DefaultValue(typeof(CheckState), "Unchecked"), Description("Check state of the node.")]
    public CheckState CheckState
    {
      get
      {
        return _checkState;
      }
      set
      {
        if (_checkState != value)
        {
          if (Cache.TreeControl != null && !Cache.TreeControl.OnBeforeCheck(this))
          {
            return;
          }
          _checkState = value;
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.OnAfterCheck(this);
            Cache.TreeControl.InvalidateNodeDrawing();
          }
          this.OnCheckStateChanged();
        }
      }
    }
    [Browsable(false)]
    public bool Checked
    {
      get
      {
        return this.CheckState == CheckState.Checked;
      }
      set
      {
        this.CheckState = (value ? CheckState.Checked : CheckState.Unchecked);
      }
    }
    [Category("Behavior"), DefaultValue(true), Description("Should the Node be visible.")]
    public bool Visible
    {
      get
      {
        return this.GetFlag(Node.Flags.Visible);
      }
      set
      {
        if (this.GetFlag(Node.Flags.Visible) != value)
        {
          this.SetFlag(Node.Flags.Visible, value);
          this.OnVisibleChanged();
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.InvalidateNodeDrawing();
          }
        }
      }
    }
    [Browsable(false)]
    public bool IsVisible
    {
      get
      {
        return this.Visible;
      }
    }
    [Category("Behavior"), DefaultValue(false), Description("Should the Node be expanded.")]
    public bool Expanded
    {
      get
      {
        return this.GetFlag(Node.Flags.Expanded);
      }
      set
      {
        if (value)
        {
          this.Expand();
          return;
        }
        this.Collapse();
      }
    }
    [Browsable(false)]
    public bool IsExpanded
    {
      get
      {
        return this.Expanded;
      }
    }
    [Category("Behavior"), DefaultValue(true), Description("Can the Node be selected.")]
    public bool Selectable
    {
      get
      {
        return this.GetFlag(Node.Flags.Selectable);
      }
      set
      {
        if (this.GetFlag(Node.Flags.Selectable) != value)
        {
          this.SetFlag(Node.Flags.Selectable, value);
          this.OnSelectableChanged();
        }
      }
    }
    [Browsable(false)]
    public bool IsSelected
    {
      get
      {
        return Cache.TreeControl != null && Cache.TreeControl.IsNodeSelected(this);
      }
    }
    [Browsable(false), Category("Data"), Description("Get the path from root node to this node.")]
    public string FullPath
    {
      get
      {
        string text = this.Text;
        if (Cache.TreeControl != null)
        {
          string pathSeparator = Cache.TreeControl.PathSeparator;
          for (Node parent = this.Parent; parent != null; parent = parent.Parent)
          {
            text = parent.Text + pathSeparator + text;
          }
        }
        return text;
      }
    }
    [Browsable(false), Category("Data"), DefaultValue(null), Description("User defined unique key to associate with node.")]
    public object Key
    {
      get
      {
        return _key;
      }
      set
      {
        if (_key != value)
        {
          if (Cache.TreeControl != null)
          {
            Cache.TreeControl.NodeKeyChanged(this, _key, value);
          }
          _key = value;
          this.OnKeyChanged();
        }
      }
    }
    [Browsable(false), Category("Data"), DefaultValue(null), Description("User defined data associated with node.")]
    public object Tag
    {
      get
      {
        return _tag;
      }
      set
      {
        if (_tag != value)
        {
          _tag = value;
          this.OnTagChanged();
        }
      }
    }
    [Browsable(false)]
    public Size Size
    {
      get
      {
        return Cache.Size;
      }
    }
    [Browsable(false)]
    public Rectangle Bounds
    {
      get
      {
        return Cache.Bounds;
      }
    }
    [Browsable(false)]
    public Rectangle ChildBounds
    {
      get
      {
        return Cache.ChildBounds;
      }
    }
    [Browsable(false)]
    public bool IsSizeDirty
    {
      get
      {
        return Cache.IsSizeDirty;
      }
    }
    [Browsable(false)]
    public int Index
    {
      get
      {
        if (this.ParentNodes == null)
        {
          return -1;
        }
        return this.ParentNodes.IndexOf(this);
      }
    }
    [Browsable(false)]
    public Node FirstNode
    {
      get
      {
        return this;
      }
    }
    [Browsable(false)]
    public Node FirstDisplayedNode
    {
      get
      {
        if (!this.Visible)
        {
          return null;
        }
        return this;
      }
    }
    [Browsable(false)]
    public Node LastNode
    {
      get
      {
        Node node = this;
        if (this.Nodes.Count > 0)
        {
          int index = this.Nodes.Count - 1;
          node = this.Nodes[index].Nodes.GetLastNode();
          if (node == null)
          {
            node = this.Nodes[index];
          }
        }
        return node;
      }
    }
    [Browsable(false)]
    public Node LastDisplayedNode
    {
      get
      {
        Node node = this.Visible ? this : null;
        if (node != null && this.Expanded)
        {
          Node lastDisplayedNode = this.Nodes.GetLastDisplayedNode();
          if (lastDisplayedNode != null)
          {
            node = lastDisplayedNode;
          }
        }
        return node;
      }
    }
    [Browsable(false)]
    public Node NextNode
    {
      get
      {
        Node result = null;
        if (this.Nodes.Count > 0)
        {
          result = this.Nodes.GetFirstNode();
        }
        else
        {
          for (Node node = this; node != null; node = node.Parent)
          {
            int num = node.ParentNodes.IndexOf(node);
            if (node.ParentNodes.Count > num + 1)
            {
              result = node.ParentNodes[num + 1];
              break;
            }
          }
        }
        return result;
      }
    }
    [Browsable(false)]
    public Node NextDisplayedNode
    {
      get
      {
        Node node = this.Expanded ? this.Nodes.GetFirstDisplayedNode() : null;
        if (node == null)
        {
          for (Node node2 = this; node2 != null; node2 = node2.Parent)
          {
            int num = node2.ParentNodes.IndexOf(node2);
            if (node2.ParentNodes.Count > num + 1)
            {
              for (int i = num + 1; i < node2.ParentNodes.Count; i++)
              {
                if (node2.ParentNodes[i].Visible)
                {
                  node = node2.ParentNodes[i];
                  break;
                }
              }
              if (node != null)
              {
                break;
              }
            }
          }
        }
        return node;
      }
    }
    [Browsable(false)]
    public Node PreviousNode
    {
      get
      {
        if (this.ParentNodes == null)
        {
          return null;
        }
        int num = this.ParentNodes.IndexOf(this);
        if (num > 0)
        {
          Node node = this.ParentNodes[num - 1];
          return node.LastNode;
        }
        return this.Parent;
      }
    }
    [Browsable(false)]
    public Node PreviousDisplayedNode
    {
      get
      {
        if (this.ParentNodes == null)
        {
          return null;
        }
        int num = this.ParentNodes.IndexOf(this);
        Node node = null;
        for (int i = num - 1; i >= 0; i--)
        {
          if (this.ParentNodes[i].Visible)
          {
            node = this.ParentNodes[i];
            break;
          }
        }
        if (node != null)
        {
          if (node.Expanded)
          {
            node = node.LastDisplayedNode;
          }
          return node;
        }
        return this.Parent;
      }
    }
    protected internal NodeCache Cache
    {
      get
      {
        return _cache;
      }
    }
    internal bool Removing
    {
      get
      {
        return this.GetFlag(Node.Flags.Removing);
      }
      set
      {
        this.SetFlag(Node.Flags.Removing, value);
      }
    }
    internal Node Original
    {
      get
      {
        return _original;
      }
      set
      {
        _original = value;
      }
    }
    public Node()
    {
      this.CommonConstruct();
    }
    public Node(string text)
    {
      this.CommonConstruct();
      _text = text;
    }
    private void CommonConstruct()
    {
      _parentNodes = null;
      _nodes = new NodeCollection(this);
      _cache = new NodeCache();
      _vc = null;
      _original = null;
      this.ResetText();
      this.ResetTooltip();
      this.ResetNodeFont();
      this.ResetForeColor();
      this.ResetBackColor();
      this.ResetIcon();
      this.ResetImage();
      this.ResetImageIndex();
      this.ResetSelectedIcon();
      this.ResetSelectedImage();
      this.ResetSelectedImageIndex();
      this.ResetIndicator();
      this.ResetCheckStates();
      this.ResetCheckState();
      this.ResetExpanded();
      this.ResetVisible();
      this.ResetSelectable();
      this.ResetKey();
      this.ResetTag();
    }
    private bool ShouldSerializeText()
    {
      return this.Text != "Node";
    }
    public void ResetText()
    {
      this.Text = "Node";
    }
    public void ResetTooltip()
    {
      this.Tooltip = "";
    }
    private bool ShouldSerializeNodeFont()
    {
      return _nodeFont != null;
    }
    public void ResetNodeFont()
    {
      this.NodeFont = null;
    }
    private bool ShouldSerializeForeColor()
    {
      return _foreColor != Color.Empty;
    }
    public void ResetForeColor()
    {
      this.ForeColor = Color.Empty;
    }
    private bool ShouldSerializeBackColor()
    {
      return _backColor != Color.Empty;
    }
    public void ResetBackColor()
    {
      this.BackColor = Color.Empty;
    }
    public void ResetImageIndex()
    {
      this.ImageIndex = -1;
    }
    public void ResetSelectedImageIndex()
    {
      this.SelectedImageIndex = -1;
    }
    public void ResetIcon()
    {
      this.Icon = null;
    }
    public void ResetSelectedIcon()
    {
      this.SelectedIcon = null;
    }
    public void ResetImage()
    {
      this.Image = null;
    }
    public void ResetSelectedImage()
    {
      this.SelectedImage = null;
    }
    public void ResetIndicator()
    {
      this.Indicator = Indicator.None;
    }
    public void ResetCheckStates()
    {
      this.CheckStates = NodeCheckStates.Inherit;
    }
    public void ResetCheckState()
    {
      this.CheckState = CheckState.Unchecked;
    }
    public void ResetVisible()
    {
      this.Visible = true;
    }
    public void Show()
    {
      this.Visible = true;
    }
    public void Hide()
    {
      this.Visible = false;
    }
    public void ResetExpanded()
    {
      this.Expanded = false;
    }
    public void Expand()
    {
      if (!this.IsExpanded)
      {
        if (Cache.TreeControl != null && !Cache.TreeControl.OnBeforeExpand(this))
        {
          return;
        }
        this.SetFlag(Node.Flags.Expanded, true);
        if (Cache.TreeControl != null)
        {
          Cache.TreeControl.OnAfterExpand(this);
          Cache.TreeControl.InvalidateNodeDrawing();
        }
        this.OnExpandedChanged();
      }
    }
    public void ExpandAll()
    {
      this.RecurseExpanded(this, true);
    }
    public void Collapse()
    {
      if (this.IsExpanded)
      {
        if (Cache.TreeControl != null && !Cache.TreeControl.OnBeforeCollapse(this))
        {
          return;
        }
        this.SetFlag(Node.Flags.Expanded, false);
        if (Cache.TreeControl != null)
        {
          Cache.TreeControl.OnAfterCollapse(this);
          Cache.TreeControl.InvalidateNodeDrawing();
        }
        this.OnExpandedChanged();
      }
    }
    public void CollapseAll()
    {
      this.RecurseExpanded(this, false);
    }
    public void Toggle()
    {
      if (this.IsExpanded)
      {
        this.Collapse();
        return;
      }
      this.Expand();
    }
    public void ResetSelectable()
    {
      this.Selectable = true;
    }
    public void Select()
    {
      if (Cache.TreeControl != null && !this.IsSelected)
      {
        Cache.TreeControl.SelectNode(this, false, true);
      }
    }
    public void Deselect()
    {
      if (Cache.TreeControl != null && this.IsSelected)
      {
        Cache.TreeControl.DeselectNode(this, false);
      }
    }
    public void BeginEdit()
    {
      INodeVC vC = this.VC;
      if (vC != null && Cache.TreeControl != null)
      {
        vC.BeginEditNode(Cache.TreeControl, this);
      }
    }
    public void ResetKey()
    {
      this.Key = null;
    }
    public void ResetTag()
    {
      this.Tag = null;
    }
    public void Remove()
    {
      if (this.ParentNodes != null)
      {
        this.ParentNodes.Remove(this);
      }
    }
    public int GetNodeCount()
    {
      return this.Nodes.Count;
    }
    public object Clone()
    {
      Node node = new Node();
      node._text = _text;
      node._nodeFont = _nodeFont;
      node._nodeFontBoldItalic = _nodeFontBoldItalic;
      node._nodeFontHeight = _nodeFontHeight;
      node._backColor = _backColor;
      node._foreColor = _foreColor;
      node._imageIndex = _imageIndex;
      node._selectedImageIndex = _selectedImageIndex;
      node._icon = _icon;
      node._selectedIcon = _selectedIcon;
      node._image = _image;
      node._selectedImage = _selectedImage;
      node._checkState = _checkState;
      node._checkStates = _checkStates;
      node._indicator = _indicator;
      node._flags = _flags;
      node._tag = _tag;
      node._vc = _vc;
      node._original = this;
      IEnumerator enumerator = this.Nodes.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node2 = (Node)enumerator.Current;
          node.Nodes.Add((Node)node2.Clone());
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
      return node;
    }
    public void UpdateInstance(Node source)
    {
      _text = source._text;
      _nodeFont = source._nodeFont;
      _nodeFontBoldItalic = source._nodeFontBoldItalic;
      _nodeFontHeight = source._nodeFontHeight;
      _backColor = source._backColor;
      _foreColor = source._foreColor;
      _imageIndex = source._imageIndex;
      _selectedImageIndex = source._selectedImageIndex;
      _icon = source._icon;
      _selectedIcon = source._selectedIcon;
      _image = source._image;
      _selectedImage = source._selectedImage;
      _checkState = source._checkState;
      _checkStates = source._checkStates;
      _indicator = source._indicator;
      _flags = source._flags;
      _tag = source._tag;
      _vc = source._vc;
    }
    protected virtual void OnTextChanged()
    {
      if (this.TextChanged != null)
      {
        this.TextChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnTooltipChanged()
    {
      if (this.TooltipChanged != null)
      {
        this.TooltipChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnNodeFontChanged()
    {
      if (this.NodeFontChanged != null)
      {
        this.NodeFontChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnForeColorChanged()
    {
      if (this.ForeColorChanged != null)
      {
        this.ForeColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnBackColorChanged()
    {
      if (this.BackColorChanged != null)
      {
        this.BackColorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnSelectedImageIndexChanged()
    {
      if (this.SelectedImageIndexChanged != null)
      {
        this.SelectedImageIndexChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnImageIndexChanged()
    {
      if (this.ImageIndexChanged != null)
      {
        this.ImageIndexChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnIndicatorChanged()
    {
      if (this.IndicatorChanged != null)
      {
        this.IndicatorChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnIconChanged()
    {
      if (this.IconChanged != null)
      {
        this.IconChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnSelectedIconChanged()
    {
      if (this.SelectedIconChanged != null)
      {
        this.SelectedIconChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnImageChanged()
    {
      if (this.ImageChanged != null)
      {
        this.ImageChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnSelectedImageChanged()
    {
      if (this.SelectedImageChanged != null)
      {
        this.SelectedImageChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckStateChanged()
    {
      INodeVC vC = this.VC;
      if (vC != null && Cache.TreeControl != null)
      {
        vC.NodeCheckStateChanged(Cache.TreeControl, this);
      }
      if (this.CheckStateChanged != null)
      {
        this.CheckStateChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnCheckStatesChanged()
    {
      if (this.CheckStatesChanged != null)
      {
        this.CheckStatesChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnVisibleChanged()
    {
      INodeVC vC = this.VC;
      if (vC != null && Cache.TreeControl != null)
      {
        vC.NodeVisibleChanged(Cache.TreeControl, this);
      }
      if (this.VisibleChanged != null)
      {
        this.VisibleChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnExpandedChanged()
    {
      INodeVC vC = this.VC;
      if (vC != null && Cache.TreeControl != null)
      {
        vC.NodeExpandedChanged(Cache.TreeControl, this);
      }
      if (this.ExpandedChanged != null)
      {
        this.ExpandedChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnSelectableChanged()
    {
      INodeVC vC = this.VC;
      if (vC != null && Cache.TreeControl != null)
      {
        vC.NodeSelectableChanged(Cache.TreeControl, this);
      }
      if (this.SelectableChanged != null)
      {
        this.SelectableChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnKeyChanged()
    {
      if (this.KeyChanged != null)
      {
        this.KeyChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnTagChanged()
    {
      if (this.TagChanged != null)
      {
        this.TagChanged.Invoke(this, EventArgs.Empty);
      }
    }
    protected virtual void OnVCChanged()
    {
      if (this.VCChanged != null)
      {
        this.VCChanged.Invoke(this, EventArgs.Empty);
      }
    }
    internal Font GetNodeFont()
    {
      return _nodeFont;
    }
    internal Font GetNodeBoldItalicFont()
    {
      if (_nodeFontBoldItalic != null)
      {
        _nodeFontBoldItalic = new Font(_nodeFont, FontStyle.Bold|FontStyle.Italic);
      }
      return _nodeFontBoldItalic;
    }
    internal Color GetNodeForeColor()
    {
      return _foreColor;
    }
    internal void SetTreeControl(TreeControl tc)
    {
      if (Cache.TreeControl != null)
      {
        Cache.TreeControl.NodeRemoved(this);
      }
      if (tc != null)
      {
        tc.NodeAdded(this);
      }
      Cache.TreeControl = tc;
      this.Nodes.SetTreeControl(tc);
    }
    internal void SetReferences(Node parentNode, NodeCollection parentNodes, TreeControl tc)
    {
      this.Removing = false;
      if (Cache.TreeControl != null)
      {
        Cache.TreeControl.NodeRemoved(this);
      }
      if (tc != null)
      {
        tc.NodeAdded(this);
      }
      _parentNodes = parentNodes;
      Cache.ParentNode = parentNode;
      Cache.TreeControl = tc;
      this.Nodes.SetTreeControl(tc);
    }
    internal int GetNodeFontHeight()
    {
      if (_nodeFont == null)
      {
        return Cache.TreeControl.GetFontHeight();
      }
      if (_nodeFontHeight == -1)
      {
        _nodeFontHeight = _nodeFont.Height;
      }
      return _nodeFontHeight;
    }
    internal int GetNodeGroupFontHeight()
    {
      if (_nodeFont == null)
      {
        return Cache.TreeControl.GetGroupFontHeight();
      }
      if (_nodeFontHeight == -1)
      {
        _nodeFontHeight = _nodeFont.Height;
      }
      return _nodeFontHeight;
    }
    private void RecurseExpanded(Node n, bool expand)
    {
      if (expand)
      {
        n.Expand();
      }
      else
      {
        n.Collapse();
      }
      IEnumerator enumerator = n.Nodes.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node n2 = (Node)enumerator.Current;
          this.RecurseExpanded(n2, expand);
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
    private bool GetFlag(Node.Flags flag)
    {
      return (_flags & flag) == flag;
    }
    private void SetFlag(Node.Flags flag, bool val)
    {
      if (val)
      {
        _flags |= flag;
        return;
      }
      _flags &= ~flag;
    }
  }
}
