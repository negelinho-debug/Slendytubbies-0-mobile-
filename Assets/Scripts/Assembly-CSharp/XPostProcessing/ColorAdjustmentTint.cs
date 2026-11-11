using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorAdjustmentTintRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/Tint", true)]
public class ColorAdjustmentTint : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter indensity = new FloatParameter
	{
		value = 0.1f
	};

	[ColorUsage(true, true)]
	public ColorParameter colorTint = new ColorParameter
	{
		value = new Color(0.9f, 1f, 0f, 1f)
	};
}
