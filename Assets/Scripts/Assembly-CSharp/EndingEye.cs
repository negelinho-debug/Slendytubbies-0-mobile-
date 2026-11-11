using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class EndingEye : MonoBehaviour
{
	public Light _spotLight;

	public StudioEventEmitter _musicEmitter;

	public float _pitchFrequency = 1f;

	public float _minIntensity;

	public float _maxIntensity = 3f;

	[Range(1f, 50f)]
	public int _smoothing = 5;

	public float _lerpSpeed = 4f;

	private Queue<float> smoothQueue;

	private float lastSum;

	private void Start()
	{
		smoothQueue = new Queue<float>(_smoothing);
	}

	private void Update()
	{
		if (smoothQueue.Count >= _smoothing)
		{
			lastSum -= smoothQueue.Dequeue();
		}
		float num = Random.Range(_minIntensity, _maxIntensity);
		smoothQueue.Enqueue(num);
		lastSum += num;
		_spotLight.intensity = Mathf.Lerp(_spotLight.intensity, lastSum / (float)smoothQueue.Count, Time.deltaTime * _lerpSpeed);
		_musicEmitter.SetParameter("Pitch", (Mathf.Sin(Time.time * _pitchFrequency) + 1f) * 0.5f);
	}
}
