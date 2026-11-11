using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuText : MonoBehaviour
{
	public static MenuText Instance;

	public TMP_Text _TMPText;

	[Space(10f)]
	public TMP_FontAsset _YDKJFont;

	public string[] _YDKJLanguages;

	public TMP_FontAsset _TypewriterFont;

	private Element currentElement;

	private TMP_TextInfo textInfo;

	private float charTime;

	private int paragraphIndex = -1;

	public static char[] comma = new char[1] { ',' };

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			Instance = this;
		}
		_TMPText.text = "";
		if (currentElement.flags.Contains(TextFlags.WaitInput))
		{
			InputManager.Instance.SetLayout(InputLayoutLayer.Interaction, PlayerInputLayout.GameplayWithMouse);
		}
		bool flag = false;
		for (int i = 0; i < _YDKJLanguages.Length; i++)
		{
			if (GameState.saveState.Language == _YDKJLanguages[i])
			{
				flag = true;
				break;
			}
		}
		_TMPText.font = (flag ? _YDKJFont : _TypewriterFont);
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
		if (paragraphIndex >= 0)
		{
			currentElement.excelElement.ReadLocalized(paragraphIndex);
		}
	}

	private void LateUpdate()
	{
		if (paragraphIndex < 0)
		{
			return;
		}
		charTime += Time.smoothDeltaTime;
		_TMPText.ForceMeshUpdate();
		textInfo = _TMPText.textInfo;
		int characterCount = textInfo.characterCount;
		GlitchText();
		for (int i = 0; i < textInfo.meshInfo.Length; i++)
		{
			TMP_MeshInfo tMP_MeshInfo = textInfo.meshInfo[i];
			tMP_MeshInfo.mesh.vertices = tMP_MeshInfo.vertices;
			_TMPText.UpdateGeometry(tMP_MeshInfo.mesh, i);
		}
		bool flag = _TMPText.maxVisibleCharacters >= characterCount;
		while (!flag && charTime >= 0f && charTime >= currentElement.typeTime)
		{
			charTime -= currentElement.typeTime;
			_TMPText.maxVisibleCharacters++;
			if (_TMPText.maxVisibleCharacters >= characterCount)
			{
				break;
			}
		}
		if (!flag && currentElement.flags.Contains(TextFlags.WaitInput) && (InputManager.Instance.SpaceTriggered() || InputManager.Instance.LMBTriggered()))
		{
			_TMPText.maxVisibleCharacters = characterCount;
		}
		if (!flag)
		{
			return;
		}
		if (currentElement.flags.Contains(TextFlags.WaitInput))
		{
			if (InputManager.Instance.SpaceTriggered() || InputManager.Instance.LMBTriggered())
			{
				ProgressText();
			}
		}
		else if (charTime > currentElement.timeAdd)
		{
			ProgressText();
		}
	}

	private void GlitchText()
	{
		if (currentElement.intensity == 0f)
		{
			return;
		}
		int characterCount = textInfo.characterCount;
		float num = Time.time * currentElement.frequency;
		for (int i = 0; i < characterCount; i++)
		{
			TMP_CharacterInfo tMP_CharacterInfo = textInfo.characterInfo[i];
			if (tMP_CharacterInfo.isVisible)
			{
				int vertexIndex = tMP_CharacterInfo.vertexIndex;
				int materialReferenceIndex = tMP_CharacterInfo.materialReferenceIndex;
				Vector3[] vertices = textInfo.meshInfo[materialReferenceIndex].vertices;
				float x = (float)i * 17.123f + num;
				float x2 = (Mathf.PerlinNoise(x, 0.1f) - 0.5f) * currentElement.intensity;
				float y = (Mathf.PerlinNoise(x, 1.3f) - 0.5f) * currentElement.intensity;
				Vector2 vector = new Vector2(x2, y);
				if (UnityEngine.Random.value < currentElement.glitchChance * Time.deltaTime * 60f)
				{
					vector *= UnityEngine.Random.Range(2f, 4f);
				}
				vertices[vertexIndex] += (Vector3)vector;
				vertices[vertexIndex + 1] += (Vector3)vector;
				vertices[vertexIndex + 2] += (Vector3)vector;
				vertices[vertexIndex + 3] += (Vector3)vector;
			}
		}
		for (int j = 0; j < textInfo.meshInfo.Length; j++)
		{
			TMP_MeshInfo tMP_MeshInfo = textInfo.meshInfo[j];
			tMP_MeshInfo.mesh.vertices = tMP_MeshInfo.vertices;
			_TMPText.UpdateGeometry(tMP_MeshInfo.mesh, j);
		}
	}

	private void ProgressText()
	{
		charTime = 0f;
		paragraphIndex++;
		_TMPText.maxVisibleCharacters = 0;
		if (currentElement.excelElement == null || paragraphIndex >= currentElement.excelElement.length)
		{
			paragraphIndex = -1;
			if (currentElement.excelElement != null && currentElement.flags.Contains(TextFlags.EventOnComplete))
			{
				currentElement.onComplete.Invoke();
			}
		}
		else
		{
			SetText();
		}
	}

	public void StopText()
	{
		_TMPText.text = "";
		paragraphIndex = -1;
	}

	private void SetText()
	{
		if (currentElement.timeAdd == 0f)
		{
			currentElement.timeAdd = ParseOrDefault(currentElement.excelElement?.Read("#TIME_ADD", paragraphIndex), 2.5f);
		}
		if (currentElement.timeLimit == 0f)
		{
			currentElement.timeLimit = ParseOrDefault(currentElement.excelElement?.Read("#TIME", paragraphIndex), 0f);
		}
		if (currentElement.charSpeed == 0f)
		{
			currentElement.charSpeed = ParseOrDefault(currentElement.excelElement?.Read("#CHAR_SPEED", paragraphIndex), 25f);
		}
		if (currentElement.excelElement != null)
		{
			currentElement.text = currentElement.excelElement?.ReadLocalized(paragraphIndex);
		}
		float num = 1f / currentElement.charSpeed;
		if (currentElement.timeLimit > 0f)
		{
			float num2 = Mathf.Min(1f, currentElement.timeLimit / num);
			currentElement.timeAdd *= num2;
			currentElement.typeTime *= num2;
		}
		else
		{
			currentElement.typeTime = num;
		}
		_TMPText.text = currentElement.text;
		_TMPText.maxVisibleCharacters = 0;
	}

	public void StartText(Element element)
	{
		StopText();
		charTime = 0f;
		paragraphIndex = 0;
		currentElement = element;
		SetText();
	}

	public void StartText(ExcelElement element)
	{
		Element element2 = new Element
		{
			excelElement = element
		};
		StartText(element2);
	}

	public static float ParseOrDefault(ReadOnlySpan<char> chars, float @default)
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

	public void ChangeScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}
}
