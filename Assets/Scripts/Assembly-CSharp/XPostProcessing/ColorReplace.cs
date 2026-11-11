using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(ColorReplaceRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/ColorReplace", true)]
public class ColorReplace : PostProcessEffectSettings
{
	[ColorUsage(true, true)]
	public ColorParameter FromColor = new ColorParameter
	{
		value = new Color(0.8f, 0f, 0f, 1f)
	};

	[ColorUsage(true, true)]
	public ColorParameter ToColor = new ColorParameter
	{
		value = new Color(0f, 0.8f, 0f, 1f)
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
