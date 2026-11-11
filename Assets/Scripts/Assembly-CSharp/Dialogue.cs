using System;
using System.Globalization;
using System.Text;
using FMODUnity;
using STR;
using TMPro;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
	[Serializable]
	public struct FloatMacro
	{
		public string macro;

		public float value;
	}

	private struct LineMarker
	{
		public float value;
	}

	private static Dialogue instance;

	private float characterTime;

	private ExcelElement currentDialogue;

	private float currentDialogueDelay;

	private int charIndex;

	private int paragraphIndex;

	private DialogueLineInfo lineInfo;

	public TMP_Text _text;

	public TMP_Text _characterText;

	public FloatMacro[] _macros;

	public float _punctuationPause;

	public float _defaultPause;

	[Space]
	public float _defaultTypingSpeed;

	[Space(10f)]
	public TMP_FontAsset _YDKJFont;

	public string[] _YDKJLanguages;

	public TMP_FontAsset _TypewriterFont;

	public static Action<int> OnLineChanged;

	private const char cnull = '\0';

	private char[] comma = new char[1] { ',' };

	private void Awake()
	{
		if ((bool)instance)
		{
			base.gameObject.SetActive(value: false);
			Debug.Log("Dialogue player instance already exists, disabling", this);
			return;
		}
		lineInfo.text = new StringBuilder(512);
		lineInfo.chars = new ManagedUnsafeList<DialogueChar>(512);
		_text.text = "";
		_characterText.text = "";
		instance = this;
		charIndex = -1;
		bool flag = false;
		for (int i = 0; i < _YDKJLanguages.Length; i++)
		{
			if (GameState.saveState.Language == _YDKJLanguages[i])
			{
				flag = true;
				break;
			}
		}
		_text.font = (flag ? _YDKJFont : _TypewriterFont);
		_characterText.font = (flag ? _YDKJFont : _TypewriterFont);
	}

	private void LateUpdate()
	{
		if (charIndex < 0)
		{
			return;
		}
		if (currentDialogueDelay > 0f)
		{
			currentDialogueDelay -= Time.smoothDeltaTime;
			return;
		}
		characterTime += Time.smoothDeltaTime;
		bool flag = false;
		bool flag2 = charIndex >= lineInfo.chars.Length || _text.maxVisibleCharacters >= _text.textInfo.characterCount;
		while (!flag2 && characterTime >= 0f)
		{
			DialogueChar dialogueChar = lineInfo.chars[charIndex];
			if (!(characterTime >= dialogueChar.typeTime))
			{
				break;
			}
			characterTime -= dialogueChar.typeTime;
			_text.maxVisibleCharacters++;
			charIndex++;
			flag = true;
			if (_text.maxVisibleCharacters >= _text.textInfo.characterCount)
			{
				break;
			}
		}
		_characterText.text = lineInfo.charInfo.name;
		_characterText.color = lineInfo.charInfo.color;
		if (flag)
		{
			RuntimeManager.PlayOneShot(lineInfo.charInfo.voice);
		}
		if (flag2 && characterTime > lineInfo.timeAdd)
		{
			NextLine();
		}
	}

	private void NextLine()
	{
		OnLineChanged?.Invoke(paragraphIndex);
		if (!currentDialogue)
		{
			charIndex = -1;
			Debug.LogError("Next line called while no active dialogue??", this);
			return;
		}
		_characterText.text = "";
		characterTime = 0f;
		charIndex = 0;
		paragraphIndex++;
		while (paragraphIndex < currentDialogue.length)
		{
			string text = currentDialogue.Read("#CONTROL", paragraphIndex);
			if (!string.IsNullOrEmpty(text))
			{
				lineInfo.element = currentDialogue;
				string line = currentDialogue.ReadLocalized(paragraphIndex);
				lineInfo.character = text;
				lineInfo.timeLimit = ParseOrDefault(currentDialogue.Read("#TIME", paragraphIndex), 0f);
				lineInfo.timeAdd = ParseOrDefault(currentDialogue.Read("#TIME_ADD", paragraphIndex), 3.5f);
				lineInfo.charSpeed = ParseOrDefault(currentDialogue.Read("#CHAR_SPEED", paragraphIndex), _defaultTypingSpeed);
				ParseDialogueLine(line, ref lineInfo);
				if (lineInfo.character.StartsWith("!"))
				{
					lineInfo.charInfo = default(GameCharacterInfo);
					lineInfo.timeAdd = lineInfo.timeLimit;
					_characterText.text = "";
				}
				else
				{
					Localization.GetCharacterInfo(lineInfo.character, out lineInfo.charInfo);
				}
				break;
			}
			paragraphIndex++;
		}
		if (paragraphIndex >= currentDialogue.length)
		{
			charIndex = -1;
			currentDialogue = null;
			_text.text = "";
			_characterText.text = "";
			lineInfo.Clear();
		}
	}

	public static void Play(ExcelElement element, float delay = 0f)
	{
		if ((bool)instance)
		{
			if (!element)
			{
				Debug.LogError("Can't play null dialogue", instance);
				return;
			}
			instance.currentDialogueDelay = delay;
			instance.currentDialogue = element;
			instance.paragraphIndex = -1;
			instance.NextLine();
		}
	}

	public static void StopAllDialogue()
	{
		if ((bool)instance)
		{
			instance.currentDialogue = null;
			instance.charIndex = -1;
			instance._text.text = "";
			instance._characterText.text = "";
			instance.lineInfo.Clear();
		}
	}

	public static DialogueLineInfo GetCurrentLineInfo()
	{
		if (!instance)
		{
			return default(DialogueLineInfo);
		}
		return instance.lineInfo;
	}

	public static bool IsPlaying(ExcelElement dialogue = null)
	{
		if (dialogue == null)
		{
			return instance.lineInfo.element != null;
		}
		return instance.lineInfo.element == dialogue;
	}

	private void ParseDialogueLine(string line, ref DialogueLineInfo info)
	{
		if (info.text == null)
		{
			info.text = new StringBuilder();
		}
		if (info.chars == null)
		{
			info.chars = new ManagedUnsafeList<DialogueChar>();
		}
		info.text.Clear();
		info.chars.Clear();
		ManagedUnsafeList<DialogueChar> chars = info.chars;
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		float num = 1f / info.charSpeed;
		float num2 = 0f;
		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			switch (c)
			{
			case '<':
				stringBuilder2.Append(c);
				while (Next(line, i++) != '>')
				{
					stringBuilder2.Append(line[i]);
				}
				stringBuilder2.Append(line[i]);
				break;
			case '|':
			{
				LineMarker lineMarker = ParseMarker(line, ref i);
				if (Next(line, i) != 0)
				{
					if (char.IsLetterOrDigit(Next(line, i)))
					{
						chars.Add(new DialogueChar
						{
							typeTime = num
						});
						stringBuilder.Append(' ');
						stringBuilder2.Append(' ');
					}
					i++;
					chars.Add(new DialogueChar
					{
						typeTime = lineMarker.value
					});
					stringBuilder.Append(line[i]);
					stringBuilder2.Append(line[i]);
				}
				break;
			}
			default:
			{
				float num3 = num;
				if (char.IsWhiteSpace(c) && Prev(line, i) != '\'' && char.IsPunctuation(Prev(line, i)))
				{
					num3 += _punctuationPause;
				}
				chars.Add(new DialogueChar
				{
					typeTime = num3
				});
				stringBuilder.Append(c);
				stringBuilder2.Append(c);
				break;
			}
			}
		}
		for (int j = 0; j < chars.Length; j++)
		{
			num2 += chars[j].typeTime;
		}
		num2 += info.timeAdd;
		if (info.timeLimit > 0f)
		{
			float num4 = Mathf.Min(1f, info.timeLimit / num2);
			info.timeAdd *= num4;
			if (num4 < 1f)
			{
				for (int k = 0; k < chars.Length; k++)
				{
					chars[k].typeTime *= num4;
				}
			}
		}
		charIndex = 0;
		_text.SetText(stringBuilder2);
		_text.maxVisibleCharacters = 0;
	}

	private float FindMacro(ReadOnlySpan<char> macroName)
	{
		for (int i = 0; i < _macros.Length; i++)
		{
			if (MemoryExtensions.Equals(macroName, _macros[i].macro, StringComparison.InvariantCultureIgnoreCase))
			{
				return _macros[i].value;
			}
		}
		return _defaultPause;
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
		return FindMacro(chars);
	}

	private LineMarker ParseMarker(string line, ref int iChar)
	{
		int num = iChar + 1;
		char c;
		do
		{
			c = Next(line, iChar++);
		}
		while (c != 0 && c != '|');
		int num2 = iChar;
		int num3 = num;
		LineMarker result = default(LineMarker);
		result.value = ParseOrDefault(line.Substring(num3, num2 - num3), _defaultPause);
		return result;
	}

	private char Next(string str, int index)
	{
		if (index + 1 >= str.Length)
		{
			return '\0';
		}
		return str[index + 1];
	}

	private char Prev(string str, int index)
	{
		if (index - 1 < 0)
		{
			return '\0';
		}
		return str[index - 1];
	}
}
