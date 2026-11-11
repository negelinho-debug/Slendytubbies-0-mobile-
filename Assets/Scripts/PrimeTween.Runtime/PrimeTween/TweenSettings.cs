using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace PrimeTween;

[Serializable]
public struct TweenSettings
{
	public float duration;

	[Tooltip("Easing curve of an animation.\n\nDefault is Ease.OutQuad. The Default ease can be modified via 'PrimeTweenConfig.defaultEase' setting.\n\nSet to Ease.Custom to control the easing with custom AnimationCurve.")]
	public Ease ease;

	[Tooltip("A custom Animation Curve that will work as an easing curve.")]
	[CanBeNull]
	public AnimationCurve customEase;

	[Tooltip("The number of repetitions. Setting cycles to '-1' will repeat the animation indefinitely.")]
	public int cycles;

	[Tooltip("Controls how the animation behaves with multiple cycles.")]
	public CycleMode cycleMode;

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

	[NonSerialized]
	internal ParametricEase parametricEase;

	[NonSerialized]
	internal float parametricEaseStrength;

	[NonSerialized]
	internal float parametricEasePeriod;

	internal const float minDuration = 0.0001f;

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

	internal TweenSettings(float duration, Ease ease, Easing? customEasing, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, UpdateType updateType = default(UpdateType))
	{
		this.duration = duration;
		AnimationCurve animationCurve = customEasing?.curve;
		if (ease == Ease.Custom && customEasing.HasValue && customEasing.GetValueOrDefault().parametricEase == ParametricEase.None && (animationCurve == null || !ValidateCustomCurveKeyframes(animationCurve)))
		{
			Debug.LogError("Ease is Ease.Custom, but AnimationCurve is not configured correctly. Using Ease.Default instead.");
			ease = Ease.Default;
		}
		this.ease = ease;
		customEase = ((ease == Ease.Custom) ? animationCurve : null);
		this.cycles = cycles;
		this.cycleMode = cycleMode;
		this.startDelay = startDelay;
		this.endDelay = endDelay;
		this.useUnscaledTime = useUnscaledTime;
		parametricEase = customEasing?.parametricEase ?? ParametricEase.None;
		parametricEaseStrength = customEasing?.parametricEaseStrength ?? float.NaN;
		parametricEasePeriod = customEasing?.parametricEasePeriod ?? float.NaN;
		_useFixedUpdate = updateType == UpdateType.FixedUpdate;
		_updateType = updateType.enumValue;
	}

	public TweenSettings(float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, UpdateType updateType = default(UpdateType))
		: this(duration, ease, null, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, updateType)
	{
	}

	public TweenSettings(float duration, Easing easing, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, UpdateType updateType = default(UpdateType))
		: this(duration, easing.ease, easing, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, updateType)
	{
	}

	public TweenSettings(float duration, AnimationCurve ease, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, UpdateType updateType = default(UpdateType))
		: this(duration, Ease.Custom, Easing.Curve(ease), cycles, cycleMode, startDelay, endDelay, useUnscaledTime, updateType)
	{
	}

	internal static void setCyclesTo1If0(ref int cycles)
	{
		if (cycles == 0)
		{
			cycles = 1;
		}
	}

	internal void SetValidValues()
	{
		validateFiniteDuration(ref duration);
		validateFiniteDuration(ref startDelay);
		validateFiniteDuration(ref endDelay);
		setCyclesTo1If0(ref cycles);
		if (duration != 0f)
		{
			duration = Mathf.Max(0.0001f, duration);
		}
		startDelay = Mathf.Max(0f, startDelay);
		endDelay = Mathf.Max(0f, endDelay);
	}

	internal static void validateFiniteDuration(ref float f)
	{
		if (float.IsNaN(f) || float.IsInfinity(f))
		{
			Debug.LogError("Tween's duration is invalid.");
			f = 0f;
		}
	}

	internal static bool ValidateCustomCurve([NotNull] AnimationCurve curve)
	{
		return true;
	}

	internal static bool ValidateCustomCurveKeyframes([NotNull] AnimationCurve curve)
	{
		return true;
	}
}
[Serializable]
public struct TweenSettings<T> where T : struct
{
	[Tooltip("If true, the current value of an animated property will be used instead of the 'startValue'.\n\nThis field typically should not be manipulated directly. Instead, it's set by TweenSettings(T endValue, TweenSettings settings) constructor or by WithDirection() method.")]
	public bool startFromCurrent;

	[Tooltip("Start value of an animation.\n\nFor example, if you're animating a window, the 'startValue' can represent the closed (off-screen) position of the window.")]
	public T startValue;

	[Tooltip("End value of an animation.\n\nFor example, if you're animating a window, the 'endValue' can represent the opened position of the window.")]
	public T endValue;

	public TweenSettings settings;

	public TweenSettings(T endValue, TweenSettings settings)
	{
		startFromCurrent = true;
		startValue = default(T);
		this.endValue = endValue;
		this.settings = settings;
	}

	public TweenSettings(T startValue, T endValue, TweenSettings settings)
	{
		startFromCurrent = false;
		this.startValue = startValue;
		this.endValue = endValue;
		this.settings = settings;
	}

	public TweenSettings(T endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, UpdateType updateType = default(UpdateType))
		: this(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, updateType))
	{
	}

	public TweenSettings(T startValue, T endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, UpdateType updateType = default(UpdateType))
		: this(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, updateType))
	{
	}

	public TweenSettings(T endValue, float duration, Easing customEase, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, UpdateType updateType = default(UpdateType))
		: this(endValue, new TweenSettings(duration, customEase, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, updateType))
	{
	}

	public TweenSettings(T startValue, T endValue, float duration, Easing customEase, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false, UpdateType updateType = default(UpdateType))
		: this(startValue, endValue, new TweenSettings(duration, customEase, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, updateType))
	{
	}

	public readonly TweenSettings<T> WithDirection(bool toEndValue, bool _startFromCurrent = true)
	{
		if (startFromCurrent)
		{
			Debug.LogWarning("startFromCurrent is already enabled on this TweenSettings. The WithDirection() should be called on the TweenSettings once to choose the direction.");
		}
		TweenSettings<T> result = this;
		result.startFromCurrent = _startFromCurrent;
		if (toEndValue)
		{
			return result;
		}
		ref T reference = ref result.startValue;
		ref T reference2 = ref result.endValue;
		T val = result.endValue;
		T val2 = result.startValue;
		reference = val;
		reference2 = val2;
		return result;
	}

	public readonly TweenSettings<T> WithDirection(bool toEndValue, T currentValue)
	{
		TweenSettings<T> result = this;
		if (result.startFromCurrent)
		{
			result.startFromCurrent = false;
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		result.startValue = currentValue;
		result.endValue = (toEndValue ? endValue : startValue);
		return result;
	}
}
