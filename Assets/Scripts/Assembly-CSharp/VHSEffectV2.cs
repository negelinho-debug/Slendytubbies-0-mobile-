using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[PostProcess(typeof(VHSEffectV2Renderer), PostProcessEvent.AfterStack, "Custom/VHS Effect V2", false)]
public class VHSEffectV2 : PostProcessEffectSettings
{
	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (base.IsEnabledAndSupported(context))
		{
			return Application.isPlaying;
		}
		return false;
	}
}
