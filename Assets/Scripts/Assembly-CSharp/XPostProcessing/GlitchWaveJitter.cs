using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchWaveJitterRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/WaveJitter", true)]
public class GlitchWaveJitter : PostProcessEffectSettings
{
	public DirectionParameter jitterDirection = new DirectionParameter
	{
		value = Direction.Horizontal
	};

	public IntervalTypeParameter intervalType = new IntervalTypeParameter
	{
		value = IntervalType.Random
	};

	[Range(0f, 50f)]
	public FloatParameter frequency = new FloatParameter
	{
		value = 5f
	};

	[Range(0f, 50f)]
	public FloatParameter RGBSplit = new FloatParameter
	{
		value = 20f
	};

	[Range(0f, 1f)]
	public FloatParameter speed = new FloatParameter
	{
		value = 0.25f
	};

	[Range(0f, 2f)]
	public FloatParameter amount = new FloatParameter
	{
		value = 0f
	};

	public BoolParameter customResolution = new BoolParameter
	{
		value = false
	};

	public Vector2Parameter resolution = new Vector2Parameter
	{
		value = new Vector2(640f, 480f)
	};
}
