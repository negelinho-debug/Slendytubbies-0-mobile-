using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(PixelizeLedV2Renderer), PostProcessEvent.BeforeStack, "X-PostProcessing/Pixelize/PixelizeLed", true)]
public class PixelizeLed : PostProcessEffectSettings
{
	[Range(0.01f, 1f)]
	public FloatParameter pixelSize = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0.01f, 1f)]
	public FloatParameter ledRadius = new FloatParameter
	{
		value = 1f
	};

	[ColorUsage(true, true)]
	public ColorParameter BackgroundColor = new ColorParameter
	{
		value = new Color(0f, 0f, 0f)
	};

	public BoolParameter useAutoScreenRatio = new BoolParameter
	{
		value = true
	};

	[Range(0.2f, 5f)]
	public FloatParameter pixelRatio = new FloatParameter
	{
		value = 1f
	};
}
