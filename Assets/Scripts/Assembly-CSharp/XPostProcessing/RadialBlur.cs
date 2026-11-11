using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(RadialBlurRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Blur/RadialBlur/RadialBlurV1", true)]
public class RadialBlur : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter BlurRadius = new FloatParameter
	{
		value = 0.6f
	};

	[Range(2f, 30f)]
	public IntParameter Iteration = new IntParameter
	{
		value = 10
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
