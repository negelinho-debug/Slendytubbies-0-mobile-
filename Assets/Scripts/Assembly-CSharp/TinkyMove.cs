using System;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(MinigameAgent))]
public class TinkyMove : AttackController, IDamagable
{
	public List<Transform> _points;

	public float _switchPointDistance;

	[Header("Stun")]
	public float _stunDuration = 1f;

	public float _nextStunDelay = 0.5f;

	[Header("Attack")]
	public float _attackDistance = 1.5f;

	[Header("Sound")]
	public EventReference _axeSwing;

	public EventReference _axeHit;

	private MinigameAgent gameAgent;

	private bool isEnded;

	private bool isStunned;

	private float stunTimer;

	private float nextStunDelay;

	private int pointIndex;

	private bool hasAttacked;

	private void Start()
	{
		gameAgent = GetComponent<MinigameAgent>();
		gameAgent.agent.SetDestination(_points[pointIndex].position);
	}

	private void OnEnable()
	{
		VideoManager.OnFinished = (Action)Delegate.Combine(VideoManager.OnFinished, new Action(EndingSwitchScene));
	}

	private void OnDisable()
	{
		VideoManager.OnFinished = (Action)Delegate.Remove(VideoManager.OnFinished, new Action(EndingSwitchScene));
	}

	private void EndingSwitchScene()
	{
		GameState.saveState.Flags.Add(GameCompletionFlags.Po);
		LevelSwitch.instance.SwitchToMenu(LevelSwitch.SwitchType.NoiseNoIntro);
	}

	private void Update()
	{
		if (pointIndex < _points.Count && Vector3.Distance(base.transform.position, _points[pointIndex].position) <= _switchPointDistance && pointIndex < _points.Count - 1)
		{
			if (pointIndex == 1)
			{
				Vector3 normalized = (base.transform.position + Vector3.up * 1.5f - Camera.main.transform.position).normalized;
				Player.Instance.Look.LockView(Quaternion.LookRotation(normalized), 2.5f, 10f);
			}
			pointIndex++;
			gameAgent.agent.SetDestination(_points[pointIndex].position);
		}
		float num = Vector3.Distance(base.transform.position, Player.Instance.transform.position);
		if (!gameAgent.agent.isStopped && num <= _attackDistance)
		{
			gameAgent.agent.isStopped = true;
			RuntimeManager.PlayOneShot(_axeSwing, base.transform.position);
			gameAgent.animator.SetTrigger("Attack");
			Vector3 normalized2 = (base.transform.position + Vector3.up * 1.6f - Camera.main.transform.position).normalized;
			Player.Instance.Look.LockView(Quaternion.LookRotation(normalized2), 3f, 5f);
		}
		if (pointIndex > _points.Count - 2 && gameAgent.agent.remainingDistance <= 0.5f)
		{
			nextStunDelay = float.PositiveInfinity;
			_attackDistance = 999f;
		}
		if (stunTimer > 0f)
		{
			stunTimer -= Time.deltaTime;
		}
		else if (stunTimer < 0f && isStunned)
		{
			isStunned = false;
			gameAgent.agent.isStopped = false;
			nextStunDelay = Time.time + _nextStunDelay;
			GetComponent<FootstepSystem>().PlaySoundImmediate();
		}
		gameAgent.animator.SetBool("IsStunned", isStunned);
	}

	public override void SetAttackActive(bool active)
	{
		if (active && !hasAttacked)
		{
			hasAttacked = true;
			RuntimeManager.PlayOneShot(_axeHit, base.transform.position);
			VideoManager.Instance.PlayVideo(Videos.PoDeath);
			isEnded = true;
		}
	}

	public void Damage()
	{
		if (!isStunned && !(Time.time < nextStunDelay))
		{
			isStunned = true;
			gameAgent.agent.isStopped = true;
			stunTimer = _stunDuration;
		}
	}
}
