using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorReplaceV2Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/ColorReplaceV2", true)]
public class ColorReplaceV2 : PostProcessEffectSettings
{
	public GradientParameter FromGradientColor = new GradientParameter
	{
		value = null
	};

	public GradientParameter ToGradientColor = new GradientParameter
	{
		value = null
	};

	[Range(0f, 100f)]
	public FloatParameter gridentSpeed = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 1f)]
	public FloatParameter Range = new FloatParameter
	{
		value = 0.2f
	};

	[Range(0f, 1f)]
	public FloatParameter Fuzziness = new FloatParameter
	{
		value = 0.5f
	};
}
