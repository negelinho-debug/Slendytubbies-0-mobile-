using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(SharpenV2Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/ImageProcessing/SharpenV2", true)]
public class SharpenV2 : PostProcessEffectSettings
{
	[Range(0f, 5f)]
	public FloatParameter Sharpness = new FloatParameter
	{
		value = 0.5f
	};
}
