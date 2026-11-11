using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WhiteTerminal : IInteractable
{
	public GameObject _lerpPoint;

	public float _lerpDuration;

	[Header("UI")]
	public RectTransform _cursor;

	public RectTransform _hotSpotPointer;

	public GameObject _errorScreen;

	public GameObject _gameScreen;

	public GameObject _loadingScreen;

	public Vector2 _flickerDuration = new Vector2(2f, 0.1f);

	public float _cursorSpeed;

	public float _borderSize;

	[Space(10f)]
	public TMP_Text _speedText;

	public Slider _progressSlider;

	public float _maxDisplaySpeed = 2.99f;

	[Header("Sounds")]
	public EventReference _beepReference;

	public EventReference _errorReference;

	[Header("Parameters")]
	public float _maxDownloadSpeed = 1f;

	public Vector2 _DownloadDistanceLimits;

	public Vector2 _moveHotSpotLimits;

	public UnityEvent _onComplete;

	public StudioEventEmitter _musicEmitter;

	public Transform _terminalLoadingSoundSource;

	public EventReference _terminalLoadingSoundEvent;

	public static Action StopInteraction;

	public static Action FirstTime;

	private bool firstTime;

	private EventInstance beepInstance;

	private Vector2 startPosition;

	private Vector2 hotSpotPosition;

	private float moveHotspotTimer;

	private bool hasCompleted;

	private float downloadSpeed;

	private float progress;

	private Image hotSpotImage;

	private Color hotSpotColor;

	private float flickerTime;

	private bool isEnded;

	private bool isInteracting;

	private float cancelDelay;

	private void Start()
	{
		hotSpotImage = _hotSpotPointer.GetComponent<Image>();
		hotSpotColor = hotSpotImage.color;
		startPosition = _cursor.anchoredPosition;
		MoveHotSpot();
	}

	private void OnEnable()
	{
		StopInteraction = (Action)Delegate.Combine(StopInteraction, new Action(SetUnactive));
		beepInstance = RuntimeManager.CreateInstance(_beepReference);
		beepInstance.set3DAttributes(base.transform.position.To3DAttributes());
		beepInstance.start();
		_loadingScreen.SetActive(value: false);
		_gameScreen.SetActive(value: true);
	}

	private void OnDisable()
	{
		StopInteraction = (Action)Delegate.Remove(StopInteraction, new Action(SetUnactive));
		beepInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		beepInstance.release();
	}

	private void Update()
	{
		if (!isEnded)
		{
			if (isInteracting && Time.time > cancelDelay && (InputManager.Instance.EscTriggered() || InputManager.Instance.LMBTriggered() || InputManager.Instance.TriggeredE()))
			{
				StopInteracting();
			}
			if (downloadSpeed > 0f)
			{
				moveHotspotTimer -= Time.deltaTime;
			}
			if (moveHotspotTimer <= 0f && flickerTime < 0.1f)
			{
				MoveHotSpot();
			}
			float num = Vector2.Distance(_cursor.anchoredPosition, hotSpotPosition);
			float num2 = 1f - (num - _DownloadDistanceLimits.x) / (_DownloadDistanceLimits.y - _DownloadDistanceLimits.x);
			downloadSpeed = Mathf.Lerp(0f, _maxDownloadSpeed, num2);
			beepInstance.setParameterByName("Pitch", num2);
			float num3 = Mathf.Lerp(_flickerDuration.x, _flickerDuration.y, num2);
			flickerTime += Time.deltaTime;
			if (flickerTime > num3)
			{
				flickerTime = 0f;
			}
			_hotSpotPointer.sizeDelta = Vector2.Lerp(Vector2.zero, Vector2.one * 150f, flickerTime / num3);
			hotSpotColor.a = Mathf.Lerp(1f, 0f, flickerTime / num3);
			hotSpotImage.color = hotSpotColor;
			float num4 = Mathf.Lerp(0f, _maxDisplaySpeed, downloadSpeed / _maxDownloadSpeed);
			_speedText.text = "Current speed: " + num4 + " Kbps";
			if (!firstTime && progress >= 0.1f)
			{
				firstTime = true;
				FirstTime?.Invoke();
			}
			progress += downloadSpeed * Time.deltaTime;
			_progressSlider.value = progress;
			if (!hasCompleted && progress >= 1f)
			{
				hasCompleted = true;
				isEnded = true;
				StartCoroutine(EndingPartOne());
			}
		}
	}

	private IEnumerator EndingPartOne()
	{
		StopInteraction();
		yield return new WaitForSeconds(1f);
		beepInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		_gameScreen.SetActive(value: false);
		_loadingScreen.SetActive(value: true);
	}

	public IEnumerator EndingPartTwo()
	{
		_musicEmitter.enabled = false;
		EventInstance pcStartup = RuntimeManager.CreateInstance(_terminalLoadingSoundEvent);
		pcStartup.set3DAttributes(_terminalLoadingSoundSource.To3DAttributes());
		pcStartup.start();
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * 0.1f;
			t = Mathf.Min(1f, t);
			pcStartup.setParameterByName("LoadingProgress", t);
			yield return null;
		}
		pcStartup.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		pcStartup.release();
		_loadingScreen.SetActive(value: false);
		_errorScreen.SetActive(value: true);
		RuntimeManager.PlayOneShot(_errorReference, base.transform.position);
		yield return new WaitForSeconds(5f);
		_onComplete.Invoke();
	}

	private void LateUpdate()
	{
		if (isInteracting)
		{
			_cursor.anchoredPosition += InputManager.Instance.GetMovement().normalized * _cursorSpeed * 200f * Time.deltaTime;
			if (Vector2.Distance(startPosition, _cursor.anchoredPosition) >= _borderSize)
			{
				Vector2 normalized = (_cursor.anchoredPosition - startPosition).normalized;
				_cursor.anchoredPosition = new Vector2(normalized.x * _borderSize + startPosition.x, normalized.y * _borderSize + startPosition.y);
			}
		}
	}

	private void SetUnactive()
	{
		if (isInteracting)
		{
			StopInteracting();
		}
		Active = false;
		moveHotspotTimer = float.PositiveInfinity;
	}

	private void StopInteracting()
	{
		cancelDelay = Time.time;
		isInteracting = false;
		Player.Instance.Brain.DefaultBlend.Time = _lerpDuration;
		Player.Instance.FreezePlayer(isInteracting);
		_lerpPoint.SetActive(isInteracting);
		InputManager.Instance.SetLayout(InputLayoutLayer.Gameplay, PlayerInputLayout.Gameplay);
	}

	private void MoveHotSpot()
	{
		hotSpotPosition = UnityEngine.Random.insideUnitCircle * _borderSize;
		_hotSpotPointer.anchoredPosition = hotSpotPosition;
		moveHotspotTimer = UnityEngine.Random.Range(_moveHotSpotLimits.x, _moveHotSpotLimits.y);
	}

	public override void Use()
	{
		if (!(Time.time <= cancelDelay))
		{
			cancelDelay = Time.time;
			isInteracting = true;
			Player.Instance.Brain.DefaultBlend.Time = _lerpDuration;
			Player.Instance.FreezePlayer(isInteracting);
			_lerpPoint.SetActive(isInteracting);
		}
	}
}
