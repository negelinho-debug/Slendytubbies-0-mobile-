using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(KawaseBlurRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Blur/KawaseBlur", true)]
public class KawaseBlur : PostProcessEffectSettings
{
	[Range(0f, 5f)]
	public FloatParameter BlurRadius = new FloatParameter
	{
		value = 0.5f
	};

	[Range(1f, 10f)]
	public IntParameter Iteration = new IntParameter
	{
		value = 6
	};

	[Range(1f, 8f)]
	public FloatParameter RTDownScaling = new FloatParameter
	{
		value = 2f
	};
}
