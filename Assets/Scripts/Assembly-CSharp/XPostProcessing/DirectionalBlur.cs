using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(DirectionalBlurRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Blur/DirectionalBlur", true)]
public class DirectionalBlur : PostProcessEffectSettings
{
	[Range(0f, 5f)]
	public FloatParameter BlurRadius = new FloatParameter
	{
		value = 1f
	};

	[Range(1f, 30f)]
	public IntParameter Iteration = new IntParameter
	{
		value = 12
	};

	[Range(0f, 6f)]
	public FloatParameter Angle = new FloatParameter
	{
		value = 0.5f
	};

	[Range(1f, 10f)]
	public FloatParameter RTDownScaling = new FloatParameter
	{
		value = 1f
	};
}
