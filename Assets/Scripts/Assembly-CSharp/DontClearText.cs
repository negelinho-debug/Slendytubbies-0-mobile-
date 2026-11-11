using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DontClearText : MonoBehaviour
{
	[Serializable]
	public struct LineInfo
	{
		public string text;

		public float timeAdd;

		public float timeLimit;

		public float charSpeed;

		public float typeTime;
	}

	public ExcelElement _element;

	public UnityEvent _onCompletion;

	private TMP_Text TMPText;

	private List<LineInfo> lines = new List<LineInfo>();

	private float charTime;

	private int paragraphIndex;

	private int paragraphLength;

	private int paragraphStartChar;

	private char[] comma = new char[1] { ',' };

	private void Start()
	{
		TMPText = GetComponent<TMP_Text>();
		Refresh();
	}

	private void OnEnable()
	{
		Localization.OnLanguageChanged += Refresh;
	}

	private void OnDisable()
	{
		Localization.OnLanguageChanged -= Refresh;
	}

	private void Refresh()
	{
		TMPText.text = "";
		for (int i = 0; i < _element.length; i++)
		{
			LineInfo item = new LineInfo
			{
				text = _element.ReadLocalized(i),
				timeAdd = ParseOrDefault(_element.Read("#TIME_ADD", i), 2.5f),
				timeLimit = ParseOrDefault(_element.Read("#TIME", i), 0f),
				charSpeed = ParseOrDefault(_element.Read("#CHAR_SPEED", i), 25f)
			};
			float num = 1f / item.charSpeed;
			if (item.timeLimit > 0f)
			{
				float num2 = Mathf.Min(1f, item.timeLimit / num);
				item.timeAdd *= num2;
				item.typeTime *= num2;
			}
			else
			{
				item.typeTime = num;
			}
			lines.Add(item);
			TMP_Text tMPText = TMPText;
			tMPText.text = tMPText.text + lines[i].text + "\n";
		}
		paragraphLength = lines[paragraphIndex].text.Length;
		paragraphStartChar = 0;
		TMPText.maxVisibleCharacters = 0;
	}

	private void LateUpdate()
	{
		if (paragraphIndex > lines.Count - 1)
		{
			return;
		}
		charTime += Time.smoothDeltaTime;
		bool flag = TMPText.maxVisibleCharacters >= paragraphStartChar + paragraphLength;
		while (!flag && charTime >= 0f && charTime >= lines[paragraphIndex].typeTime)
		{
			charTime -= lines[paragraphIndex].typeTime;
			TMPText.maxVisibleCharacters++;
			if (TMPText.maxVisibleCharacters >= paragraphStartChar + paragraphLength)
			{
				break;
			}
		}
		bool flag2 = InputManager.Instance.SpaceTriggered() || InputManager.Instance.LMBTriggered();
		if (!flag && flag2)
		{
			TMPText.maxVisibleCharacters = paragraphStartChar + paragraphLength;
		}
		if (flag && (charTime > lines[paragraphIndex].timeAdd || flag2))
		{
			charTime = 0f;
			paragraphIndex++;
			if (paragraphIndex > lines.Count - 1)
			{
				_onCompletion.Invoke();
				return;
			}
			paragraphStartChar += paragraphLength + 1;
			paragraphLength = lines[paragraphIndex].text.Length;
		}
	}

	private float ParseOrDefault(ReadOnlySpan<char> chars, float @default)
	{
		if (chars.IsEmpty || chars.IsWhiteSpace())
		{
			return @default;
		}
		int num = chars.IndexOf(MemoryExtensions.AsSpan(comma), StringComparison.Ordinal);
		float result;
		if (num >= 0)
		{
			Span<char> span = stackalloc char[chars.Length];
			chars.CopyTo(span);
			span[num] = '.';
			if (float.TryParse(span, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				return result;
			}
		}
		else if (float.TryParse(chars, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
		{
			return result;
		}
		return @default;
	}
}
