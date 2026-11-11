using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(HorizontalSelectorPanel))]
public class LanguageSelector : MonoBehaviour
{
	private HorizontalSelectorPanel selector;

	private void OnEnable()
	{
		selector = GetComponent<HorizontalSelectorPanel>();
		HorizontalSelectorPanel horizontalSelectorPanel = selector;
		horizontalSelectorPanel.OnSelectionChanged = (Action<int>)Delegate.Combine(horizontalSelectorPanel.OnSelectionChanged, new Action<int>(ChangeLanguage));
		Refresh();
	}

	private void OnValidate()
	{
		Refresh();
	}

	private void Refresh()
	{
		selector = GetComponent<HorizontalSelectorPanel>();
		List<LanguageDescriptor> list = new List<LanguageDescriptor>(Localization.Load()._languages);
		string[] array = new string[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = list[i].nativeName;
		}
		selector.SetOptions(array);
		selector.SelectOption(Localization.Load().IndexOf(Localization.Language), silent: true);
	}

	private void OnDisable()
	{
		HorizontalSelectorPanel horizontalSelectorPanel = selector;
		horizontalSelectorPanel.OnSelectionChanged = (Action<int>)Delegate.Remove(horizontalSelectorPanel.OnSelectionChanged, new Action<int>(ChangeLanguage));
	}

	private void ChangeLanguage(int obj)
	{
		Localization.SetLanguage(Localization.Load()._languages[obj].id);
	}
}
