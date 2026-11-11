using System;
using UnityEngine;

public class Placement : MonoBehaviour
{
	[Serializable]
	public struct Hand
	{
		public Transform origin;

		public Transform target;

		public Transform hint;

		public float rayDistance;

		internal float desiredLift;

		internal float currentLift;
	}

	public float _smoothAmount = 10f;

	public LayerMask _layerMask;

	public Hand[] _hands;

	[Space(10f)]
	public Transform _pelvis;

	public Transform _refObject;

	private Vector3 _initialPelvisPos;

	private float _currentPelvisOffset;

	private void Start()
	{
		_initialPelvisPos = _pelvis.position;
	}

	private void Update()
	{
		for (int i = 0; i < _hands.Length; i++)
		{
			if (Physics.Raycast(new Vector3(_hands[i].target.position.x, _hands[i].hint.position.y, _hands[i].target.position.z), Vector3.down, out var hitInfo, _hands[i].rayDistance, _layerMask))
			{
				_hands[i].desiredLift = hitInfo.point.y - _hands[i].origin.position.y;
			}
			else
			{
				_hands[i].desiredLift = 0f;
			}
			_hands[i].currentLift = Mathf.Lerp(_hands[i].currentLift, _hands[i].desiredLift, Time.deltaTime * _smoothAmount);
			Vector3 position = _hands[i].origin.position + Vector3.up * _hands[i].currentLift;
			_hands[i].target.position = position;
		}
	}

	private void LateUpdate()
	{
		float b = (_hands[0].currentLift + _hands[1].currentLift + _hands[2].currentLift + _hands[3].currentLift) / (float)_hands.Length;
		_currentPelvisOffset = Mathf.Lerp(_currentPelvisOffset, b, Time.deltaTime * _smoothAmount);
		Vector3 initialPelvisPos = _initialPelvisPos;
		initialPelvisPos.y -= _currentPelvisOffset;
		Transform pelvis = _pelvis;
		Vector3 position = (_refObject.position = new Vector3(_pelvis.position.x, initialPelvisPos.y, _pelvis.position.z));
		pelvis.position = position;
	}
}
