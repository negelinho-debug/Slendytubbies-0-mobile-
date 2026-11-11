using System;
using System.Runtime.CompilerServices;

namespace PrimeTween;

internal static class Mathf
{
	internal const float PI = MathF.PI;

	private static volatile float FloatMinNormal = 1.1754944E-38f;

	private static volatile float FloatMinDenormal = float.Epsilon;

	private static bool IsFlushToZeroEnabled = (double)FloatMinDenormal == 0.0;

	private static readonly float Epsilon = (IsFlushToZeroEnabled ? FloatMinNormal : FloatMinDenormal);

	internal static float Min(float a, float b)
	{
		if (!(a < b))
		{
			return b;
		}
		return a;
	}

	internal static float Max(float a, float b)
	{
		if (!(a > b))
		{
			return b;
		}
		return a;
	}

	internal static bool Approximately(float a, float b)
	{
		return Abs(b - a) < Max(1E-06f * Max(Abs(a), Abs(b)), Epsilon * 8f);
	}

	internal static float Abs(float f)
	{
		if (!(f < 0f))
		{
			return f;
		}
		return 0f - f;
	}

	internal static int Abs(int value)
	{
		return Math.Abs(value);
	}

	internal static float InverseLerp(float a, float b, float value)
	{
		if (a == b)
		{
			return 0f;
		}
		return Clamp01((value - a) / (b - a));
	}

	internal static float Clamp01(float value)
	{
		if ((double)value < 0.0)
		{
			return 0f;
		}
		if (!((double)value > 1.0))
		{
			return value;
		}
		return 1f;
	}

	internal static float Sqrt(float f)
	{
		return (float)Math.Sqrt(f);
	}

	internal static float Lerp(float a, float b, float t)
	{
		return a + (b - a) * Clamp01(t);
	}

	internal static float Pow(float f, float p)
	{
		return (float)Math.Pow(f, p);
	}

	internal static float Asin(float f)
	{
		return (float)Math.Asin(f);
	}

	internal static float Sin(float f)
	{
		return (float)Math.Sin(f);
	}

	internal static float Cos(float f)
	{
		return (float)Math.Cos(f);
	}

	internal static float Acos(float f)
	{
		return (float)Math.Acos(f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static float Sign(float f)
	{
		if (!((double)f >= 0.0))
		{
			return -1f;
		}
		return 1f;
	}

	internal static float Clamp(float value, float min, float max)
	{
		if (value < min)
		{
			value = min;
		}
		else if (value > max)
		{
			value = max;
		}
		return value;
	}

	internal static int RoundToInt(float f)
	{
		return (int)Math.Round(f);
	}

	internal static float LerpUnclamped(float a, float b, float t)
	{
		return a + (b - a) * t;
	}
}
