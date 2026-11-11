using System.Collections;
using FMOD.Studio;
using FMODUnity;
using HostGame;
using UnityEngine;
using UnityEngine.AI;

public class YetiTubi : AttackController
{
	public enum YetiState
	{
		HurtingStageOne,
		HurtingStageTwo,
		HurtingStageThree,
		HurtingStun,
		ChasingPlayer,
		KillingPlayer
	}

	[Header("Basics")]
	public BreakingNewsSeanAddedCustardCollectingToTheGuardianCollection _videoGame;

	public NavMeshAgent _agent;

	public Animator _bodyVisuals;

	private YetiState state;

	private YetiState lastState;

	private Transform playerTransform;

	public Transform headBoneTransform;

	public float _walkSpeedStart;

	public float _walkSpeedEndgame;

	[CurveSetup(CurveColor.Blue, 0f, 1f, 0f, 1f)]
	public AnimationCurve _walkSpeedProgressCurve;

	[Header("Attacking")]
	public float _attackDistance;

	[Header("Attacking")]
	public float _attackDuration;

	public EventReference _attackSound;

	private bool hasAttacked;

	[Header("Suffering")]
	public EventReference _deathSound;

	public Renderer _bodyAlive;

	public Renderer _bodyDead;

	public ParticleSystem _explosion;

	public float _stageOneDuration;

	public float _stageTwoDuration;

	public float _stageThreeDuration;

	public float _stunDuration;

	public EventReference _stunSoundEvent;

	public float _parryTime = 0.3f;

	private float _sufferBeginTimeStamp;

	private float _stageSwapTimeStamp;

	private bool isDead;

	private Remote remote;

	private EventInstance attackSoundInstance;

	private bool playedHurtSoundInStageOne;

	private float attackTimer;

	public bool shouldSuffer { get; set; }

	private void Start()
	{
		playerTransform = Player.Instance.transform;
		state = YetiState.ChasingPlayer;
		remote = Object.FindObjectOfType<Remote>();
	}

	private void OnDestroy()
	{
		if (attackSoundInstance.isValid())
		{
			attackSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			attackSoundInstance.release();
		}
	}

