using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchRGBSplitV2Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/RGBSplitV2", true)]
public class GlitchRGBSplitV2 : PostProcessEffectSettings
{
	public GlitchRGBSplitDirectionParameter SplitDirection = new GlitchRGBSplitDirectionParameter
	{
		value = DirectionEX.Horizontal
	};

	[Range(0f, 1f)]
	public FloatParameter Amount = new FloatParameter
	{
		value = 0.5f
	};

	[Range(1f, 6f)]
	public FloatParameter Amplitude = new FloatParameter
	{
		value = 3f
	};

	[Range(0f, 2f)]
	public FloatParameter Speed = new FloatParameter
	{
		value = 1f
	};
}
