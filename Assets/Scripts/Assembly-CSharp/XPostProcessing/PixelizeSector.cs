using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(PixelizeSectorRenderer), PostProcessEvent.BeforeStack, "X-PostProcessing/Pixelize/PixelizeSector", true)]
public class PixelizeSector : PostProcessEffectSettings
{
	[Range(0.01f, 1f)]
	public FloatParameter pixelSize = new FloatParameter
	{
		value = 0.8f
	};

	[Range(0.01f, 1f)]
	public FloatParameter circleRadius = new FloatParameter
	{
		value = 0.8f
	};

	[Range(0.2f, 5f)]
	[Tooltip("Pixel interval X")]
	public FloatParameter pixelIntervalX = new FloatParameter
	{
		value = 1f
	};

	[Range(0.2f, 5f)]
	[Tooltip("Pixel interval Y")]
	public FloatParameter pixelIntervalY = new FloatParameter
	{
		value = 1f
	};

	[ColorUsage(true, true)]
	public ColorParameter BackgroundColor = new ColorParameter
	{
		value = new Color(0f, 0f, 0f)
	};
}