	private void Update()
	{
		if (shouldSuffer && state == YetiState.ChasingPlayer)
		{
			shouldSuffer = false;
			Suffer();
		}
		bool flag = lastState != state;
		lastState = state;
		switch (state)
		{
		case YetiState.HurtingStageOne:
			if (flag)
			{
				playedHurtSoundInStageOne = false;
			}
			if (!playedHurtSoundInStageOne && Time.time - _sufferBeginTimeStamp > 1f)
			{
				remote.SetLightsEnabled(1);
				playedHurtSoundInStageOne = true;
				RuntimeManager.PlayOneShotAttached(_stunSoundEvent, headBoneTransform.gameObject);
			}
			if (Time.time >= _stageSwapTimeStamp)
			{
				_bodyVisuals.CrossFade("SufferStageTwo", 0.1f);
				_stageSwapTimeStamp = Time.time + _stageTwoDuration;
				state = YetiState.HurtingStageTwo;
			}
			break;
		case YetiState.HurtingStageTwo:
			if (flag)
			{
				remote.SetLightsEnabled(2);
				RuntimeManager.PlayOneShotAttached(_stunSoundEvent, headBoneTransform.gameObject);
			}
			if (Time.time >= _stageSwapTimeStamp)
			{
				_bodyVisuals.CrossFade("SufferStageThree", 0.1f);
				state = YetiState.HurtingStageThree;
				Quaternion normalized2 = Quaternion.LookRotation(headBoneTransform.position - Camera.main.transform.position).normalized;
				Player.Instance.Look.LockView(normalized2, 100f, 5f);
				Player.Instance.Controller.m_WalkSpeed = 0f;
				remote._controlsLocked = true;
				remote.GetComponent<ItemSwitcher>().controlsLocked = true;
				RuntimeManager.PlayOneShotAttached(_deathSound, headBoneTransform.gameObject);
				_stageSwapTimeStamp = Time.time + _stageThreeDuration;
			}
			break;
		case YetiState.HurtingStageThree:
			if (flag)
			{
				remote.SetLightsEnabled(3);
				_videoGame.musicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			}
			if (!isDead && Time.time >= _stageSwapTimeStamp)
			{
				_bodyAlive.enabled = false;
				_bodyDead.enabled = true;
				_explosion.Play();
				isDead = true;
				StartCoroutine(WinGame());
			}
			break;
		case YetiState.HurtingStun:
			if (Time.time >= _stageSwapTimeStamp)
			{
				_bodyVisuals.CrossFade("Walking", 0.1f);
				state = YetiState.ChasingPlayer;
			}
			break;
		case YetiState.ChasingPlayer:
		{
			Vector3 position = playerTransform.position;
			position.y = base.transform.position.y;
			Quaternion b = Quaternion.LookRotation(position - base.transform.position);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, Time.deltaTime * _agent.angularSpeed);
			if ((base.transform.position - Player.Instance.transform.position).sqrMagnitude <= _attackDistance * _attackDistance)
			{
				_agent.speed = 0f;
				_bodyVisuals.Play("Attack");
				state = YetiState.KillingPlayer;
			}
			else
			{
				float t = _walkSpeedProgressCurve.Evaluate(_videoGame._gameProgress);
				_agent.speed = Mathf.Lerp(_walkSpeedStart, _walkSpeedEndgame, t);
				_bodyVisuals.SetFloat("WaklSpeed", _videoGame._gameProgress - 0.001f);
				_agent.SetDestination(playerTransform.position);
			}
			break;
		}
		case YetiState.KillingPlayer:
			Player.Instance.Controller.walkSpeedMultiplier *= 0f;
			if (flag)
			{
				Quaternion normalized = Quaternion.LookRotation(headBoneTransform.position - Camera.main.transform.position).normalized;
				Player.Instance.Look.LockView(normalized, 100f, 5f);
				attackSoundInstance = RuntimeManager.CreateInstance(_attackSound);
				attackSoundInstance.start();
				attackTimer = 0f;
			}
			attackTimer += Time.deltaTime;
			if (attackTimer < _parryTime)
			{
				if (shouldSuffer)
				{
					Player.Instance.Look.LockView(Quaternion.identity, 0f, 0f);
					Suffer();
					attackSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
				}
			}
			else
			{
				Debug.Log("Hello dear my friend");
				_videoGame.musicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
				remote.enabled = false;
				remote.humEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			}
			if (attackTimer >= _attackDuration)
			{
				Object.FindObjectOfType<PlayerHealth>().DieInstantly();
			}
			break;
		}
	}

	private IEnumerator WinGame()
	{
		yield return new WaitForSeconds(0.45f);
		Application.Quit();
	}

	public void Suffer()
	{
		_agent.speed = 0f;
		if (state != YetiState.HurtingStageThree)
		{
			_sufferBeginTimeStamp = Time.time;
			_bodyVisuals.CrossFade("SufferStageOne", 0.1f);
			_stageSwapTimeStamp = Time.time + _stageOneDuration;
			state = YetiState.HurtingStageOne;
		}
	}

	public void RelaxBuddy()
	{
		shouldSuffer = false;
		if (state == YetiState.HurtingStageOne)
		{
			if (Time.time - _sufferBeginTimeStamp > 1f)
			{
				_stageSwapTimeStamp = Time.time + 2f;
				state = YetiState.HurtingStun;
			}
			else
			{
				_bodyVisuals.CrossFade("Walking", 0.1f);
				state = YetiState.ChasingPlayer;
			}
		}
		else if (state == YetiState.HurtingStageTwo)
		{
			_stageSwapTimeStamp = Time.time + _stunDuration;
			state = YetiState.HurtingStun;
		}
	}

	public override void SetAttackActive(bool active)
	{
		if (active)
		{
			if (!hasAttacked)
			{
				hasAttacked = true;
				state = YetiState.KillingPlayer;
			}
		}
		else
		{
			hasAttacked = false;
		}
	}
}
