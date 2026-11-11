using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;

namespace HostGame.HeightFog;

[Serializable]
[PostProcess(typeof(HeightFogRenderer), PostProcessEvent.BeforeTransparent, "Custom/Height Fog", true)]
public class HeightFog : PostProcessEffectSettings
{
	[Tooltip("[Default = 0]\nDensity of the fog particles. To work correctly you need to keep 'Smooth Length' equal to 1")]
	[Range(0f, 1f)]
	public FloatParameter density = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("[Default = -1]\nOffsets fog density from camera")]
	[UnityEngine.Rendering.PostProcessing.Min(0f)]
	public FloatParameter minDistnace = new FloatParameter
	{
		value = -1f
	};

	[Tooltip("[Default = 1]")]
	[UnityEngine.Rendering.PostProcessing.Min(0f)]
	public FloatParameter smoothLength = new FloatParameter
	{
		value = 1f
	};

	[FormerlySerializedAs("maxDistance")]
	[Range(0f, 1f)]
	public FloatParameter densityAmplifier = new FloatParameter();

	[Space]
	[ColorUsage(false, true)]
	public ColorParameter emissive = new ColorParameter
	{
		value = Color.white
	};

	[Space]
	[UnityEngine.Rendering.PostProcessing.Min(0f)]
	public FloatParameter sunLightStength = new FloatParameter();

	[Tooltip("Multiply - 'Sun Color' will multiply color of the current directional light\nOverride - 'Sun Color' will replace color of the current directional light")]
	public SunColorTypeEnumParameter sunMode = new SunColorTypeEnumParameter();

	[ColorUsage(false, true)]
	public ColorParameter sunColor = new ColorParameter
	{
		value = Color.white
	};

	[Header("Phase")]
	[Tooltip("[Default = -0.5]\nNegative values will collect light from the sun into a point, around zero will evenly spread light color and positive values will make sun appear on the opposite side")]
	[Range(-1f, 1f)]
	public FloatParameter phase = new FloatParameter
	{
		value = -0.5f
	};

	[Range(0f, 1f)]
	public FloatParameter phaseW0 = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 1f)]
	public FloatParameter phaseW1 = new FloatParameter
	{
		value = 0f
	};

	[Header("Blur")]
	[Range(0f, 16f)]
	public FloatParameter blurPower = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 1f)]
	public FloatParameter blurStrength = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 1f)]
	public FloatParameter blurRadius = new FloatParameter
	{
		value = 1f
	};

	[Header("Height")]
	[UnityEngine.Rendering.PostProcessing.Min(0f)]
	public FloatParameter minHeight = new FloatParameter();

	[UnityEngine.Rendering.PostProcessing.Min(0f)]
	public FloatParameter transitionLength = new FloatParameter
	{
		value = 100f
	};

	[Header("Sky")]
	public BoolParameter skyHandling = new BoolParameter
	{
		value = true
	};

	public FloatParameter skyPower = new FloatParameter
	{
		value = 1f
	};

	[Tooltip("Represents percentage of sky that will be populated with fog. Everything below left handle is fully covered in fog, everything above is fully visible")]
	[MinMax(0f, 1f)]
	public Vector2Parameter skyFillRange = new Vector2Parameter
	{
		value = new Vector2(0f, 1f)
	};
}
