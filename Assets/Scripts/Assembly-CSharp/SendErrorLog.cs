using System;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SendErrorLog : MonoBehaviour
{
	public ExcelElement _element;

	public TMP_Text _errorLog;

	public TMP_Text _copyTextFrom;

	public UnityEvent _onCompletion;

	private int index;

	private float waitTime;

	private StringBuilder historyBuilder = new StringBuilder();

	private char[] comma = new char[1] { ',' };

	private void Start()
	{
		_errorLog.text = "";
		historyBuilder.AppendLine(_copyTextFrom.text);
		_errorLog.SetText(historyBuilder);
		index = -1;
	}

	private void Update()
	{
		waitTime -= Time.deltaTime;
		if (waitTime <= 0f)
		{
			index++;
			if (index > _element.length - 1)
			{
				_onCompletion.Invoke();
				return;
			}
			waitTime = ParseOrDefault(_element.Read("#TIME_ADD", index), 2.5f);
			historyBuilder.AppendLine(_element.ReadLocalized(index));
			_errorLog.SetText(historyBuilder);
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
