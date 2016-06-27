using System;
namespace Aspire.UiToolbox.Controls
{
  public enum ActAsButton
  {
    No,
    WholeControl,
    JustArrow
  }

  public enum ArrowButton
  {
    None,
    UpArrow,
    DownArrow,
    LeftArrow,
    RightArrow,
    Pinned,
    Unpinned
  }

  public enum CheckState
  {
    Unchecked,
    Mixed,
    Checked
  }

  public enum CheckStates
  {
    None,
    TwoStateCheck,
    ThreeStateCheck,
    Radio
  }

  public enum ClickExpandAction
  {
    None,
    Expand,
    Toggle
  }

  [Flags]
  public enum CompactFlags
  {
    RemoveEmptyTabLeaf = 1,
    RemoveEmptyTabSequence,
    ReduceSingleEntries = 4,
    ReduceSameDirection = 8,
    All = 15
  }

  public enum DisplayTabModes
  {
    HideAll,
    ShowAll,
    ShowActiveLeaf,
    ShowMouseOver,
    ShowActiveAndMouseOver,
    ShowOnlyMultipleTabs
  }
  
  public enum DrawStyle
  {
    Plain,
    Themed,
    Gradient
  }

  public enum GradientColoring
  {
    LightBackToBack,
    LightBackToDarkBack,
    BackToDarkBack,
    BackToGradientColor,
    LightBackToGradientColor
  }

  public enum GradientDirection
  {
    None = -1,
    LeftToRight,
    TopLeftToBottomRight = 45,
    TopToBottom = 90,
    TopRightToBottomLeft = 135,
    RightToLeft = 180,
    BottomRightToTopLeft = 225,
    BottomToTop = 270,
    BottomLeftToTopRight = 315
  }
  
  public enum GroupBorderStyle
  {
    None,
    AllEdges,
    VerticalEdges,
    BottomEdge
  }

  public enum GroupColoring
  {
    ControlProperties,
    Office2003Light,
    Office2003Dark
  }

  public enum HideTabsModes
  {
    ShowAlways,
    HideAlways,
    HideUsingLogic,
    HideWithoutMouse
  }

  public enum IDE2005Style
  {
    Standard,
    StandardDark,
    Enhanced,
    EnhancedDark
  }

  public enum ImageAlignment
  {
    Near,
    Far
  }

  public enum Indicator
  {
    None = -1,
    FlagRed,
    FlagOrange,
    FlagGreen,
    FlagBlue,
    FlagGray,
    ArrowRed,
    ArrowOrange,
    ArrowGreen,
    ArrowBlue,
    ArrowGray,
    BoxRed,
    BoxOrange,
    BoxGreen,
    BoxBlue,
    BoxGray,
    TickRed,
    TickBrown,
    TickGreen,
    TickBlue,
    TickBlack,
    CrossRed,
    CrossBrown,
    CrossGreen,
    CrossBlue,
    CrossBlack,
    QuestionRed,
    QuestionBrown,
    QuestionGreen,
    QuestionBlue,
    QuestionBlack,
    Tick,
    NoEntryBig,
    NoEntrySmall,
    Exclamation,
    Error,
    Lock,
    Lightning,
    Paperclip,
    Graph
  }

  public enum Indicators
  {
    None,
    AtRoot,
    AtGroup
  }

  public enum LineBoxVisibility
  {
    Nowhere,
    OnlyAtRoot,
    OnlyBelowRoot,
    Everywhere
  }

  public enum LineDashStyle
  {
    Solid,
    Dot,
    Dash
  }

  public enum NodeCheckStates
  {
    None,
    TwoStateCheck,
    ThreeStateCheck,
    Radio,
    Inherit
  }

  public enum Office2003Color
  {
    Disable,
    Base,
    Light,
    Dark,
    Enhanced
  }

  public enum OfficeStyle
  {
    SoftWhite,
    LightWhite,
    DarkWhite,
    SoftEnhanced,
    LightEnhanced,
    DarkEnhanced,
    Light,
    Dark
  }

  public enum PanelBorder
  {
    None,
    Sunken,
    Raised,
    Dotted,
    Dashed,
    Solid
  }

  public enum ScrollVisibility
  {
    Never,
    Always,
    WhenNeeded
  }

  public enum SelectMode
  {
    None,
    Single,
    Multiple
  }

  public enum Status
  {
    Default,
    Yes,
    No
  }

  public enum TitleEdge
  {
    Top,
    Left,
    Bottom,
    Right
  }

  public enum TreeBorderStyle
  {
    None,
    Theme,
    Solid,
    Dashed,
    Dotted,
    Flat3D,
    Bump3D,
    Etched3D,
    Adjust3D,
    Raised3D,
    RaisedInner3D,
    RaisedOuter3D,
    Sunken3D,
    SunkenInner3D,
    SunkenOuter3D
  }

  public enum TreeControlStyles
  {
    StandardPlain,
    StandardThemed,
    Explorer,
    Navigator,
    Group,
    GroupOfficeLight,
    GroupOfficeDark,
    List
  }

  public enum TreeGradientColoring
  {
    VeryLightToColor,
    LightToColor,
    LightToDark,
    LightToVeryDark,
    VeryLightToVeryDark
  }

  public enum VerticalGranularity
  {
    Pixel,
    Node
  }

  public enum ViewControllers
  {
    Default,
    Group
  }

  public enum VisualAppearance
  {
    MultiDocument,
    MultiForm,
    MultiBox
  }

}
