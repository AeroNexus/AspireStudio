using System;
using System.Drawing;

namespace Aspire.Utilities
{
	public static class Utils
	{
		public static string Formatted(this Color color)
		{
			if (color.IsNamedColor)
				return color.Name;
			else
				return string.Format("{0}:{1}:{2}:{3}",color.A,color.R,color.G,color.B);
		}

		public static Color Parse(this Color color, string text)
		{
			if (text.IndexOf(':') == -1)
				return Color.FromName(text);
			else
			{
				var tokens = text.Split(':');
				byte
					a = byte.Parse(tokens[0]), r = byte.Parse(tokens[1]),
					g = byte.Parse(tokens[2]), b = byte.Parse(tokens[3]);
				return Color.FromArgb(a,r,g,b);
			}
		}

	}
}
