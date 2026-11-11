using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchScreenJumpRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/ScreenJump", false)]
public class GlitchScreenJump : PostProcessEffectSettings
{
	public GlitchScreenJumpDirectionParameter ScreenJumpDirection = new GlitchScreenJumpDirectionParameter
	{
		value = Direction.Vertical
	};

	[Range(0f, 1f)]
	public FloatParameter ScreenJumpIndensity = new FloatParameter
	{
		value = 0.35f
	};
}
