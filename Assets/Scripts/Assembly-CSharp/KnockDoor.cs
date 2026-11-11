using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class KnockDoor : IInteractable
{
	public int _knocksForEvent = 3;

	public UnityEvent[] _events;

	[Space(10f)]
	public Animator _playerAnimator;

	public string _knockStateName = "DoorKnock";

	public int _animLayer;

	public EventReference _knock;

	private int index;

	private int knocks;

	public override void Use()
	{
		RuntimeManager.PlayOneShot(_knock);
		_playerAnimator.Play(_knockStateName, _animLayer, 0f);
		if (Dialogue.IsPlaying())
		{
			return;
		}
		knocks++;
		if (knocks >= _knocksForEvent)
		{
			knocks = 0;
			if (index < _events.Length)
			{
				_events[index]?.Invoke();
				index++;
			}
		}
	}
}
