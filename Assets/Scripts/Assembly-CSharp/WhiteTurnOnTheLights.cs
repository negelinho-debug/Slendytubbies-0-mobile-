using System.Collections;
using UnityEngine;

public class WhiteTurnOnTheLights : MonoBehaviour
{
	public static bool isRestart;

	public float _inDarknessTime;

	public float _musicDelayTime;

	public GameObject _musicEmitter;

	public GameObject _blindfold;

	public GameObject _lightBuzzingEmitter;

	public GameObject _lightOnEmitter;

	public DoorGame _doorGame;

	private IEnumerator Start()
	{
		if (isRestart)
		{
			_blindfold.SetActive(value: false);
			_lightBuzzingEmitter.SetActive(value: true);
			_musicEmitter.SetActive(value: true);
			_doorGame.enabled = true;
		}
		else
		{
			_blindfold.SetActive(value: true);
			yield return new WaitForSeconds(_inDarknessTime);
			_lightOnEmitter.SetActive(value: true);
			_blindfold.SetActive(value: false);
			yield return new WaitForSeconds(0.1f);
			_lightBuzzingEmitter.SetActive(value: true);
			yield return new WaitForSeconds(_musicDelayTime);
			_musicEmitter.SetActive(value: true);
			_doorGame.enabled = true;
		}
		isRestart = false;
	}
}
