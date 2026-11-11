using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorAdjustmentTechnicolorRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/Technicolor", true)]
public class ColorAdjustmentTechnicolor : PostProcessEffectSettings
{
	[Range(0f, 8f)]
	public FloatParameter exposure = new FloatParameter
	{
		value = 4f
	};

	[Range(0f, 1f)]
	public FloatParameter colorBalanceR = new FloatParameter
	{
		value = 0.2f
	};

	[Range(0f, 1f)]
	public FloatParameter colorBalanceG = new FloatParameter
	{
		value = 0.2f
	};

	[Range(0f, 1f)]
	public FloatParameter colorBalanceB = new FloatParameter
	{
		value = 0.2f
	};

	[Range(0f, 1f)]
	public FloatParameter indensity = new FloatParameter
	{
		value = 0.5f
	};
}
