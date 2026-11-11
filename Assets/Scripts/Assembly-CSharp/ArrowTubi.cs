using System.Collections;
using FMODUnity;
using HostGame;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ArrowTubi : AttackController, IDamagable
{
	public enum ArrowState
	{
		Hybernating,
		RunningAway,
		ChasingPlayer
	}

	private ArrowState state;

	private float stunTimer;

	[Header("Basics")]
	public ArrowState _initialState;

	public NavMeshAgent _agent;

	public Animator _bodyVisuals;

	public Collider _collider;

	public float _runSpeed;

	public float _agentAcceleration;

	public float _disappearDistance = 10f;

	public float _boostDistance;

	[Header("Fleeing")]
	public float _fleeStartDistance = 50f;

	public float _fleeMinRadius = 50f;

	public float _fleeMaxRadius = 50f;

	[Header("Attacking")]
	public float _attackTeleportDistance;

	public float _attackDistance;

	public float _attackDuration;

	public LayerMask _attackRaycastLayermask;

	public EventReference _attackSound;

	[Space]
	public float _stunTime;

	public float _stunSpeedMultiplier;

	private bool hasAttacked;

	private float attackuUnfreezeTimestamp;

	[Header("Health")]
	public GameObject _damageParticles;

	public EventReference _deathSound;

	public Transform _particleExplosion;

	public bool _vulnerable;

	public int _health = 100;

	public int _damagePerHit = 25;

	public float _deathAnimationDuration;

	private bool _alreadyDead;

	private NavMeshPath cachedPath;

	private Vector3[] cachedCorners = new Vector3[256];

	private Transform playerTransform => Player.Instance.transform;

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, _fleeStartDistance);
		Gizmos.color = Color.magenta * 0.5f;
		Gizmos.DrawWireSphere(base.transform.position, _fleeMinRadius);
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(base.transform.position, _fleeMaxRadius);
	}

	private void Start()
	{
		state = _initialState;
		cachedPath = new NavMeshPath();
		_agent.destination = base.transform.position;
		if (state == ArrowState.ChasingPlayer)
		{
			SpawnAndChasePlayer();
		}
	}

	private float ComputeDistanceToPath(in Vector3 p, NavMeshPath path)
	{
		int cornersNonAlloc = path.GetCornersNonAlloc(cachedCorners);
		float num = 100000f;
		for (int i = 0; i < cornersNonAlloc - 1; i++)
		{
			ref Vector3 reference = ref cachedCorners[i];
			ref Vector3 reference2 = ref cachedCorners[i + 1];
			Vector3 vector = p - reference;
			Vector3 vector2 = reference2 - reference;
			float num2 = Mathf.Clamp(Vector3.Dot(vector, vector2) / Vector3.Dot(vector2, vector2), 0f, 1f);
			num = Mathf.Min((vector - vector2 * num2).magnitude, num);
		}
		return num;
	}

	private float ComputePathLength(NavMeshPath path)
	{
		float num = 0f;
		int cornersNonAlloc = path.GetCornersNonAlloc(cachedCorners);
		for (int i = 0; i < cornersNonAlloc - 1; i++)
		{
			ref Vector3 reference = ref cachedCorners[i];
			ref Vector3 reference2 = ref cachedCorners[i + 1];
			num += Vector3.Distance(reference, reference2);
		}
		return num;
	}

	private Vector3 FindFleeTarget(in Vector3 playerPos, in Vector3 agentPos)
	{
		NavMeshQueryFilter filter = new NavMeshQueryFilter
		{
			agentTypeID = _agent.agentTypeID,
			areaMask = _agent.areaMask
		};
		float num = 0f;
		Vector3 result = default(Vector3);
		int num2 = 0;
		int num3 = 0;
		while (num3 < 10 && num2 < 100)
		{
			float num4 = Random.Range(_fleeMinRadius, _fleeMaxRadius);
			Vector3 vector = Quaternion.AngleAxis((float)Random.Range(0, 360), Vector3.up) * (num4 * Vector3.forward);
			if (NavMesh.SamplePosition(playerPos + vector, out var hit, 10f, filter) && NavMesh.CalculatePath(agentPos, hit.position, filter, cachedPath) && cachedPath.status == NavMeshPathStatus.PathComplete)
			{
				num3++;
				float num5 = ComputeDistanceToPath(in playerPos, cachedPath);
				Debug.DrawRay(hit.position, Vector3.up * num5 * 0.2f, Color.red, 10f);
				if (num5 > num)
				{
					Debug.DrawRay(hit.position, Vector3.up * num5 * 0.4f, Color.green, 10f);
					result = hit.position;
					num = num5;
				}
			}
			num2++;
		}
		return result;
	}

	private void FixedUpdate()
	{
		if (stunTimer > 0f)
		{
			stunTimer -= Time.deltaTime;
			float f = Mathf.Max(0f, stunTimer) / _stunTime;
			Player.Instance.Controller.walkSpeedMultiplier *= Mathf.Lerp(1f, _stunSpeedMultiplier, Mathf.Sqrt(f));
		}
	}

	private void Update()
	{
		if (_alreadyDead)
		{
			return;
		}
		switch (state)
		{
		case ArrowState.RunningAway:
		{
			_agent.speed = _runSpeed;
			_agent.acceleration = _agentAcceleration;
			Vector3 playerPos = Player.Instance.transform.position;
			Vector3 agentPos = _agent.transform.position;
			Vector3 destination = _agent.destination;
			float sqrMagnitude = (playerPos - agentPos).sqrMagnitude;
			float sqrMagnitude2 = (playerPos - destination).sqrMagnitude;
			float num3 = _fleeStartDistance * _fleeStartDistance;
			if (sqrMagnitude2 < num3 && sqrMagnitude < num3)
			{
				Vector3 destination2 = FindFleeTarget(in playerPos, in agentPos);
				_agent.SetDestination(destination2);
			}
			if (Vector3.Distance(agentPos, _agent.destination) < _agent.stoppingDistance)
			{
				Vector3 position2 = playerTransform.position;
				position2.y = base.transform.position.y;
				Quaternion b2 = Quaternion.LookRotation(position2 - base.transform.position);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b2, Time.deltaTime * _agent.angularSpeed);
			}
			break;
		}
		case ArrowState.ChasingPlayer:
		{
			Vector3 position = playerTransform.position;
			position.y = base.transform.position.y;
			Quaternion b = Quaternion.LookRotation(position - base.transform.position);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, Time.deltaTime * _agent.angularSpeed);
			NavMeshQueryFilter filter = new NavMeshQueryFilter
			{
				agentTypeID = _agent.agentTypeID,
				areaMask = _agent.areaMask
			};
			NavMesh.CalculatePath(_agent.transform.position, playerTransform.position, filter, cachedPath);
			float num = ComputePathLength(cachedPath);
			if (cachedPath.status != NavMeshPathStatus.PathInvalid)
			{
				_agent.SetPath(cachedPath);
			}
			float num2 = _runSpeed;
			float agentAcceleration = _agentAcceleration;
			if (num > _boostDistance)
			{
				num2 *= 4f;
			}
			_agent.speed = num2;
			_agent.acceleration = agentAcceleration;
			if (Time.time < attackuUnfreezeTimestamp)
			{
				return;
			}
			if ((base.transform.position - Player.Instance.transform.position).sqrMagnitude <= _attackDistance * _attackDistance)
			{
				attackuUnfreezeTimestamp = Time.time + _attackDuration;
				_agent.speed = 0f;
				_bodyVisuals.Play("Attack");
			}
			break;
		}
		}
		float value = 0f;
		if (_agent.speed > 0.01f)
		{
			value = _agent.velocity.magnitude / _agent.speed;
		}
		_bodyVisuals.SetFloat("Speed", value, 0.5f, Time.deltaTime);
	}

	public void SpawnAndChasePlayer()
	{
		if (state == ArrowState.ChasingPlayer || _alreadyDead)
		{
			return;
		}
		NavMeshQueryFilter filter = new NavMeshQueryFilter
		{
			agentTypeID = _agent.agentTypeID,
			areaMask = _agent.areaMask
		};
		Vector3 position = Player.Instance.transform.position;
		float num = Vector3.Distance(_agent.transform.position, position);
		if (state == ArrowState.RunningAway && num < _attackTeleportDistance && NavMesh.CalculatePath(_agent.transform.position, position, filter, cachedPath) && cachedPath.status == NavMeshPathStatus.PathComplete && ComputePathLength(cachedPath) < _disappearDistance + 5f)
		{
			state = ArrowState.ChasingPlayer;
			return;
		}
		Vector3 position2 = playerTransform.position;
		int i = 0;
		int num2 = 0;
		float num3 = 100000f;
		bool flag = false;
		for (; i < 100; i++)
		{
			if (num2 >= 10)
			{
				break;
			}
			Vector3 position3 = playerTransform.position;
			Vector2 vector = Random.onUnitCircle * _disappearDistance;
			position3.x += vector.x;
			position3.z += vector.y;
			if (!NavMesh.SamplePosition(position3, out var hit, 2f, _agent.areaMask) || !NavMesh.CalculatePath(position, hit.position, filter, cachedPath) || ((flag || cachedPath.status != NavMeshPathStatus.PathPartial) && cachedPath.status != NavMeshPathStatus.PathComplete))
			{
				continue;
			}
			float num4 = ComputePathLength(cachedPath);
			if (num4 > _disappearDistance * 0.5f)
			{
				if (num4 < num3)
				{
					position2 = hit.position;
				}
				if (cachedPath.status == NavMeshPathStatus.PathComplete)
				{
					flag = true;
					num2++;
				}
			}
		}
		_agent.enabled = false;
		base.transform.position = position2;
		_bodyVisuals.gameObject.SetActive(value: true);
		_collider.enabled = true;
		state = ArrowState.ChasingPlayer;
		_agent.enabled = true;
	}

	public override void SetAttackActive(bool active)
	{
		if (active)
		{
			if (!hasAttacked)
			{
				hasAttacked = true;
				if (Vector3.Distance(base.transform.position.WithY(0f), playerTransform.position.WithY(0f)) < _attackDistance)
				{
					Player.Instance.GetComponent<PlayerHealth>().Damage();
					stunTimer = Mathf.Max(stunTimer, _stunTime);
					RuntimeManager.PlayOneShotAttached(_attackSound, base.gameObject);
				}
			}
		}
		else
		{
			hasAttacked = false;
		}
	}

	public void Damage(Vector3 point)
	{
		if (_alreadyDead)
		{
			return;
		}
		if (state != ArrowState.RunningAway)
		{
			Vector3 destination = FindFleeTarget(Player.Instance.transform.position, _agent.transform.position);
			_agent.SetDestination(destination);
		}
		state = ArrowState.RunningAway;
		if (_vulnerable)
		{
			_bodyVisuals.Play(ArrowRootMotion.DAMAGE_ANIMATION);
			_health -= _damagePerHit;
			Vector3 forward = point - base.transform.position;
			forward.y = 0f;
			forward.Normalize();
			Object.Instantiate(_damageParticles, point, Quaternion.LookRotation(forward)).SetActive(value: true);
			if (_health <= 0)
			{
				StartCoroutine(DeathAnimation());
				_alreadyDead = true;
				_agent.speed = 0f;
			}
		}
	}

	private IEnumerator DeathAnimation()
	{
		_bodyVisuals.Play("Death");
		RuntimeManager.PlayOneShotAttached(_deathSound, base.gameObject);
		yield return new WaitForSeconds(_deathAnimationDuration);
		Object.Instantiate(_particleExplosion, base.transform.position + Vector3.up, Quaternion.Euler(-90f, 0f, 0f)).GetComponent<ParticleSystem>().Play();
		base.gameObject.SetActive(value: false);
	}
}
