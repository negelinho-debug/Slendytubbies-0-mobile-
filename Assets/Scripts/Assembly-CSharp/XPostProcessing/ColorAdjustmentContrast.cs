using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorAdjustmentContrastRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/Contrast", true)]
public class ColorAdjustmentContrast : PostProcessEffectSettings
{
	[Range(-1f, 2f)]
	public FloatParameter contrast = new FloatParameter
	{
		value = 0.2f
	};
}
