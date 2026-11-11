using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchImageBlockRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/ImageBlock", true)]
public class GlitchImageBlock : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter Fade = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 1f)]
	public FloatParameter Speed = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 10f)]
	public FloatParameter Amount = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 50f)]
	public FloatParameter BlockLayer1_U = new FloatParameter
	{
		value = 9f
	};

	[Range(0f, 50f)]
	public FloatParameter BlockLayer1_V = new FloatParameter
	{
		value = 9f
	};

	[Range(0f, 50f)]
	public FloatParameter BlockLayer2_U = new FloatParameter
	{
		value = 5f
	};

	[Range(0f, 50f)]
	public FloatParameter BlockLayer2_V = new FloatParameter
	{
		value = 5f
	};

	[Range(0f, 50f)]
	public FloatParameter BlockLayer1_Indensity = new FloatParameter
	{
		value = 8f
	};

	[Range(0f, 50f)]
	public FloatParameter BlockLayer2_Indensity = new FloatParameter
	{
		value = 4f
	};

	[Range(0f, 50f)]
	public FloatParameter RGBSplitIndensity = new FloatParameter
	{
		value = 0.5f
	};

	public BoolParameter BlockVisualizeDebug = new BoolParameter
	{
		value = false
	};
}
