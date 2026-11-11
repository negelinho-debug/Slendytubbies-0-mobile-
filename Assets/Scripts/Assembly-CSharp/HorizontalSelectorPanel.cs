using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HorizontalSelectorPanel : Selectable
{
	private bool isBound;

	private ExcelElement[] bindingLabels;

	private Func<int> getValueFunc;

	private Action<int> setValueFunc;

	private bool initialized;

	private bool optionsDirty;

	private Color[] allOptionsColors = Array.Empty<Color>();

	private Graphic[] allOptionsIndicators = Array.Empty<Graphic>();

	public Action<int> OnSelectionChanged;

	[SerializeField]
	private int _selectedOption;

	[SerializeField]
	private string[] _options;

	[SerializeField]
	private TMP_Text _selectedOptionText;

	public int SelectedOption => _selectedOption;

	public string SelectedOptionValue => _options[_selectedOption];

	protected override void OnEnable()
	{
		base.OnEnable();
		if (isBound)
		{
			SelectOption(getValueFunc(), silent: true);
			if (bindingLabels != null)
			{
				Localization.OnLanguageChanged -= RefreshOnLanguageChange;
				Localization.OnLanguageChanged += RefreshOnLanguageChange;
				RefreshOnLanguageChange();
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (isBound && bindingLabels != null)
		{
			Localization.OnLanguageChanged -= RefreshOnLanguageChange;
		}
	}

	private void RefreshOnLanguageChange()
	{
		for (int i = 0; i < _options.Length; i++)
		{
			_options[i] = bindingLabels[i].ReadLocalized();
		}
	}

	private void LateUpdate()
	{
		if (_selectedOption >= 0 && _selectedOption < _options.Length)
		{
			_selectedOptionText.text = _options[_selectedOption];
		}
	}

	public void NextOption()
	{
		SelectOption(_selectedOption + 1);
	}

	public void PreviousOption()
	{
		SelectOption(_selectedOption - 1);
	}

	public void SelectOption(int index, bool silent = false)
	{
		int selectedOption = _selectedOption;
		_selectedOption = index;
		ClampOption();
		if (!silent && selectedOption != _selectedOption)
		{
			OnSelectionChanged?.Invoke(_selectedOption);
			if (isBound)
			{
				setValueFunc(_selectedOption);
			}
		}
	}

	private void ClampOption()
	{
		if (_selectedOption < 0)
		{
			_selectedOption = _options.Length - 1;
		}
		if (_selectedOption >= _options.Length)
		{
			_selectedOption = 0;
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		Debug.Log("Pointer down! ");
		base.OnPointerDown(eventData);
	}

	public override void OnMove(AxisEventData eventData)
	{
		if (!IsActive() || !IsInteractable())
		{
			base.OnMove(eventData);
			return;
		}
		switch (eventData.moveDir)
		{
		case MoveDirection.Left:
			PreviousOption();
			break;
		case MoveDirection.Right:
			NextOption();
			break;
		default:
			base.OnMove(eventData);
			break;
		}
	}

	public void SetOptions(params string[] options)
	{
		_selectedOption = 0;
		_options = options;
		optionsDirty = true;
	}

	public void UpdateOptions(params string[] options)
	{
		_options = options;
	}

	private void DestroySelectors()
	{
		for (int i = 0; i < allOptionsIndicators.Length; i++)
		{
			if ((bool)allOptionsIndicators[i])
			{
				UnityEngine.Object.Destroy(allOptionsIndicators[i].gameObject);
			}
		}
	}

	public void Bind(Func<int> getValue, Action<int> onChanged, params ExcelElement[] options)
	{
		isBound = true;
		SetOptions(options.Select((ExcelElement el) => el.ReadLocalized()).ToArray());
		getValueFunc = getValue;
		setValueFunc = onChanged;
		bindingLabels = options;
		SelectOption(getValue(), silent: true);
		Localization.OnLanguageChanged += RefreshOnLanguageChange;
	}

	public void Bind(Func<int> getValue, Action<int> onChanged, params object[] options)
	{
		isBound = true;
		SetOptions(options.Select((object el) => el.ToString()).ToArray());
		getValueFunc = getValue;
		setValueFunc = onChanged;
		SelectOption(getValue(), silent: true);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		EventSystem.current.SetSelectedGameObject(base.gameObject, eventData);
		base.OnPointerEnter(eventData);
	}
}
