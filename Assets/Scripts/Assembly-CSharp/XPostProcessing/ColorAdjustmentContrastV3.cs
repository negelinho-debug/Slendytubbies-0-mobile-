using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorAdjustmentContrastV3Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/ContrastV3", true)]
public class ColorAdjustmentContrastV3 : PostProcessEffectSettings
{
	[DisplayName("Contrast Wheel")]
	[ColorWheel(ColorWheelAttribute.Mode.Contrast)]
	public Vector4Parameter contrast = new Vector4Parameter
	{
		value = new Vector4(1f, 1f, 1f, -0.1f)
	};
}
