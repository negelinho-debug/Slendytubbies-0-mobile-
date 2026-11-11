using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(RapidOldTVVignetteV2Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/Vignette/RapidOldTVVignetteV2", true)]
public class RapidOldTVVignetteV2 : PostProcessEffectSettings
{
	public VignetteTypeParameter vignetteType = new VignetteTypeParameter
	{
		value = VignetteType.ClassicMode
	};

	[Range(1f, 5000f)]
	public FloatParameter vignetteSize = new FloatParameter
	{
		value = 20f
	};

	[Range(0f, 1f)]
	public FloatParameter sizeOffset = new FloatParameter
	{
		value = 0.2f
	};

	[ColorUsage(true, true)]
	public ColorParameter vignetteColor = new ColorParameter
	{
		value = new Color(0.1f, 0.8f, 1f)
	};
}
