using FMODUnity;
using UnityEngine;

public class TubiStomp : MonoBehaviour
{
	public Transform[] _feet;

	public EventReference _stompEvent;

	public void Stomp(int foot)
	{
		RuntimeManager.PlayOneShot(_stompEvent, _feet[foot].position);
	}
}
