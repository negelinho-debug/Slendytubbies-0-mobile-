using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(MinigameAgent))]
public class FollowPlayer : AttackController
{
	public Transform[] _points;

	public float _orbitRadius = 2f;

	public float _orbitStartDistance = 2.3f;

	public Vector2 _orbitDuration = new Vector2(0.2f, 0.4f);

	[Space(10f)]
	public EvadePlayer _laaLaa;

	public float _lookSpeed = 5f;

	public float _lookAtDistance = 4f;

	public float _attackDistance = 2f;

	public float _attackCooldown = 1f;

	public float _attackDuration = 1.2f;

	[Header("Sounds")]
	public EventReference _axeSwing;

	public EventReference _axeHit;

	private MinigameAgent gameAgent;

	private float orbitTime;

	private bool orbitClockwise;

	private bool isAttacking;

	private float attackTime;

	private int index;

	private int pointsLeft;

	private bool isFollowing;

	private float cooldownTime;

	private float pointProgress;

	private bool hasAttacked;

	private void Start()
	{
		gameAgent = GetComponent<MinigameAgent>();
		orbitTime = Random.Range(_orbitDuration.x, _orbitDuration.y);
	}

	private void Update()
	{
		float num = Vector3.Distance(base.transform.position, _laaLaa.transform.position);
		Vector3 vector = _laaLaa.transform.position - base.transform.position;
		vector.y = 0f;
		Vector3 normalized = vector.normalized;
		if (orbitTime > 0f)
		{
			if (num <= _orbitStartDistance)
			{
				orbitTime -= Time.deltaTime;
			}
			Vector3 vector2 = (orbitClockwise ? new Vector3(normalized.z, 0f, 0f - normalized.x) : new Vector3(0f - normalized.z, 0f, normalized.x)) * gameAgent.agent.speed;
			float num2 = vector.magnitude - _orbitRadius;
			vector2 += normalized * num2 * 1.5f;
			gameAgent.agent.SetDestination(base.transform.position + vector2);
			return;
		}
		if (!isAttacking && num > _orbitStartDistance)
		{
			gameAgent.agent.isStopped = false;
			gameAgent.agent.SetDestination(base.transform.position + normalized * _orbitRadius);
		}
		else
		{
			gameAgent.agent.isStopped = true;
		}
		Quaternion b = Quaternion.LookRotation(_laaLaa.transform.position - base.transform.position);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * _lookSpeed);
		if (!isAttacking && num <= _attackDistance)
		{
			isAttacking = true;
			attackTime = _attackDuration;
			RuntimeManager.PlayOneShot(_axeSwing, base.transform.position);
			gameAgent.animator.SetTrigger("Attack");
		}
		if (attackTime > 0f)
		{
			attackTime -= Time.deltaTime;
		}
		else if (attackTime <= 0f && isAttacking)
		{
			isAttacking = false;
			gameAgent.agent.isStopped = false;
			orbitClockwise = !orbitClockwise;
			orbitTime = Random.Range(_orbitDuration.x, _orbitDuration.y);
		}
	}

	public override void SetAttackActive(bool active)
	{
		if (active)
		{
			if (!hasAttacked)
			{
				hasAttacked = true;
				RuntimeManager.PlayOneShot(_axeHit, base.transform.position);
				_laaLaa.TakeHit(base.transform.position, base.transform.forward);
				index++;
				if (index > _points.Length - 1)
				{
					index = _points.Length - 1;
				}
			}
		}
		else
		{
			hasAttacked = false;
		}
	}
}
