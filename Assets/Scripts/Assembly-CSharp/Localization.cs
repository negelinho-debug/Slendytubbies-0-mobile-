using System;
using System.Collections.Generic;
using System.Globalization;
using FMODUnity;
using HostGame;
using UnityEngine;

[CreateAssetMenu(menuName = "Report/Localization")]
public class Localization : ScriptableObject
{
	public static Localization instance;

	public Spreadsheet _langListSheet;

	public Spreadsheet _characterList;

	public GameCharacterInfo[] _characterColors;

	private Dictionary<string, int> languageMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	public string _defaultLanguage;

	public LanguageDescriptor[] _languages;

	public ExcelAsset[] _gameTextFiles;

	public static string Language => GameState.saveState.Language;

	public static string DefaultLanguage => Load()._defaultLanguage;

	public static event Action OnLanguageChanged;

	public static Localization Load()
	{
		if ((bool)instance)
		{
			return instance;
		}
		return instance = Resources.Load<Localization>("GameLocalization");
	}

	public static void GetCharacterInfo(string charName, out GameCharacterInfo info)
	{
		info.voice = default(EventReference);
		info.name = charName;
		info.color = Color.white;
		if (!instance._characterList)
		{
			return;
		}
		for (int i = 0; i < instance._characterList.Elements.Count; i++)
		{
			ExcelElement excelElement = instance._characterList.Elements[i];
			if (excelElement.id.Equals(charName, StringComparison.OrdinalIgnoreCase))
			{
				info.name = excelElement.Read(Language, "#ID", 0);
			}
		}
		for (int j = 0; j < instance._characterColors.Length; j++)
		{
			GameCharacterInfo gameCharacterInfo = instance._characterColors[j];
			if (gameCharacterInfo.name.Equals(charName, StringComparison.OrdinalIgnoreCase))
			{
				info.color = gameCharacterInfo.color;
				info.voice = gameCharacterInfo.voice;
			}
		}
	}

	private void OnEnable()
	{
		RefreshLanguageMap();
		RefreshMaps();
		ExcelAsset.OnAnyExcelAssetImported += RefreshMaps;
	}

	private void OnDisable()
	{
		ExcelAsset.OnAnyExcelAssetImported -= RefreshMaps;
	}

	private void RefreshMaps()
	{
		for (int i = 0; i < _gameTextFiles.Length; i++)
		{
			_gameTextFiles[i].PrepareMap();
		}
	}

	private void OnValidate()
	{
		RefreshLanguageMap();
		RefreshMaps();
	}

	private void RefreshLanguageMap()
	{
		languageMap.Clear();
		for (int i = 0; i < _languages.Length; i++)
		{
			languageMap[_languages[i].id] = i;
		}
	}

	public static string GetFallback(string language)
	{
		LanguageDescriptor[] languages = Load()._languages;
		for (int i = 0; i < languages.Length; i++)
		{
			if (languages[i].id.Equals(language, StringComparison.OrdinalIgnoreCase))
			{
				return languages[i].fallback;
			}
		}
		return "";
	}

	public static void SetLanguage(string langISO)
	{
		langISO = Load().ValidateLanguage(langISO);
		if (GameState.saveState.Language != langISO)
		{
			GameState.saveState.Language = langISO;
			Localization.OnLanguageChanged?.Invoke();
		}
	}

	public static string GetLanguageIdFromSystem()
	{
		string twoLetterISOLanguageName = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
		LanguageDescriptor[] languages = Load()._languages;
		for (int i = 0; i < languages.Length; i++)
		{
			if (languages[i].id.Equals(twoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
			{
				return languages[i].id;
			}
			if (twoLetterISOLanguageName.StartsWith(languages[i].id, StringComparison.OrdinalIgnoreCase))
			{
				return languages[i].id;
			}
		}
		if (twoLetterISOLanguageName == "ru" || twoLetterISOLanguageName == "ua")
		{
			return "ru";
		}
		return "en";
	}

	public int IndexOf(string lang)
	{
		return languageMap.GetValueOrDefault(lang, -1);
	}

	public string ValidateLanguage(string lang)
	{
		if (string.IsNullOrWhiteSpace(lang))
		{
			return _defaultLanguage;
		}
		for (int i = 0; i < _languages.Length; i++)
		{
			if (_languages[i].id.Equals(lang, StringComparison.OrdinalIgnoreCase))
			{
				return lang;
			}
		}
		return _defaultLanguage;
	}

	public static ExcelElement FindElement(string id, string fallback = null)
	{
		for (int i = 0; i < instance._gameTextFiles.Length; i++)
		{
			ExcelElement excelElement = instance._gameTextFiles[i].FindElement(id);
			if ((bool)excelElement)
			{
				return excelElement;
			}
		}
		if (fallback != null)
		{
			return ExcelElement.Make(fallback);
		}
		return null;
	}

	public static string FindText(string id, int row = 0)
	{
		for (int i = 0; i < instance._gameTextFiles.Length; i++)
		{
			ExcelElement excelElement = instance._gameTextFiles[i].FindElement(id);
			if ((bool)excelElement)
			{
				return excelElement.ReadLocalized(row);
			}
		}
		return "MISSING";
	}
}
