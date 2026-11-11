using UnityEngine;
using UnityEngine.AI;

public class ArrowRootMotion : MonoBehaviour
{
	public static readonly int DAMAGE_ANIMATION = Animator.StringToHash("TakeDamage");

	public NavMeshAgent _agent;

	public Animator _animator;

	private void OnAnimatorMove()
	{
		AnimatorStateInfo currentAnimatorStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
		AnimatorStateInfo nextAnimatorStateInfo = _animator.GetNextAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.shortNameHash == DAMAGE_ANIMATION || nextAnimatorStateInfo.shortNameHash == DAMAGE_ANIMATION)
		{
			Vector3 velocity = _animator.velocity;
			_agent.nextPosition = base.transform.position + velocity * Time.deltaTime;
		}
	}
}
