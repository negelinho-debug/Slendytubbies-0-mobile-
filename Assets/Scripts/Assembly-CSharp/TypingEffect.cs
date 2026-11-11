using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TypingEffect : MonoBehaviour
{
	[Serializable]
	public struct TypeText
	{
		public string text;

		public float typingDuration;

		public float waitDuration;
	}

	public List<TypeText> _typeTexts;

	public UnityEvent _onEnd;

	private TMP_Text tmp;

	private int index;

	private float waitTimer;

	private float characterTime;

	private List<string> cleanTexts;

	private int paragraphStartChar;

	private int paragraphLength;

	private void Start()
	{
		tmp = GetComponent<TMP_Text>();
		tmp.text = "";
		cleanTexts = new List<string>();
		foreach (TypeText typeText in _typeTexts)
		{
			cleanTexts.Add(StripRichText(typeText.text));
			TMP_Text tMP_Text = tmp;
			tMP_Text.text = tMP_Text.text + typeText.text + "\n";
		}
		tmp.maxVisibleCharacters = 0;
		index = 0;
		paragraphStartChar = 0;
		paragraphLength = cleanTexts[index].Length;
	}

	private void LateUpdate()
	{
		if (index > _typeTexts.Count - 1)
		{
			return;
		}
		characterTime += Time.smoothDeltaTime;
		float num = _typeTexts[index].typingDuration / (float)paragraphLength;
		if (tmp.maxVisibleCharacters < paragraphStartChar + paragraphLength)
		{
			if (characterTime >= num)
			{
				tmp.maxVisibleCharacters++;
				characterTime = 0f;
			}
			if (InputManager.Instance.SpaceTriggered() || InputManager.Instance.EnterTriggered() || InputManager.Instance.LMBTriggered())
			{
				tmp.maxVisibleCharacters = paragraphStartChar + paragraphLength;
			}
			return;
		}
		waitTimer += Time.deltaTime;
		if (InputManager.Instance.SpaceTriggered() || InputManager.Instance.EnterTriggered() || InputManager.Instance.LMBTriggered())
		{
			waitTimer = _typeTexts[index].waitDuration;
		}
		if (waitTimer >= _typeTexts[index].waitDuration)
		{
			waitTimer = 0f;
			characterTime = 0f;
			index++;
			if (index > _typeTexts.Count - 1)
			{
				_onEnd.Invoke();
				return;
			}
			paragraphStartChar += paragraphLength + 1;
			paragraphLength = cleanTexts[index].Length;
		}
	}

	private string StripRichText(string input)
	{
		return Regex.Replace(input, "<.*?>", "");
	}
}
