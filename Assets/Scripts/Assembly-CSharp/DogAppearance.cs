using UnityEngine;

public class DogAppearance : MonoBehaviour
{
	public Material[] _faceMaterials;

	public float _faceSwapDelay = 0.15f;

	public int _facesCount = 3;

	public float _fadeInDuration = 0.3f;

	private float faceTimer;

	private int faceIndex;

	private float fadeTarget;

	private float fadeStart;

	private float fadeDuration;

	private float fadeTime;

	public bool isFading { get; private set; }

	public Renderer WDrenderer { get; private set; }

	public Material[] WDmaterials { get; private set; }

	private void Awake()
	{
		WDrenderer = GetComponentInChildren<Renderer>();
		WDmaterials = WDrenderer.materials;
		ChangeAppearance();
		FadeIn(_fadeInDuration);
	}

	private void Update()
	{
		if (faceIndex < _facesCount)
		{
			faceTimer -= Time.deltaTime;
			if (faceTimer <= 0f)
			{
				WDmaterials[0].SetTextureOffset("_MainTex", new Vector2((float)faceIndex / (float)_facesCount, 0f));
				WDmaterials[1].SetTextureOffset("_MainTex", new Vector2((float)faceIndex / (float)_facesCount, 0f));
				WDrenderer.materials = WDmaterials;
				faceTimer = _faceSwapDelay;
				faceIndex++;
			}
		}
		if (!isFading)
		{
			return;
		}
		if (fadeTime < fadeDuration)
		{
			fadeTime += Time.deltaTime;
			float value = Mathf.Lerp(fadeStart, fadeTarget, fadeTime / fadeDuration);
			for (int i = 0; i < WDmaterials.Length; i++)
			{
				WDmaterials[i].SetFloat("_Transparency", value);
				WDrenderer.materials = WDmaterials;
			}
		}
		else
		{
			isFading = false;
		}
	}

	public void ChangeAppearance()
	{
		faceIndex = 0;
		faceTimer = _faceSwapDelay;
		for (int i = 0; i < WDmaterials.Length; i++)
		{
			if (i < WDmaterials.Length - 1)
			{
				int num = Random.Range(0, _faceMaterials.Length);
				WDmaterials[i] = new Material(_faceMaterials[num]);
				WDmaterials[i].SetTextureOffset("_MainTex", Vector2.zero);
			}
		}
		WDrenderer.materials = WDmaterials;
	}

	public void FadeIn(float duration)
	{
		isFading = true;
		fadeTarget = 1f;
		fadeStart = 0f;
		fadeDuration = duration;
		fadeTime = 0f;
	}

	public void FadeOut(float duration)
	{
		isFading = true;
		fadeTarget = 0f;
		fadeStart = 1f;
		fadeDuration = duration;
		fadeTime = 0f;
	}
}
