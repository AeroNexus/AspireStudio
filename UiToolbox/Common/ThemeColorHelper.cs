using System;
using System.Drawing;

using Microsoft.Win32;

namespace Aspire.UiToolbox.Common
{
  public class ThemeColorHelper : IDisposable
  {
    private static int THEME_BLUE = 0;
    private static int THEME_GREEN = 1;
    private static int THEME_SILVER = 2;
    private static int THEME_CLASSIC = 3;
    private static Color[] _themeBaseColor = new Color[]
		{
			Color.FromArgb(195, 218, 249),
			Color.FromArgb(241, 240, 227),
			Color.FromArgb(242, 242, 247)
		};
    private static Color[] _themeBase1Color = new Color[]
		{
			Color.FromArgb(195, 218, 249),
			Color.FromArgb(241, 240, 227),
			Color.FromArgb(242, 242, 247)
		};
    private static Color[] _themeBase2Color = new Color[]
		{
			Color.FromArgb(158, 190, 245),
			Color.FromArgb(217, 217, 167),
			Color.FromArgb(215, 215, 229)
		};
    private static Color[] _themeDarkBaseColor = new Color[]
		{
			Color.FromArgb(89, 135, 214),
			Color.FromArgb(175, 192, 130),
			Color.FromArgb(168, 167, 191)
		};
    private static Color[] _themeDarkBase2Color = new Color[]
		{
			Color.FromArgb(3, 56, 147),
			Color.FromArgb(99, 122, 68),
			Color.FromArgb(112, 111, 145)
		};
    private static Color[] _themeTrackDarkColor = new Color[]
		{
			Color.FromArgb(0, 45, 150),
			Color.FromArgb(117, 141, 94),
			Color.FromArgb(124, 124, 148)
		};
    private static Color[] _themeTrackBaseColor1 = new Color[]
		{
			Color.FromArgb(254, 149, 78),
			Color.FromArgb(254, 149, 78),
			Color.FromArgb(254, 149, 78)
		};
    private static Color[] _themeTrackBaseColor2 = new Color[]
		{
			Color.FromArgb(255, 211, 142),
			Color.FromArgb(255, 211, 142),
			Color.FromArgb(255, 211, 142)
		};
    private static Color[] _themeTrackLightColor1 = new Color[]
		{
			Color.FromArgb(255, 244, 204),
			Color.FromArgb(255, 244, 204),
			Color.FromArgb(255, 244, 204)
		};
    private static Color[] _themeTrackLightColor2 = new Color[]
		{
			Color.FromArgb(255, 208, 145),
			Color.FromArgb(255, 208, 145),
			Color.FromArgb(255, 208, 145)
		};
    private static Color[] _themeTrackLightLightColor1 = new Color[]
		{
			Color.FromArgb(255, 213, 140),
			Color.FromArgb(255, 213, 140),
			Color.FromArgb(255, 213, 140)
		};
    private static Color[] _themeTrackLightLightColor2 = new Color[]
		{
			Color.FromArgb(255, 173, 86),
			Color.FromArgb(255, 173, 86),
			Color.FromArgb(255, 173, 86)
		};
    private static Color[] _themeMenuBorderColor = new Color[]
		{
			Color.FromArgb(0, 45, 150),
			Color.FromArgb(117, 141, 94),
			Color.FromArgb(124, 124, 148)
		};
    private static Color[] _themeMenuItemBorderColor = new Color[]
		{
			Color.FromArgb(0, 0, 128),
			Color.FromArgb(62, 93, 56),
			Color.FromArgb(75, 75, 111)
		};
    private static Color[] _themeMenuBackColor = new Color[]
		{
			Color.FromArgb(246, 246, 246),
			Color.FromArgb(244, 244, 238),
			Color.FromArgb(253, 250, 255)
		};
    private static Color[] _themeMenuSeparatorColor = new Color[]
		{
			Color.FromArgb(106, 140, 203),
			Color.FromArgb(96, 128, 88),
			Color.FromArgb(110, 109, 143)
		};
    private static Color[] _themeTrackMenuInsideColor = new Color[]
		{
			Color.FromArgb(255, 238, 194),
			Color.FromArgb(255, 238, 194),
			Color.FromArgb(255, 238, 194)
		};
    private static Color[] _themeMenuCheckInsideColor = new Color[]
		{
			Color.FromArgb(255, 192, 111),
			Color.FromArgb(255, 192, 111),
			Color.FromArgb(255, 192, 111)
		};
    private static Color[] _themeTrackMenuCheckInsideColor = new Color[]
		{
			Color.FromArgb(255, 128, 62),
			Color.FromArgb(255, 128, 62),
			Color.FromArgb(255, 128, 62)
		};
    private static Color[] _themeCaptionColor1 = new Color[]
		{
			Color.FromArgb(218, 234, 253),
			Color.FromArgb(237, 242, 212),
			Color.FromArgb(240, 240, 248)
		};
    private static Color[] _themeCaptionColor2 = new Color[]
		{
			Color.FromArgb(123, 164, 224),
			Color.FromArgb(181, 196, 143),
			Color.FromArgb(147, 145, 176)
		};
    private static Color[] _themeCaptionSelectColor1 = new Color[]
		{
			Color.FromArgb(255, 213, 140),
			Color.FromArgb(255, 213, 140),
			Color.FromArgb(255, 213, 140)
		};
    private static Color[] _themeCaptionSelectColor2 = new Color[]
		{
			Color.FromArgb(255, 173, 86),
			Color.FromArgb(255, 173, 86),
			Color.FromArgb(255, 173, 86)
		};
    private static Color[] _themeSpotColor1 = new Color[]
		{
			Color.FromArgb(39, 65, 118),
			Color.FromArgb(81, 94, 51),
			Color.FromArgb(84, 84, 117)
		};
    private static Color[] _themeSpotColor2 = new Color[]
		{
			Color.FromArgb(255, 255, 255),
			Color.FromArgb(255, 255, 255),
			Color.FromArgb(255, 255, 255)
		};
    private static Color[] _themeSepDarkColor = new Color[]
		{
			Color.FromArgb(106, 140, 203),
			Color.FromArgb(96, 128, 88),
			Color.FromArgb(110, 109, 143)
		};
    private static Color[] _themeSepLightColor = new Color[]
		{
			Color.FromArgb(241, 249, 255),
			Color.FromArgb(244, 247, 222),
			Color.FromArgb(255, 255, 255)
		};
    private static Color[] _themeOpenBaseColor1 = new Color[]
		{
			Color.FromArgb(227, 238, 255),
			Color.FromArgb(237, 239, 214),
			Color.FromArgb(231, 233, 241)
		};
    private static Color[] _themeOpenBaseColor2 = new Color[]
		{
			Color.FromArgb(147, 181, 231),
			Color.FromArgb(194, 206, 159),
			Color.FromArgb(186, 185, 205)
		};
    private static Color[] _themeColumnBaseColor1 = new Color[]
		{
			Color.FromArgb(227, 238, 255),
			Color.FromArgb(237, 239, 214),
			Color.FromArgb(231, 233, 241)
		};
    private static Color[] _themeColumnBaseColor2 = new Color[]
		{
			Color.FromArgb(147, 181, 231),
			Color.FromArgb(194, 206, 159),
			Color.FromArgb(186, 185, 205)
		};
    private static Color[] _themeActiveBorderColor = new Color[]
		{
			Color.FromArgb(0, 45, 150),
			Color.FromArgb(117, 141, 94),
			Color.FromArgb(124, 124, 148)
		};
    private static Color[] _themeActiveTabColor = new Color[]
		{
			Color.FromArgb(255, 255, 255),
			Color.FromArgb(255, 255, 255),
			Color.FromArgb(255, 255, 255)
		};
    private static Color[] _themeActiveTabButtonColor = new Color[]
		{
			Color.FromArgb(0, 0, 0),
			Color.FromArgb(0, 0, 0),
			Color.FromArgb(0, 0, 0)
		};
    private static Color[] _ide2005Base1Color = new Color[]
		{
			Color.FromArgb(243, 242, 231),
			Color.FromArgb(243, 242, 231),
			Color.FromArgb(243, 243, 247)
		};
    private static Color[] _ide2005Base2Color = new Color[]
		{
			Color.FromArgb(229, 229, 215),
			Color.FromArgb(229, 229, 215),
			Color.FromArgb(215, 215, 229)
		};
    private static Color[] _ide2005BaseStub = new Color[]
		{
			Color.FromArgb(255, 255, 255),
			Color.FromArgb(255, 255, 255),
			Color.FromArgb(255, 255, 255)
		};
    private static Color[] _ide2005CaptionSelectColor1 = new Color[]
		{
			Color.FromArgb(58, 127, 234),
			Color.FromArgb(182, 195, 146),
			Color.FromArgb(211, 212, 221)
		};
    private static Color[] _ide2005CaptionSelectColor2 = new Color[]
		{
			Color.FromArgb(49, 106, 197),
			Color.FromArgb(145, 160, 117),
			Color.FromArgb(166, 165, 192)
		};
    private static Color[] _ide2005OpenBaseColor1 = new Color[]
		{
			Color.FromArgb(251, 251, 249),
			Color.FromArgb(251, 251, 249),
			Color.FromArgb(231, 233, 241)
		};
    private static Color[] _ide2005OpenBaseColor2 = new Color[]
		{
			Color.FromArgb(247, 245, 239),
			Color.FromArgb(247, 245, 239),
			Color.FromArgb(186, 185, 205)
		};
    private static Color[] _ide2005MenuBorderColor = new Color[]
		{
			Color.FromArgb(138, 134, 122),
			Color.FromArgb(138, 134, 122),
			Color.FromArgb(138, 134, 122)
		};
    private static Color[] _ide2005ColumnBaseColor1 = new Color[]
		{
			Color.FromArgb(254, 254, 251),
			Color.FromArgb(254, 254, 251),
			Color.FromArgb(231, 233, 241)
		};
    private static Color[] _ide2005ColumnBaseColor2 = new Color[]
		{
			Color.FromArgb(196, 195, 172),
			Color.FromArgb(196, 195, 172),
			Color.FromArgb(186, 185, 205)
		};
    private bool _themeTested;
    private int _currentTheme;
    public ThemeColorHelper()
    {
      _themeTested = false;
      _currentTheme = ThemeColorHelper.THEME_CLASSIC;
      SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }
    public void Reset()
    {
      _themeTested = false;
    }
    public Color BaseColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Window, 0.8f, SystemColors.Control, 0.2f);
      }
      if (style == VisualStyle.IDE2005)
      {
        return SystemColors.Control;
      }
      return ThemeColorHelper._themeBaseColor[_currentTheme];
    }
    public Color BaseColor1(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Window, 0.8f, SystemColors.Control, 0.2f);
      }
      if (style == VisualStyle.IDE2005)
      {
        return ThemeColorHelper._ide2005Base1Color[_currentTheme];
      }
      return ThemeColorHelper._themeBase1Color[_currentTheme];
    }
    public Color BaseColor2(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        if (style == VisualStyle.IDE2005)
        {
          return SystemColors.Control;
        }
        return ColorHelper.MergeColors(SystemColors.Window, 0.16f, SystemColors.Control, 0.84f);
      }
      else
      {
        if (style == VisualStyle.IDE2005)
        {
          return ThemeColorHelper._ide2005Base2Color[_currentTheme];
        }
        return ThemeColorHelper._themeBase2Color[_currentTheme];
      }
    }
    public Color BaseColorStub(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return SystemColors.ControlLightLight;
      }
      if (style == VisualStyle.IDE2005)
      {
        return ThemeColorHelper._ide2005BaseStub[_currentTheme];
      }
      return SystemColors.ControlLightLight;
    }
    public Color DarkBaseColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005)
      {
        return SystemColors.ControlDark;
      }
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Window, 0.8f, SystemColors.Control, 0.2f);
      }
      return ThemeColorHelper._themeDarkBaseColor[_currentTheme];
    }
    public Color DarkBaseColor2(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return SystemColors.Control;
      }
      if (style == VisualStyle.IDE2005)
      {
        return SystemColors.ControlDark;
      }
      return ThemeColorHelper._themeDarkBase2Color[_currentTheme];
    }
    public Color TrackDarkColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.7f, SystemColors.Window, 0.3f);
      }
      return ThemeColorHelper._themeTrackDarkColor[_currentTheme];
    }
    public Color TrackBaseColor1(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.5f, SystemColors.Window, 0.5f);
      }
      return ThemeColorHelper._themeTrackBaseColor1[_currentTheme];
    }
    public Color TrackBaseColor2(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.5f, SystemColors.Window, 0.5f);
      }
      return ThemeColorHelper._themeTrackBaseColor2[_currentTheme];
    }
    public Color TrackLightColor1(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.3f, SystemColors.Window, 0.7f);
      }
      return ThemeColorHelper._themeTrackLightColor1[_currentTheme];
    }
    public Color TrackLightColor2(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.3f, SystemColors.Window, 0.7f);
      }
      return ThemeColorHelper._themeTrackLightColor2[_currentTheme];
    }
    public Color TrackLightLightColor1(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.1f, SystemColors.Control, 0.4f, SystemColors.Window, 0.5f);
      }
      return ThemeColorHelper._themeTrackLightLightColor1[_currentTheme];
    }
    public Color TrackLightLightColor2(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.1f, SystemColors.Control, 0.4f, SystemColors.Window, 0.5f);
      }
      return ThemeColorHelper._themeTrackLightLightColor2[_currentTheme];
    }
    public Color OpenBorderColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.ControlDark, 0.8f, SystemColors.ControlText, 0.2f);
      }
      if (style == VisualStyle.IDE2005)
      {
        return ThemeColorHelper._ide2005MenuBorderColor[_currentTheme];
      }
      return ThemeColorHelper._themeMenuBorderColor[_currentTheme];
    }
    public Color MenuItemBorderColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.7f, SystemColors.Window, 0.3f);
      }
      return ThemeColorHelper._themeMenuItemBorderColor[_currentTheme];
    }
    public Color MenuBackColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Control, 0.145f, SystemColors.Window, 0.855f);
      }
      return ThemeColorHelper._themeMenuBackColor[_currentTheme];
    }
    public Color MenuSeparatorColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.ControlDark, 0.7f, SystemColors.Window, 0.3f);
      }
      return ThemeColorHelper._themeMenuSeparatorColor[_currentTheme];
    }
    public Color TrackMenuInsideColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.3f, SystemColors.Window, 0.7f);
      }
      return ThemeColorHelper._themeTrackMenuInsideColor[_currentTheme];
    }
    public Color MenuCheckInsideColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.1f, SystemColors.Control, 0.4f, SystemColors.Window, 0.5f);
      }
      return ThemeColorHelper._themeMenuCheckInsideColor[_currentTheme];
    }
    public Color TrackMenuCheckInsideColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.3f, SystemColors.Control, 0.7f);
      }
      return ThemeColorHelper._themeTrackMenuCheckInsideColor[_currentTheme];
    }
    public Color CaptionColor1(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return SystemColors.Control;
      }
      if (style == VisualStyle.IDE2005)
      {
        return ColorHelper.MergeColors(SystemColors.Control, 0.5f, SystemColors.ControlDark, 0.5f);
      }
      return ThemeColorHelper._themeCaptionColor1[_currentTheme];
    }
    public Color CaptionColor2(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return SystemColors.ControlDark;
      }
      if (style == VisualStyle.IDE2005)
      {
        return ColorHelper.MergeColors(SystemColors.Control, 0.5f, SystemColors.ControlDark, 0.5f);
      }
      return ThemeColorHelper._themeCaptionColor2[_currentTheme];
    }
    public Color CaptionSelectColor1(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.3f, SystemColors.Window, 0.7f);
      }
      if (style == VisualStyle.IDE2005)
      {
        return ThemeColorHelper._ide2005CaptionSelectColor1[_currentTheme];
      }
      return ThemeColorHelper._themeCaptionSelectColor1[_currentTheme];
    }
    public Color CaptionSelectColor2(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Highlight, 0.5f, SystemColors.Window, 0.5f);
      }
      if (style == VisualStyle.IDE2005)
      {
        return ThemeColorHelper._ide2005CaptionSelectColor2[_currentTheme];
      }
      return ThemeColorHelper._themeCaptionSelectColor2[_currentTheme];
    }
    public Color SpotColor1(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return SystemColors.ControlDarkDark;
      }
      return ThemeColorHelper._themeSpotColor1[_currentTheme];
    }
    public Color SpotColor2(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return SystemColors.ControlLightLight;
      }
      return ThemeColorHelper._themeSpotColor2[_currentTheme];
    }
    public Color SepDarkColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.ControlDark, 0.7f, SystemColors.Window, 0.3f);
      }
      return ThemeColorHelper._themeSepDarkColor[_currentTheme];
    }
    public Color SepLightColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return Color.White;
      }
      return ThemeColorHelper._themeSepLightColor[_currentTheme];
    }
    public Color OpenBaseColor1(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Window, 0.855f, SystemColors.Control, 0.145f);
      }
      if (style == VisualStyle.IDE2005)
      {
        return ThemeColorHelper._ide2005OpenBaseColor1[_currentTheme];
      }
      return ThemeColorHelper._themeOpenBaseColor1[_currentTheme];
    }
    public Color OpenBaseColor2(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Window, 0.58f, SystemColors.Control, 0.42f);
      }
      if (style == VisualStyle.IDE2005)
      {
        return ThemeColorHelper._ide2005OpenBaseColor2[_currentTheme];
      }
      return ThemeColorHelper._themeOpenBaseColor2[_currentTheme];
    }
    public Color ColumnBaseColor1(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return ColorHelper.MergeColors(SystemColors.Window, 0.855f, SystemColors.Control, 0.145f);
      }
      if (style == VisualStyle.IDE2005)
      {
        return ThemeColorHelper._ide2005ColumnBaseColor1[_currentTheme];
      }
      return ThemeColorHelper._themeColumnBaseColor1[_currentTheme];
    }
    public Color ColumnBaseColor2(VisualStyle style)
    {
      UpdateThemeDetails();
      if (_currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        if (style == VisualStyle.IDE2005)
        {
          return ColorHelper.MergeColors(SystemColors.ControlDarkDark, 0.32f, SystemColors.Control, 0.68f);
        }
        return ColorHelper.MergeColors(SystemColors.Window, 0.075f, SystemColors.Control, 0.925f);
      }
      else
      {
        if (style == VisualStyle.IDE2005)
        {
          return ThemeColorHelper._ide2005ColumnBaseColor2[_currentTheme];
        }
        return ThemeColorHelper._themeColumnBaseColor2[_currentTheme];
      }
    }
    public Color ActiveTabColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return SystemColors.Control;
      }
      return ThemeColorHelper._themeActiveTabColor[_currentTheme];
    }
    public Color ActiveTabButtonColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return SystemColors.Control;
      }
      return ThemeColorHelper._themeActiveTabButtonColor[_currentTheme];
    }
    public Color ActiveBorderColor(VisualStyle style)
    {
      UpdateThemeDetails();
      if (style == VisualStyle.IDE2005 || _currentTheme == ThemeColorHelper.THEME_CLASSIC)
      {
        return SystemColors.ControlDark;
      }
      return ThemeColorHelper._themeActiveBorderColor[_currentTheme];
    }
    public void Dispose()
    {
      SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
    }
    private void UpdateThemeDetails()
    {
      if (!_themeTested)
      {
        string empty = string.Empty;
        string empty2 = string.Empty;
        string empty3 = string.Empty;
        _currentTheme = ThemeColorHelper.THEME_CLASSIC;
        try
        {
          if (ThemeHelper.GetCurrentThemeName(ref empty, ref empty2, ref empty3))
          {
            string text;
            if ((text = empty2) != null)
            {
              text = string.IsInterned(text);
              if (text != "HomeStead")
              {
                if (text != "Metallic")
                {
                  if (text == "NormalColor")
                  {
                    _currentTheme = ThemeColorHelper.THEME_BLUE;
                  }
                }
                else
                {
                  _currentTheme = ThemeColorHelper.THEME_SILVER;
                }
              }
              else
              {
                _currentTheme = ThemeColorHelper.THEME_GREEN;
              }
            }
          }
        }
        catch
        {
        }
        _themeTested = true;
      }
    }
    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
      if (e.Category == UserPreferenceCategory.Color)
      {
        _themeTested = false;
      }
    }
  }
}