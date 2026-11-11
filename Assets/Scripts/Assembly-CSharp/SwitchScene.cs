using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;

public class SwitchScene : MonoBehaviour
{
	public UnityEvent _onStart;

	public float _startDelay;

	public float _waitTime;

	public string _nextSceneName;

	[Header("Transition")]
	public PostProcessVolume _transitionVolume;

	public float _transitionDuration;

	private void Start()
	{
		StartCoroutine(SwitchSceneWithDelay());
	}

	private IEnumerator SwitchSceneWithDelay()
	{
		yield return new WaitForSeconds(_startDelay);
		_onStart.Invoke();
		yield return new WaitForSeconds(_waitTime);
		float time = 0f;
		if (_transitionVolume != null && _transitionDuration > 0f)
		{
			while (time < _transitionDuration)
			{
				time += Time.deltaTime;
				_transitionVolume.weight = Mathf.Lerp(0f, 1f, time / _transitionDuration);
				yield return null;
			}
		}
		LevelSwitch.instance.SwitchToMenu(LevelSwitch.SwitchType.NoiseNoIntro);
	}
}
