using UnityEngine.Rendering.PostProcessing;

[PostProcess(typeof(LevelTransitionEffectRenderer), PostProcessEvent.AfterStack, "Level Transition", true)]
public class LevelTransitionEffect : PostProcessEffectSettings
{
	public TextureParameter _noise = new TextureParameter();

	public FloatParameter _noiseStrength = new FloatParameter();

	public Vector2Parameter _noiseTiling = new Vector2Parameter();

	public Vector2Parameter _offset = new Vector2Parameter();

	public FloatParameter _blurStrength = new FloatParameter();
}
