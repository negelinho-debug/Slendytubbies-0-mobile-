using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchTileJitterRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/TileJitter", true)]
public class GlitchTileJitter : PostProcessEffectSettings
{
	public DirectionParameter jitterDirection = new DirectionParameter
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

	public DirectionParameter splittingDirection = new DirectionParameter
	{
		value = Direction.Vertical
	};

	[Range(0f, 50f)]
	public FloatParameter splittingNumber = new FloatParameter
	{
		value = 5f
	};

	[Range(0f, 100f)]
	public FloatParameter amount = new FloatParameter
	{
		value = 10f
	};

	[Range(0f, 1f)]
	public FloatParameter speed = new FloatParameter
	{
		value = 0.35f
	};
}
