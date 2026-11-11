using UnityEngine;
using UnityEngine.AI;

public class MinigameAgent : MonoBehaviour
{
	public float _turnSpeed = 5f;

	public NavMeshAgent agent { get; private set; }

	public Animator animator { get; private set; }

	public AnimatorStateInfo stateInfo { get; private set; }

	public Transform lookTarget { get; private set; }

	public Transform moveTarget { get; private set; }

	protected void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponentInChildren<Animator>();
	}

	protected void Update()
	{
		animator.SetFloat("Speed", agent.velocity.magnitude);
		if (lookTarget != null)
		{
			Quaternion b = Quaternion.LookRotation(new Vector3(lookTarget.position.x, base.transform.position.y, lookTarget.position.z) - base.transform.position);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, Time.deltaTime * _turnSpeed);
		}
		if (moveTarget != null)
		{
			agent.SetDestination(moveTarget.position);
		}
	}

	public void SetLookTarget(Transform target)
	{
		lookTarget = ((target != base.transform) ? target : null);
	}

	public void SetMoveTarget(Transform target)
	{
		moveTarget = ((target != base.transform) ? target : null);
	}

	public void Reset()
	{
		lookTarget = null;
		moveTarget = null;
	}
}
