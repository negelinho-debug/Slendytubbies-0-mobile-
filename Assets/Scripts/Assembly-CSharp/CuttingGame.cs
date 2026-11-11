using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CuttingGame : SplineGame
{
	public OnGameEnd _onGameEnd;

	public UnityEvent _onCompletion;

	[Header("Model")]
	public ParticleSystem _bloodSplatter;

	public SkinnedMeshRenderer[] _renderers;

	public string[] _shapeKeys;

	public float[] _offsets;

	[Header("Sound")]
	public EventReference _cuttingReference;

	public float _changeVolumeSpeed;

	[Header("Noise")]
	public float _noiseAmplitude = 1f;

	public float _noiseFrequency = 3f;

	public float _noiseLerpSpeed = 5f;

	[Header("Blood")]
	public Image[] _bloodImages;

	public float _bloodFadeOutDuration = 2f;

	public float _distanceBetweenBlood = 1500f;

	private bool isEnded;

	private EventInstance currentInstance;

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
		currentInstance = RuntimeManager.CreateInstance(_cuttingReference);
		currentInstance.start();
		bloodDistance = _distanceBetweenBlood;
		for (int i = 0; i < _bloodImages.Length; i++)
		{
			_bloodImages[i].color = Color.clear;
		}
	}

	private void OnDisable()
	{
		currentInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		currentInstance.release();
	}

	private new void Update()
	{
		base.Update();
		float magnitude = InputManager.Instance.GetLookDelta().magnitude;
		bool flag = isDragging && dot > _maxDot && magnitude > 0.01f;
		targetVolume = (flag ? 1f : 0f);
		currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * _changeVolumeSpeed);
		currentInstance.setParameterByName("Volume", currentVolume);
		ParticleSystem.EmissionModule emission = _bloodSplatter.emission;
		emission.enabled = flag;
		targetAmplitude = (flag ? _noiseAmplitude : 0f);
		currentAmplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, Time.deltaTime * _noiseLerpSpeed);
		perlinNoise.AmplitudeGain = currentAmplitude;
		perlinNoise.FrequencyGain = _noiseFrequency;
		if (isEnded)
		{
			return;
		}
		if (isDragging)
		{
			SkinnedMeshRenderer[] renderers = _renderers;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in renderers)
			{
				for (int j = 0; j < _shapeKeys.Length; j++)
				{
					int blendShapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(_shapeKeys[j]);
					float num = 1f / (float)_shapeKeys.Length;
					float num2 = (float)j * num;
					float b = num2 + num;
					float value = Mathf.InverseLerp(num2, b, currentT) * 100f;
					skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, value);
				}
				for (int k = 0; k < _offsets.Length; k++)
				{
					float num3 = 1f / (float)_offsets.Length * 0.75f;
					float num4 = (float)k * num3 + num3;
					if (currentT >= num4)
					{
						if (skinnedMeshRenderer.materials.Length > 2)
						{
							skinnedMeshRenderer.materials[1].mainTextureOffset = new Vector2(_offsets[k], 0f);
						}
						else
						{
							skinnedMeshRenderer.materials[0].mainTextureOffset = new Vector2(_offsets[k], 0f);
						}
					}
				}
			}
			if (dot > _maxDot && magnitude > 0.01f)
			{
				bloodDistance -= magnitude;
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
		if (currentT >= 1f)
		{
			isEnded = true;
			InputManager.Instance.SetLayout(InputLayoutLayer.Gameplay, PlayerInputLayout.Gameplay);
			if (_onGameEnd.Contains(OnGameEnd.HoldBlackScreen))
			{
				StartCoroutine(ScreenFade.Instance.HoldBlackScreen(1f, CompleteGame));
			}
			else
			{
				CompleteGame();
			}
		}
	}

	private void CompleteGame()
	{
		_onCompletion?.Invoke();
		if (_onGameEnd.Contains(OnGameEnd.UnfreezePlayer))
		{
			Player.Instance.FreezePlayer(isFrozen: false);
		}
		if (_onGameEnd.Contains(OnGameEnd.DisableObject))
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
