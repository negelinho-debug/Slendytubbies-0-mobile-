using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorAdjustmentWhiteBalanceRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/WhiteBalance", true)]
public class ColorAdjustmentWhiteBalance : PostProcessEffectSettings
{
	[Range(-1f, 1f)]
	public FloatParameter temperature = new FloatParameter
	{
		value = 0f
	};

	[Range(-1f, 1f)]
	public FloatParameter tint = new FloatParameter
	{
		value = 0f
	};
}
