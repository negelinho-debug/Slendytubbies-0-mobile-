using UnityEngine;
using UnityEngine.Events;

public class ToFlashback : MonoBehaviour
{
	public UnityEvent _flashbackEvent;

	public float _blackScreenDuration;

	public float _fadeDuration;

	public void Execute()
	{
		StartCoroutine(ScreenFade.Instance.HoldBlackScreen(_blackScreenDuration, _flashbackEvent.Invoke, _fadeDuration));
		RenderSettings.fog = true;
	}
}
