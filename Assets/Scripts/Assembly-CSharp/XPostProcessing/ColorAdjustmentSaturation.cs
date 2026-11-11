using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorAdjustmentSaturationRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/Saturation", true)]
public class ColorAdjustmentSaturation : PostProcessEffectSettings
{
	[Range(0f, 2f)]
	public FloatParameter saturation = new FloatParameter
	{
		value = 1f
	};
}
