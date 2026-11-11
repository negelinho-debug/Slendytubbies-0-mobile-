using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SubwaySurf : MonoBehaviour
{
	public Transform[] _lanes;

	public float _laneMoveSpeed;

	public float _laneSwitchDuration;

	public float _distanceToTravel;

	public LayerMask _layerMask;

	[Space(10f)]
	public float _distanceBetweenPlayerSteps = 2.2f;

	public float _distanceBetweenWhiteSteps = 2f;

	public EventReference _footstepsReference;

	[Header("Watch Dogs")]
	public GameObject _prefab;

	public Vector2 _spawnLimits;

	public Vector2 _distanceLimits;

	[Header("White")]
	public GameObject _whiteModel;

	public float _distanceToWhite = 8f;

	[Header("View Bob")]
	public float _bobFrequency;

	public Vector2 _bobAmplitude;

	[Header("Ending")]
	public UnityEvent _endingEvent;

	public Transform _teleportPoint;

	private EventInstance playerFootsteps;

	private EventInstance whiteFootsteps;

	private float nextStepPlayer;

	private float nextStepWhite;

	private bool inTransition;

	private bool hasEnded;

	private VideoPlayer video;

	private float loopDuration;

	private float nextSpawn;

	private int currentLane;

	private float lanePosition;

	private float laneSwitchTimer;

	private float distanceTravelled;

	private Animator whiteAnimator;

	private int whiteLane;

	private float whiteLanePosition;

	private float whiteLaneSwitchTimer;

	private float whiteNextSwitchDelay;

	private Vector3 currentPosition;

	private Collider[] colliders = new Collider[10];

	private void Start()
	{
		video = GetComponent<VideoPlayer>();
		loopDuration = (float)video.length;
		InputManager.Instance.SetLayout(InputLayoutLayer.Gameplay, PlayerInputLayout.Gameplay);
		Player.Instance.FreezePlayer(isFrozen: true);
		whiteAnimator = _whiteModel.GetComponent<Animator>();
		currentLane = 1;
		laneSwitchTimer = _laneSwitchDuration;
		float num = UnityEngine.Random.Range(_spawnLimits.x, _spawnLimits.y);
		float num2 = Mathf.Ceil((Time.time + num) / loopDuration);
		nextSpawn = num2 * loopDuration;
		nextStepPlayer = _distanceBetweenPlayerSteps;
		nextStepWhite = _distanceBetweenWhiteSteps;
	}

	private void OnEnable()
	{
		VideoManager.OnFinished = (Action)Delegate.Combine(VideoManager.OnFinished, new Action(RestartGame));
		playerFootsteps = RuntimeManager.CreateInstance(_footstepsReference);
		playerFootsteps.setParameterByName("Surface", 4f);
		playerFootsteps.setParameterByName("Is2D", 1f);
		whiteFootsteps = RuntimeManager.CreateInstance(_footstepsReference);
		whiteFootsteps.setParameterByName("Surface", 4f);
	}

	private void OnDisable()
	{
		VideoManager.OnFinished = (Action)Delegate.Remove(VideoManager.OnFinished, new Action(RestartGame));
		playerFootsteps.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		playerFootsteps.release();
		whiteFootsteps.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		whiteFootsteps.release();
	}

	private void RestartGame()
	{
		LevelSwitch.instance.SwitchLevel(SceneManager.GetActiveScene().name, LevelSwitch.SwitchType.NoiseNoIntro);
	}

	private void Update()
	{
		if (!hasEnded)
		{
			distanceTravelled += _laneMoveSpeed * Time.deltaTime;
			if (distanceTravelled > _distanceToTravel && !inTransition)
			{
				inTransition = true;
				StartCoroutine(Ending());
			}
			if (Time.time > nextSpawn)
			{
				SpawnWD();
			}
			MovePlayer();
			CheckCollision();
			MoveWhite();
			float time = Time.time;
			float y = Mathf.Sin(time * (_bobFrequency / 2f)) * _bobAmplitude.x;
			float x = Mathf.Sin(time * _bobFrequency) * _bobAmplitude.y;
			Player.Instance.Look._cameraHolder.rotation = Quaternion.Euler(x, y, 0f);
		}
	}

	private void SpawnWD()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_prefab);
		int num = (((float)UnityEngine.Random.Range(0, 1) < 0.5f) ? currentLane : UnityEngine.Random.Range(0, _lanes.Length));
		float num2 = UnityEngine.Random.Range(_distanceLimits.x, _distanceLimits.y);
		gameObject.transform.position = new Vector3(_lanes[num].position.x, 0f, distanceTravelled + num2);
		gameObject.transform.rotation = Quaternion.LookRotation(_lanes[num].position - gameObject.transform.position);
		UnityEngine.Object.Destroy(gameObject, _spawnLimits.y);
		float num3 = UnityEngine.Random.Range(_spawnLimits.x, _spawnLimits.y);
		float num4 = Mathf.Ceil((Time.time + num3) / loopDuration);
		nextSpawn = num4 * loopDuration;
	}

	private void MovePlayer()
	{
		if (Keyboard.current.aKey.wasPressedThisFrame)
		{
			if (currentLane > 0)
			{
				currentLane--;
			}
			laneSwitchTimer = 0f;
		}
		if (Keyboard.current.dKey.wasPressedThisFrame)
		{
			if (currentLane < _lanes.Length - 1)
			{
				currentLane++;
			}
			laneSwitchTimer = 0f;
		}
		laneSwitchTimer += Time.deltaTime;
		float x = _lanes[currentLane].position.x;
		lanePosition = Mathf.Lerp(lanePosition, x, laneSwitchTimer / _laneSwitchDuration);
		currentPosition = new Vector3(lanePosition, 0f, distanceTravelled);
		Player.Instance.Motor.SetPosition(currentPosition);
		if (distanceTravelled > nextStepPlayer)
		{
			playerFootsteps.start();
			nextStepPlayer = distanceTravelled + _distanceBetweenPlayerSteps;
		}
	}

	private void MoveWhite()
	{
		float num = distanceTravelled + _distanceToWhite;
		Vector3 origin = _whiteModel.transform.position + Vector3.up;
		if (Time.time > whiteNextSwitchDelay && Physics.Raycast(origin, _whiteModel.transform.forward, out var hitInfo, 3f))
		{
			if (hitInfo.collider.CompareTag("Enemy"))
			{
				if (whiteLane < _lanes.Length - 1)
				{
					whiteLane++;
					whiteAnimator.Play("StrafeR", 1);
				}
				else
				{
					whiteLane--;
					whiteAnimator.Play("StrafeL", 1);
				}
			}
			whiteLaneSwitchTimer = 0f;
			whiteNextSwitchDelay = Time.time + 0.1f;
		}
		whiteLaneSwitchTimer += Time.deltaTime;
		float x = _lanes[whiteLane].position.x;
		whiteLanePosition = Mathf.Lerp(whiteLanePosition, x, whiteLaneSwitchTimer / _laneSwitchDuration);
		_whiteModel.transform.position = new Vector3(whiteLanePosition, 0f, distanceTravelled + _distanceToWhite);
		if (num > nextStepWhite)
		{
			whiteFootsteps.set3DAttributes(_whiteModel.To3DAttributes());
			whiteFootsteps.start();
			nextStepWhite = num + _distanceBetweenWhiteSteps;
		}
	}

	private void CheckCollision()
	{
		if (Physics.OverlapCapsuleNonAlloc(Player.Instance.transform.position, Camera.main.transform.position, 0.5f, colliders, _layerMask) <= 0)
		{
			return;
		}
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].CompareTag("Enemy"))
			{
				VideoManager.Instance.PlayVideo(Videos.WatchDogScreamer);
				hasEnded = true;
				break;
			}
		}
	}

	private IEnumerator Ending()
	{
		float time = 0f;
		float duration = 1f;
		while (time < duration)
		{
			time += Time.deltaTime;
			RenderSettings.fogDensity = Mathf.Lerp(0.1f, 1f, time / duration);
			yield return null;
		}
		hasEnded = true;
		_endingEvent?.Invoke();
		Player.Instance.Teleport(_teleportPoint);
		time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			RenderSettings.fogDensity = Mathf.Lerp(1f, 0f, time / duration);
			yield return null;
		}
		Player.Instance.FreezePlayer(isFrozen: false);
		base.gameObject.SetActive(value: false);
	}
}
