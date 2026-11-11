using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorAdjustmentHueRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/Hue", true)]
public class ColorAdjustmentHue : PostProcessEffectSettings
{
	[Range(-180f, 180f)]
	public FloatParameter HueDegree = new FloatParameter
	{
		value = 20f
	};
}
