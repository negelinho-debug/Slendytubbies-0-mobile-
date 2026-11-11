using UnityEngine;

namespace RetroTVFX.Extras;

[ExecuteInEditMode]
public class LoResEffect : MonoBehaviour
{
	public int ScreenResX = 320;

	public int ScreenResY = 240;

	public bool PointFilter = true;

	public bool OverrideAspect;

	public float CamAspect = 1f;

	public Camera MainCam;

	public Camera[] CamArray;

	private RenderTexture _tempTex;

	private void OnDestroy()
	{
		cleanupTempTex();
	}

	private void cleanupTempTex()
	{
		if (_tempTex != null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(_tempTex);
			}
			else
			{
				Object.DestroyImmediate(_tempTex);
			}
		}
	}

	private void createTempTex(int depth, RenderTextureFormat format)
	{
		cleanupTempTex();
		_tempTex = new RenderTexture(ScreenResX, ScreenResY, depth, format);
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (MainCam == null)
		{
			Graphics.Blit(src, dest);
			return;
		}
		if (_tempTex == null || _tempTex.width != ScreenResX || _tempTex.height != ScreenResY)
		{
			createTempTex(src.depth, src.format);
		}
		float num = (float)ScreenResX / (float)ScreenResY;
		float aspect = (OverrideAspect ? CamAspect : num);
		_tempTex.filterMode = ((!PointFilter) ? FilterMode.Bilinear : FilterMode.Point);
		MainCam.aspect = aspect;
		MainCam.targetTexture = _tempTex;
		MainCam.Render();
		if (CamArray != null)
		{
			for (int i = 0; i < CamArray.Length; i++)
			{
				CamArray[i].aspect = aspect;
				CamArray[i].targetTexture = _tempTex;
				CamArray[i].Render();
			}
		}
		Graphics.Blit(_tempTex, dest);
	}
}
