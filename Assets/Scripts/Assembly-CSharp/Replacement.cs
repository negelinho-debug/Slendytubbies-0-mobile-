using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Replacement : MonoBehaviour
{
	[Serializable]
	public struct BoneIK
	{
		public TwoBoneIKConstraint constraint;

		public string curveName;

		public float rayDistance;

		public float limbLength;

		internal float limbDistance;
	}

	public Animator _animator;

	public float _smoothAmount = 10f;

	public LayerMask _layerMask;

	public BoneIK[] _hands;

	public Transform _pelvis;

	private float _initialPelvisY;

	private float _pelvisVelocity;

	private void Start()
	{
		_initialPelvisY = _pelvis.position.y;
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		for (int i = 0; i < _hands.Length; i++)
		{
			_hands[i].constraint.weight = _animator.GetFloat(_hands[i].curveName);
			TwoBoneIKConstraintData data = _hands[i].constraint.data;
			Transform target = data.target;
			Vector3 vector = new Vector3(data.tip.position.x, data.root.position.y, data.tip.position.z);
			if (Physics.Raycast(vector, Vector3.down, out var hitInfo, _hands[i].rayDistance, _layerMask))
			{
				Vector3 vector2 = hitInfo.point + Vector3.up * _animator.GetFloat(_hands[i].curveName);
				target.position = Vector3.Slerp(target.position, vector2, Time.deltaTime * _smoothAmount);
				Debug.DrawLine(vector, vector2);
			}
			_hands[i].limbDistance = Vector3.Distance(data.root.position, target.position);
		}
		float a = _hands[0].limbDistance - _hands[0].limbLength;
		float b = _hands[1].limbDistance - _hands[1].limbLength;
		Mathf.Max(Mathf.Max(a, b));
	}
}
