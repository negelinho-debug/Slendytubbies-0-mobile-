using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class StartTransition : MonoBehaviour
{
	public float _transitionDuration = 1f;

	private PostProcessVolume transitionVolume;

	private void Start()
	{
		transitionVolume = GetComponent<PostProcessVolume>();
		StartCoroutine(TransitionAnimation());
	}

	private IEnumerator TransitionAnimation()
	{
		float time = 0f;
		while (time < _transitionDuration)
		{
			time += Time.deltaTime;
			transitionVolume.weight = Mathf.Lerp(1f, 0f, time / _transitionDuration);
			yield return null;
		}
	}
}
