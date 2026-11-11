using System.Collections.Generic;
using UnityEngine;

public class GameSave : ScriptableObject
{
	public List<string> CompletionState;

	public GameCompletionFlags Flags;

	public string Language;

	public static GameSave CreateDefault()
	{
		GameSave gameSave = ScriptableObject.CreateInstance<GameSave>();
		gameSave.CompletionState = new List<string>();
		gameSave.Language = Localization.GetLanguageIdFromSystem();
		return gameSave;
	}
}
