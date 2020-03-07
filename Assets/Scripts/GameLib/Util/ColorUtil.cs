using UnityEngine;

namespace GameLib
{
	public static class ColorUtil
	{
		public static string ColorToHex(Color color)
		{
			int r = Mathf.RoundToInt(color.r * 255f);
			int g = Mathf.RoundToInt(color.g * 255f);
			int b = Mathf.RoundToInt(color.b * 255f);
			int a = Mathf.RoundToInt(color.a * 255f);
			return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", r, g, b, a);
		}

		public static Color HexToColor(string hex)
		{
			byte br = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte bg = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			byte bb = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			byte ba = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
			float r = br / 255f;
			float g = bg / 255f;
			float b = bb / 255f;
			float a = ba / 255f;
			return new Color(r, g, b, a);
		}
	}
}