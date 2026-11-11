using System;
using System.IO;
using UnityEngine;

namespace XPostProcessing;

public class XPostProcessingUtility
{
	private static XPostProcessingUtility _instance;

	private static int resetFrameCount = 0;

	private static Color srcColor;

	private static Color dstColor;

	public int LastSelectedCategory;

	public int ThumbWidth;

	public int ThumbHeight;

	public int cache_ThumbWidth;

	public int cache_ThumbHeight;

	public bool cache_IsLinear;

	public RenderTexture PreviewRT;

	private static string TypePreFix = "XPostProcessing.";

	public static XPostProcessingUtility Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new XPostProcessingUtility();
			}
			return _instance;
		}
	}

	public static Color GetRandomLerpColor(int RandomFrameCount, float lerpSpeed)
	{
		if (resetFrameCount == 0)
		{
			srcColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
		}
		dstColor = Color.Lerp(dstColor, srcColor, lerpSpeed);
		resetFrameCount++;
		if (resetFrameCount > RandomFrameCount)
		{
			resetFrameCount = 0;
		}
		return dstColor;
	}

	public static Color RandomColor()
	{
		return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
	}

	public static void DumpRenderTexture(RenderTexture rt, string pngOutPath)
	{
		RenderTexture active = RenderTexture.active;
		Texture2D texture2D = new Texture2D(rt.width, rt.height);
		RenderTexture.active = rt;
		texture2D.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);
		texture2D.Apply();
		File.WriteAllBytes(pngOutPath, texture2D.EncodeToPNG());
		RenderTexture.active = active;
	}

	public static Type GetSettingByName(string typeFullName)
	{
		return Type.GetType(TypePreFix + typeFullName);
	}
}
