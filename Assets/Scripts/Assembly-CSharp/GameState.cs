using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState
{
	public const string SITE0_NAME = "site17";

	public const string SITE0_FILE_NAME = "_";

	public static string savePath;

	public static GameSave saveState;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitGameState()
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Level Switch"));
		}
		savePath = GetSavePath();
		bool num = File.Exists(savePath);
		saveState = GameSave.CreateDefault();
		if (num)
		{
			LoadSaveFile();
		}
		else
		{
			SaveGameToDisk();
		}
		if (!saveState.Flags.Contains(GameCompletionFlags.Prologue))
		{
			SceneManager.LoadScene("Beginning");
		}
		WhiteTurnOnTheLights.isRestart = false;
		if (saveState.Flags.Contains(GameCompletionFlags.Intro))
		{
			Terminal.initialState = InitialState.LoggedBoot;
		}
		else
		{
			Terminal.initialState = InitialState.FirstBoot;
		}
		Localization.SetLanguage(saveState.Language);
	}

	private static string GetSavePath()
	{
		return Application.dataPath + "/save.json";
	}

	public static void LoadSaveFile()
	{
		if (!saveState)
		{
			saveState = GameSave.CreateDefault();
		}
		LoadObject(ref saveState, savePath);
	}

	public static void SaveGameToDisk()
	{
		SaveObject(saveState, savePath, pretty: false);
	}

	private static bool LoadObject<T>(ref T obj, string path) where T : ScriptableObject
	{
		if (!File.Exists(path))
		{
			return false;
		}
		try
		{
			string json = File.ReadAllText(path);
			T val = UnityEngine.Object.Instantiate(obj);
			JsonUtility.FromJsonOverwrite(json, val);
			obj = val;
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
	}

	private static bool SaveObject(ScriptableObject obj, string path, bool pretty)
	{
		if (!obj)
		{
			Debug.LogError("Trying to save null object to disk, aborting");
			return false;
		}
		string contents = JsonUtility.ToJson(UnityEngine.Object.Instantiate(obj), pretty);
		File.WriteAllText(path, contents);
		return true;
	}
}
