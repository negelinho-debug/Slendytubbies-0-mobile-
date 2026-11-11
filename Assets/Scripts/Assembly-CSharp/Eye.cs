using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using PrimeTween;
using UnityEngine;

public class Eye : MonoBehaviour, IDamagable
{
	public enum EyeStates
	{
		Static,
		Orbit,
		Evade
	}

	public float _timeOffset;

	public float _colorTransitionTime = 0.5f;

	public Color _startingColor = new Color(1f, 1f, 1f);

	public Color _evadeColor = new Color(1f, 71f / 95f, 0.6999999f);

	public Color _damageColor = new Color(1f, 0f, 0f);

	public MeshRenderer _meshRenderer;

	private MaterialPropertyBlock _propertyBlock;

	private Color _currentColor;

	private Color _targetColor;

	private float _currentEyeMaterialState;

	private float _targetEyeMaterialState;

	private float _colorChangeProgress;

	[Space(10f)]
	public float _turnAngle = 30f;

	public float _orbitTurnSpeed = 1f;

	public float _orbitSpotAngle = 40f;

	[Space(10f)]
	public Vector2 _orbitAmplitude = new Vector2(0.05f, 0.2f);

	public float _orbitFrequency = 5f;

	[Space(10f)]
	public float _evadeTurnSpeed = 2f;

	public float _evadeSpotAngle = 70f;

	public Vector2 _evadeAmplitude = new Vector2(0.05f, 0.2f);

	public float _evadeFrequency = 5f;

	[Space(10f)]
	public EventReference _idleRef;

	public EventReference _deathRef;

	private Light spotLight;

	private EyeStates state;

	private EventInstance idleInst;

	private bool isInvincible;

	private Vector3 floatingOffset;

	private Vector3 currentPosition;

	private Vector3 desiredPosition;

	private Vector3 currentForward;

	private Vector2 floatAmplitude;

	private float floatFrequency;

	private float turnSpeed;

	public static Action LookedAt;

	public static Action OnDeath;

	public static Action<GameObject> OnDamage;

	private void Awake()
	{
		spotLight = GetComponentInChildren<Light>();
		_targetColor = _startingColor;
		_targetEyeMaterialState = 0f;
		_currentColor = _targetColor;
		_currentEyeMaterialState = 0f;
		_colorChangeProgress = 1f;
		_propertyBlock = new MaterialPropertyBlock();
		if (spotLight != null)
		{
			spotLight.spotAngle = _orbitSpotAngle;
		}
		currentForward = base.transform.forward;
		turnSpeed = _orbitTurnSpeed;
		floatAmplitude = _orbitAmplitude;
		floatFrequency = _orbitFrequency;
		idleInst = RuntimeManager.CreateInstance(_idleRef);
		RuntimeManager.AttachInstanceToGameObject(idleInst, base.gameObject);
		idleInst.start();
	}

	private void Start()
	{
		state = EyeStates.Static;
		LookedAt?.Invoke();
		currentPosition = desiredPosition;
	}

	private void OnEnable()
	{
		OnDamage = (Action<GameObject>)Delegate.Combine(OnDamage, new Action<GameObject>(LookAtDeath));
		OnDeath = (Action)Delegate.Combine(OnDeath, new Action(ToEvade));
	}

	private void OnDisable()
	{
		OnDamage = (Action<GameObject>)Delegate.Remove(OnDamage, new Action<GameObject>(LookAtDeath));
		OnDeath = (Action)Delegate.Remove(OnDeath, new Action(ToEvade));
		idleInst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		idleInst.release();
	}

