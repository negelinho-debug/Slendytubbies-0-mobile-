using UnityEngine;
using UnityEngine.Animations;

public class AttackAnimation : StateMachineBehaviour
{
	[Range(0f, 1f)]
	[SerializeField]
	private float m_NormalizedAttackStartTime;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_NormalizedAttackEndTime;

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		if (stateInfo.normalizedTime >= m_NormalizedAttackEndTime)
		{
			animator.GetComponentInParent<AttackController>().SetAttackActive(active: false);
		}
		else if (stateInfo.normalizedTime >= m_NormalizedAttackStartTime)
		{
			animator.GetComponentInParent<AttackController>().SetAttackActive(active: true);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponentInParent<AttackController>().SetAttackActive(active: false);
	}
}
