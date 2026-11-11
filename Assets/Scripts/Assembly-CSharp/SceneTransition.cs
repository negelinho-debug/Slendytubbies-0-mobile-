using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
	public PostProcessVolume _transitionVolume;

	public float _transitionDuration = 1f;

	public string _nextSceneName;

	public void StartTransition()
	{
		StartCoroutine(TransitionAnimation());
	}

	private IEnumerator TransitionAnimation()
	{
		float time = 0f;
		while (time < _transitionDuration)
		{
			time += Time.deltaTime;
			_transitionVolume.weight = Mathf.Lerp(0f, 1f, time / _transitionDuration);
			yield return null;
		}
		SceneManager.LoadScene(_nextSceneName);
	}
}
