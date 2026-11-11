using System;
using UnityEngine;

public static class Easing
{
	public static float easeOutBack(float x)
	{
		return 1f + 2.70158f * Mathf.Pow(x - 1f, 3f) + 1.70158f * Mathf.Pow(x - 1f, 2f);
	}

	public static float easeOutSine(float x)
	{
		return Mathf.Sin(x * MathF.PI / 2f);
	}

	public static float easeInCubic(float x)
	{
		return x * x * x;
	}

	public static float easeInSine(float x)
	{
		return 1f - Mathf.Cos(x * MathF.PI / 2f);
	}

	public static float easeOutQuint(float x)
	{
		return 1f - Mathf.Pow(1f - x, 5f);
	}
}
