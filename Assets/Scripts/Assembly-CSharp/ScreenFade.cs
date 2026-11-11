using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
	public static ScreenFade Instance;

	public Image _fadeImage;

	private Color fadeColor;

	private float startOpacity;

	private float endOpacity;

	private float fadeDuration;

	private float fadeTime;

	public bool isFading { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			Instance = this;
		}
		fadeColor = _fadeImage.color;
	}

	public void StartFade(float start, float end, float duration)
	{
		startOpacity = (fadeColor.a = start);
		endOpacity = end;
		fadeDuration = duration;
		fadeTime = 0f;
		_fadeImage.color = fadeColor;
	}

	public void FadeIn(float duration)
	{
		StartFade(0f, 1f, duration);
	}

	public void FadeOut(float duration)
	{
		StartFade(1f, 0f, duration);
	}

	public void SetFade(float opacity)
	{
		fadeColor.a = opacity;
		_fadeImage.color = fadeColor;
		endOpacity = opacity - opacity;
	}

	public IEnumerator HoldBlackScreen(float holdDuration, Action executeOnEnd = null, float duration = 0f)
	{
		SetFade(1f);
		yield return new WaitForSeconds(holdDuration);
		if (duration > 0f)
		{
			StartFade(1f, 0f, duration);
		}
		executeOnEnd?.Invoke();
	}

	private void Update()
	{
		if (fadeTime < fadeDuration)
		{
			fadeTime += Time.deltaTime;
			fadeColor.a = Mathf.Lerp(startOpacity, endOpacity, fadeTime / fadeDuration);
			_fadeImage.color = fadeColor;
		}
		isFading = fadeTime < fadeDuration || fadeColor.a != endOpacity;
	}
}
