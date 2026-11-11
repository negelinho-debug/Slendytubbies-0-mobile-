using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class COATTerminal : MonoBehaviour
{
	private enum State
	{
		Printing,
		ReadingInput
	}

	private enum ShowWindowFlags
	{
		MAXIMIZE = 3
	}

	private static TerminalState lastTerminalState;

	public static TerminalInitialState initialState;

	private TerminalUser currentUser;

	private State state;

	private bool isInitialized;

	private int charactersPrintedThisFrame;

	private int lastPrintFrame;

	private int terminalWidth;

	private int terminalHeight;

	private int cursorX;

	private int cursorY;

	private char[] terminalChars;

	private StringBuilder renderedChars;

	private int inputCursorX;

	private int inputCursorY;

	private int inputLength;

	private char[] inputChars;

	private int printSpeed;

	public LevelList _levelList;

	[Space]
	public ExcelElement _incomingMailText;

	public ExcelElement _caseNameText;

	[Space]
	public COATTerminalInput _input;

	[Tooltip("In characters per second")]
	public int _printingSpeed = 60;

	public float _monospaceWidth = 10f;

	public float _monospaceHeight = 10f;

	public TMP_Text _terminalText;

	public TMP_Text _commandText;

	public Image _cursor;

	public float _cursorBlinkInterval;

	public Camera _camera;

	public Color _cameraBackground;

	public Image _commandImage;

	public float _commandImageHeight;

	private float cursorBlink;

	private static readonly TerminalUser[] users = new TerminalUser[1]
	{
		new TerminalUser("adm63", "lenny")
	};

	private IEnumerator Start()
	{
		WhiteTurnOnTheLights.isRestart = false;
		_terminalText.SetText("");
		_commandText.SetText("");
		Vector2 r = _commandImage.rectTransform.sizeDelta;
		r.y = 0f;
		if (initialState == TerminalInitialState.FirstBoot)
		{
			currentUser.username = "";
		}
		else
		{
			currentUser.username = "adm63";
		}
		_commandImage.rectTransform.sizeDelta = r;
		yield return null;
		if (initialState != TerminalInitialState.Restore)
		{
			yield return new WaitForSeconds(2f);
			Color startColor = _camera.backgroundColor;
			float tCam = 0f;
			while (tCam < 1f)
			{
				_camera.backgroundColor = Color.Lerp(startColor, _cameraBackground, tCam);
				tCam += Time.deltaTime;
				yield return null;
			}
			yield return new WaitForSeconds(1f);
			float tRec = 0f;
			while (tRec < 1f)
			{
				r.y = tRec * _commandImageHeight;
				_commandImage.rectTransform.sizeDelta = r;
				tRec += Time.deltaTime;
				yield return null;
			}
			yield return new WaitForSeconds(1f);
		}
		_camera.backgroundColor = _cameraBackground;
		r.y = _commandImageHeight;
		_commandImage.rectTransform.sizeDelta = r;
		inputLength = 0;
		inputChars = new char[32];
		cursorX = 0;
		cursorY = 0;
		charactersPrintedThisFrame = 0;
		printSpeed = _printingSpeed;
		float width = _terminalText.rectTransform.rect.width;
		float height = _terminalText.rectTransform.rect.height;
		terminalWidth = (int)(width / _monospaceWidth);
		terminalHeight = (int)(height / _monospaceHeight);
		UnityEngine.Debug.Log($"TERMINAL INIT -- W: {terminalWidth}, H: {terminalHeight}");
		renderedChars = new StringBuilder(terminalWidth * terminalHeight);
		terminalChars = new char[terminalWidth * terminalHeight];
		int num = 0;
		for (int i = 0; i < terminalHeight; i++)
		{
			for (int j = 0; j < terminalWidth; j++)
			{
				terminalChars[num++] = ' ';
			}
		}
		_terminalText.SetText(terminalChars);
		EventSystem.current.SetSelectedGameObject(_input.gameObject);
		isInitialized = true;
		TerminalMain().AttachExternalCancellation(base.gameObject.GetCancellationTokenOnDestroy()).Forget();
	}

	private void OnEnable()
	{
		_input.onTerminalInput += OnTerminalInput;
	}

	private void OnDisable()
	{
		_input.onTerminalInput -= OnTerminalInput;
	}

	private void LateUpdate()
	{
		InputManager.Instance.SetLayout(InputLayoutLayer.UI, PlayerInputLayout.KeyboardOnly);
		EventSystem.current.SetSelectedGameObject(_input.gameObject);
		if (state == State.ReadingInput)
		{
			cursorBlink += Time.deltaTime;
			if (cursorBlink > _cursorBlinkInterval)
			{
				cursorBlink = 0f;
				_cursor.enabled = !_cursor.enabled;
			}
			_cursor.rectTransform.anchoredPosition = new Vector2(_monospaceWidth * (float)(inputCursorX + inputLength), _monospaceHeight * (float)(-inputCursorY));
		}
		else
		{
			_cursor.enabled = false;
		}
		if (!isInitialized)
		{
			return;
		}
		renderedChars.Clear();
		for (int i = 0; i < terminalChars.Length; i++)
		{
			char c = terminalChars[i];
			if (c == '\0')
			{
				c = (char)UnityEngine.Random.Range(33, 126);
			}
			renderedChars.Append(c);
		}
		_terminalText.SetText(renderedChars);
	}

	private async UniTask TerminalMain()
	{
		TerminalInitialState terminalInitialState = initialState;
		if (terminalInitialState == TerminalInitialState.FirstBoot || terminalInitialState == TerminalInitialState.LoggedBoot)
		{
			if (initialState == TerminalInitialState.FirstBoot)
			{
				await Println("C.O.A.T. Archive Mainframe Terminal. OS Ver 2.9.");
				await SelectUser();
				GameState.saveState.Flags.Add(GameCompletionFlags.Intro);
				GameState.SaveGameToDisk();
				await Clear();
				await Print("Loading profile data");
				await UniTask.WaitForSeconds(1f);
				await Print(".");
				await UniTask.WaitForSeconds(1f);
				await Print(".");
				await UniTask.WaitForSeconds(1f);
				await Print(".\n");
			}
			if (initialState == TerminalInitialState.LoggedBoot)
			{
				await Print("Loading profile data...\n");
			}
			await Println("Login complete.\n");
			await UniTask.WaitForSeconds(1f);
			await Println("Welcome to the C.O.A.T. Archiving System, " + currentUser.username + ".\n");
			await Println("Use 'help' command to receive tips on navigating the terminal.\n");
			await Println("You have [6] unreviewed archive files.");
		}
		if (initialState == TerminalInitialState.Restore)
		{
			RestoreTerminalState();
		}
		while ((bool)base.gameObject)
		{
			await ExecuteCommand(await ReadLineFromUser());
		}
	}

	private void RestoreTerminalState()
	{
		if (lastTerminalState.terminalChars != null && lastTerminalState.terminalChars.Length == terminalChars.Length)
		{
			cursorX = lastTerminalState.cursorX;
			cursorY = lastTerminalState.cursorY;
			Array.Copy(lastTerminalState.terminalChars, terminalChars, terminalChars.Length);
			currentUser = lastTerminalState.user;
			NewLine();
		}
	}

	private void SaveTerminalState()
	{
		lastTerminalState.terminalChars = new char[terminalChars.Length];
		Array.Copy(terminalChars, lastTerminalState.terminalChars, terminalChars.Length);
		lastTerminalState.cursorX = cursorX;
		lastTerminalState.cursorY = cursorY;
		lastTerminalState.user = currentUser;
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

	private async UniTask SelectUser()
	{
		await Println("Please enter your username:");
		bool user_selected = false;
		bool hint1_opened = false;
		while (!user_selected)
		{
			string text = (await ReadLineFromUser()).ToLower();
			int num = -1;
			for (int i = 0; i < users.Length; i++)
			{
				if (users[i].username == text)
				{
					num = i;
					break;
				}
			}
			if (num >= 0)
			{
				currentUser = users[num];
				user_selected = true;
				continue;
			}
			await Println("Username not found. Try again using correct name or contact system administrator.");
			if (!hint1_opened)
			{
				string text2 = _incomingMailText.ReadLocalized();
				text2 = text2.Replace("{0}", _caseNameText.ReadLocalized());
				string text3 = Application.dataPath + "/morning_mail.txt";
				File.WriteAllText(text3, text2);
				Process process = Process.Start("notepad.exe", text3);
				ShowWindow(process);
				hint1_opened = true;
			}
		}
		await Println("Enter your password:");
		bool hint2_opened = false;
		bool password_entered = false;
		while (!password_entered)
		{
			if ((await ReadLineFromUser()).ToLower() == currentUser.password)
			{
				password_entered = true;
				continue;
			}
			await Println("Wrong password.");
			if (!hint2_opened)
			{
				string text4 = Application.dataPath + "/dont_forget.txt";
				File.WriteAllText(text4, "lenny");
				Process process2 = Process.Start("notepad.exe", text4);
				ShowWindow(process2);
				hint2_opened = true;
			}
		}
	}

	public async UniTask ExecuteCommand(string command)
	{
		if (string.IsNullOrWhiteSpace(command))
		{
			return;
		}
		string[] words = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < words.Length; i++)
		{
			words[i] = words[i].ToLower();
		}
		switch (words[0])
		{
		case "quit":
			Application.Quit();
			return;
		case "help":
			await Println("Terminal supports following commands:");
			await Println("- status - display list of pending documents for review and their review status.");
			await Println("- send - sends reviewed documents to the archive server.");
			await Println("- open [NAME] - opens selected archive.");
			await Println("- lang [LANG] - changes terminal language.");
			return;
		case "status":
		{
			for (int j = 0; j < _levelList._levelInfos.Length; j++)
			{
				LevelList.LevelInfo levelInfo = _levelList._levelInfos[j];
				if (levelInfo.displayByDefault)
				{
					await Println("- " + levelInfo.archiveName);
				}
			}
			return;
		}
		case "send":
			await Println("Not all archives were properly reviewed.");
			await Println("Use 'status' command to check status of each pending archive.");
			return;
		case "open":
		{
			if (words.Length < 2)
			{
				await Println("Command requires a file name.");
				return;
			}
			string lvl = words[1];
			for (int j = 0; j < _levelList._levelInfos.Length; j++)
			{
				LevelList.LevelInfo info = _levelList._levelInfos[j];
				if (string.Equals(info.archiveName, lvl, StringComparison.OrdinalIgnoreCase))
				{
					await Print("Loading");
					await UniTask.WaitForSeconds(1f);
					await Print(".");
					await UniTask.WaitForSeconds(1f);
					await Print(".");
					SaveTerminalState();
					LevelSwitch.instance.SwitchLevel(info.sceneName, LevelSwitch.SwitchType.Noise);
					await UniTask.WaitForSeconds(1f);
				}
			}
			return;
		}
		}
		string lvl2 = words[0];
		for (int j = 0; j < _levelList._levelInfos.Length; j++)
		{
			LevelList.LevelInfo info = _levelList._levelInfos[j];
			if (string.Equals(info.archiveName, lvl2, StringComparison.OrdinalIgnoreCase))
			{
				await Print("Loading");
				await UniTask.WaitForSeconds(1f);
				await Print(".");
				await UniTask.WaitForSeconds(1f);
				await Print(".");
				SaveTerminalState();
				LevelSwitch.instance.SwitchLevel(info.sceneName, LevelSwitch.SwitchType.Noise);
				await UniTask.WaitForSeconds(1f);
			}
		}
		await Println("Unrecognized command '" + words[0] + "'. Use 'help' to get list of available commands");
	}

	public async UniTask Println(string text)
	{
		await Print(text);
		await Print('\n');
	}

	public async UniTask Clear()
	{
		for (int y = 0; y < terminalHeight; y++)
		{
			for (int i = 0; i < terminalWidth; i++)
			{
				Put(i, y, ' ');
			}
			await AddCharsThisFrameCounter();
		}
		cursorX = 0;
		cursorY = 0;
	}

	private async UniTask AddCharsThisFrameCounter()
	{
		charactersPrintedThisFrame++;
		int num = (int)(Time.deltaTime * (float)printSpeed);
		if (charactersPrintedThisFrame > num)
		{
			await UniTask.Yield();
			charactersPrintedThisFrame = 0;
		}
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

	public async UniTask<string> ReadLineFromUser()
	{
		await Print("\n>");
		await Print(currentUser.username);
		await Print(':');
		string result = await ReadLine();
		NewLine();
		return result;
	}

	public async UniTask<string> ReadLine()
	{
		inputCursorX = cursorX;
		inputCursorY = cursorY;
		inputLength = 0;
		state = State.ReadingInput;
		while (state == State.ReadingInput)
		{
			for (int i = 0; i < inputLength; i++)
			{
				Put(inputCursorX + i, inputCursorY, inputChars[i]);
			}
			await UniTask.Yield();
		}
		NewLine();
		return new string(inputChars, 0, inputLength);
	}

	public async UniTask Print(char c)
	{
		if (c == '\n')
		{
			NewLine();
			return;
		}
		Put(cursorX, cursorY, c);
		MoveCursorForward();
		await AddCharsThisFrameCounter();
	}

	private void MoveCursorForward()
	{
		cursorX++;
		if (cursorX >= terminalWidth)
		{
			cursorX = 0;
			cursorY++;
			if (cursorY >= terminalHeight)
			{
				cursorY = terminalHeight - 1;
				MoveUp();
			}
		}
	}

	private void NewLine()
	{
		cursorX = 0;
		cursorY++;
		if (cursorY >= terminalHeight)
		{
			cursorY = terminalHeight - 1;
			MoveUp();
		}
	}

	private void MoveUp()
	{
		for (int i = 1; i < terminalHeight; i++)
		{
			for (int j = 0; j < terminalWidth; j++)
			{
				char c = Get(j, i);
				Put(j, i - 1, c);
			}
		}
		for (int k = 0; k < terminalWidth; k++)
		{
			int y = terminalHeight - 1;
			Put(k, y, ' ');
		}
	}

	private char Get(int x, int y)
	{
		int num = x + y * terminalWidth;
		return terminalChars[num];
	}

	private void Put(int x, int y, char c)
	{
		int num = x + y * terminalWidth;
		terminalChars[num] = c;
	}

	private bool IsValidChar(char c)
	{
		if (c >= ' ')
		{
			return c <= '~';
		}
		return false;
	}

	private void OnTerminalInput(Event uiEvent, BaseEventData eventData)
	{
		if (uiEvent.type != EventType.KeyDown)
		{
			return;
		}
		switch (uiEvent.keyCode)
		{
		case KeyCode.Return:
			if (state == State.Printing)
			{
				printSpeed = 9999;
				break;
			}
			printSpeed = _printingSpeed;
			state = State.Printing;
			break;
		case KeyCode.Backspace:
			if (inputLength > 0)
			{
				inputLength--;
				Put(inputCursorX + inputLength, inputCursorY, ' ');
			}
			break;
		}
		if (state == State.ReadingInput && inputLength < inputChars.Length && IsValidChar(uiEvent.character))
		{
			inputChars[inputLength++] = uiEvent.character;
		}
	}
}
