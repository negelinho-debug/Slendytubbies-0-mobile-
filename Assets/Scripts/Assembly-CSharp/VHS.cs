using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(VHSRenderer), PostProcessEvent.AfterStack, "Custom/VHS", true)]
public sealed class VHS : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter colorBleedingIntensity = new FloatParameter
	{
		value = 0.5f
	};

	[Range(2f, 8f)]
	[Tooltip("Color bleed iterations")]
	public IntParameter colorBleedIterations = new IntParameter
	{
		value = 2
	};

	[Range(0f, 1f)]
	public FloatParameter grainIntensity = new FloatParameter
	{
		value = 0.1f
	};

	[Range(0.01f, 2f)]
	public FloatParameter grainScale = new FloatParameter
	{
		value = 0.1f
	};

	[Range(0f, 1f)]
	public FloatParameter stripeNoiseDensity = new FloatParameter
	{
		value = 0.1f
	};

	[Range(0f, 1f)]
	public FloatParameter stripeNoiseOpacity = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 2f)]
	public FloatParameter edgeIntensity = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 0.005f)]
	public FloatParameter edgeDistance = new FloatParameter
	{
		value = 0.002f
	};

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (base.IsEnabledAndSupported(context))
		{
			if (!((float)colorBleedingIntensity > 0f) && !((float)edgeIntensity > 0f) && (!((float)stripeNoiseDensity > 0f) || !((float)stripeNoiseOpacity > 0f)))
			{
				return (float)grainIntensity > 0f;
			}
			return true;
		}
		return false;
	}
}
