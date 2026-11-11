using UnityEngine;

namespace HostGame;

public class CurveSetupAttribute : PropertyAttribute
{
	public Color Color { get; }

	public Rect Range { get; }

	public static Color FromHEX(int hex)
	{
		float r = (float)((hex & 0xFF0000) >> 16) / 255f;
		float g = (float)((hex & 0xFF00) >> 8) / 255f;
		float b = (float)(hex & 0xFF) / 255f;
		return new Color(r, g, b, 1f);
	}

	public CurveSetupAttribute(int color, float minX, float maxX, float minY, float maxY)
	{
		Color = FromHEX(color);
		Range = new Rect(minX, minY, maxX - minX, maxY - minY);
	}

	public CurveSetupAttribute(CurveColor color, float minX, float maxX, float minY, float maxY)
	{
		switch (color)
		{
		case CurveColor.Red:
			Color = Color.red;
			break;
		case CurveColor.Green:
			Color = Color.green;
			break;
		case CurveColor.Yellow:
			Color = Color.yellow;
			break;
		case CurveColor.Blue:
			Color = Color.blue;
			break;
		}
		Range = new Rect(minX, minY, maxX - minX, maxY - minY);
	}
}
