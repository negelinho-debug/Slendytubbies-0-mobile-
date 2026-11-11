using FMODUnity;
using UnityEngine;

public class Crowbar : MonoBehaviour
{
	private int hideCrowbarStateNameId = Animator.StringToHash("CrowbarEnd");

	private int takeCrowbarStateNameId = Animator.StringToHash("CrowbarStart");

	public ExcelElement[] _knockLogs;

	public ExcelElement[] _repeatLogs;

	public int _knocksForDialog = 3;

	[Space(10f)]
	public LayerMask _layerMask;

	public float _attackDuration = 0.3f;

	public float _attackDistance = 3f;

	public string[] _attackStates;

	[Space(10f)]
	public Collider _DoorCollider;

	public float _knockDistance = 3f;

	public float _knockAngle = 45f;

	public string _knockStateName;

	[Header("Sounds")]
	public EventReference _crowbarSwing;

	public EventReference _crowbarHit;

	public EventReference _knock;

	private Animator animator;

	private float distanceToDoor;

	private float doorDot;

	private int knocks;

	private int logIndex;

	private bool isHit;

	private bool isAttacking;

	private float attackTime;

	private float lastKnockTime = -10000f;

	private string currentStateName;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		Transform transform = Camera.main.transform;
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(transform.position, transform.forward, out hitInfo, _knockDistance, _layerMask);
		bool flag2 = flag && hitInfo.collider == _DoorCollider;
		distanceToDoor = Vector3.Distance(base.transform.position, _DoorCollider.transform.position);
		doorDot = Vector3.Dot(_DoorCollider.transform.forward, Player.Instance.transform.forward);
		if (!Player.Instance.Controller.IsFreezed && InputManager.Instance.LMBTriggered())
		{
			if (flag2)
			{
				RuntimeManager.PlayOneShot(_knock);
				animator.Play(_knockStateName, 1, 0f);
				lastKnockTime = Time.time;
				if (!Dialogue.IsPlaying())
				{
					knocks++;
					if (knocks >= _knocksForDialog)
					{
						knocks = 0;
						if (logIndex >= _knockLogs.Length)
						{
							int num = Random.Range(0, _repeatLogs.Length);
							Dialogue.Play(_repeatLogs[num]);
						}
						else
						{
							Dialogue.Play(_knockLogs[logIndex]);
							logIndex++;
						}
					}
				}
			}
			else if (!isAttacking)
			{
				isHit = false;
				isAttacking = true;
				attackTime = _attackDuration;
				int num2 = Random.Range(0, _attackStates.Length);
				currentStateName = _attackStates[num2];
				animator.CrossFade(currentStateName, 0.1f, 0, 0f);
				RuntimeManager.PlayOneShot(_crowbarSwing);
			}
		}
		if (isAttacking)
		{
			if (attackTime > 0f)
			{
				attackTime -= Time.deltaTime;
			}
			else
			{
				isAttacking = false;
			}
			if (!isHit && attackTime <= _attackDuration * 0.85f && flag)
			{
				isHit = true;
				IDamagable component = hitInfo.collider.GetComponent<IDamagable>();
				if (component != null)
				{
					RuntimeManager.PlayOneShot(_crowbarHit);
					component.Damage();
				}
			}
		}
		else if (flag2)
		{
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			AnimatorStateInfo nextAnimatorStateInfo = animator.GetNextAnimatorStateInfo(0);
			if (currentAnimatorStateInfo.shortNameHash != hideCrowbarStateNameId && nextAnimatorStateInfo.shortNameHash != hideCrowbarStateNameId)
			{
				animator.CrossFadeInFixedTime(hideCrowbarStateNameId, 0.1f);
			}
		}
		else
		{
			AnimatorStateInfo currentAnimatorStateInfo2 = animator.GetCurrentAnimatorStateInfo(0);
			AnimatorStateInfo nextAnimatorStateInfo2 = animator.GetNextAnimatorStateInfo(0);
			if (currentAnimatorStateInfo2.shortNameHash == hideCrowbarStateNameId && nextAnimatorStateInfo2.shortNameHash != takeCrowbarStateNameId)
			{
				animator.CrossFadeInFixedTime(takeCrowbarStateNameId, 0.1f);
			}
		}
	}
}
