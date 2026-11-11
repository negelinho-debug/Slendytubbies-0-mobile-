using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(SharpenV1Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/ImageProcessing/SharpenV1", true)]
public class SharpenV1 : PostProcessEffectSettings
{
	[Range(0f, 5f)]
	public FloatParameter Strength = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 1f)]
	public FloatParameter Threshold = new FloatParameter
	{
		value = 0.1f
	};
}
