using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMODSoundPlayer
{
	public EventInstance currentInstance { get; private set; }

	public void PlaySound(EventReference soundEvent, Vector3 position = default(Vector3))
	{
		EventDescription eventDescription = RuntimeManager.GetEventDescription(soundEvent);
		eventDescription.unloadSampleData();
		eventDescription.loadSampleData();
		EventInstance eventInstance = (currentInstance = RuntimeManager.CreateInstance(soundEvent));
		if (position != default(Vector3))
		{
			eventInstance.setParameterByName("Is2D", 0f);
			eventInstance.set3DAttributes(position.To3DAttributes());
		}
		else
		{
			eventInstance.setParameterByName("Is2D", 1f);
		}
		eventInstance.start();
	}
}
