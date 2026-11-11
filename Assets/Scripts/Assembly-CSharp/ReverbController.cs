using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class ReverbController : MonoBehaviour
{
	public EventReference m_ReverbSnapshot;

	private EventInstance currentInstance;

	private void Start()
	{
		EventInstance eventInstance = RuntimeManager.CreateInstance(m_ReverbSnapshot);
		currentInstance = eventInstance;
	}

	private void OnDisable()
	{
		currentInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		currentInstance.release();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other == Player.Instance.Capsule)
		{
			currentInstance.start();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other == Player.Instance.Capsule)
		{
			currentInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
	}
}
