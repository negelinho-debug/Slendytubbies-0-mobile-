using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(PixelizeLeafRenderer), PostProcessEvent.BeforeStack, "X-PostProcessing/Pixelize/PixelizeLeaf", true)]
public class PixelizeLeaf : PostProcessEffectSettings
{
	[Range(0.01f, 1f)]
	public FloatParameter pixelSize = new FloatParameter
	{
		value = 0.8f
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

	[Range(0.2f, 5f)]
	[Tooltip("像素缩放X")]
	public FloatParameter pixelScaleX = new FloatParameter
	{
		value = 1f
	};

	[Range(0.2f, 5f)]
	[Tooltip("像素缩放Y")]
	public FloatParameter pixelScaleY = new FloatParameter
	{
		value = 1f
	};
}
