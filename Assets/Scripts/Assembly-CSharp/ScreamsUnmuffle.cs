using FMODUnity;
using UnityEngine;

public class ScreamsUnmuffle : MonoBehaviour
{
	public float _duration;

	private StudioEventEmitter emitter;

	private bool start;

	private float time;

	private void Start()
	{
		emitter = GetComponent<StudioEventEmitter>();
	}

	private void Update()
	{
		if (start && time < _duration)
		{
			time += Time.deltaTime;
			float value = Mathf.Lerp(0f, 1f, time / _duration);
			emitter.EventInstance.setParameterByName("Muffle", value);
		}
	}

	public void Unmuffle()
	{
		start = true;
	}
}
