using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class ResetSave : MonoBehaviour
{
	public PostProcessVolume _resetVolume;

	public float _resetDuration = 1f;

	public float _resetCancel = 0.3f;

	private bool isResetting;

	private float resetTime;

	private float startWeight;

	private float targetWeight;

	private float targetDuration;

	private void Update()
	{
		if (!isResetting && Keyboard.current.backquoteKey.isPressed)
		{
			isResetting = true;
			startWeight = _resetVolume.weight;
			targetWeight = 1f;
			targetDuration = _resetDuration;
			resetTime = 0f;
		}
		else if (isResetting && !Keyboard.current.backquoteKey.isPressed)
		{
			isResetting = false;
			startWeight = _resetVolume.weight;
			targetWeight = 0f;
			targetDuration = _resetCancel;
			resetTime = 0f;
		}
		if (resetTime < targetDuration)
		{
			resetTime += Time.deltaTime;
			_resetVolume.weight = Mathf.Lerp(startWeight, targetWeight, resetTime / targetDuration);
		}
		else if (isResetting && resetTime >= targetDuration)
		{
			GameState.saveState.CompletionState = new List<string>();
			GameState.SaveGameToDisk();
			SceneManager.LoadScene("Menu");
		}
	}
}
