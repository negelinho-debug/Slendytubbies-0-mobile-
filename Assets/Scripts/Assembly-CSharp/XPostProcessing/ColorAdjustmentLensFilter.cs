using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorAdjustmentLensFilterRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/LensFilter", true)]
public class ColorAdjustmentLensFilter : PostProcessEffectSettings
{
	[ColorUsage(true, true)]
	public ColorParameter LensColor = new ColorParameter
	{
		value = new Color(1f, 1f, 0.1f, 1f)
	};

	[Range(0f, 1f)]
	public FloatParameter Indensity = new FloatParameter
	{
		value = 0.5f
	};
}
