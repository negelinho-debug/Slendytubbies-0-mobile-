using HostGame;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class LocalizeLabel : MonoBehaviour
{
	public bool _replaceMode;

	public string _format;

	public TMP_Text _text;

	public ExcelElement _label;

	public string _placeholder;

	private void OnEnable()
	{
		Localization.OnLanguageChanged += Refresh;
		ExcelAsset.OnAnyExcelAssetImported += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		ExcelAsset.OnAnyExcelAssetImported -= Refresh;
		Localization.OnLanguageChanged -= Refresh;
	}

	public void Refresh(ExcelElement element)
	{
		_label = element;
		Refresh();
	}

	public void Refresh()
	{
		if ((bool)_label)
		{
			string text = _label.ReadLocalized();
			if (_replaceMode)
			{
				_text.text = string.Format(text.Replace("{}", "{0}"), _format);
			}
			else if (string.IsNullOrEmpty(_format))
			{
				_text.text = text;
			}
			else
			{
				_text.text = string.Format(_format.Replace("{}", "{0}"), text);
			}
		}
		else if (!string.IsNullOrEmpty(_placeholder))
		{
			_text.text = _placeholder;
		}
	}

	private void OnValidate()
	{
		Refresh();
	}

	private void Reset()
	{
		_text = GetComponentInChildren<TMP_Text>();
	}
}
