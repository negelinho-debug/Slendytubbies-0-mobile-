using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(AuroraVignetteRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Vignette/AuroraVignette", true)]
public class AuroraVignette : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter vignetteArea = new FloatParameter
	{
		value = 0.8f
	};

	[Range(0f, 1f)]
	public FloatParameter vignetteSmothness = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 1f)]
	public FloatParameter vignetteFading = new FloatParameter
	{
		value = 1f
	};

	[Range(0.1f, 1f)]
	public FloatParameter colorChange = new FloatParameter
	{
		value = 0.1f
	};

	[Range(0f, 2f)]
	public FloatParameter colorFactorR = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 2f)]
	public FloatParameter colorFactorG = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 2f)]
	public FloatParameter colorFactorB = new FloatParameter
	{
		value = 1f
	};

	[Range(-2f, 2f)]
	public FloatParameter flowSpeed = new FloatParameter
	{
		value = 1f
	};
}
