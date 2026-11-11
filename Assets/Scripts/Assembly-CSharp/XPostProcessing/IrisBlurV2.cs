using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(IrisBlurV2Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/Blur/IrisBlur/IrisBlurV2", true)]
public class IrisBlurV2 : PostProcessEffectSettings
{
	[Range(0f, 3f)]
	public FloatParameter BlurRadius = new FloatParameter
	{
		value = 1f
	};

	[Range(8f, 128f)]
	public IntParameter Iteration = new IntParameter
	{
		value = 60
	};

	[Range(-1f, 1f)]
	public FloatParameter centerOffsetX = new FloatParameter
	{
		value = 0f
	};

	[Range(-1f, 1f)]
	public FloatParameter centerOffsetY = new FloatParameter
	{
		value = 0f
	};

	[Range(0f, 50f)]
	public FloatParameter AreaSize = new FloatParameter
	{
		value = 8f
	};

	public BoolParameter showPreview = new BoolParameter
	{
		value = false
	};
}
