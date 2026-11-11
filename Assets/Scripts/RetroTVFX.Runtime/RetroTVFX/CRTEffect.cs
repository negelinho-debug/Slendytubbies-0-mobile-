using UnityEngine;

namespace RetroTVFX;

[ExecuteInEditMode]
public class CRTEffect : MonoBehaviour
{
	private const int PASS_COMPOSITE_ENCODE = 0;

	private const int PASS_COMPOSITE_DECODE = 1;

	private const int PASS_COMPOSITE_FINAL = 2;

	private const int PASS_VGA = 4;

	private const int PASS_COMPONENT = 5;

	private const int PASS_SVIDEO_ENCODE = 6;

	private const int PASS_SVIDEO_DECODE = 7;

	private const int PASS_TV_OVERLAY = 3;

	[Header("Shader Properties")]
	[HideInInspector]
	public Shader shader;

	[Tooltip("Quality of the luma & chroma filters")]
	public FilterQuality FilterQuality;

	[Tooltip("Which style of video to use")]
	public VideoType VideoMode = VideoType.Composite;

	public int DisplaySizeX = 960;

	public int DisplaySizeY = 480;

	public bool StretchToDisplay = true;

	public float AspectRatio = 1.33f;

	[Tooltip("Apply curvature to display")]
	public bool EnableTVCurvature;

	[Range(0f, 1f)]
	public float Curvature;

	[Tooltip("Overlay image applied (before curvature)")]
	public Texture2D TVOverlay;

	public bool EnablePixelMask = true;

	public Texture2D PixelMaskTexture;

	public int MaskRepeatX = 160;

	public int MaskRepeatY = 90;

	[Range(1f, 2f)]
	public float PixelMaskBrightness = 1f;

	public Vector2 IQOffset = Vector2.zero;

	public Vector2 IQScale = Vector2.one;

	[Range(0f, 2f)]
	public float RFNoise = 0.25f;

	[Range(0f, 4f)]
	public float LumaSharpen;

	public bool QuantizeRGB;

	[Range(2f, 8f)]
	public int RBits = 8;

	[Range(2f, 8f)]
	public int GBits = 8;

	[Range(2f, 8f)]
	public int BBits = 8;

	public bool EnableBurstCountAnimation = true;

	public bool AntiFlicker;

	public bool EnableRollingFlicker;

	[Range(0f, 1f)]
	public float RollingFlickerFactor = 0.25f;

	[Range(0f, 2f)]
	public float RollingVSyncTime = 1f;

	private Material _material;

	private RenderTexture _compositeTemp;

	private int _frameCount;

	private float _flickerOffset;

	private bool _antiFlickerEnabled;

	private bool _rollingFlickerEnabled;

	private bool _pixelMaskEnabled;

	private bool _tvCurvatureEnabled;

	private bool _quantizeRGBEnabled;

