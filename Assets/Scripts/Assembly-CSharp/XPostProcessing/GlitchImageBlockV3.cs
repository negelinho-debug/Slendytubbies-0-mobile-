using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing;

[Serializable]
[PostProcess(typeof(GlitchImageBlockV3Renderer), PostProcessEvent.AfterStack, "X-PostProcessing/Glitch/ImageBlockV3", true)]
public class GlitchImageBlockV3 : PostProcessEffectSettings
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
}
