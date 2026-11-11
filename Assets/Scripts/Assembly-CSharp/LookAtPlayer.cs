using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
	public float _turnSpeed = 5f;

	public bool _toRoot;

	private void Update()
	{
		Quaternion b = Quaternion.LookRotation((_toRoot ? Player.Instance.transform.position : Camera.main.transform.position) - base.transform.position);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * _turnSpeed);
	}
}
