using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchRGBSplitV4Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/RGBSplitV4", true)]
public class GlitchRGBSplitV4 : PostProcessEffectSettings
{
	public GlitchRGBSplitDirectionParameter splitDirection = new GlitchRGBSplitDirectionParameter
	{
		value = DirectionEX.Horizontal
	};

	[Range(-1f, 1f)]
	public FloatParameter indensity = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 100f)]
	public FloatParameter speed = new FloatParameter
	{
		value = 10f
	};
}
