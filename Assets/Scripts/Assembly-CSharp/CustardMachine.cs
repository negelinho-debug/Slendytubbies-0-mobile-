using System;
using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;
using XPostProcessing;

public class CustardMachine : IInteractable
{
	[Serializable]
	public struct MachineInteractions
	{
		public UnityEvent InteractionEvent;
	}

	[Header("Effects")]
	public PostProcessVolume _angryVolume;

	public PostProcessVolume _noiseVignetteVolume;

	public float _sobelIntensity = 2f;

	public float _jitterFrequency = 6f;

	public EventReference _machineReference;

	public EventReference _eatingReference;

	[Header("Interaction")]
	public Transform _lookTarget;

	public float _lookAtSpeed = 5f;

	public float _interactionDuration = 1.5f;

	public float _stepAwayDistance = 5f;

	public MachineInteractions[] _interactions;

	[Space(10f)]
	public ExcelElement _lastInteractionElement;

	public ExcelElement _stepAwayElement;

	[Space(10f)]
	public float _intensity = 1.5f;

	public float _frequency = 25f;

	public float _glitchChance = 0.25f;

	private HungerMeter hungerMeter;

	private bool shownText;

	private bool hasEaten;

	private bool hasInteracted;

	private float interactionTime;

	private int interactionCount;

	private EdgeDetectionSobel sobel;

	private GlitchWaveJitter jitter;

	private void Start()
	{
		hungerMeter = Player.Instance.GetComponent<HungerMeter>();
		_angryVolume.profile.TryGetSettings<EdgeDetectionSobel>(out sobel);
		_angryVolume.profile.TryGetSettings<GlitchWaveJitter>(out jitter);
		sobel.edgeWidth.value = 0f;
		jitter.frequency.value = 0f;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		VideoManager.OnFinished = (Action)Delegate.Remove(VideoManager.OnFinished, new Action(OnInteraction));
	}

	private void Update()
	{
		if (!shownText && hungerMeter.Hunger >= 1f)
		{
			shownText = true;
			Element element = new Element
			{
				excelElement = _stepAwayElement,
				intensity = _intensity,
				frequency = _frequency,
				glitchChance = _glitchChance
			};
			MenuText.Instance.StartText(element);
		}
		_ = hasInteracted;
	}

	private void OnInteraction()
	{
		Debug.Log($"Why {interactionCount}");
		_interactions[interactionCount].InteractionEvent.Invoke();
		interactionCount++;
		float num = (float)interactionCount / (float)(_interactions.Length - 1);
		sobel.edgeWidth.value = num * _sobelIntensity;
		jitter.frequency.value = num * _jitterFrequency;
		_noiseVignetteVolume.weight = num;
		shownText = false;
		hasInteracted = false;
		Player.Instance.Controller.IsFreezed = false;
		VideoManager.OnFinished = (Action)Delegate.Remove(VideoManager.OnFinished, new Action(OnInteraction));
	}

	public override void Use()
	{
		if (!(hungerMeter.Hunger < 1f))
		{
			Debug.Log("Usage happened");
			MenuText.Instance.StopText();
			if (interactionCount < _interactions.Length - 1)
			{
				Player.Instance.Controller.IsFreezed = true;
				VideoManager.Instance.PlayVideo(Videos.CustardMachine);
				RuntimeManager.PlayOneShot(_machineReference, base.transform.position);
				VideoManager.OnFinished = (Action)Delegate.Combine(VideoManager.OnFinished, new Action(OnInteraction));
				hungerMeter.NextHungerLevel();
			}
			else
			{
				Active = false;
				Element element = new Element
				{
					excelElement = _lastInteractionElement,
					intensity = _intensity,
					frequency = _frequency,
					glitchChance = _glitchChance
				};
				MenuText.Instance.StartText(element);
				StartCoroutine(fuckYouRomka());
			}
		}
	}

	private IEnumerator fuckYouRomka()
	{
		yield return new WaitForSeconds(2f);
		_interactions[^1].InteractionEvent.Invoke();
	}
}
