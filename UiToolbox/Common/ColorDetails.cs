using System;
using System.Drawing;
using System.Windows.Forms;

namespace Aspire.UiToolbox.Common
{
  public class ColorDetails : IDisposable
  {
    private bool _256Colors;
    private bool _defaultBaseColor;
    private bool _defaultTrackColor;
    private Color _baseColor;
    private Color _baseDarkerColor;
    private Color _baseDarkColor;
    private Color _baseLightColor;
    private Color _menuSeparatorColor;
    private Color _trackBaseColor;
    private Color _trackLightColor;
    private Color _trackLightLightColor;
    private Color _trackMenuInsideColor;
    private Color _trackMenuCheckInsideColor;
    private Color _menuCheckInsideColor;
    private Color _trackDarkColor;
    private Color _openBaseColor;
    private Color _openBorderColor;
    private VisualStyle _style;
    private ThemeColorHelper _themeColorHelper;
    public virtual VisualStyle Style
    {
      get
      {
        return _style;
      }
      set
      {
        _style = value;
      }
    }
    public bool DefaultBaseColor
    {
      get
      {
        return _defaultBaseColor;
      }
      set
      {
        _defaultBaseColor = value;
      }
    }
    public bool DefaultTrackColor
    {
      get
      {
        return _defaultTrackColor;
      }
      set
      {
        _defaultTrackColor = value;
      }
    }
    public Color BaseColor
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.BaseColor(this.Style);
        }
        return _baseColor;
      }
    }
    public Color BaseDarkerColor
    {
      get
      {
        return _baseDarkerColor;
      }
    }
    public Color BaseColor1
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.BaseColor1(this.Style);
        }
        return _baseColor;
      }
    }
    public Color BaseColor2
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.BaseColor2(this.Style);
        }
        return _baseColor;
      }
    }
    public Color BaseColorStub
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.BaseColorStub(this.Style);
        }
        return _baseColor;
      }
    }
    public Color DarkBaseColor
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.DarkBaseColor(this.Style);
        }
        return _baseDarkColor;
      }
    }
    public Color DarkBaseColor2
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.DarkBaseColor2(this.Style);
        }
        return ControlPaint.Dark(_baseDarkColor);
      }
    }
    public Color MenuSeparatorColor
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.MenuSeparatorColor(this.Style);
        }
        return _menuSeparatorColor;
      }
    }
    public Color TrackBaseColor1
    {
      get
      {
        if (_defaultTrackColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.TrackBaseColor1(this.Style);
        }
        return _trackBaseColor;
      }
    }
    public Color TrackBaseColor2
    {
      get
      {
        if (_defaultTrackColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.TrackBaseColor2(this.Style);
        }
        return _trackBaseColor;
      }
    }
    public Color TrackLightColor1
    {
      get
      {
        if (_defaultTrackColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.TrackLightColor1(this.Style);
        }
        return _trackLightColor;
      }
    }
    public Color TrackLightColor2
    {
      get
      {
        if (_defaultTrackColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.TrackLightColor2(this.Style);
        }
        return _trackLightColor;
      }
    }
    public Color TrackLightLightColor1
    {
      get
      {
        if (_defaultTrackColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.TrackLightLightColor1(this.Style);
        }
        return _trackLightLightColor;
      }
    }
    public Color TrackLightLightColor2
    {
      get
      {
        if (_defaultTrackColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.TrackLightLightColor2(this.Style);
        }
        return _trackLightLightColor;
      }
    }
    public Color TrackDarkColor
    {
      get
      {
        if (_defaultTrackColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.TrackDarkColor(this.Style);
        }
        return _trackDarkColor;
      }
    }
    public Color OpenBorderColor
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.OpenBorderColor(this.Style);
        }
        return _baseDarkColor;
      }
    }
    public Color MenuItemBorderColor
    {
      get
      {
        if (_defaultTrackColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.MenuItemBorderColor(this.Style);
        }
        return _trackDarkColor;
      }
    }
    public Color MenuBackColor
    {
      get
      {
        if (_defaultTrackColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.MenuBackColor(this.Style);
        }
        return _trackDarkColor;
      }
    }
    public Color TrackMenuInsideColor
    {
      get
      {
        if (_defaultTrackColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.TrackMenuInsideColor(this.Style);
        }
        return _trackMenuInsideColor;
      }
    }
    public Color MenuCheckInsideColor
    {
      get
      {
        if (_defaultTrackColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.MenuCheckInsideColor(this.Style);
        }
        return _menuCheckInsideColor;
      }
    }
    public Color TrackMenuCheckInsideColor
    {
      get
      {
        if (_defaultTrackColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.TrackMenuCheckInsideColor(this.Style);
        }
        return _trackMenuCheckInsideColor;
      }
    }
    public Color CaptionColor1
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.CaptionColor1(this.Style);
        }
        return _baseLightColor;
      }
    }
    public Color CaptionColor2
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.CaptionColor2(this.Style);
        }
        return _baseDarkColor;
      }
    }
    public Color CaptionSelectColor1
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.CaptionSelectColor1(this.Style);
        }
        return _baseLightColor;
      }
    }
    public Color CaptionSelectColor2
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.CaptionSelectColor2(this.Style);
        }
        return _baseDarkColor;
      }
    }
    public Color SpotColor1
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.SpotColor1(this.Style);
        }
        return _baseDarkColor;
      }
    }
    public Color SpotColor2
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.SpotColor2(this.Style);
        }
        return _baseLightColor;
      }
    }
    public Color SepDarkColor
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.SepDarkColor(this.Style);
        }
        return _baseDarkColor;
      }
    }
    public Color SepLightColor
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.SepLightColor(this.Style);
        }
        return _baseLightColor;
      }
    }
    public Color OpenBaseColor1
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.OpenBaseColor1(this.Style);
        }
        return _openBaseColor;
      }
    }
    public Color OpenBaseColor2
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.OpenBaseColor2(this.Style);
        }
        return _openBaseColor;
      }
    }
    public Color ColumnBaseColor1
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.ColumnBaseColor1(this.Style);
        }
        return _openBaseColor;
      }
    }
    public Color ColumnBaseColor2
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.ColumnBaseColor2(this.Style);
        }
        return _openBaseColor;
      }
    }
    public Color ActiveBorderColor
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.ActiveBorderColor(this.Style);
        }
        return _baseDarkColor;
      }
    }
    public Color ActiveTabColor
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.ActiveTabColor(this.Style);
        }
        return ControlPaint.LightLight(_baseColor);
      }
    }
    public Color ActiveTabButtonColor
    {
      get
      {
        if (_defaultBaseColor && (this.Style == VisualStyle.Office2003 || this.Style == VisualStyle.IDE2005))
        {
          return _themeColorHelper.ActiveTabButtonColor(this.Style);
        }
        return ControlPaint.DarkDark(_baseColor);
      }
    }
    public ColorDetails()
    {
      _256Colors = (ColorHelper.ColorDepth() == 8);
      _themeColorHelper = new ThemeColorHelper();
      _defaultBaseColor = true;
      _defaultTrackColor = true;
      _style = VisualStyle.Office2003;
      this.DefineBaseColors(SystemColors.Control);
      this.DefineTrackColors(SystemColors.Highlight);
    }
    public void Dispose()
    {
      _themeColorHelper.Dispose();
    }
    public void Reset()
    {
      _themeColorHelper.Reset();
    }
    public void DefineBaseColors(Color baseColor)
    {
      _baseColor = baseColor;
      _baseDarkColor = CommandDraw.BaseDarkFromBase(_baseColor, _256Colors);
      _baseDarkerColor = ColorHelper.MergeColors(_baseColor, 0.95f, _baseDarkColor, 0.05f);
      _baseLightColor = CommandDraw.BaseLightFromBase(_baseColor, _256Colors);
      _openBaseColor = CommandDraw.OpenBaseFromBase(_baseColor, _256Colors);
      _openBorderColor = CommandDraw.OpenBorderFromBase(_baseColor, _256Colors);
      _menuSeparatorColor = CommandDraw.MenuSeparatorFromBase(_baseColor, _256Colors);
    }
    public void DefineTrackColors(Color track)
    {
      _trackBaseColor = CommandDraw.TrackBaseFromTrack(track, _256Colors);
      _trackLightColor = CommandDraw.TrackLightFromTrack(track, _256Colors);
      _trackLightLightColor = CommandDraw.TrackLightLightFromTrack(track, _baseColor, _256Colors);
      _trackDarkColor = CommandDraw.TrackDarkFromTrack(track, _256Colors);
      _trackMenuInsideColor = CommandDraw.TrackMenuInsideFromTrack(track, _256Colors);
      _menuCheckInsideColor = CommandDraw.MenuCheckInsideColor(track, _256Colors);
      _trackMenuCheckInsideColor = CommandDraw.TrackMenuCheckInsideColor(track, _256Colors);
    }
  }
}
