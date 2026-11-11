using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(PixelizeDiamondRenderer), PostProcessEvent.BeforeStack, "X-PostProcessing/Pixelize/PixelizeDiamond", true)]
public class PixelizeDiamond : PostProcessEffectSettings
{
	[Range(0.01f, 1f)]
	public FloatParameter pixelSize = new FloatParameter
	{
		value = 0.2f
	};
}
