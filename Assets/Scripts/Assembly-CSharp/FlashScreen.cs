using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class FlashScreen : MonoBehaviour
{
	public Image _screen;

	[Range(0f, 1f)]
	public float _startAlpha;

	[Range(0f, 1f)]
	public float _endAlpha = 1f;

	public float _startDelay = 0.2f;

	public float _flashDuration = 1f;

	public void Flash()
	{
		Color color = _screen.color;
		color.a = _startAlpha;
		_screen.color = color;
		color.a = _endAlpha;
		Image screen = _screen;
		Color endValue = color;
		float flashDuration = _flashDuration;
		float startDelay = _startDelay;
		Tween.Color((Graphic)screen, endValue, flashDuration, default(PrimeTween.Easing), 1, CycleMode.Restart, startDelay, 0f, useUnscaledTime: false);
	}
}
