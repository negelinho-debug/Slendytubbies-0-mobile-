using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class ScanLinesRenderer : PostProcessEffectRenderer<ScanLines>
{
	private Shader shader;

	public override void Init()
	{
		shader = Shader.Find("Hidden/Custom/ScanLines");
	}

	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(shader);
		propertySheet.properties.SetFloat("_Speed", base.settings.speed);
		propertySheet.properties.SetFloat("_Thickness", base.settings.thickness);
		propertySheet.properties.SetFloat("_Intensity", base.settings.intensity);
		propertySheet.properties.SetFloat("_TimeY", Time.time);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
