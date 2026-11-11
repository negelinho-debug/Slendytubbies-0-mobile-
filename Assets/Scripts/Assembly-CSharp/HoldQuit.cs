using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HoldQuit : MonoBehaviour
{
	public enum QuitTo
	{
		Menu,
		Game
	}

	public QuitTo _quitTo;

	public TMP_Text _quitText;

	public CanvasGroup _quitGroup;

	public Slider _quitSlider;

	[Space(10f)]
	public float _quitDuration = 1f;

	public float _quitCancel = 0.3f;

	[Space(10f)]
	public ExcelElement _quitToMenu;

	public ExcelElement _quitGame;

	private bool isQuitting;

	private float quitTime;

	private float startWeight;

	private float targetWeight;

	private float targetDuration;

	private void Start()
	{
		_quitGroup.alpha = 0f;
		string text = ((_quitTo == QuitTo.Menu) ? _quitToMenu.ReadLocalized() : _quitGame.ReadLocalized());
		text = text.Replace("{}", "[Esc]");
		_quitText.text = text;
	}

	private void Update()
	{
		if (!isQuitting && Keyboard.current.escapeKey.isPressed)
		{
			isQuitting = true;
			startWeight = TransitionVolume.Instance.volume.weight;
			targetWeight = 1f;
			targetDuration = _quitDuration;
			quitTime = 0f;
		}
		else if (isQuitting && !Keyboard.current.escapeKey.isPressed)
		{
			isQuitting = false;
			startWeight = TransitionVolume.Instance.volume.weight;
			targetWeight = 0f;
			targetDuration = _quitCancel;
			quitTime = 0f;
		}
		if (quitTime < targetDuration)
		{
			quitTime += Time.deltaTime;
			_quitGroup.alpha = Mathf.Lerp(startWeight, targetWeight, quitTime / targetDuration);
			_quitSlider.value = Mathf.Lerp(startWeight, targetWeight, quitTime / targetDuration);
			TransitionVolume.Instance.volume.weight = Mathf.Lerp(startWeight, targetWeight, quitTime / targetDuration);
		}
		else if (isQuitting && quitTime >= targetDuration)
		{
			if (_quitTo == QuitTo.Menu)
			{
				LevelSwitch.instance.SwitchToMenu(LevelSwitch.SwitchType.Noise);
				base.enabled = false;
			}
			else
			{
				Debug.Log("QUIT");
				Application.Quit();
			}
		}
	}
}
