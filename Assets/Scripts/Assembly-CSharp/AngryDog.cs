using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(MinigameAgent))]
public class AngryDog : AttackController, IDamagable
{
	public float _moveSpeed = 5f;

	public float _attackDistance = 2f;

	public float _deathDuration = 0.5f;

	public float _nextAttackDelay = 0.5f;

	public LayerMask _layerMask;

	public EventReference _attackSound;

	[Header("Animations")]
	public string _runAttackStateName = "RunAttack";

	public string _attack2StateName = "Attack2";

	public string _attackStateName = "Attack";

	public string _deathStateName = "Death";

	private MinigameAgent gameAgent;

	private DogAppearance appearance;

	private bool isDead;

	private float attackTime;

	private bool hasAttacked;

	private void Awake()
	{
		gameAgent = GetComponent<MinigameAgent>();
		appearance = GetComponent<DogAppearance>();
		Respawn();
	}

	private void Update()
	{
		if (Time.time >= attackTime)
		{
			gameAgent.agent.isStopped = false;
		}
		Vector3 vector = base.transform.position - Player.Instance.transform.position;
		if (!isDead && !gameAgent.agent.isStopped && vector.sqrMagnitude <= _attackDistance * _attackDistance)
		{
			attackTime = Time.time + _nextAttackDelay;
			gameAgent.agent.isStopped = true;
			int num = Random.Range(0, 1);
			gameAgent.animator.Play((num == 0) ? _attackStateName : _attack2StateName);
		}
		if (isDead && !appearance.isFading)
		{
			DogSpawner.Instance.ReturnObject(base.gameObject);
		}
	}

	public override void SetAttackActive(bool active)
	{
		if (active)
		{
			if (!hasAttacked)
			{
				hasAttacked = true;
				if (Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, out var hitInfo, _attackDistance, _layerMask) && hitInfo.collider == Player.Instance.Capsule)
				{
					hitInfo.collider.GetComponent<PlayerHealth>().Damage();
					RuntimeManager.PlayOneShot(_attackSound, base.transform.position);
				}
			}
		}
		else
		{
			hasAttacked = false;
		}
	}

	public void Respawn()
	{
		gameAgent.SetLookTarget(Player.Instance.transform);
		gameAgent.SetMoveTarget(Player.Instance.transform);
		gameAgent.agent.speed = _moveSpeed;
		gameAgent.agent.isStopped = false;
		isDead = false;
		appearance.FadeIn(appearance._fadeInDuration);
	}

	public void Damage()
	{
		if (!isDead)
		{
			gameAgent.agent.isStopped = true;
			appearance.FadeOut(_deathDuration);
			isDead = true;
			gameAgent.animator.Play(_deathStateName);
		}
	}
}
