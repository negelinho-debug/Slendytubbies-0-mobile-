using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MinigameAgent))]
public class EvadePlayer : MonoBehaviour
{
	[Header("Anchoring & Bounds")]
	public Collider _bound;

	public Transform _endPoint;

	public ParticleSystem _blood;

	[Header("Behavior")]
	public float _lookSpeed = 5f;

	public float _fleeDistance = 8f;

	public float _stepAwayDistance = 3f;

	public int _hitsToComplete = 3;

	[Header("Timing")]
	public float _postFleeCooldown = 0.8f;

	public float _postHitRecoveryBuffer = 0.2f;

	[Header("Events")]
	public UnityEvent _onLastHit;

	public UnityEvent _onCompletion;

	[Header("Dialogue")]
	public ExcelElement[] _hurtElements;

	private CapsuleCollider capsule;

	private MinigameAgent gameAgent;

	private string impactStateName = "Impact";

	private bool isFleeing;

	private bool isRecovering;

	private bool isLastInvoked;

	private bool isCompleted;

	private bool inCooldown;

	private int currentFleeSide = 1;

	private int hits;

	private Vector3 lastDestination;

	private void Start()
	{
		gameAgent = GetComponent<MinigameAgent>();
		gameAgent.agent.updateRotation = false;
		gameAgent.agent.updatePosition = true;
		gameAgent.agent.isStopped = false;
		capsule = GetComponent<CapsuleCollider>();
	}

	private void Update()
	{
		if (isCompleted)
		{
			HandleCompletionBehavior();
			return;
		}
		Vector3 position = Player.Instance.transform.position;
		float distanceToPlayer = Vector3.Distance(base.transform.position, position);
		if (!isRecovering && !inCooldown)
		{
			if (isFleeing)
			{
				HandleFleeState(distanceToPlayer);
			}
			else
			{
				HandleEvadeBehavior(position, distanceToPlayer);
			}
		}
	}

	private void HandleEvadeBehavior(Vector3 playerPos, float distanceToPlayer)
	{
		gameAgent.agent.updateRotation = false;
		if (distanceToPlayer < _stepAwayDistance)
		{
			Vector3 normalized = (base.transform.position - playerPos).normalized;
			Vector3 normalized2 = (_bound.transform.position - base.transform.position).normalized;
			Vector3 destination = base.transform.position + (_bound.bounds.Contains(base.transform.position) ? normalized : normalized2) * _stepAwayDistance;
			gameAgent.agent.SetDestination(destination);
		}
		Quaternion b = Quaternion.LookRotation(playerPos - base.transform.position);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * _lookSpeed);
	}

	private void HandleFleeState(float distanceToPlayer)
	{
		if (hits < _hitsToComplete && gameAgent.agent.remainingDistance <= gameAgent.agent.stoppingDistance)
		{
			isFleeing = false;
			StartCoroutine(FleeCooldown());
		}
		else if (hits >= _hitsToComplete)
		{
			if (!isLastInvoked)
			{
				isLastInvoked = true;
				_onLastHit?.Invoke();
			}
			if (Vector3.Distance(base.transform.position, _endPoint.position) <= gameAgent.agent.stoppingDistance)
			{
				gameAgent.agent.isStopped = true;
				isFleeing = false;
				isCompleted = true;
			}
		}
	}

	private void HandleCompletionBehavior()
	{
		Vector3 position = Player.Instance.transform.position;
		float num = Vector3.Distance(base.transform.position, position);
		Quaternion b = Quaternion.LookRotation(position - base.transform.position);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * _lookSpeed);
		gameAgent.agent.updateRotation = true;
		if (num <= _stepAwayDistance)
		{
			StartCoroutine(ScreenFade.Instance.HoldBlackScreen(1f, CompleteGame));
		}
	}

	private void CompleteGame()
	{
		_onCompletion.Invoke();
		ScreenFade.Instance.StartFade(1f, 0f, 1f);
		base.gameObject.SetActive(value: false);
	}

	public void TakeHit(Vector3 position, Vector3 direction)
	{
		if (!isCompleted)
		{
			Dialogue.Play(_hurtElements[hits]);
			hits++;
			gameAgent.agent.isStopped = true;
			gameAgent.agent.updateRotation = true;
			isRecovering = true;
			isFleeing = false;
			gameAgent.animator.SetTrigger("Impact");
			Vector3 normalized = direction.normalized;
			Vector3 destination = ((hits >= _hitsToComplete) ? _endPoint.position : (base.transform.position + normalized * _fleeDistance));
			_blood.transform.position = capsule.ClosestPointOnBounds(position + Vector3.up);
			_blood.transform.rotation = Quaternion.LookRotation(position + Vector3.up - _blood.transform.position);
			_blood.Play(withChildren: true);
			gameAgent.agent.SetDestination(destination);
			StartCoroutine(WaitForAnimation(impactStateName));
		}
	}

	private IEnumerator WaitForAnimation(string animState)
	{
		yield return null;
		while (!gameAgent.animator.GetCurrentAnimatorStateInfo(0).IsName(animState))
		{
			yield return null;
		}
		float length = gameAgent.animator.GetCurrentAnimatorStateInfo(0).length;
		yield return new WaitForSeconds(length + _postHitRecoveryBuffer);
		isFleeing = true;
		isRecovering = false;
		gameAgent.agent.isStopped = false;
	}

	private IEnumerator FleeCooldown()
	{
		inCooldown = true;
		yield return new WaitForSeconds(_postFleeCooldown);
		inCooldown = false;
	}
}
