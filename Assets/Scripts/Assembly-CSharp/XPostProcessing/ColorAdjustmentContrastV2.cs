using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorAdjustmentContrastV2Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/ContrastV2", true)]
public class ColorAdjustmentContrastV2 : PostProcessEffectSettings
{
	[Range(-1f, 5f)]
	public FloatParameter Contrast = new FloatParameter
	{
		value = 0.2f
	};

	[Range(-1f, 1f)]
	public FloatParameter ContrastFactorR = new FloatParameter
	{
		value = 0f
	};

	[Range(-1f, 1f)]
	public FloatParameter ContrastFactorG = new FloatParameter
	{
		value = 0f
	};

	[Range(-1f, 1f)]
	public FloatParameter ContrastFactorB = new FloatParameter
	{
		value = 0f
	};
}
