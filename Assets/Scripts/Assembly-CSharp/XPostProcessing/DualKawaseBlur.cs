using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(DualKawaseBlurRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Blur/DualKawaseBlur", true)]
public class DualKawaseBlur : PostProcessEffectSettings
{
	[Range(0f, 15f)]
	public FloatParameter BlurRadius = new FloatParameter
	{
		value = 5f
	};

	[Range(1f, 10f)]
	public IntParameter Iteration = new IntParameter
	{
		value = 4
	};

	[Range(1f, 10f)]
	public FloatParameter RTDownScaling = new FloatParameter
	{
		value = 2f
	};
}
