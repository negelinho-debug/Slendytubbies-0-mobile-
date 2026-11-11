using FMODUnity;
using PrimeTween;
using UnityEngine;

public class Door : IInteractable
{
	public float _openAngle = 100f;

	public float _openDuration = 1f;

	public float _pushDistance = 1f;

	public float _pushForce = 5f;

	[Space(10f)]
	public EventReference _openReference;

	public EventReference _closeReference;

	private bool isOpen;

	public void DoorSetActive(bool active)
	{
		Active = active;
	}

	public override void Use()
	{
		if (!isOpen)
		{
			UseDoor();
		}
	}

	public void UseDoor()
	{
		Tween.LocalRotation(base.transform, new Vector3(0f, isOpen ? 0f : _openAngle, 0f), _openDuration);
		RuntimeManager.PlayOneShot(isOpen ? _closeReference : _openReference, base.transform.position);
		Vector3 vector = Player.Instance.Capsule.ClosestPoint(base.transform.position);
		if ((base.transform.position - vector).sqrMagnitude <= _pushDistance * _pushDistance)
		{
			Player.Instance.Controller.Push(base.transform.forward.normalized * _pushForce);
		}
		isOpen = true;
	}
}
