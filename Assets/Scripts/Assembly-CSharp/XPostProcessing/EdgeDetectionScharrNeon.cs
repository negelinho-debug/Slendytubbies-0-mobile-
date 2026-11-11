using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(EdgeDetectionScharrNeonRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/EdgeDetection/EdgeDetectionScharrNeon", true)]
public class EdgeDetectionScharrNeon : PostProcessEffectSettings
{
	[Range(0.05f, 5f)]
	public FloatParameter EdgeWidth = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 1f)]
	public FloatParameter BackgroundFade = new FloatParameter
	{
		value = 1f
	};

	[Range(0.2f, 2f)]
	public FloatParameter Brigtness = new FloatParameter
	{
		value = 1f
	};

	[ColorUsage(true, true)]
	public ColorParameter BackgroundColor = new ColorParameter
	{
		value = new Color(0f, 0f, 0f, 1f)
	};
}
