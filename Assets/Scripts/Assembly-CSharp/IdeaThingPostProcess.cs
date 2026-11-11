using UnityEngine.Rendering.PostProcessing;

[PostProcess(typeof(IdeaThingPostRenderer), PostProcessEvent.BeforeStack, "Idea Thing", false)]
public class IdeaThingPostProcess : PostProcessEffectSettings
{
	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (!context.camera.TryGetComponent<IdeaThing>(out var component) || !component.rtColor)
		{
			return false;
		}
		return base.IsEnabledAndSupported(context);
	}
}
