using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace PrimeTween;

[Serializable]
public struct ShakeSettings
{
	internal const float defaultFrequency = 10f;

	[Tooltip("Strength is applied per-axis in local space coordinates.\n\nShakes: the strongest strength component will be used as the main frequency axis. Shakes on secondary axes happen randomly instead of following the frequency.\n\nPunches: strength determines punch direction.\n\nStrength is measured in units (position/scale) or Euler angles (rotation).")]
	public Vector3 strength;

	public float duration;

	[Tooltip("Number of shakes per second.")]
	public float frequency;

	[Tooltip("With enabled falloff shake starts at the highest strength and fades to the end.")]
	public bool enableFalloff;

	[Tooltip("Falloff ease is inverted to achieve the effect of shake 'fading' over time. Typically, eases go from 0 to 1, but falloff ease goes from 1 to 0.\n\nDefault is Ease.Linear.\n\nSet to Ease.Custom to have manual control over shake's 'strength' over time.")]
	public Ease falloffEase;

	[Tooltip("Shake's 'strength' over time.")]
	[CanBeNull]
	public AnimationCurve strengthOverTime;

	[Tooltip("Represents how asymmetrical the shake is.\n\n'0' means the shake is symmetrical around the initial value.\n\n'1' means the shake is asymmetrical and will happen between the initial position and the value of the 'strength' vector.\n\nWhen used with punches, can be treated as the resistance to 'recoil': '0' is full recoil, '1' is no recoil.")]
	[Range(0f, 1f)]
	public float asymmetry;

	[Tooltip("Ease between adjacent shake points.\n\nDefault is Ease.OutQuad.")]
	public Ease easeBetweenShakes;

	[Tooltip("The number of repetitions. Setting cycles to '-1' will repeat the animation indefinitely.")]
	public int cycles;

	[Tooltip("Delays the start of a tween.")]
	public float startDelay;

	[Tooltip("Delays the completion of a tween.\n\nFor example, can be used to add the delay between cycles.\n\nOr can be used to postpone the execution of the onComplete callback.")]
	public float endDelay;

	[Tooltip("The animation will use real time, ignoring 'Time.timeScale'.")]
	public bool useUnscaledTime;

	[SerializeField]
	[FormerlySerializedAs("useFixedUpdate")]
	[HideInInspector]
	private bool _useFixedUpdate;

	[SerializeField]
	[Tooltip("Controls Unity's event function, which updates the animation.\n\nThe default is MonoBehaviour.Update().")]
	internal _UpdateType _updateType;

	[Obsolete("use 'updateType' instead.")]
	public bool useFixedUpdate
	{
		get
		{
			if (!(updateType == UpdateType.FixedUpdate))
			{
				return _useFixedUpdate;
			}
			return true;
		}
		set
		{
			_updateType = ((!value) ? _UpdateType.Update : _UpdateType.FixedUpdate);
			_useFixedUpdate = value;
		}
	}

	public UpdateType updateType
	{
		get
		{
			if (!_useFixedUpdate)
			{
				return new UpdateType(_updateType);
			}
			return UpdateType.FixedUpdate;
		}
		set
		{
			_updateType = value.enumValue;
			_useFixedUpdate = value == UpdateType.FixedUpdate;
		}
	}

	[field: NonSerialized]
	internal bool isPunch { get; private set; }

	internal TweenSettings tweenSettings => new TweenSettings(duration, Ease.Linear, cycles, CycleMode.Restart, startDelay, endDelay, useUnscaledTime, updateType);

	internal ShakeSettings(Vector3 strength, float duration, float frequency, Ease? falloffEase, [CanBeNull] AnimationCurve strengthOverTime, Ease easeBetweenShakes, float asymmetryFactor, int cycles, float startDelay, float endDelay, bool useUnscaledTime, UpdateType updateType)
	{
		this.frequency = frequency;
		this.strength = strength;
		this.duration = duration;
		if (falloffEase == Ease.Custom && (strengthOverTime == null || !TweenSettings.ValidateCustomCurve(strengthOverTime)))
		{
			Debug.LogError("Shake falloff is Ease.Custom, but strengthOverTime is not configured correctly. Using Ease.Linear instead.");
			falloffEase = Ease.Linear;
		}
		this.falloffEase = falloffEase.GetValueOrDefault();
		this.strengthOverTime = ((falloffEase == Ease.Custom) ? strengthOverTime : null);
		enableFalloff = falloffEase.HasValue;
		this.easeBetweenShakes = easeBetweenShakes;
		this.cycles = cycles;
		this.startDelay = startDelay;
		this.endDelay = endDelay;
		this.useUnscaledTime = useUnscaledTime;
		asymmetry = asymmetryFactor;
		isPunch = false;
		_useFixedUpdate = updateType == UpdateType.FixedUpdate;
		_updateType = updateType.enumValue;
	}

	public ShakeSettings(Vector3 strength, float duration = 0.5f, float frequency = 10f, bool enableFalloff = true, Ease easeBetweenShakes = Ease.Default, float asymmetryFactor = 0f, int cycles = 1, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, UpdateType updateType = default(UpdateType))
		: this(strength, duration, frequency, enableFalloff ? new Ease?(Ease.Default) : ((Ease?)null), null, easeBetweenShakes, asymmetryFactor, cycles, startDelay, endDelay, useUnscaledTime, updateType)
	{
	}

	public ShakeSettings(Vector3 strength, float duration, float frequency, AnimationCurve strengthOverTime, Ease easeBetweenShakes = Ease.Default, float asymmetryFactor = 0f, int cycles = 1, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, UpdateType updateType = default(UpdateType))
		: this(strength, duration, frequency, Ease.Custom, strengthOverTime, easeBetweenShakes, asymmetryFactor, cycles, startDelay, endDelay, useUnscaledTime, updateType)
	{
	}

	internal readonly ShakeSettings WithPunch()
	{
		ShakeSettings result = this;
		result.isPunch = true;
		return result;
	}
}
