using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchRGBSplitV3Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/RGBSplitV3", true)]
public class GlitchRGBSplitV3 : PostProcessEffectSettings
{
	public DirectionEXParameter SplitDirection = new DirectionEXParameter
	{
		value = DirectionEX.Horizontal
	};

	public IntervalTypeParameter intervalType = new IntervalTypeParameter
	{
		value = IntervalType.Random
	};

	[Range(0.1f, 25f)]
	public FloatParameter Frequency = new FloatParameter
	{
		value = 3f
	};

	[Range(0f, 200f)]
	public FloatParameter Amount = new FloatParameter
	{
		value = 30f
	};

	[Range(0f, 15f)]
	public FloatParameter Speed = new FloatParameter
	{
		value = 20f
	};
}
