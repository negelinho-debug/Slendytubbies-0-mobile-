using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CuttingWhite : SplineGame
{
	public UnityEvent _onCompletion;

	[Header("Model")]
	public ParticleSystem _bloodSplatter;

	public float _sectionMult = 1f;

	public Renderer[] _renderers;

	public float[] _offsets;

	[Header("Sound")]
	public StudioEventEmitter _eventEmitter;

	public EventReference _cuttingReference;

	public EventReference _faceOnReference;

	public EventReference _breathingReference;

	public float _changeVolumeSpeed;

	[Header("Noise")]
	public float _noiseAmplitude = 1f;

	public float _noiseFrequency = 3f;

	public float _noiseLerpSpeed = 5f;

	[Header("Blood")]
	public Image[] _bloodImages;

	public float _bloodFadeOutDuration = 2f;

	public float _distanceBetweenBlood = 1500f;

	[Header("Ending")]
	public GameObject _faceWhite;

	public GameObject _faceOnCamera;

	[Space(10f)]
	public float _faceOffDelay = 0.2f;

	public float _faceOffDuration = 0.8f;

	public float _faceAppearDelay = 0.2f;

	public float _endingDelay = 1f;

	private bool isComplete;

	private EventInstance cuttingInstance;

	private EventInstance breathingInstance;

	private float currentVolume;

	private float targetVolume;

	private CinemachineBasicMultiChannelPerlin perlinNoise;

	private float currentAmplitude;

	private float targetAmplitude;

	private int lastBloodImage = -1;

	private float bloodDistance;

	private new void Start()
	{
		base.Start();
		perlinNoise = GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
		Player.Instance.Brain.DefaultBlend.Time = 0f;
		cuttingInstance = RuntimeManager.CreateInstance(_cuttingReference);
		cuttingInstance.start();
		breathingInstance = RuntimeManager.CreateInstance(_breathingReference);
		_eventEmitter.enabled = true;
		_faceOnCamera.gameObject.SetActive(value: false);
		bloodDistance = _distanceBetweenBlood;
		for (int i = 0; i < _bloodImages.Length; i++)
		{
			_bloodImages[i].color = Color.clear;
		}
	}

	private void OnDisable()
	{
		cuttingInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		cuttingInstance.release();
		breathingInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		breathingInstance.release();
	}

	private new void Update()
	{
		base.Update();
		bool flag = isDragging && dot > _maxDot && InputManager.Instance.GetLookDelta().magnitude > 0.01f;
		targetVolume = (flag ? 1f : 0f);
		currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * _changeVolumeSpeed);
		cuttingInstance.setParameterByName("Volume", currentVolume);
		ParticleSystem.EmissionModule emission = _bloodSplatter.emission;
		emission.enabled = flag;
		float x = Mathf.Lerp(90f, -270f, currentT);
		_bloodSplatter.transform.rotation = Quaternion.Euler(x, -90f, 0f);
		targetAmplitude = (flag ? _noiseAmplitude : 0f);
		currentAmplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, Time.deltaTime * _noiseLerpSpeed);
		perlinNoise.AmplitudeGain = currentAmplitude;
		perlinNoise.FrequencyGain = _noiseFrequency;
		if (isDragging)
		{
			Renderer[] renderers = _renderers;
			foreach (Renderer renderer in renderers)
			{
				for (int j = 0; j < _offsets.Length; j++)
				{
					float num = 1f / (float)_offsets.Length * _sectionMult;
					float num2 = (float)j * num + num;
					if (currentT >= num2)
					{
						if (renderer.materials.Length > 2)
						{
							renderer.materials[1].mainTextureOffset = new Vector2(_offsets[j], 0f);
						}
						else
						{
							renderer.materials[0].mainTextureOffset = new Vector2(_offsets[j], 0f);
						}
					}
				}
			}
			if (dot > _maxDot && InputManager.Instance.GetLookDelta().magnitude > 0.01f)
			{
				bloodDistance -= InputManager.Instance.GetLookDelta().magnitude;
			}
			if (bloodDistance <= 0f)
			{
				List<int> list = Enumerable.Range(0, _bloodImages.Length).ToList();
				list.Remove(lastBloodImage);
				int index = Random.Range(0, list.Count);
				Tween.Color((Graphic)_bloodImages[list[index]], Color.white, Color.clear, _bloodFadeOutDuration, default(PrimeTween.Easing), 1, CycleMode.Restart, 0f, 0f, useUnscaledTime: false);
				bloodDistance = _distanceBetweenBlood;
				lastBloodImage = list[index];
			}
		}
		if (currentT >= 1f && !isComplete)
		{
			isComplete = true;
			StartCoroutine(Ending());
		}
	}

	private IEnumerator Ending()
	{
		_handle.gameObject.SetActive(value: false);
		yield return new WaitForSeconds(_faceOffDelay);
		Vector3 startPos = _faceWhite.transform.position;
		float time = 0f;
		while (time < _faceOffDuration)
		{
			time += Time.deltaTime;
			_faceWhite.transform.position = Vector3.Lerp(startPos, startPos + Vector3.up, time / _faceOffDuration);
			yield return null;
		}
		_faceWhite.gameObject.SetActive(value: false);
		yield return new WaitForSeconds(_faceAppearDelay);
		RuntimeManager.PlayOneShot(_faceOnReference);
		_faceOnCamera.gameObject.SetActive(value: true);
		breathingInstance.start();
		yield return new WaitForSeconds(_endingDelay);
		_onCompletion.Invoke();
	}
}
