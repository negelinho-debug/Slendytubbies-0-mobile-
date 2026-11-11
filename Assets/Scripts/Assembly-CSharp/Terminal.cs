using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class Terminal : MonoBehaviour
{
	private enum State
	{
		Printing,
		ReadingInput
	}

	[Flags]
	public enum LevelInfoFlags
	{
		IsRecord0 = 1,
		IsHidden = 2
	}

	[Serializable]
	public struct LevelInfo
	{
		public string sceneName;

		public string archiveName;

		public LevelInfoFlags flags;

		public GameCompletionFlags requiredFlag;

		public GameCompletionFlags completedFlag;

		public ExcelElement log;
	}

	[Serializable]
	public struct StartupSequenceInfo
	{
		public float startDelay;

		public float beginTypingDelay;

		public GameObject startSoundEmitter;

		public GameObject pcSoundEmitter;

		public GameObject menuMusicEmitter;

		public PostProcessVolume terminalEnableVolume;

		public float terminalEnableTime;
	}

	private enum ShowWindowFlags
	{
		MAXIMIZE = 3
	}

	private static List<char> lastTerminalChars = new List<char>();

	public static InitialState initialState;

	public StartupSequenceInfo _startupInfo;

	public TMP_InputField _inputField;

	public TMP_Text _commandHistory;

	public int _printSpeed;

	[Space(10f)]
	public ExcelElement _incomingMailText;

	public LevelInfo[] _levelInfos;

	public TerminalLocalization _termLocal;

	[Space(10f)]
	public EventReference _keyboardType;

	public EventReference _keyboardEnter;

	public EventReference _keyboardSpace;

	public EventReference _keyboardBackspace;

	[Space(10f)]
	public GameObject _endText;

	private List<char> terminalChars = new List<char>();

	private StringBuilder renderedChars = new StringBuilder();

	private State state;

	private float nextCharTime;

	private bool skipRequested;

	private float skipDelay;

	private int lastCaretPosition;

	private int typeProgress = -1;

	private string sendString;

	private List<int> allowedChars = new List<int>();

	private void Start()
	{
		_inputField.ActivateInputField();
		allowedChars = Enumerable.Range(33, 94).ToList();
		allowedChars.Remove(92);
		allowedChars.Remove(47);
		WhiteTurnOnTheLights.isRestart = false;
		nextCharTime = Time.unscaledTime;
		QualitySettings.vSyncCount = 1;
		TerminalMain().AttachExternalCancellation(base.gameObject.GetCancellationTokenOnDestroy()).Forget();
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
		initialState = InitialState.LoggedBoot;
		LevelSwitch.instance.SwitchLevel(SceneManager.GetActiveScene().name, LevelSwitch.SwitchType.NoiseNoIntro);
	}

	private void Update()
	{
		InputManager.Instance.SetLayout(InputLayoutLayer.Gameplay, PlayerInputLayout.KeyboardOnly);
		if (state == State.ReadingInput)
		{
			if (initialState == InitialState.Clawy)
			{
				if (InputManager.Instance.AnyTriggered())
				{
					typeProgress++;
					string text = "send";
					if (typeProgress < text.Length)
					{
						_inputField.text += text[typeProgress];
						RuntimeManager.PlayOneShot(_keyboardType);
					}
					else
					{
						_inputField.onEndEdit?.Invoke(_inputField.text);
						RuntimeManager.PlayOneShot(_keyboardEnter);
					}
				}
			}
			else if (InputManager.Instance.AnyTriggered())
			{
				if (InputManager.Instance.EnterTriggered())
				{
					RuntimeManager.PlayOneShot(_keyboardEnter);
				}
				else if (InputManager.Instance.SpaceTriggered())
				{
					RuntimeManager.PlayOneShot(_keyboardSpace);
				}
				else if (InputManager.Instance.BackspaceTriggered())
				{
					RuntimeManager.PlayOneShot(_keyboardBackspace);
				}
				else
				{
					RuntimeManager.PlayOneShot(_keyboardType);
				}
			}
		}
		else if (Time.time > skipDelay && InputManager.Instance.AnyTriggered())
		{
			skipRequested = true;
		}
	}

	private void LateUpdate()
	{
		if (state == State.ReadingInput)
		{
			_inputField.gameObject.SetActive(value: true);
			_inputField.ActivateInputField();
		}
		else
		{
			_inputField.gameObject.SetActive(value: false);
			_inputField.DeactivateInputField();
		}
		renderedChars.Clear();
		for (int i = 0; i < terminalChars.Count; i++)
		{
			char c = terminalChars[i];
			if (c == '\0')
			{
				int index = UnityEngine.Random.Range(0, allowedChars.Count);
				c = (char)allowedChars[index];
			}
			renderedChars.Append(c);
		}
		_commandHistory.SetText(renderedChars);
	}

	private void SetActiveSafe(GameObject go, bool is_active)
	{
		if ((bool)go)
		{
			go.SetActive(is_active);
		}
	}

	private async UniTask TerminalMain()
	{
		InitialState initialState = Terminal.initialState;
		if (initialState == InitialState.FirstBoot || initialState == InitialState.LoggedBoot)
		{
			if (Terminal.initialState == InitialState.FirstBoot)
			{
				await UniTask.WaitForSeconds(_startupInfo.startDelay);
			}
			SetActiveSafe(_startupInfo.startSoundEmitter, is_active: true);
			await UniTask.WaitForSeconds(0.2f);
			float tStartup = 0f;
			while (tStartup < 1f)
			{
				tStartup += Time.deltaTime * (1f / _startupInfo.terminalEnableTime);
				tStartup = Mathf.Clamp01(tStartup);
				_startupInfo.terminalEnableVolume.weight = tStartup;
				await UniTask.Yield();
			}
			SetActiveSafe(_startupInfo.pcSoundEmitter, is_active: true);
			await UniTask.WaitForSeconds(_startupInfo.beginTypingDelay);
			if (Terminal.initialState == InitialState.FirstBoot)
			{
				await PrintEl(_termLocal.term_OS);
				await SelectUser();
				GameState.saveState.Flags.Add(GameCompletionFlags.Intro);
				GameState.SaveGameToDisk();
				await Clear();
				await Print(_termLocal.term_Profile.ReadLocalized());
				await UniTask.WaitForSeconds(1f);
				await Print(".");
				await UniTask.WaitForSeconds(1f);
				await Print(".");
				await UniTask.WaitForSeconds(1f);
				await Print(".\n");
			}
			if (Terminal.initialState == InitialState.LoggedBoot)
			{
				await Println(_termLocal.term_Profile.ReadLocalized() + "...");
			}
			await PrintEl(_termLocal.term_Login);
			await UniTask.WaitForSeconds(1f);
			string text = _termLocal.term_Welcome.ReadLocalized();
			text = text.Replace("{0}", "adm63");
			await Println(text);
		}
		SetActiveSafe(_startupInfo.pcSoundEmitter, is_active: true);
		SetActiveSafe(_startupInfo.menuMusicEmitter, is_active: true);
		if ((bool)_startupInfo.terminalEnableVolume)
		{
			_startupInfo.terminalEnableVolume.weight = 1f;
		}
		if (Terminal.initialState == InitialState.Restore)
		{
			terminalChars = lastTerminalChars;
		}
		if ((GameState.saveState.Flags & GameCompletionFlags.MainGame) != GameCompletionFlags.MainGame)
		{
			await PrintStatus();
			await Println(_termLocal.term_Review.ReadLocalized(1));
		}
		else
		{
			await Println(_termLocal.term_Review_Comp.ReadLocalized());
			await Println(_termLocal.term_Review_Comp.ReadLocalized(1));
			string text2 = _termLocal.term_Review_Comp.ReadLocalized(2);
			text2 = text2.Replace("{0}", "\0");
			await Println(text2);
		}
		if (Terminal.initialState == InitialState.Clawy)
		{
			_inputField.enabled = false;
		}
		while ((bool)base.gameObject)
		{
			await OnFieldSubmit(await ReadLine());
		}
	}

	private async UniTask SelectUser()
	{
		await PrintEl(_termLocal.term_User);
		bool user_selected = false;
		while (!user_selected)
		{
			if (await ReadLine() == "adm63")
			{
				user_selected = true;
				continue;
			}
			await PrintEl(_termLocal.term_User_Error);
			string hintText = _incomingMailText.ReadLocalized().Replace("{0}", "Site #12");
			string path = "./morning_mail.txt";
			OpenTextFile(path, hintText);
		}
		await PrintEl(_termLocal.term_Pass);
		bool password_entered = false;
		while (!password_entered)
		{
			if (await ReadLine() == "lenny")
			{
				password_entered = true;
				continue;
			}
			await PrintEl(_termLocal.term_Pass_Error);
			string hintText2 = "lenny";
			string path2 = "./dont_forget.txt";
			OpenTextFile(path2, hintText2);
		}
	}

	private void OpenTextFile(string path, string hintText)
	{
		try
		{
			File.WriteAllText(path, hintText);
			Process process = Process.Start("notepad.exe", path);
			ShowWindow(process);
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	[DllImport("user32.dll")]
	internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

	[DllImport("user32.dll")]
	internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	private void ShowWindow(Process process)
	{
		IntPtr mainWindowHandle = process.MainWindowHandle;
		if (mainWindowHandle != IntPtr.Zero)
		{
			SetForegroundWindow(mainWindowHandle);
			ShowWindow(mainWindowHandle, 3);
		}
	}

	public async UniTask Println(string text)
	{
		await Print(text);
		await Print('\n');
	}

	public async UniTask Print(string text)
	{
		if (text != null)
		{
			for (int printedChars = 0; printedChars < text.Length; printedChars++)
			{
				await Print(text[printedChars]);
			}
		}
	}

	public async UniTask Print(char c)
	{
		terminalChars.Add(c);
		if (c == '\n')
		{
			skipRequested = false;
		}
		await AddCharsThisFrameCounter();
	}

	public async UniTask Clear()
	{
		for (int i = 0; i < terminalChars.Count; i++)
		{
			if (terminalChars[i] == '\n')
			{
				await AddCharsThisFrameCounter();
			}
			else
			{
				terminalChars[i] = ' ';
			}
		}
		terminalChars.Clear();
		await UniTask.WaitForSeconds(0.3f);
	}

	private async UniTask AddCharsThisFrameCounter()
	{
		if (_printSpeed > 0)
		{
			float num = 1f / (float)_printSpeed;
			if (Time.unscaledTime > nextCharTime + 1f)
			{
				nextCharTime = Time.unscaledTime;
			}
			nextCharTime += num;
			while (Time.unscaledTime < nextCharTime && !skipRequested)
			{
				await UniTask.Yield();
			}
			if (skipRequested)
			{
				nextCharTime = Time.unscaledTime;
			}
		}
	}

	private string AutoComplete(string input)
	{
		if (input == "")
		{
			return "";
		}
		bool flag = false;
		string text = input;
		int num = (from c in Regex.Replace(input, "[^0-9]", "")
			select (int)char.GetNumericValue(c)).ToArray().Length;
		if (num > 0 && num < 2)
		{
			text = text.Replace(" ", "");
			for (int num2 = 0; num2 < _levelInfos.Length; num2++)
			{
				LevelInfo levelInfo = _levelInfos[num2];
				if (text.Length > levelInfo.archiveName.Length)
				{
					continue;
				}
				for (int num3 = 0; num3 < input.Length; num3++)
				{
					if (num3 < text.Length - 1)
					{
						if (char.ToLowerInvariant(text[num3]) != char.ToLowerInvariant(levelInfo.archiveName[num3]))
						{
							text = input;
							break;
						}
						continue;
					}
					char num4 = text[num3];
					string archiveName = levelInfo.archiveName;
					if (num4 == archiveName[archiveName.Length - 1])
					{
						text = levelInfo.archiveName;
						flag = true;
						continue;
					}
					text = input;
					break;
				}
				if (flag)
				{
					break;
				}
			}
		}
		else
		{
			string[] array = new string[6] { "quit", "help", "status", "send", "export", "languages" };
			for (int num5 = 0; num5 < array.Length; num5++)
			{
				if (text.Length > array[num5].Length)
				{
					continue;
				}
				for (int num6 = 0; num6 < input.Length; num6++)
				{
					if (char.ToLowerInvariant(input[num6]) != char.ToLowerInvariant(array[num5][num6]))
					{
						text = input;
						break;
					}
					text = array[num5];
					if (num6 == input.Length - 1)
					{
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		return text;
	}

	private async UniTask OnFieldSubmit(string input)
	{
		try
		{
			string text = AutoComplete(input);
			input = text;
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		if (input == "")
		{
			return;
		}
		switch (input)
		{
		case "quit":
			Application.Quit();
			return;
		case "help":
			await PrintEl(_termLocal.term_Help);
			return;
		case "status":
			await PrintStatus();
			return;
		case "send":
		case "export":
			if (initialState == InitialState.Clawy)
			{
				await PrintEl(_termLocal.term_Complete);
				await UniTask.WaitForSeconds(1f);
				VideoManager.Instance.PlayVideo(Videos.GameEnding);
				_endText.SetActive(value: true);
				base.transform.parent.gameObject.SetActive(value: false);
			}
			else if ((GameState.saveState.Flags & GameCompletionFlags.MainGame) == GameCompletionFlags.MainGame)
			{
				string text2 = _termLocal.term_Send_Clawy.ReadLocalized();
				text2 = text2.Replace("{0}", "\0");
				text2 = text2.Replace("{1}", "\n" + _levelInfos[^1].archiveName + "\n");
				GameState.saveState.Flags.Add(GameCompletionFlags.Clawy);
				await Println(text2);
				await UniTask.WaitForSeconds(1f);
				await Println(_termLocal.term_Send_Clawy.ReadLocalized(1));
				await Println(_termLocal.term_Send_Clawy.ReadLocalized(2));
			}
			else
			{
				await PrintEl(_termLocal.term_Send);
			}
			return;
		case "languages":
		{
			for (int i = 0; i < Localization.Load()._languages.Length; i++)
			{
				await Println(Localization.Load()._languages[i].id + " - " + Localization.Load()._languages[i].name);
			}
			return;
		}
		case "site17":
			await Print(_termLocal.term_Loading.ReadLocalized());
			await UniTask.WaitForSeconds(1f);
			await Print(".");
			await UniTask.WaitForSeconds(1f);
			await Print(".\n");
			LevelSwitch.instance.SwitchLevel("GuardianCollection", LevelSwitch.SwitchType.Noise);
			await UniTask.WaitForSeconds(1f);
			return;
		}
		for (int i = 0; i < _levelInfos.Length; i++)
		{
			LevelInfo info = _levelInfos[i];
			if (string.Equals(info.archiveName, input.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				if (!GameState.saveState.Flags.Contains(info.requiredFlag))
				{
					break;
				}
				lastTerminalChars = new List<char>(terminalChars);
				await Print(_termLocal.term_Loading.ReadLocalized());
				await UniTask.WaitForSeconds(1f);
				await Print(".");
				await UniTask.WaitForSeconds(1f);
				await Print(".\n");
				if (info.log != null)
				{
					await PrintLog(info.log);
				}
				if ((_levelInfos[i].flags & LevelInfoFlags.IsRecord0) != 0)
				{
					await Println(_termLocal.term_Send_Error.ReadLocalized());
					await Println(_termLocal.term_Send_Error.ReadLocalized(1));
					await Print(_termLocal.term_Send_Error.ReadLocalized(2));
					await UniTask.WaitForSeconds(1f);
					await Print(".");
					await UniTask.WaitForSeconds(1f);
					await Print(".\n");
					await Println(_termLocal.term_Send_Error.ReadLocalized(3));
					await Println(_termLocal.term_Send_Error.ReadLocalized(4));
					await Println(_termLocal.term_Send_Error.ReadLocalized(5));
					await Print(_termLocal.term_Send_Error.ReadLocalized(2));
					await UniTask.WaitForSeconds(1f);
					await Print(".");
					await UniTask.WaitForSeconds(1f);
					await Print(".\n");
				}
				LevelSwitch.instance.SwitchLevel(info.sceneName, LevelSwitch.SwitchType.Noise);
				await UniTask.WaitForSeconds(1f);
			}
		}
		for (int i = 0; i < Localization.Load()._languages.Length; i++)
		{
			if (string.Equals(Localization.Load()._languages[i].id, input, StringComparison.OrdinalIgnoreCase))
			{
				Localization.SetLanguage(Localization.Load()._languages[i].id);
				GameState.SaveGameToDisk();
				await UniTask.WaitForSeconds(1f);
			}
		}
		string text3 = _termLocal.term_Error.ReadLocalized();
		text3 = text3.Replace("{0}", input);
		await Println(text3);
	}

	private async UniTask PrintStatus()
	{
		string text = _termLocal.term_Review.ReadLocalized();
		int num = 0;
		string text2 = "\n";
		for (int i = 0; i < _levelInfos.Length; i++)
		{
			if ((_levelInfos[i].flags & LevelInfoFlags.IsHidden) == 0)
			{
				if ((GameState.saveState.Flags & _levelInfos[i].completedFlag) != _levelInfos[i].completedFlag)
				{
					num++;
				}
				if (GameState.saveState.Flags.Contains(_levelInfos[i].requiredFlag))
				{
					bool flag = GameState.saveState.Flags.Contains(_levelInfos[i].completedFlag);
					text2 = text2 + _levelInfos[i].archiveName + (flag ? " [X]" : " [-]") + "\n";
				}
			}
		}
		text = text.Replace("{0}", num.ToString());
		text = text.Replace("{1}", text2);
		await Print(text);
	}

	public async UniTask PrintEl(ExcelElement element)
	{
		for (int i = 0; i < element.length; i++)
		{
			await Println(element.ReadLocalized(i));
		}
	}

	public async UniTask PrintLog(ExcelElement log)
	{
		_commandHistory.text = "";
		terminalChars.Clear();
		renderedChars.Clear();
		await UniTask.WaitForSeconds(1f);
		_commandHistory.alignment = TextAlignmentOptions.Center;
		for (int i = 0; i < log.length; i++)
		{
			await Println(log.ReadLocalized(i));
			while (!skipRequested)
			{
				await UniTask.Yield();
			}
			if (i < log.length)
			{
				_commandHistory.text = "";
				terminalChars.Clear();
				renderedChars.Clear();
			}
		}
	}

	public async UniTask<string> ReadLine()
	{
		state = State.ReadingInput;
		string input = null;
		while (string.IsNullOrWhiteSpace(input))
		{
			input = await _inputField.onEndEdit;
		}
		input = input.Trim();
		_inputField.text = "";
		await Println(input);
		input = input.ToLower();
		state = State.Printing;
		skipRequested = false;
		skipDelay = Time.time + 0.3f;
		return input;
	}
}
