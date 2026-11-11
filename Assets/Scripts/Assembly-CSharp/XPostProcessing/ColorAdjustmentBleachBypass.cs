using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorAdjustmentBleachBypassRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/BleachBypass", true)]
public class ColorAdjustmentBleachBypass : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter Indensity = new FloatParameter
	{
		value = 0.5f
	};
}
