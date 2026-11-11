using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class DoorGame : IInteractable
{
	public Canvas _miniGameUI;

	public RectTransform _safeZone;

	public RectTransform _cursor;

	public WhiteTerminal _terminal;

	public Element _hint;

	[Header("Cursor")]
	public float _cursorSpeed = 2f;

	public float _dampingTime = 0.1f;

	public float _noiseAmount = 25f;

	public float _noiseSpeed = 3f;

	[Header("Impulse")]
	public Vector2 _ImpulseLimits;

	public float _ImpulseDuration = 0.3f;

	public Vector2 _TimeBetweenImpulse;

	[Header("Door")]
	public float _easeOutDuration = 0.2f;

	public float _closeIncrement = 0.1f;

	public float _openIncrement = 0.2f;

	public float _doorEndAngle = 60f;

	public Vector2 _stayShutLimits;

	public UnityEvent _onFail;

	[Header("Ending")]
	public Transform _terminalTarget;

	public Transform _tinkyTarget;

	public GameObject _failedText;

	public float _doorLookAtDuration = 0.5f;

	public float _doorOpenDuration = 0.6f;

	public float _endingDelay = 0.1f;

	[Header("Sound")]
	public EventReference _doorShut;

	public EventReference _doorOpenStart;

	public EventReference _doorOpenLoop;

	public EventReference _doorOpenEnd;

	public EventReference _doorKnock;

	public EventReference _disappointment;

	public float _volumeShiftDuration = 0.3f;

	private EventInstance currentInstance;

	private bool isInteracting;

	private float cancelDelay;

	private bool isClosed;

	private bool openOneLastTime;

	private bool tutorialEnded;

	private bool isEnded;

	private Vector2 impulseDirection;

	private float impulseTime;

	private float nextImpulse;

	private float currentForce;

	private float progress;

	private float easeTime;

	private bool inSafeZone;

	private float safeZoneBlockTime;

	private float noiseTime;

	private Vector2 smoothMouse;

	private Vector2 smoothVelocity;

	private float stayShutTime;

	private float targetVolume;

	private float currentVolume;

	private float startVolume;

	private float volumeTimer;

	private IInteractable _interactableImplementation;

	private void Start()
	{
		currentInstance = RuntimeManager.CreateInstance(_doorOpenLoop);
		currentInstance.start();
		FreezeGame();
		_miniGameUI.gameObject.SetActive(value: false);
		StartCoroutine(StartPlayerGuide());
	}

	private void FreezeGame()
	{
		progress = 1f;
		isClosed = true;
		volumeTimer = _volumeShiftDuration;
		stayShutTime = float.PositiveInfinity;
	}

	private void OnEnable()
	{
		WhiteTerminal.FirstTime = (Action)Delegate.Combine(WhiteTerminal.FirstTime, new Action(StartGame));
		WhiteTerminal.StopInteraction = (Action)Delegate.Combine(WhiteTerminal.StopInteraction, new Action(AllowOneLastOpening));
	}

	private void OnDisable()
	{
		WhiteTerminal.StopInteraction = (Action)Delegate.Remove(WhiteTerminal.StopInteraction, new Action(AllowOneLastOpening));
		WhiteTerminal.FirstTime = (Action)Delegate.Remove(WhiteTerminal.FirstTime, new Action(StartGame));
		currentInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		currentInstance.release();
	}

	private void Update()
	{
		if (isEnded)
		{
			return;
		}
		if (!isClosed)
		{
			base.transform.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0f, _doorEndAngle, 0f), 1f - progress);
		}
		easeTime += Time.deltaTime;
		currentForce = (inSafeZone ? _closeIncrement : (0f - _openIncrement)) * Time.deltaTime;
		inSafeZone = isInteracting && Vector2.Distance(_cursor.anchoredPosition, _safeZone.anchoredPosition) <= _safeZone.rect.width / 2f;
		if (isInteracting)
		{
			if (inSafeZone && Time.time > safeZoneBlockTime && !isClosed && progress >= 1f)
			{
				Active = false;
				isClosed = true;
				startVolume = currentVolume;
				targetVolume = 0f;
				volumeTimer = 0f;
				RuntimeManager.PlayOneShot(_doorShut);
				stayShutTime = UnityEngine.Random.Range(_stayShutLimits.x, _stayShutLimits.y);
				StopInteracting();
				if (!tutorialEnded)
				{
					tutorialEnded = true;
					StartCoroutine(EnableTerminal());
					FreezeGame();
				}
				if (openOneLastTime)
				{
					FreezeGame();
					StartCoroutine(_terminal.EndingPartTwo());
				}
			}
			if (!isClosed && Time.time > nextImpulse)
			{
				StartImpulse();
			}
			if (Time.time > cancelDelay && (InputManager.Instance.EscTriggered() || InputManager.Instance.LMBTriggered() || InputManager.Instance.TriggeredE()))
			{
				StopInteracting();
			}
		}
		if (progress > 0f)
		{
			progress += currentForce;
		}
		if (stayShutTime > 0f)
		{
			progress = 1f;
			stayShutTime -= Time.deltaTime;
		}
		else if (stayShutTime <= 0f && isClosed)
		{
			RuntimeManager.PlayOneShot(_doorOpenStart);
			Active = true;
			isClosed = false;
			targetVolume = 1f;
			startVolume = currentVolume;
			volumeTimer = 0f;
			StartImpulse();
		}
		if (volumeTimer <= _volumeShiftDuration)
		{
			volumeTimer += Time.deltaTime;
			currentVolume = Mathf.Lerp(startVolume, targetVolume, volumeTimer / _volumeShiftDuration);
			currentInstance.setParameterByName("Volume", currentVolume);
		}
		if (progress <= 0f)
		{
			ForceOpen(0f);
		}
	}

	private void StartGame()
	{
		stayShutTime = 0f;
	}

	private void AllowOneLastOpening()
	{
		openOneLastTime = true;
	}

	private void StopInteracting()
	{
		isInteracting = false;
		Player.Instance.FreezePlayer(isInteracting);
		_miniGameUI.gameObject.SetActive(isInteracting);
		InputManager.Instance.SetLayout(InputLayoutLayer.Gameplay, PlayerInputLayout.Gameplay);
	}

	public void ForceOpen(float _delay)
	{
		if (_delay <= 0f)
		{
			WhiteTerminal.StopInteraction?.Invoke();
		}
		StartCoroutine(Ending(_delay));
	}

	private void LateUpdate()
	{
		if (isInteracting)
		{
			smoothMouse = Vector2.SmoothDamp(smoothMouse, InputManager.Instance.GetLookDelta() * _cursorSpeed, ref smoothVelocity, _dampingTime);
			_cursor.anchoredPosition += smoothMouse;
			noiseTime += Time.deltaTime * _noiseSpeed;
			Vector2 vector = new Vector2(Mathf.Sin(noiseTime * 1.3f), Mathf.Sin(noiseTime * 1.7f + 10f));
			_cursor.anchoredPosition += vector * _noiseAmount;
			if (impulseTime < _ImpulseDuration)
			{
				impulseTime += Time.deltaTime;
				_cursor.anchoredPosition = Vector2.Lerp(_cursor.anchoredPosition, impulseDirection, impulseTime / _ImpulseDuration);
			}
			float num = (float)Screen.width / 2f - _cursor.rect.width / 2f;
			float num2 = (float)Screen.height / 2f - _cursor.rect.height / 2f;
			_cursor.anchoredPosition = new Vector2(Mathf.Clamp(_cursor.anchoredPosition.x, 0f - num, num), Mathf.Clamp(_cursor.anchoredPosition.y, 0f - num2, num2));
		}
	}

	private IEnumerator StartPlayerGuide()
	{
		yield return new WaitForSeconds(5f);
		RuntimeManager.PlayOneShot(_doorKnock, _tinkyTarget.position);
		yield return new WaitForSeconds(0.25f);
		RuntimeManager.PlayOneShot(_doorKnock, _tinkyTarget.position);
		yield return new WaitForSeconds(0.25f);
		RuntimeManager.PlayOneShot(_doorKnock, _tinkyTarget.position);
		yield return new WaitForSeconds(1f);
		Quaternion normalized = Quaternion.LookRotation(_tinkyTarget.position - Camera.main.transform.position).normalized;
		Player.Instance.Look.LockView(normalized, 1f, 5f);
		yield return new WaitForSeconds(1f);
		StartGame();
	}

	private IEnumerator EnableTerminal()
	{
		yield return new WaitForSeconds(3f);
		_terminal.enabled = true;
	}

	private IEnumerator Ending(float delay)
	{
		isEnded = true;
		Quaternion normalized = Quaternion.LookRotation(_tinkyTarget.position - Camera.main.transform.position).normalized;
		Player.Instance.Look.LockView(normalized, 5f, 5f);
		yield return new WaitForSeconds(_doorLookAtDuration);
		Vector3 startEuler = base.transform.localEulerAngles;
		float time = 0f;
		RuntimeManager.PlayOneShot(_doorOpenEnd, _tinkyTarget.position);
		while (time < _doorOpenDuration)
		{
			time += Time.deltaTime;
			float t = Easing.easeOutBack(Mathf.Clamp01(time / _doorOpenDuration));
			base.transform.localEulerAngles = Vector3.Slerp(startEuler, new Vector3(0f, 125f, 0f), t);
			yield return null;
		}
		base.transform.localEulerAngles = new Vector3(0f, 125f, 0f);
		if (delay > 0f)
		{
			RuntimeManager.PlayOneShot(_disappointment);
			_failedText.SetActive(value: true);
			yield return new WaitForSeconds(_endingDelay);
			_onFail.Invoke();
		}
		else
		{
			WhiteTurnOnTheLights.isRestart = true;
			LevelSwitch.instance.SwitchLevel(SceneManager.GetActiveScene().name, LevelSwitch.SwitchType.NoiseNoIntro);
		}
		_failedText.SetActive(value: false);
		base.gameObject.SetActive(value: false);
	}

	public override void Use()
	{
		cancelDelay = Time.time;
		isInteracting = true;
		Player.Instance.FreezePlayer(isInteracting);
		_miniGameUI.gameObject.SetActive(isInteracting);
		_cursor.anchoredPosition = Vector2.zero;
		StartImpulse();
	}

	private void StartImpulse()
	{
		impulseDirection = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(_ImpulseLimits.x, _ImpulseLimits.y);
		nextImpulse = Time.time + UnityEngine.Random.Range(_TimeBetweenImpulse.x, _TimeBetweenImpulse.y);
		impulseTime = 0f;
		safeZoneBlockTime = Time.time + _ImpulseDuration;
	}
}
