using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class ClawyGame : MonoBehaviour
{
	[Header("Player")]
	public Animator _viewAnimator;

	public Animator _gunAnimator;

	public Transform[] _handsRoot;

	public Transform _handsTarget;

	public float _targetDistance = 2f;

	public float _targetSensitivity = 0.05f;

	public Vector3[] _angleOffsets;

	public GameObject _muzzleFlash;

	public GameObject[] _lights;

	public float _lightsOutTime = 0.1f;

	public GameObject[] _rounds;

	[Space(10f)]
	public float _edgeThreshold = 100f;

	public float _maxRotationSpeed = 100f;

	public float _smoothTime = 0.1f;

	[Space(10f)]
	public float _reloadResetDelay = 0.2f;

	public float _addBulletCooldown = 0.1f;

	public int _maxBullets = 5;

	public Vector2 _handPatternSize = new Vector2(0.001f, 0.002f);

	public float _handPatternSpeed = 2f;

	public LayerMask _layerMask;

	[Header("Clawy")]
	public Animator _clawyAnimator;

	public Animator _clawyDead;

	public Collider _clawyCollider;

	public Transform[] _clawyPoints;

	public int _clawyMaxHealth;

	public float _clawyCooldown;

	public float _clawyRunDelay;

	public float _clawyRunDuration;

	public float _clawyAttackDistance;

	public float _clawyAttackTime = 0.5f;

	[Header("Sounds")]
	public EventReference _revolverEmpty;

	public EventReference _revolverShoot;

	public EventReference _revolverHit;

	public EventReference _revolverReload;

	public EventReference _ClawyScream;

	public EventReference _ClawyRun;

	public EventReference _ClawyHit;

	[Header("Start")]
	public Element _hintElement;

	public float _hintStayDuration;

	public float _timeBetweenHints;

	[Header("Ending")]
	public float _endingRotate = 0.3f;

	public float _endingWait = 1f;

	public GameObject _endingPopUp;

	public GameObject _musicEmitter;

	private EventInstance clawyStepsInstance;

	private EventInstance clawyHitInstance;

	private Vector2 viewportPos = new Vector2(0.5f, 0.5f);

	private float currentSpeed;

	private float targetRotation;

	private float currentRotationVelocity;

	private AnimatorStateInfo playerStateInfo;

	private float reloadResetTime;

	private bool addBullet;

	private float addBulletTime;

	private int bulletsAmount;

	private Transform clawy;

	private AnimatorStateInfo clawyStateInfo;

	private bool isSpawned;

	private int clawyHealth;

	private float clawyCooldownTime;

	private int currentPointIndex;

	private bool isOut;

	private float lightsOutTimer;

	private bool hasStarted;

	private bool hasEnded;

	private float nextR;

	private void Start()
	{
		Player.Instance.FreezePlayer(isFrozen: true);
		InputManager.Instance.SetLayout(InputLayoutLayer.Gameplay, PlayerInputLayout.Gameplay);
		clawyHealth = _clawyMaxHealth;
		clawyCooldownTime = float.PositiveInfinity;
		clawyStepsInstance = RuntimeManager.CreateInstance(_ClawyRun);
		clawyHitInstance = RuntimeManager.CreateInstance(_ClawyHit);
		clawy = _clawyAnimator.transform.parent;
		_muzzleFlash.SetActive(value: false);
		clawy.position = new Vector3(0f, 25f, 0f);
		_hintElement.timeAdd = float.PositiveInfinity;
		MenuText.Instance.StartText(_hintElement);
		for (int i = 0; i < _rounds.Length; i++)
		{
			_rounds[i].SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		VideoManager.OnFinished = (Action)Delegate.Combine(VideoManager.OnFinished, new Action(EndTerminal));
	}

	private void OnDisable()
	{
		VideoManager.OnFinished = (Action)Delegate.Remove(VideoManager.OnFinished, new Action(EndTerminal));
		clawyStepsInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		clawyStepsInstance.release();
		if (clawyHitInstance.isValid())
		{
			clawyHitInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			clawyHitInstance.release();
		}
	}

	private void Update()
	{
		if (!hasStarted)
		{
			Color white = Color.white;
			white.a = Mathf.Sin(Time.time * _hintStayDuration);
			MenuText.Instance._TMPText.color = white;
			if (bulletsAmount >= _maxBullets)
			{
				hasStarted = true;
				clawyCooldownTime = _clawyCooldown;
				MenuText.Instance.StopText();
			}
		}
		LightsOut();
		if (!hasEnded)
		{
			UpdatePlayer();
			if (clawyCooldownTime > 0f)
			{
				clawyCooldownTime -= Time.deltaTime;
			}
			else if (hasStarted && clawyCooldownTime <= 0f && !isSpawned)
			{
				isSpawned = true;
				StartCoroutine(ClawyRun());
			}
		}
	}

	private void ResetClawy()
	{
		if (!ScreenFade.Instance.isFading)
		{
			StopAllCoroutines();
			RuntimeManager.PlayOneShot(_revolverHit, clawy.position);
			clawyStepsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			clawyHitInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			clawy.position = new Vector3(0f, 25f, 0f);
			clawyCooldownTime = _clawyCooldown;
			isSpawned = false;
			clawyHealth--;
			if (clawyHealth <= 0)
			{
				hasEnded = true;
				VideoManager.Instance.PlayVideo(Videos.ClawyEnding);
				_musicEmitter.SetActive(value: false);
			}
		}
	}

	private void EndTerminal()
	{
		Terminal.initialState = InitialState.Clawy;
		_endingPopUp.SetActive(value: true);
	}

	private IEnumerator GameEnd()
	{
		hasEnded = true;
		_clawyAnimator.gameObject.SetActive(value: false);
		_clawyDead.gameObject.SetActive(value: true);
		_clawyDead.SetTrigger("Die");
		clawy.position = Vector3.Lerp(_clawyPoints[currentPointIndex].position, Player.Instance.transform.position, 0.8f);
		clawy.rotation = Quaternion.LookRotation(Player.Instance.transform.position - clawy.position);
		float time = 0f;
		Quaternion startRotation = Player.Instance.transform.rotation;
		Quaternion startCamera = Player.Instance.Look._cameraHolder.localRotation;
		Vector3 startTarget = _handsTarget.position;
		while (time < _endingRotate)
		{
			time += Time.deltaTime;
			Quaternion b = Quaternion.LookRotation(clawy.position - Player.Instance.transform.position);
			Quaternion rotation = Quaternion.Slerp(startRotation, b, time / _endingRotate);
			Player.Instance.transform.rotation = rotation;
			_viewAnimator.transform.rotation = rotation;
			Quaternion localRotation = Quaternion.Slerp(startCamera, Quaternion.Euler(Quaternion.LookRotation(clawy.position - Player.Instance.Camera.transform.position).eulerAngles.x, 0f, 0f), time / _endingRotate);
			Player.Instance.Look._cameraHolder.localRotation = localRotation;
			Vector3 b2 = Player.Instance.Camera.transform.position + Player.Instance.Camera.transform.forward * _targetDistance;
			_handsTarget.position = Vector3.Lerp(startTarget, b2, time / _endingRotate);
			yield return null;
		}
		yield return new WaitForSeconds(_endingWait);
		Terminal.initialState = InitialState.Clawy;
		_endingPopUp.SetActive(value: true);
	}

	public void CloseGame()
	{
		Debug.Log("QUIT");
		Application.Quit();
	}

	private IEnumerator ClawyRun()
	{
		currentPointIndex = UnityEngine.Random.Range(0, _clawyPoints.Length);
		clawy.position = _clawyPoints[currentPointIndex].position;
		Vector3 startPos = clawy.position;
		float time = 0f;
		float distance = 99f;
		RuntimeManager.PlayOneShot(_ClawyScream, clawy.position);
		yield return new WaitForSeconds(_clawyRunDelay);
		clawyStepsInstance.set3DAttributes(clawy.position.To3DAttributes());
		clawyStepsInstance.start();
		while (distance > _clawyAttackDistance)
		{
			time += Time.deltaTime;
			distance = Vector3.Distance(clawy.position, Player.Instance.transform.position);
			clawy.rotation = Quaternion.LookRotation(Player.Instance.transform.position - clawy.position);
			clawy.position = Vector3.Lerp(startPos, Player.Instance.transform.position, time / _clawyRunDuration);
			yield return null;
		}
		clawyStepsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		_clawyAnimator.SetTrigger("Attack");
		yield return new WaitForSeconds(_clawyAttackTime);
		clawyHitInstance.start();
		ScreenFade.Instance.SetFade(1f);
		yield return new WaitForSeconds(0.6f);
		ScreenFade.Instance.StartFade(1f, 0f, 0.5f);
		clawy.position = new Vector3(0f, 25f, 0f);
		clawyCooldownTime = _clawyCooldown;
		clawyHealth = _clawyMaxHealth;
		isSpawned = false;
	}

	private void UpdatePlayer()
	{
		float b = 0f;
		if (viewportPos.x < _edgeThreshold)
		{
			float num = 1f - viewportPos.x / _edgeThreshold;
			b = (0f - _maxRotationSpeed) * num;
		}
		else if (viewportPos.x > 1f - _edgeThreshold)
		{
			float num2 = 1f - (1f - viewportPos.x) / _edgeThreshold;
			b = _maxRotationSpeed * num2;
		}
		currentSpeed = Mathf.Lerp(currentSpeed, b, Time.deltaTime / _smoothTime);
		Player.Instance.transform.Rotate(Vector3.up, currentSpeed * Time.deltaTime);
		_viewAnimator.transform.Rotate(Vector3.up, currentSpeed * Time.deltaTime);
		if (InputManager.Instance.LMBTriggered() && bulletsAmount > 0)
		{
			playerStateInfo = _viewAnimator.GetCurrentAnimatorStateInfo(0);
			if (playerStateInfo.IsName("Idle") && !_viewAnimator.IsInTransition(0))
			{
				bulletsAmount--;
				RuntimeManager.PlayOneShot(_revolverShoot);
				_viewAnimator.SetTrigger("Shoot");
				_gunAnimator.SetTrigger("Shoot");
				lightsOutTimer = _lightsOutTime;
				for (int i = 0; i < _maxBullets; i++)
				{
					if (i >= bulletsAmount)
					{
						_rounds[i].SetActive(value: false);
					}
				}
				Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(viewportPos.x, viewportPos.y, 0f)), out var hitInfo, 100f, _layerMask);
				if (hitInfo.collider == _clawyCollider)
				{
					ResetClawy();
				}
			}
		}
		else if (InputManager.Instance.LMBTriggered() && bulletsAmount <= 0)
		{
			RuntimeManager.PlayOneShot(_revolverEmpty);
		}
		if (InputManager.Instance.TriggeredR() && bulletsAmount < _maxBullets)
		{
			reloadResetTime = _reloadResetDelay;
			playerStateInfo = _viewAnimator.GetCurrentAnimatorStateInfo(0);
			if (playerStateInfo.IsName("Idle"))
			{
				_viewAnimator.SetBool("IsReloading", value: true);
				_gunAnimator.SetBool("IsReloading", value: true);
			}
		}
		if (reloadResetTime > 0f)
		{
			reloadResetTime -= Time.deltaTime;
			playerStateInfo = _viewAnimator.GetCurrentAnimatorStateInfo(0);
			if (playerStateInfo.IsName("ReloadLoop") && !_viewAnimator.IsInTransition(0) && !addBullet)
			{
				addBulletTime = _addBulletCooldown;
				addBullet = true;
			}
		}
		else
		{
			playerStateInfo = _viewAnimator.GetCurrentAnimatorStateInfo(0);
			if (!playerStateInfo.IsName("Idle"))
			{
				_viewAnimator.SetBool("IsReloading", value: false);
				_gunAnimator.SetBool("IsReloading", value: false);
			}
		}
		if (!addBullet)
		{
			return;
		}
		addBulletTime -= Time.deltaTime;
		if (!(addBulletTime < 0f))
		{
			return;
		}
		addBullet = false;
		if (bulletsAmount < _maxBullets)
		{
			bulletsAmount++;
			if (bulletsAmount > _maxBullets)
			{
				bulletsAmount = _maxBullets;
			}
			for (int j = 0; j < bulletsAmount; j++)
			{
				_rounds[j].SetActive(value: true);
			}
			RuntimeManager.PlayOneShot(_revolverReload);
		}
	}

	private void LightsOut()
	{
		if (lightsOutTimer > 0f)
		{
			lightsOutTimer -= Time.deltaTime;
			if (!isOut)
			{
				isOut = true;
				_viewAnimator.speed = 0f;
				_gunAnimator.speed = 0f;
				_muzzleFlash.SetActive(value: true);
				GameObject[] lights = _lights;
				for (int i = 0; i < lights.Length; i++)
				{
					lights[i].SetActive(value: false);
				}
			}
		}
		else if (lightsOutTimer <= 0f && isOut)
		{
			isOut = false;
			_viewAnimator.speed = 1f;
			_gunAnimator.speed = 1f;
			_muzzleFlash.SetActive(value: false);
			GameObject[] lights = _lights;
			for (int i = 0; i < lights.Length; i++)
			{
				lights[i].SetActive(value: true);
			}
		}
	}

	private void LateUpdate()
	{
		Vector2 vector = InputManager.Instance.GetLookDelta() * _targetSensitivity;
		viewportPos.x = Mathf.Clamp(viewportPos.x + vector.x + Mathf.Cos(Time.time * _handPatternSpeed) * _handPatternSize.x, 0f, 1f);
		viewportPos.y = Mathf.Clamp(viewportPos.y + vector.y + Mathf.Sin(Time.time * _handPatternSpeed * 2f) * _handPatternSize.y, 0f, 1f);
		Vector3 position = Camera.main.ViewportToWorldPoint(new Vector3(viewportPos.x, viewportPos.y, _targetDistance));
		if (!hasEnded && !isOut)
		{
			_handsTarget.position = position;
		}
	}
}
