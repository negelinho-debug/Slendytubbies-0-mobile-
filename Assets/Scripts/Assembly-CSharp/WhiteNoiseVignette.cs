using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(WhiteNoiseVignetteRenderer), PostProcessEvent.AfterStack, "Custom/White Noise Vignette", true)]
public sealed class WhiteNoiseVignette : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter intensity = new FloatParameter
	{
		value = 0f
	};

	[Range(0.01f, 1f)]
	public FloatParameter smoothness = new FloatParameter
	{
		value = 0.2f
	};

	[Range(0f, 1f)]
	public FloatParameter roundness = new FloatParameter
	{
		value = 1f
	};

	public BoolParameter rounded = new BoolParameter
	{
		value = false
	};

	[Range(1f, 500f)]
	public FloatParameter noiseScale = new FloatParameter
	{
		value = 120f
	};

	[ColorUsage(false, true)]
	public ColorParameter noiseColor = new ColorParameter
	{
		value = Color.white
	};
}
