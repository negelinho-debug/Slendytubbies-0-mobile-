using UnityEngine;

public class AnimationCurveLimitAttribute : PropertyAttribute
{
	public Color? Color { get; }

	public Rect Range { get; }

	public static Color FromHEX(int hex)
	{
		float r = (float)((hex & 0xFF0000) >> 16) / 255f;
		float g = (float)((hex & 0xFF00) >> 8) / 255f;
		float b = (float)(hex & 0xFF) / 255f;
		return new Color(r, g, b, 1f);
	}

	public AnimationCurveLimitAttribute()
		: this(0f, 1f, 0f, 1f)
	{
	}

	public AnimationCurveLimitAttribute(float minX, float maxX, float minY, float maxY)
	{
		Range = new Rect(minX, minY, maxX - minX, maxY - minY);
	}

	public AnimationCurveLimitAttribute(float minX, float maxX, float minY, float maxY, int color)
	{
		Range = new Rect(minX, minY, maxX - minX, maxY - minY);
		Color = FromHEX(color);
	}

	public AnimationCurveLimitAttribute(float minX, float maxX)
	{
		Range = new Rect(minX, float.NegativeInfinity, maxX - minX, float.PositiveInfinity);
	}

	public AnimationCurveLimitAttribute(float minX, float maxX, int color)
	{
		Range = new Rect(minX, float.NegativeInfinity, maxX - minX, float.PositiveInfinity);
		Color = FromHEX(color);
	}
}
