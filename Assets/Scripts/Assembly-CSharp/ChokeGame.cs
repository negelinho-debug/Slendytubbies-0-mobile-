using FMOD.Studio;
using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class ChokeGame : SpamGame
{
	[Header("Variables")]
	public float _progressPerTick = 0.02f;

	public AnimationCurve _difficultyCurve;

	[Header("Animations")]
	public Animator _dipsyAnimator;

	public Animator _viewAnimator;

	public string _isChokingBoolName = "IsChoking";

	public string _progressParameterName = "Progress";

	public EventReference _chokeStartReference;

	public EventReference _chokingReference;

	public StudioEventEmitter _musicEmitter;

	[Header("Effects")]
	public RectTransform _tapUI;

	public float _tapSize;

	public float _effectDuration = 1f;

	[Space(10f)]
	public Slider _rateSlider;

	public Color _sliderCorrectColor;

	public Color _sliderWrongColor;

	[Space(10f)]
	public float _minFov = 45f;

	public float _noiseAmplitude = 1f;

	public float _noiseFrequency = 3f;

	private bool isChoking;

	private float chokingProgress;

	private Vector2 tapStartSize;

	private float tapEffectTime;

	private float tapEffectDuration;

	private Image sliderFill;

	private Color currentColor;

	private CinemachineCamera cinemachine;

	private CinemachineBasicMultiChannelPerlin perlinNoise;

	private float maxFov;

	private EventInstance currentInstance;

	private new void Start()
	{
		base.Start();
		sliderFill = _rateSlider.gameObject.GetComponentInChildren<Image>();
		tapStartSize = _tapUI.sizeDelta;
		cinemachine = GetComponentInChildren<CinemachineCamera>();
		perlinNoise = GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
		maxFov = cinemachine.Lens.FieldOfView;
		currentInstance = RuntimeManager.CreateInstance(_chokingReference);
		_musicEmitter.EventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
	}

	private void Reset()
	{
		gameTime = 0f;
		chokingProgress = 0f;
		isChoking = false;
		_dipsyAnimator.SetBool(_isChokingBoolName, value: false);
		_viewAnimator.SetBool(_isChokingBoolName, value: false);
		_musicEmitter.EventInstance.setParameterByName("Track", 0f);
	}

	private void OnDisable()
	{
		currentInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		currentInstance.release();
	}

	private new void Update()
	{
		base.Update();
		UpdateTrigger();
		if (!isChoking)
		{
			gameTime = Time.time + _gameDuration;
			if (isTapping)
			{
				isChoking = true;
				_dipsyAnimator.SetBool(_isChokingBoolName, value: true);
				_viewAnimator.SetBool(_isChokingBoolName, value: true);
				_musicEmitter.EventInstance.setParameterByName("Track", 1f);
				_musicEmitter.EventInstance.start();
				RuntimeManager.PlayOneShot(_chokeStartReference);
				currentInstance.start();
				TapTrigger();
			}
			return;
		}
		if (isTapping)
		{
			TapTrigger();
		}
		if (currentRate > _minThreshold)
		{
			chokingProgress += _progressPerTick * Time.deltaTime;
			currentInstance.setParameterByName("Progress", chokingProgress);
			_dipsyAnimator.SetFloat(_progressParameterName, chokingProgress);
			currentDecay = _decayPerTick * _difficultyCurve.Evaluate(chokingProgress);
			currentMinThreshold = _minThreshold * _difficultyCurve.Evaluate(chokingProgress);
		}
		if (Time.time >= gameTime)
		{
			Player.Instance.FreezePlayer(isFrozen: false);
			if (chokingProgress > 0.5f)
			{
				_onSuccess.Invoke();
				base.gameObject.SetActive(value: false);
			}
			else
			{
				_onFailure.Invoke();
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private void TapTrigger()
	{
		tapEffectTime = 0f;
		tapEffectDuration = _effectDuration;
	}

	private void UpdateTrigger()
	{
		if (tapEffectTime <= tapEffectDuration)
		{
			tapEffectTime += Time.deltaTime;
			float t = Easing.easeOutBack(Mathf.Clamp01(tapEffectTime / tapEffectDuration));
			_tapUI.sizeDelta = Vector2.Lerp(new Vector2(_tapSize, _tapSize), tapStartSize, t);
		}
		currentColor = ((currentRate > _minThreshold) ? _sliderCorrectColor : _sliderWrongColor);
		sliderFill.color = currentColor;
		_rateSlider.value = currentRate;
		float num = Mathf.InverseLerp(0f, 1f, currentRate);
		cinemachine.Lens.FieldOfView = Mathf.Lerp(maxFov, _minFov, num);
		perlinNoise.AmplitudeGain = Mathf.Lerp(0f, _noiseAmplitude, num);
		perlinNoise.FrequencyGain = Mathf.Lerp(0f, _noiseFrequency, num);
		currentInstance.setParameterByName("Volume", num);
	}
}
