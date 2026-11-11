using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorAdjustmentBrightnessRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/Brightness", true)]
public class ColorAdjustmentBrightness : PostProcessEffectSettings
{
	[Range(-0.9f, 1f)]
	public FloatParameter brightness = new FloatParameter
	{
		value = 0f
	};
}
