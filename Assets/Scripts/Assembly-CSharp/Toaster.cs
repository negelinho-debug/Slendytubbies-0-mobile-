using FMODUnity;
using UnityEngine;

public class Toaster : IInteractable
{
	public ExcelElement _firstUseDialogue;

	public EventReference _toasterReference;

	public GameObject _toastPrefab;

	public ParticleSystem _smokeParticles;

	public Transform _spawnPoint;

	public float _upwardForce = 5f;

	private bool firstUse;

	public override void Use()
	{
		if (!firstUse)
		{
			Dialogue.Play(_firstUseDialogue);
			firstUse = true;
		}
		_smokeParticles.Play();
		RuntimeManager.PlayOneShot(_toasterReference, base.transform.position);
		Player.Instance.GetComponent<HungerMeter>().AddHunger(0.05f);
		GameObject obj = Object.Instantiate(_toastPrefab, _spawnPoint.position, _toastPrefab.transform.rotation);
		Rigidbody component = obj.GetComponent<Rigidbody>();
		Vector3 force = new Vector3(Random.Range(-1f, 1f), _upwardForce, Random.Range(-1f, 1f));
		component.AddForce(force, ForceMode.Impulse);
		Vector3 torque = Vector3.Cross(obj.transform.forward, force.normalized) * 15f;
		component.AddTorque(torque, ForceMode.Impulse);
		Object.Destroy(obj, 5f);
	}
}
