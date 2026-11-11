using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchLineBlockRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/LineBlock", true)]
public class GlitchLineBlock : PostProcessEffectSettings
{
	public DirectionParameter blockDirection = new DirectionParameter
	{
		value = Direction.Horizontal
	};

	public IntervalTypeParameter intervalType = new IntervalTypeParameter
	{
		value = IntervalType.Random
	};

	[Range(0f, 25f)]
	public FloatParameter frequency = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 1f)]
	public FloatParameter Amount = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0.1f, 10f)]
	public FloatParameter LinesWidth = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 1f)]
	public FloatParameter Speed = new FloatParameter
	{
		value = 0.8f
	};

	[Range(0f, 13f)]
	public FloatParameter Offset = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 1f)]
	public FloatParameter Alpha = new FloatParameter
	{
		value = 1f
	};
}
