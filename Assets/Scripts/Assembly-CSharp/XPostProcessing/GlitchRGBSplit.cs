using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchRGBSplitRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/RGBSplit", true)]
public class GlitchRGBSplit : PostProcessEffectSettings
{
	public GlitchRGBSplitDirectionParameter SplitDirection = new GlitchRGBSplitDirectionParameter
	{
		value = DirectionEX.Horizontal
	};

	[Range(0f, 1f)]
	public FloatParameter Fading = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 5f)]
	public FloatParameter Amount = new FloatParameter
	{
		value = 0f
	};

	[Range(0f, 10f)]
	public FloatParameter Speed = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 1f)]
	public FloatParameter CenterFading = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 5f)]
	public FloatParameter AmountR = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 5f)]
	public FloatParameter AmountB = new FloatParameter
	{
		value = 1f
	};
}
