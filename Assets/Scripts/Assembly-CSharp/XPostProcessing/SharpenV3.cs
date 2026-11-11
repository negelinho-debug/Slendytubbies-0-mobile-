using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(SharpenV3Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/ImageProcessing/SharpenV3", true)]
public class SharpenV3 : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter Sharpness = new FloatParameter
	{
		value = 0.5f
	};
}
