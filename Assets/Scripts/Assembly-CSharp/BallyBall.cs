using FMODUnity;
using UnityEngine;

public class BallyBall : IInteractable
{
	public Transform _safeAreaOrigin;

	public float _maxSafeDistance = 8f;

	public float _unstuckForce = 0.3f;

	[Space(10f)]
	public float hitForceMin;

	public float hitForceMax;

	public EventReference _hitEvent;

	public EventReference _collideEvent;

	private Rigidbody rigidBody;

	private float wakeTime;

	private void Awake()
	{
		rigidBody = GetComponent<Rigidbody>();
		wakeTime = Time.time;
	}

	private void Update()
	{
		if (Vector3.Distance(base.transform.position, _safeAreaOrigin.position) > _maxSafeDistance)
		{
			Vector3 normalized = (_safeAreaOrigin.position - base.transform.position).normalized;
			rigidBody.AddForce(normalized * _unstuckForce, ForceMode.VelocityChange);
		}
	}

	public override void Use()
	{
		HungerMeter component = Player.Instance.GetComponent<HungerMeter>();
		Vector3 vector = Vector3.Reflect(Player.Instance.Camera.transform.forward, Vector3.up);
		float num = Random.Range(hitForceMin, hitForceMax);
		component.AddHunger(0.2f);
		rigidBody.AddForce(vector * num, ForceMode.Impulse);
		if (num > 0.01f)
		{
			RuntimeManager.PlayOneShotAttached(_hitEvent, base.gameObject);
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (!(Time.time - wakeTime < 1f))
		{
			RuntimeManager.PlayOneShotAttached(_collideEvent, base.gameObject);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		if (_safeAreaOrigin != null)
		{
			Gizmos.DrawWireSphere(_safeAreaOrigin.position, _maxSafeDistance);
		}
	}
}
