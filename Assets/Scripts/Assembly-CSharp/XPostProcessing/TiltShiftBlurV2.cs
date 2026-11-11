using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(TiltShiftBlurV2Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/Blur/TiltShiftBlur/TiltShiftBlurV2", true)]
public class TiltShiftBlurV2 : PostProcessEffectSettings
{
	[Range(0f, 3f)]
	public FloatParameter BlurRadius = new FloatParameter
	{
		value = 1f
	};

	[Range(8f, 128f)]
	public IntParameter Iteration = new IntParameter
	{
		value = 32
	};

	[Range(-1f, 1f)]
	public FloatParameter centerOffset = new FloatParameter
	{
		value = 0f
	};

	[Range(0f, 20f)]
	public FloatParameter AreaSize = new FloatParameter
	{
		value = 1f
	};

	[Range(1f, 20f)]
	public FloatParameter areaSmooth = new FloatParameter
	{
		value = 1.2f
	};

	public BoolParameter showPreview = new BoolParameter
	{
		value = false
	};
}
