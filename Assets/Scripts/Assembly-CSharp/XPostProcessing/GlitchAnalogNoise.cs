using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchAnalogNoiseRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/AnalogNoise", true)]
public class GlitchAnalogNoise : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter NoiseSpeed = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 1f)]
	public FloatParameter NoiseFading = new FloatParameter
	{
		value = 0f
	};

	[Range(0f, 1f)]
	public FloatParameter LuminanceJitterThreshold = new FloatParameter
	{
		value = 0.8f
	};
}
