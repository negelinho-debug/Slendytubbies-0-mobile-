using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
	public Transform _target;

	public float _speed = 5f;

	private void Update()
	{
		Quaternion rotation = Quaternion.LookRotation((((_target != null) ? _target.position : base.transform.position) - Player.Instance.Camera.transform.position).normalized);
		Player.Instance.Look.LockView(rotation, Time.deltaTime, _speed);
	}
}
