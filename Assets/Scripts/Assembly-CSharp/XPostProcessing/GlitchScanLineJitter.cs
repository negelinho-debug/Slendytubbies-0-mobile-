using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchScanLineJitterRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/ScanLineJitter", true)]
public class GlitchScanLineJitter : PostProcessEffectSettings
{
	public DirectionParameter JitterDirection = new DirectionParameter
	{
		value = Direction.Horizontal
	};

	public IntervalTypeParameter intervalType = new IntervalTypeParameter
	{
		value = IntervalType.Random
	};

	[Range(0f, 25f)]
	public FloatParameter frequency = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 1f)]
	public FloatParameter JitterIndensity = new FloatParameter
	{
		value = 0.1f
	};
}
