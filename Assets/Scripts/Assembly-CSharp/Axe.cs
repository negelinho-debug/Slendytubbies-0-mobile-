using System;
using FMODUnity;
using UnityEngine;

public class Axe : MonoBehaviour, ISillyItem
{
	public LayerMask _layerMask;

	public float _attackDuration = 0.3f;

	public float _attackDistance = 3f;

	public string[] _attackStates;

	public Transform _cameraTransform;

	public Transform _damageTransform;

	[Header("Sounds")]
	public EventReference _swing;

	public PhysicsMaterial _dogMat;

	public EventReference _dogHit;

	public EventReference _eyeHit;

	[Space]
	public ArrowTubi _arrow;

	private Animator animator;

	private bool isHit;

	[NonSerialized]
	public bool isAttacking;

	private float attackTime;

	private string currentStateName;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	public void Equip()
	{
		base.enabled = true;
	}

	public void Unequip()
	{
		base.enabled = false;
	}

	private void Update()
	{
		if (!_cameraTransform)
		{
			_cameraTransform = Camera.main.transform;
		}
		if (InputManager.Instance.LMBTriggered() && !isAttacking)
		{
			isHit = false;
			isAttacking = true;
			attackTime = _attackDuration;
			int num = UnityEngine.Random.Range(0, _attackStates.Length);
			currentStateName = _attackStates[num];
			animator.CrossFade(currentStateName, 0.1f, 0, 0f);
			RuntimeManager.PlayOneShotAttached(_swing, _cameraTransform.gameObject);
		}
		if (!isAttacking)
		{
			return;
		}
		if (attackTime > 0f)
		{
			attackTime -= Time.deltaTime;
		}
		else
		{
			isAttacking = false;
		}
		RaycastHit hitInfo;
		if ((bool)_arrow)
		{
			Vector3 to = _arrow.transform.position - Player.Instance.transform.position;
			to.y = 0f;
			float magnitude = to.magnitude;
			to.Normalize();
			Vector3 forward = _cameraTransform.forward;
			forward.y = 0f;
			forward.Normalize();
			float num2 = Vector3.Angle(forward, to);
			if (!isHit && attackTime <= _attackDuration * 0.85f && num2 < 40f && magnitude < _attackDistance)
			{
				isHit = true;
				Vector3 point = MathEx.ClosestPointOnCylinder(_cameraTransform.position, _arrow.transform.position + Vector3.up * _arrow._agent.height * 0.5f, 0.4f, _arrow._agent.height);
				RuntimeManager.PlayOneShotAttached((_arrow.GetComponent<Collider>().material == _dogMat) ? _dogHit : _eyeHit, _cameraTransform.gameObject);
				_arrow.Damage(point);
			}
		}
		else if (!isHit && attackTime <= _attackDuration * 0.85f && Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out hitInfo, _attackDistance, _layerMask))
		{
			isHit = true;
			IDamagable component = hitInfo.collider.GetComponent<IDamagable>();
			if (component != null)
			{
				RuntimeManager.PlayOneShotAttached((hitInfo.collider.material == _dogMat) ? _dogHit : _eyeHit, Camera.main.gameObject);
				component.Damage(hitInfo.point);
			}
		}
	}
}
