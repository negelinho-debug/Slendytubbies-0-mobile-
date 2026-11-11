using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public class Remote : MonoBehaviour, ISillyItem
{
	public Axe _axeReference;

	public Animator _animatorReference;

	public float _soundStartDelay;

	public EventReference _hum;

	public EventReference _beep;

	public ArrowTubi _arrowTub;

	public YetiTubi _yetiTub;

	public float _usageRadius;

	public EventInstance humEvent;

	public bool _controlsLocked;

	public GameObject[] _indicators;

	public float _speedMultiplier = 0.5f;

	private float soundStartTimer;

	private float axeActivationTimer;

	private float soundPitch;

	private float soundPitchTime;

	private bool soundStarted;

	private bool axeShown;

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, _usageRadius);
	}

	private void Start()
	{
		humEvent = RuntimeManager.CreateInstance(_hum);
	}

	public void Equip()
	{
		soundPitchTime = 0f;
		soundPitch = 0f;
		soundStarted = false;
		soundStartTimer = 0f;
		base.enabled = true;
	}

	public void Unequip()
	{
		axeShown = false;
		axeActivationTimer = 0f;
		_yetiTub.RelaxBuddy();
		humEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		base.enabled = false;
		SetLightsEnabled(0);
	}

	private void OnDestroy()
	{
		if (humEvent.isValid())
		{
			humEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			humEvent.release();
		}
	}

	private void FixedUpdate()
	{
		Player.Instance.Controller.walkSpeedMultiplier *= _speedMultiplier;
	}

	private void Update()
	{
		humEvent.getPlaybackState(out var state);
		float num = Vector3.Distance(Player.Instance.transform.position, _yetiTub.transform.position);
		if (state == PLAYBACK_STATE.PLAYING)
		{
			if (num < _usageRadius || _controlsLocked)
			{
				soundPitchTime += Time.deltaTime;
				soundPitch += Time.deltaTime / (_yetiTub._stageOneDuration + _yetiTub._stageTwoDuration + _yetiTub._stageThreeDuration);
				humEvent.setParameterByName("Pitch", soundPitch);
				humEvent.setParameterByName("PitchStatus", Mathf.Clamp01(soundPitchTime));
				_yetiTub.shouldSuffer = true;
			}
		}
		else
		{
			soundPitchTime = 0f;
			soundPitch = 0f;
		}
		if (_controlsLocked || !Mouse.current.rightButton.isPressed)
		{
			return;
		}
		soundStartTimer += Time.deltaTime;
		if (soundStartTimer >= _soundStartDelay)
		{
			if (_arrowTub != null)
			{
				_arrowTub.SpawnAndChasePlayer();
			}
			if (!soundStarted)
			{
				RuntimeManager.PlayOneShot(_beep);
				humEvent.start();
				humEvent.setParameterByName("Pitch", 0f);
				humEvent.setParameterByName("PitchStatus", 0f);
				soundStarted = true;
			}
		}
	}

	public void SetLightsEnabled(int count)
	{
		for (int i = 0; i < _indicators.Length; i++)
		{
			_indicators[i].SetActive(i < count);
		}
	}
}
