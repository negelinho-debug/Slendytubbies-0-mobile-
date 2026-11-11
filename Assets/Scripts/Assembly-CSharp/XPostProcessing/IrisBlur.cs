using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(IrisBlurRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Blur/IrisBlur/IrisBlurV1", true)]
public class IrisBlur : PostProcessEffectSettings
{
	public IrisBlurQualityLevelParameter QualityLevel = new IrisBlurQualityLevelParameter
	{
		value = IrisBlurQualityLevel.High_Quality
	};

	[Range(0f, 1f)]
	public FloatParameter AreaSize = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 1f)]
	public FloatParameter BlurRadius = new FloatParameter
	{
		value = 1f
	};

	[Range(1f, 8f)]
	public IntParameter Iteration = new IntParameter
	{
		value = 2
	};

	[Range(1f, 2f)]
	public FloatParameter RTDownScaling = new FloatParameter
	{
		value = 1f
	};
}
