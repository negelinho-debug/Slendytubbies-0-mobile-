using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class LevelSwitch : MonoBehaviour
{
	public enum SwitchType
	{
		Noise,
		NoiseNoIntro,
		BlackScreen
	}

	private LevelTransitionEffect effect;

	public static LevelSwitch instance;

	public EventReference _loadingSound;

	public PostProcessVolume volume;

	public float offsetBeginTime;

	public float offsetBeginAmount;

	[AnimationCurveLimit]
	public AnimationCurve offsetBeginCurve = new AnimationCurve();

	[Space]
	public float minNoiseDuration;

	[Space]
	public float offsetEndTime;

	public float offsetEndAmount;

	[AnimationCurveLimit]
	public AnimationCurve offsetEndCurve = new AnimationCurve();

	private void Awake()
	{
		if ((bool)instance && instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void SwitchToMenu(SwitchType type, InitialState terminal = InitialState.Restore)
	{
		Terminal.initialState = terminal;
		StartCoroutine(SwitchLevelRoutine("Menu", type));
	}

	public void SwitchLevel(string levelName, SwitchType type)
	{
		StartCoroutine(SwitchLevelRoutine(levelName, type));
	}

	private IEnumerator SwitchLevelRoutine(string level, SwitchType type)
	{
		effect = volume.profile.GetSetting<LevelTransitionEffect>();
		volume.enabled = true;
		Bus bus = RuntimeManager.GetBus("bus:/Sound");
		if (bus.isValid())
		{
			bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		else
		{
			Debug.LogError("Could not get sound bus");
		}
		EventInstance loadingSoundInstance = RuntimeManager.CreateInstance(_loadingSound);
		loadingSoundInstance.start();
		float t;
		if (type != SwitchType.NoiseNoIntro)
		{
			t = 0f;
			while (t < 1f)
			{
				t += Time.deltaTime * (1f / offsetBeginTime);
				float num = offsetBeginCurve.Evaluate(t);
				effect._offset.value = new Vector2(0f, offsetBeginAmount * num);
				yield return null;
			}
			effect._offset.value = new Vector2(0f, offsetBeginAmount);
			yield return null;
		}
		SceneManager.LoadScene("Dummy_Scene", LoadSceneMode.Single);
		yield return null;
		effect._noiseStrength.value = 1f;
		AsyncOperation op = SceneManager.LoadSceneAsync(level);
		op.allowSceneActivation = false;
		float tNoise = 0f;
		while (tNoise < minNoiseDuration)
		{
			tNoise += Time.deltaTime;
			yield return null;
		}
		op.allowSceneActivation = true;
		yield return op;
		yield return new WaitForSeconds(0.1f);
		if (loadingSoundInstance.isValid())
		{
			loadingSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			loadingSoundInstance.release();
		}
		TransitionVolume.Instance.volume.weight = 0f;
		effect._noiseStrength.value = 0f;
		t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * (1f / offsetEndTime);
			float num2 = 1f - offsetEndCurve.Evaluate(t);
			effect._offset.value = new Vector2(0f, offsetEndAmount * num2);
			yield return null;
		}
		volume.enabled = false;
	}
}
