using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchScreenShakeRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/ScreenShake", true)]
public class GlitchScreenShake : PostProcessEffectSettings
{
	public GlitchScreenShakeDirectionParameter ScreenShakeDirection = new GlitchScreenShakeDirectionParameter
	{
		value = Direction.Horizontal
	};

	[Range(0f, 1f)]
	public FloatParameter ScreenShakeIndensity = new FloatParameter
	{
		value = 0.5f
	};
}
