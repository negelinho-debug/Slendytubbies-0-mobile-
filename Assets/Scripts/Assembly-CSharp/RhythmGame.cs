using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RhythmGame : MonoBehaviour
{
	[Header("Animations")]
	public Animator _dipsyAnimator;

	public Animator _viewAnimator;

	public string _isChokingBoolName = "IsChoking";

	public string _progressParameterName = "Progress";

	public EventReference _chokeStartReference;

	public EventReference _chokingReference;

	public EventReference _hitReference;

	public StudioEventEmitter _musicEmitter;

	[Header("Tap Reaction")]
	public GameObject _trigger;

	public Image _romkaIFuckingHateYou;

	public float _tapSize;

	public float _effectDuration = 1f;

	[Space(10f)]
	public RectTransform _hitCheck;

	public float _checkDuration = 0.5f;

	public float _checkStartSize = 150f;

	public float _checkEndSize = 300f;

	public float _impulseForce = 1f;

	[Range(0.01f, 1f)]
	public float _checkAlpha = 0.3f;

	public Color _hitSuccess;

	public Color _hitFailure;

	[Header("Notes")]
	public GameObject _notePrefab;

	public Color _startColor;

	public Color _endColor;

	public float _scaleStartDuration = 0.7f;

	public float _scaleMiddleDuration = 0.5f;

	public float _scaleEndDuration = 1.5f;

	public float _endSize;

	[Space(10f)]
	public Vector2 _sizeLimits;

	public int _notesForCompletion;

	public UnityEvent _onCompletion;

	private bool isChoking;

	private bool isBlocked;

	private RectTransform triggerRect;

	private Vector2 triggerStartSize;

	private float triggerEffectTime;

	private float triggerEffectDuration;

	private float checkEffectTime;

	private float checkEffectDuration;

	private Image checkImage;

	private Color checkColor;

	private List<RectTransform> notes = new List<RectTransform>();

	private float spawnTimer;

	private float iFuckingHateROMKA;

	private int notesHit;

	private EventInstance chokingInstance;

	private EventInstance musicInstance;

	private CinemachineImpulseSource impulseSource;

	private void Start()
	{
		impulseSource = GetComponent<CinemachineImpulseSource>();
		triggerRect = _trigger.GetComponent<RectTransform>();
		triggerStartSize = triggerRect.sizeDelta;
		checkImage = _trigger.GetComponent<Image>();
		Player.Instance.Brain.DefaultBlend.Time = 0f;
		chokingInstance = RuntimeManager.CreateInstance(_chokingReference);
		_musicEmitter.gameObject.SetActive(value: true);
		_musicEmitter.EventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
	}

	private void OnDestroy()
	{
		chokingInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		chokingInstance.release();
	}

	private void Update()
	{
		UpdateTrigger();
		UpdateCheck();
		if (iFuckingHateROMKA > 0f)
		{
			iFuckingHateROMKA = Mathf.Max(0f, iFuckingHateROMKA - Time.deltaTime * 2f);
			Color color = Color.Lerp(_hitFailure, _hitSuccess, iFuckingHateROMKA);
			checkImage.color = color;
			color = _hitSuccess / 2f;
			color.a = iFuckingHateROMKA * iFuckingHateROMKA / 2f;
			_romkaIFuckingHateYou.color = color;
		}
		if (!isChoking)
		{
			if (InputManager.Instance.LMBTriggered() || InputManager.Instance.SpaceTriggered())
			{
				isChoking = true;
				_dipsyAnimator.SetBool(_isChokingBoolName, value: true);
				_viewAnimator.SetBool(_isChokingBoolName, value: true);
				_musicEmitter.EventInstance.setParameterByName("Track", 1f);
				_musicEmitter.EventInstance.start();
				RuntimeManager.PlayOneShot(_chokeStartReference);
				chokingInstance.start();
				TapTrigger(0f);
			}
			return;
		}
		spawnTimer -= Time.deltaTime;
		if (spawnTimer <= 0f)
		{
			spawnTimer = lerp3(_scaleStartDuration, _scaleMiddleDuration, _scaleEndDuration, Easing.easeInCubic((float)notesHit / (float)_notesForCompletion));
			GameObject gameObject = Object.Instantiate(_notePrefab, _trigger.transform.parent, worldPositionStays: false);
			gameObject.transform.SetAsFirstSibling();
			StartCoroutine(NoteScale(gameObject));
		}
		if ((InputManager.Instance.LMBTriggered() || InputManager.Instance.SpaceTriggered()) && !isBlocked)
		{
			isBlocked = true;
			foreach (RectTransform note in notes)
			{
				if (note.sizeDelta.magnitude > new Vector2(_sizeLimits.x, _sizeLimits.x).magnitude && note.sizeDelta.magnitude < new Vector2(_sizeLimits.y, _sizeLimits.y).magnitude)
				{
					notesHit++;
					RuntimeManager.PlayOneShot(_hitReference);
					if (notesHit >= _notesForCompletion)
					{
						chokingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
						InputManager.Instance.SetLayout(InputLayoutLayer.Gameplay, PlayerInputLayout.Gameplay);
						StartCoroutine(ScreenFade.Instance.HoldBlackScreen(1f, CompleteGame));
					}
					isBlocked = false;
					chokingInstance.setParameterByName("Progress", (float)notesHit / (float)_notesForCompletion);
					impulseSource.GenerateImpulse(_impulseForce);
					iFuckingHateROMKA = 1f;
				}
			}
			TapTrigger(_checkDuration);
		}
		_dipsyAnimator.SetFloat(_progressParameterName, Mathf.Clamp01((float)notesHit / (float)_notesForCompletion));
	}

	private void CompleteGame()
	{
		_onCompletion.Invoke();
		ScreenFade.Instance.StartFade(1f, 0f, 1.3f);
		Player.Instance.FreezePlayer(isFrozen: false);
		base.gameObject.SetActive(value: false);
	}

	private void TapTrigger(float checkDuration)
	{
		triggerEffectTime = 0f;
		triggerEffectDuration = _effectDuration;
		checkEffectTime = 0f;
		checkEffectDuration = checkDuration;
		_hitCheck.sizeDelta = Vector2.one * _checkStartSize;
	}

	private void UpdateTrigger()
	{
		if (triggerEffectTime <= triggerEffectDuration)
		{
			triggerEffectTime += Time.deltaTime;
			float t = Easing.easeOutBack(Mathf.Clamp01(triggerEffectTime / triggerEffectDuration));
			triggerRect.sizeDelta = Vector2.Lerp(new Vector2(_tapSize, _tapSize), triggerStartSize, t);
		}
	}

	private void UpdateCheck()
	{
		if (checkEffectTime <= checkEffectDuration)
		{
			checkEffectTime += Time.deltaTime;
			float t = Mathf.Clamp01(checkEffectTime / checkEffectDuration);
			_hitCheck.sizeDelta = Vector2.Lerp(Vector2.one * _checkStartSize, Vector2.one * _checkEndSize, t);
		}
	}

	private IEnumerator NoteScale(GameObject note)
	{
		RectTransform rect = note.GetComponent<RectTransform>();
		Image image = note.GetComponent<Image>();
		notes.Add(rect);
		_ = image.color;
		float time = 0f;
		float duration = lerp3(_scaleStartDuration, _scaleMiddleDuration, _scaleEndDuration, Easing.easeInCubic((float)notesHit / (float)_notesForCompletion));
		while (time < duration)
		{
			time += Time.deltaTime;
			float num = Mathf.Clamp01(time / duration);
			float t = Easing.easeOutSine(num);
			rect.sizeDelta = Vector2.Lerp(Vector2.zero, Vector2.one * _endSize, t);
			Color color = Color.Lerp(_startColor, _endColor, num * num);
			image.color = color;
			yield return null;
		}
		isBlocked = false;
		notes.Remove(rect);
		Object.Destroy(note);
	}

	public static float lerp3(float a, float b, float c, float t)
	{
		float a2 = Mathf.Lerp(a, b, t);
		float b2 = Mathf.Lerp(b, c, t);
		return Mathf.Lerp(a2, b2, t);
	}
}
