using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(BokehBlurRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Blur/BokehBlur", true)]
public class BokehBlur : PostProcessEffectSettings
{
	[Range(0f, 3f)]
	public FloatParameter BlurRadius = new FloatParameter
	{
		value = 1f
	};

	[Range(8f, 128f)]
	public IntParameter Iteration = new IntParameter
	{
		value = 32
	};

	[Range(1f, 10f)]
	public FloatParameter RTDownScaling = new FloatParameter
	{
		value = 2f
	};
}
