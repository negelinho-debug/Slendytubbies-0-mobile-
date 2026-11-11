using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(TiltShiftBlurRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Blur/TiltShiftBlur/TiltShiftBlurV1", true)]
public class TiltShiftBlur : PostProcessEffectSettings
{
	public TiltShiftBlurQualityLevelParameter QualityLevel = new TiltShiftBlurQualityLevelParameter
	{
		value = TiltShiftBlurQualityLevel.High_Quality
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
