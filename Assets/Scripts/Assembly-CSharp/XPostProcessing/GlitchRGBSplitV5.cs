using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchRGBSplitV5Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/RGBSplitV5", true)]
public class GlitchRGBSplitV5 : PostProcessEffectSettings
{
	[Range(0f, 5f)]
	public FloatParameter Amplitude = new FloatParameter
	{
		value = 3f
	};

	[Range(0f, 1f)]
	public FloatParameter Speed = new FloatParameter
	{
		value = 0.1f
	};
}
