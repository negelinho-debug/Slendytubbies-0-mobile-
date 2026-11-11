using UnityEngine;

namespace RetroTVFX.Extras;

[ExecuteInEditMode]
public class FadeEffect : MonoBehaviour
{
	[HideInInspector]
	public Shader FadeShader;

	public Color FadeColor = Color.black;

	[Range(0f, 1f)]
	public float FadeSeparation = 0.5f;

	[Range(0f, 1f)]
	public float FadeFactor;

	private Material _mat;

	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			Object.Destroy(_mat);
		}
		else
		{
			Object.DestroyImmediate(_mat);
		}
	}

	private float eval(float input, float start, float end)
	{
		return Mathf.Clamp01((input - start) / (end - start));
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (_mat == null)
		{
			_mat = new Material(FadeShader);
		}
		float num = FadeSeparation * 0.66f;
		float x = eval(FadeFactor, 0f, 1f - num);
		float y = eval(FadeFactor, num * 0.5f, 1f - num * 0.5f);
		float z = eval(FadeFactor, num, 1f);
		_mat.SetColor("_FadeColor", FadeColor);
		_mat.SetVector("_FadeFactor", new Vector4(x, y, z, 0f));
		Graphics.Blit(src, dest, _mat, 0);
	}
}