	private void Update()
	{
		Vector3.Dot((Player.Instance.transform.position - currentPosition).normalized, currentForward);
		Quaternion b = Quaternion.LookRotation(Camera.main.transform.position - base.transform.position);
		Quaternion.LookRotation(currentForward);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * turnSpeed);
		float num = Time.time + _timeOffset;
		float x = Mathf.Sin(num * (floatFrequency / 2f)) * floatAmplitude.x;
		float y = Mathf.Sin(num * floatFrequency) * floatAmplitude.y;
		floatingOffset = new Vector3(x, y, 0f);
		base.transform.position = currentPosition + floatingOffset;
		if (_colorChangeProgress < 1f)
		{
			_colorChangeProgress = Mathf.Min(_colorChangeProgress + Time.deltaTime / _colorTransitionTime, 1f);
			spotLight.color = Color.Lerp(_currentColor, _targetColor, _colorChangeProgress);
			_propertyBlock.SetFloat("_Progress", Mathf.Lerp(_currentEyeMaterialState, _targetEyeMaterialState, _colorChangeProgress));
		}
		else
		{
			_meshRenderer.SetPropertyBlock(_propertyBlock);
			_currentEyeMaterialState = _targetEyeMaterialState;
			_currentColor = _targetColor;
		}
	}

	public void SetDesiredPosition(Vector3 pos)
	{
		desiredPosition = pos;
	}

	public void Damage()
	{
		if (!isInvincible)
		{
			DogSpawner.Instance.SetActive(active: false);
			DogSpawner.Instance.RemoveAll();
			OnDamage?.Invoke(base.gameObject);
			StartCoroutine(DeathAnim());
		}
	}

	private IEnumerator DeathAnim()
	{
		idleInst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		RuntimeManager.PlayOneShot(_deathRef, base.transform.position);
		float time = 0f;
		float duration = 1.2f;
		Quaternion startRot = base.transform.rotation;
		Quaternion endRot = startRot * Quaternion.AngleAxis(25f, base.transform.right);
		while (time < duration)
		{
			time += Time.deltaTime;
			Quaternion quaternion = Quaternion.Slerp(startRot, endRot, time / 0.25f);
			float num = Easing.easeInSine(time + 0.2f / duration) * 20f;
			float angle = Mathf.Sin(time * 50f) * num;
			Quaternion quaternion2 = startRot * Quaternion.AngleAxis(angle, base.transform.up);
			base.transform.rotation = Quaternion.Euler(quaternion.eulerAngles.x, quaternion2.eulerAngles.y, 0f);
			yield return null;
		}
		EyeFormation.Instance._eyes.Remove(this);
		base.gameObject.SetActive(value: false);
		OnDeath?.Invoke();
	}

	private void LookAtDeath(GameObject target)
	{
		_currentColor = spotLight.color;
		_targetColor = _damageColor;
		_currentEyeMaterialState = _propertyBlock.GetFloat("_Progress");
		_targetEyeMaterialState = 1f;
		_colorChangeProgress = 0f;
		if (!(target == base.gameObject))
		{
			isInvincible = true;
			Vector3 normalized = (target.transform.position - base.transform.position).normalized;
			Tween.Rotation(base.transform, Quaternion.LookRotation(normalized), 1f, default(PrimeTween.Easing), 1, CycleMode.Restart, 0f, 3.5f);
		}
	}

	private void ToEvade()
	{
		floatAmplitude = _evadeAmplitude;
		floatFrequency = _evadeFrequency;
		turnSpeed = _evadeTurnSpeed;
		_currentColor = spotLight.color;
		_targetColor = _evadeColor;
		_currentEyeMaterialState = _propertyBlock.GetFloat("_Progress");
		_targetEyeMaterialState = 0.5f;
		_colorChangeProgress = 0f;
		StartCoroutine(StartEvade());
	}

	private IEnumerator StartEvade()
	{
		float time = 0f;
		float duration = 1f;
		Vector3 startPosition = currentPosition;
		while (time < duration)
		{
			currentPosition = Vector3.Lerp(startPosition, desiredPosition, time / duration);
			spotLight.spotAngle = Mathf.Lerp(_orbitSpotAngle, _evadeSpotAngle, time / duration);
			time += Time.deltaTime;
			yield return null;
		}
		currentPosition = desiredPosition;
		isInvincible = false;
		currentForward = (Camera.main.transform.position - base.transform.position).normalized;
		DogSpawner.Instance.SetActive(active: true);
		state = EyeStates.Evade;
	}
}
