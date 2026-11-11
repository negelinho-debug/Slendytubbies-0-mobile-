using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchImageBlockV4Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/ImageBlockV4", true)]
public class GlitchImageBlockV4 : PostProcessEffectSettings
{
	[Range(0f, 50f)]
	public FloatParameter Speed = new FloatParameter
	{
		value = 10f
	};

	[Range(0f, 50f)]
	public FloatParameter BlockSize = new FloatParameter
	{
		value = 8f
	};

	[Range(0f, 25f)]
	public FloatParameter MaxRGBSplitX = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 25f)]
	public FloatParameter MaxRGBSplitY = new FloatParameter
	{
		value = 1f
	};
}
