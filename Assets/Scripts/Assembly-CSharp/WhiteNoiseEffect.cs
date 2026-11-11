using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(WhiteNoiseRenderer), PostProcessEvent.AfterStack, "Custom/White Noise", true)]
public sealed class WhiteNoiseEffect : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter intensity = new FloatParameter
	{
		value = 0.2f
	};

	[Range(1f, 500f)]
	public FloatParameter scale = new FloatParameter
	{
		value = 100f
	};
}
