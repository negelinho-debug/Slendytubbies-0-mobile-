using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(TentBlurRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Blur/TentBlur", true)]
public class TentBlur : PostProcessEffectSettings
{
	[Range(0f, 5f)]
	public FloatParameter BlurRadius = new FloatParameter
	{
		value = 3f
	};

	[Range(1f, 20f)]
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
