using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchDigitalStripeRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/DigitalStripe", true)]
public class GlitchDigitalStripe : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter intensity = new FloatParameter
	{
		value = 0.25f
	};

	[Range(1f, 10f)]
	public IntParameter frequncy = new IntParameter
	{
		value = 3
	};

	[Range(0f, 0.99f)]
	public FloatParameter stripeLength = new FloatParameter
	{
		value = 0.89f
	};

	[Range(8f, 256f)]
	public IntParameter noiseTextureWidth = new IntParameter
	{
		value = 20
	};

	[Range(8f, 256f)]
	public IntParameter noiseTextureHeight = new IntParameter
	{
		value = 20
	};

	public BoolParameter needStripColorAdjust = new BoolParameter
	{
		value = false
	};

	[ColorUsage(true, true)]
	public ColorParameter StripColorAdjustColor = new ColorParameter
	{
		value = new Color(0.1f, 0.1f, 0.1f)
	};

	[Range(0f, 10f)]
	public FloatParameter StripColorAdjustIndensity = new FloatParameter
	{
		value = 2f
	};
}