	private bool _rfEnabled;

	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			Object.Destroy(_material);
		}
		else
		{
			Object.DestroyImmediate(_material);
		}
		_material = null;
		_antiFlickerEnabled = false;
		_rollingFlickerEnabled = false;
		_pixelMaskEnabled = false;
		_tvCurvatureEnabled = false;
		_quantizeRGBEnabled = false;
		_rfEnabled = false;
		if (_compositeTemp != null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(_compositeTemp);
			}
			else
			{
				Object.DestroyImmediate(_compositeTemp);
			}
		}
	}

	private void Update()
	{
		ensureResources();
	}

	private void LateUpdate()
	{
		if (EnableBurstCountAnimation)
		{
			_frameCount++;
			_frameCount %= 3;
		}
		if (EnableRollingFlicker)
		{
			_flickerOffset += RollingVSyncTime;
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		ensureResources();
		setKeyword("ANTI_FLICKER", AntiFlicker, ref _antiFlickerEnabled);
		setKeyword("ROLLING_FLICKER", EnableRollingFlicker, ref _rollingFlickerEnabled);
		setKeyword("PIXEL_MASK", EnablePixelMask, ref _pixelMaskEnabled);
		setKeyword("USE_TV_CURVATURE", EnableTVCurvature, ref _tvCurvatureEnabled);
		setKeyword("QUANTIZE_RGB", QuantizeRGB, ref _quantizeRGBEnabled);
		setKeyword("RF_SIGNAL", VideoMode == VideoType.RF, ref _rfEnabled);
		if (_compositeTemp == null || _compositeTemp.width != DisplaySizeX || _compositeTemp.height != DisplaySizeY)
		{
			if (_compositeTemp != null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(_compositeTemp);
				}
				else
				{
					Object.DestroyImmediate(_compositeTemp);
				}
			}
			_compositeTemp = new RenderTexture(DisplaySizeX, DisplaySizeY, src.depth, RenderTextureFormat.ARGBHalf);
		}
		if (QuantizeRGB)
		{
			Vector4 value = new Vector4(Mathf.Pow(2f, RBits), Mathf.Pow(2f, GBits), Mathf.Pow(2f, BBits), 1f);
			Vector4 value2 = new Vector4(1f / value.x, 1f / value.y, 1f / value.z, 1f);
			_material.SetVector("_QuantizeRGB", value);
			_material.SetVector("_OneOverQuantizeRGB", value2);
		}
		_material.SetFloat("_Realtime", Time.realtimeSinceStartup);
		_material.SetVector("_IQOffset", new Vector4(IQScale.x, IQScale.y, IQOffset.x, IQOffset.y));
		_material.SetMatrix("_RGB2YIQ_MAT", ColorSpaceUtils.RGB2YIQ);
		_material.SetMatrix("_YIQ2RGB_MAT", ColorSpaceUtils.YIQ2RGB);
		_material.SetFloat("_RFNoise", RFNoise);
		_material.SetFloat("_LumaSharpen", LumaSharpen);
		_material.SetInt("_Framecount", -_frameCount);
		_material.SetVector("_ScreenSize", new Vector4(DisplaySizeX, DisplaySizeY, 1f / (float)DisplaySizeX, 1f / (float)DisplaySizeY));
		_material.SetFloat("_RollingFlickerAmount", RollingFlickerFactor);
		_material.SetVector("_FlickerOffs", new Vector4(_flickerOffset, _flickerOffset + RollingVSyncTime, 0f, 0f));
		_material.SetVector("_PixelMaskScale", new Vector4(MaskRepeatX, MaskRepeatY));
		_material.SetTexture("_PixelMask", PixelMaskTexture);
		_material.SetFloat("_Brightness", PixelMaskBrightness);
		_material.SetFloat("_TVCurvature", Curvature);
		RenderTexture temporary = RenderTexture.GetTemporary(DisplaySizeX, DisplaySizeY, src.depth, RenderTextureFormat.ARGBHalf);
		RenderTexture temporary2 = RenderTexture.GetTemporary(DisplaySizeX, DisplaySizeY, src.depth, RenderTextureFormat.ARGBHalf);
		RenderTexture temporary3 = RenderTexture.GetTemporary(DisplaySizeX, DisplaySizeY, src.depth, RenderTextureFormat.ARGBHalf);
		RenderTexture src2 = temporary;
		if (VideoMode == VideoType.Composite || VideoMode == VideoType.RF)
		{
			Graphics.Blit(src, temporary);
			Graphics.Blit(temporary, temporary2, _material, 0);
			Graphics.Blit(_compositeTemp, temporary3);
			Graphics.Blit(temporary2, _compositeTemp);
			_material.SetTexture("_LastCompositeTex", temporary3);
			Graphics.Blit(temporary2, temporary, _material, 1);
			Graphics.Blit(temporary, temporary2, _material, 2);
			src2 = temporary2;
		}
		else if (VideoMode == VideoType.SVideo)
		{
			Graphics.Blit(src, temporary);
			Graphics.Blit(temporary, temporary2, _material, 6);
			Graphics.Blit(_compositeTemp, temporary3);
			Graphics.Blit(temporary2, _compositeTemp);
			_material.SetTexture("_LastCompositeTex", temporary3);
			Graphics.Blit(temporary2, temporary, _material, 7);
		}
		else if (VideoMode == VideoType.VGA)
		{
			Graphics.Blit(src, temporary, _material, 4);
		}
		else if (VideoMode == VideoType.VGAFast)
		{
			src2 = src;
		}
		else if (VideoMode == VideoType.Component)
		{
			Graphics.Blit(src, temporary, _material, 5);
		}
		if (StretchToDisplay)
		{
			GraphicsUtils.Blit(src2, dest, _material, 3);
		}
		else
		{
			float num = (float)Screen.width / (float)Screen.height;
			if (num < AspectRatio)
			{
				float width = 1f;
				float num2 = num / AspectRatio;
				float num3 = 1f - num2;
				GraphicsUtils.Blit(new Rect(0f, num3 * 0.5f, width, num2), src2, dest, _material, 3);
			}
			else
			{
				float height = 1f;
				float num4 = 1f / num * AspectRatio;
				GraphicsUtils.Blit(new Rect((1f - num4) * 0.5f, 0f, num4, height), src2, dest, _material, 3);
			}
		}
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(temporary3);
	}

	private void ensureResources()
	{
		if (_material == null)
		{
			_material = new Material(shader);
			float[] values = new float[32];
			_material.SetFloatArray("_LumaFilter", values);
			_material.SetFloatArray("_ChromaFilter", values);
		}
		_material.SetMatrix("_RGB2YIQ_MAT", ColorSpaceUtils.RGB2YIQ);
		_material.SetMatrix("_YIQ2RGB_MAT", ColorSpaceUtils.YIQ2RGB);
		_material.SetTexture("_OverlayImg", TVOverlay);
		switch (FilterQuality)
		{
		case FilterQuality.Filter_8Taps:
			_material.SetFloatArray("_LumaFilter", FilterTaps.lumaFilter8Tap);
			_material.SetFloatArray("_ChromaFilter", FilterTaps.chromaFilter8Tap);
			_material.SetInt("_FilterSize", 8);
			break;
		case FilterQuality.Filter_24Taps:
			_material.SetFloatArray("_LumaFilter", FilterTaps.lumaFilter24Taps);
			_material.SetFloatArray("_ChromaFilter", FilterTaps.chromaFilter24Taps);
			_material.SetInt("_FilterSize", 24);
			break;
		}
	}

	private void setKeyword(string keyword, bool enabled, ref bool keywordEnabled)
	{
		if (enabled != keywordEnabled)
		{
			if (enabled)
			{
				_material.EnableKeyword(keyword);
			}
			else
			{
				_material.DisableKeyword(keyword);
			}
		}
		keywordEnabled = enabled;
	}
}
