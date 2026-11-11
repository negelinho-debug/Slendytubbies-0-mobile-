using System;
using UnityEngine;

namespace PrimeTween;

internal static class StandardEasing
{
	private const float halfPi = MathF.PI / 2f;

	internal const float backEaseConst = 1.70158f;

	internal const float defaultElasticEasePeriod = 0.3f;

	private static float InElastic(float t)
	{
		return 1f - OutElastic(1f - t);
	}

	private static float OutElastic(float t)
	{
		float num = Mathf.Pow(2f, -10f * t * 1f);
		if (!(t > 0.9999f))
		{
			return num * Mathf.Sin((t - 0.075f) * (MathF.PI * 2f) / 0.3f) + 1f;
		}
		return 1f;
	}

	private static float OutBounce(float x)
	{
		if (x < 0.36363637f)
		{
			return 7.5625f * x * x;
		}
		if (x < 0.72727275f)
		{
			return 7.5625f * (x -= 0.54545456f) * x + 0.75f;
		}
		if ((double)x < 0.9090909090909091)
		{
			return 7.5625f * (x -= 0.8181818f) * x + 0.9375f;
		}
		return 7.5625f * (x -= 21f / 22f) * x + 63f / 64f;
	}

	internal static float Evaluate(float t, Ease ease)
	{
		switch (ease)
		{
		case Ease.Linear:
			return t;
		case Ease.InSine:
			return 1f - Mathf.Cos(t * (MathF.PI / 2f));
		case Ease.OutSine:
			return Mathf.Sin(t * (MathF.PI / 2f));
		case Ease.InOutSine:
			return -0.5f * (Mathf.Cos(MathF.PI * t) - 1f);
		case Ease.InQuad:
			return t * t;
		case Ease.OutQuad:
			return (0f - t) * (t - 2f);
		case Ease.InOutQuad:
			t *= 2f;
			if (t < 1f)
			{
				return 0.5f * t * t;
			}
			return -0.5f * ((t -= 1f) * (t - 2f) - 1f);
		case Ease.InCubic:
			return t * t * t;
		case Ease.OutCubic:
			return (t -= 1f) * t * t + 1f;
		case Ease.InOutCubic:
			t *= 2f;
			if (t < 1f)
			{
				return 0.5f * t * t * t;
			}
			return 0.5f * ((t -= 2f) * t * t + 2f);
		case Ease.InQuart:
			return t * t * t * t;
		case Ease.OutQuart:
			return 0f - ((t -= 1f) * t * t * t - 1f);
		case Ease.InOutQuart:
			t *= 2f;
			if (t < 1f)
			{
				return 0.5f * t * t * t * t;
			}
			return -0.5f * ((t -= 2f) * t * t * t - 2f);
		case Ease.InQuint:
			return t * t * t * t * t;
		case Ease.OutQuint:
			return (t -= 1f) * t * t * t * t + 1f;
		case Ease.InOutQuint:
			t *= 2f;
			if (t < 1f)
			{
				return 0.5f * t * t * t * t * t;
			}
			return 0.5f * ((t -= 2f) * t * t * t * t + 2f);
		case Ease.InExpo:
			if (t != 0f)
			{
				return Mathf.Pow(2f, 10f * (t - 1f));
			}
			return 0f;
		case Ease.OutExpo:
			if (t == 1f)
			{
				return 1f;
			}
			return 0f - Mathf.Pow(2f, -10f * t) + 1f;
		case Ease.InOutExpo:
			if (t == 0f)
			{
				return 0f;
			}
			if (t == 1f)
			{
				return 1f;
			}
			t *= 2f;
			if (t < 1f)
			{
				return 0.5f * Mathf.Pow(2f, 10f * (t - 1f));
			}
			return 0.5f * (0f - Mathf.Pow(2f, -10f * (t -= 1f)) + 2f);
		case Ease.InCirc:
			return 0f - (Mathf.Sqrt(1f - t * t) - 1f);
		case Ease.OutCirc:
			return Mathf.Sqrt(1f - (t -= 1f) * t);
		case Ease.InOutCirc:
			t *= 2f;
			if (t < 1f)
			{
				return -0.5f * (Mathf.Sqrt(1f - t * t) - 1f);
			}
			return 0.5f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f);
		case Ease.InBack:
			return t * t * (2.70158f * t - 1.70158f);
		case Ease.OutBack:
			return (t -= 1f) * t * (2.70158f * t + 1.70158f) + 1f;
		case Ease.InOutBack:
			t *= 2f;
			if (t < 1f)
			{
				return 0.5f * (t * t * (3.5949094f * t - 2.5949094f));
			}
			return 0.5f * ((t -= 2f) * t * (3.5949094f * t + 2.5949094f) + 2f);
		case Ease.InElastic:
			return InElastic(t);
		case Ease.OutElastic:
			return OutElastic(t);
		case Ease.InOutElastic:
			if (t < 0.5f)
			{
				return InElastic(t * 2f) * 0.5f;
			}
			return 0.5f + OutElastic((t - 0.5f) * 2f) * 0.5f;
		case Ease.InBounce:
			return 1f - OutBounce(1f - t);
		case Ease.OutBounce:
			return OutBounce(t);
		case Ease.InOutBounce:
			if (!((double)t < 0.5))
			{
				return (1f + OutBounce(2f * t - 1f)) / 2f;
			}
			return (1f - OutBounce(1f - 2f * t)) / 2f;
		default:
			Debug.LogError($"Invalid ease type: {ease}.");
			return t;
		}
	}
}
