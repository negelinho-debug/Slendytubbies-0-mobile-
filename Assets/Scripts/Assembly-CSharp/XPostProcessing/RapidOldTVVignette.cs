using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(RapidOldTVVignetteRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Vignette/RapidOldTVVignette", true)]
public class RapidOldTVVignette : PostProcessEffectSettings
{
	public VignetteTypeParameter vignetteType = new VignetteTypeParameter
	{
		value = VignetteType.ClassicMode
	};

	[Range(0f, 5f)]
	public FloatParameter vignetteIndensity = new FloatParameter
	{
		value = 1f
	};

	public Vector2Parameter vignetteCenter = new Vector2Parameter
	{
		value = new Vector2(0.5f, 0.5f)
	};

	[ColorUsage(true, true)]
	public ColorParameter vignetteColor = new ColorParameter
	{
		value = new Color(0.1f, 0.8f, 1f)
	};
}
