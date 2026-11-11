using System;
using UnityEngine;

namespace RetroTVFX.Extras;

public class AudioHumEffect : MonoBehaviour
{
	public int HumCycle = 60;

	[Range(0f, 0.001f)]
	public float HumAmount = 0.0001f;

	[Range(0f, 0.001f)]
	public float NoiseAmount;

	private float _sampleRate = 48000f;

	private float _phase;

	private System.Random _rand = new System.Random();

	private void Start()
	{
		_sampleRate = AudioSettings.outputSampleRate;
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		float num = (float)HumCycle * MathF.PI / _sampleRate;
		for (int i = 0; i < data.Length; i += channels)
		{
			_phase += num;
			float num2 = Mathf.Sin(_phase);
			num2 = ((!(num2 >= 0f)) ? (-1f) : 1f);
			num2 *= HumAmount;
			float num3 = (float)(_rand.NextDouble() * 2.0 - 1.0);
			num3 *= NoiseAmount;
			for (int j = 0; j < channels; j++)
			{
				data[i + j] += num2 + num3;
			}
			if (_phase > MathF.PI * 2f)
			{
				_phase = 0f;
			}
		}
	}
}
