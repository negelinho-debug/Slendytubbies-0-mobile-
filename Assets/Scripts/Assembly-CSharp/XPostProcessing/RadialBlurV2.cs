using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(RadialBlurV2Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/Blur/RadialBlur/RadialBlurV2", true)]
public class RadialBlurV2 : PostProcessEffectSettings
{
	public RadialBlurQualityParameter QualityLevel = new RadialBlurQualityParameter
	{
		value = RadialBlurQuality.RadialBlur_8Tap_Balance
	};

	[Range(-1f, 1f)]
	public FloatParameter BlurRadius = new FloatParameter
	{
		value = 0.6f
	};

	[Range(0f, 1f)]
	public FloatParameter RadialCenterX = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 1f)]
	public FloatParameter RadialCenterY = new FloatParameter
	{
		value = 0.5f
	};
}
