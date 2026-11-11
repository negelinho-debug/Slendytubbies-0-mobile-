using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(EdgeDetectionSobelRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/EdgeDetection/EdgeDetectionSobel", true)]
public class EdgeDetectionSobel : PostProcessEffectSettings
{
	[Range(0.05f, 5f)]
	public FloatParameter edgeWidth = new FloatParameter
	{
		value = 0.3f
	};

	[ColorUsage(true, true)]
	public ColorParameter edgeColor = new ColorParameter
	{
		value = new Color(0f, 0f, 0f, 1f)
	};

	[Range(0f, 1f)]
	public FloatParameter backgroundFade = new FloatParameter
	{
		value = 1f
	};

	[ColorUsage(true, true)]
	public ColorParameter backgroundColor = new ColorParameter
	{
		value = new Color(1f, 1f, 1f, 1f)
	};
}
