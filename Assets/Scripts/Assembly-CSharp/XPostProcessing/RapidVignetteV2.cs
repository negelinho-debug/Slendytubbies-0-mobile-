using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(RapidVignetteV2Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/Vignette/RapidVignetteV2", true)]
public class RapidVignetteV2 : PostProcessEffectSettings
{
	public VignetteTypeParameter vignetteType = new VignetteTypeParameter
	{
		value = VignetteType.ClassicMode
	};

	[Range(0f, 5f)]
	public FloatParameter vignetteIndensity = new FloatParameter
	{
		value = 0f
	};

	[Range(-1f, 1f)]
	public FloatParameter vignetteSharpness = new FloatParameter
	{
		value = 0.1f
	};

	public Vector2Parameter vignetteCenter = new Vector2Parameter
	{
		value = new Vector2(0.5f, 0.5f)
	};

	[ColorUsage(true, true)]
	public ColorParameter vignetteColor = new ColorParameter
	{
		value = new Color(1f, 1f, 1f)
	};

	public GradientParameter grident1 = new GradientParameter
	{
		value = null
	};
}
