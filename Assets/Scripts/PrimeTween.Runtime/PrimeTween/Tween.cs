using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace PrimeTween;

public readonly struct Tween : IEnumerator, IEquatable<Tween>
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This struct is needed for async/await support, you should not use it directly.")]
	public readonly struct TweenAwaiter : INotifyCompletion
	{
		private readonly Tween tween;

		public bool IsCompleted => !tween.isAlive;

		internal TweenAwaiter(Tween tween)
		{
			if (tween.isAlive && !tween.TryManipulate())
			{
				this.tween = default(Tween);
			}
			else
			{
				this.tween = tween;
			}
		}

		public void OnCompleted([NotNull] Action continuation)
		{
			try
			{
				TweenSettings<float> settings = new TweenSettings<float>(0f, 0f, float.MaxValue, Ease.Linear, -1);
				Tween obj = animate(tween.tween, ref settings, TweenAnimation.TweenType.TweenAwaiter);
				obj.tween.longParam = tween.id;
				obj.tween.managedData.OnComplete(continuation, true);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				throw;
			}
		}

		internal static void UpdateTweenAwaiter(ref TweenData rt, ref UnmanagedTweenData d)
		{
			if (d.isAlive)
			{
				ColdData coldData = rt.target as ColdData;
				if (rt.cold.longParam != coldData.id || !coldData.data.isAlive)
				{
					rt.ForceComplete(ref d);
				}
			}
		}

		public void GetResult()
		{
		}
	}

	private const string minMaxExpected = "numMinExpected/numMaxExpected parameters are no longer supported.";

	private const string localScaleRenamed = "please use 'Scale' instead of 'LocalScale'.";

	private static readonly Dictionary<int, int> textureIdTo_ST = new Dictionary<int, int>(10);

	internal readonly long id;

	internal readonly ColdData tween;

	object IEnumerator.Current => null;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'isAlive' (lower case 'i') instead.")]
	public bool IsAlive => isAlive;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'isPaused' (lower case 'i') instead.")]
	public bool IsPaused => isPaused;

	internal bool IsCreated => id != 0;

	public bool isAlive
	{
		get
		{
			if (id != 0L && tween.id == id && tween.hasData)
			{
				return tween.data.isAlive;
			}
			return false;
		}
	}

	public float elapsedTime
	{
		get
		{
			if (!ValidateIsAlive())
			{
				return 0f;
			}
			if (cyclesDone == cyclesTotal)
			{
				return duration;
			}
			float num = elapsedTimeTotal - duration * (float)cyclesDone;
			if (num < 0f)
			{
				return 0f;
			}
			return num;
		}
		set
		{
			SetElapsedTime(value);
		}
	}

	public int cyclesTotal
	{
		get
		{
			if (!ValidateIsAlive())
			{
				return 0;
			}
			return tween.data.cyclesTotal;
		}
	}

	public int cyclesDone
	{
		get
		{
			if (!ValidateIsAlive())
			{
				return 0;
			}
			return tween.data.getCyclesDone();
		}
	}

	public float duration
	{
		get
		{
			if (!ValidateIsAlive())
			{
				return 0f;
			}
			float f = tween.data.cycleDuration;
			TweenSettings.validateFiniteDuration(ref f);
			return f;
		}
	}

	public float elapsedTimeTotal
	{
		get
		{
			if (!ValidateIsAlive())
			{
				return 0f;
			}
			if (tween.data.elapsedTimeTotal == float.MaxValue)
			{
				return durationTotal;
			}
			return Mathf.Clamp(tween.data.elapsedTimeTotal - tween.data.waitDelay, 0f, durationTotal);
		}
		set
		{
			SetElapsedTimeTotal(value);
		}
	}

	public float durationTotal
	{
		get
		{
			if (!ValidateIsAlive())
			{
				return 0f;
			}
			int num = tween.data.cyclesTotal;
			if (num == -1)
			{
				return float.PositiveInfinity;
			}
			return tween.data.cycleDuration * (float)num;
		}
	}

	public float progress
	{
		get
		{
			if (!ValidateIsAlive())
			{
				return 0f;
			}
			if (duration == 0f)
			{
				return GetProgressFromState();
			}
			return Mathf.Min(elapsedTime / duration, 1f);
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (value == 1f && cyclesDone == cyclesTotal - 1)
			{
				SetElapsedTimeTotal(float.MaxValue);
			}
			else
			{
				SetElapsedTime(value * duration);
			}
		}
	}

	public float progressTotal
	{
		get
		{
			if (!ValidateIsAlive())
			{
				return 0f;
			}
			if (cyclesTotal == -1)
			{
				return 0f;
			}
			float num = durationTotal;
			if (num == 0f)
			{
				return GetProgressFromState();
			}
			return Mathf.Min(elapsedTimeTotal / num, 1f);
		}
		set
		{
			if (!ValidateIsAlive())
			{
				return;
			}
			if (cyclesTotal == -1)
			{
				Debug.LogError("It's not allowed to set progressTotal on infinite tween (cyclesTotal == -1), tween: " + ToString() + ".");
				return;
			}
			value = Mathf.Clamp01(value);
			if (value == 1f)
			{
				SetElapsedTimeTotal(float.MaxValue);
			}
			else
			{
				SetElapsedTimeTotal(value * durationTotal);
			}
		}
	}

	public float interpolationFactor
	{
		get
		{
			if (!ValidateIsAlive())
			{
				return 0f;
			}
			return Mathf.Max(0f, tween.data.easedInterpolationFactor);
		}
	}

	public bool isPaused
	{
		get
		{
			if (TryManipulate())
			{
				return tween.data.isPaused;
			}
			return false;
		}
		set
		{
			if (!TryManipulate())
			{
				return;
			}
			ref TweenData managedData = ref tween.managedData;
			ref UnmanagedTweenData data = ref tween.data;
			if (data.trySetPause(value) && !value && ((timeScale > 0f && progressTotal >= 1f) || (timeScale < 0f && progressTotal == 0f)))
			{
				if (data.IsMainSequenceRoot())
				{
					new Sequence(managedData.cold.sequence).ReleaseTweens();
				}
				else
				{
					managedData.Kill(ref data);
				}
			}
		}
	}

	public float timeScale
	{
		get
		{
			if (!TryManipulate())
			{
				return 1f;
			}
			return tween.data.timeScale;
		}
		set
		{
			if (TryManipulate())
			{
				if (float.IsNaN(value) || float.IsInfinity(value))
				{
					throw new ArgumentException(string.Format("Invalid {0}: {1}.", "timeScale", value));
				}
				tween.data.timeScale = value;
			}
		}
	}

	internal float durationWithWaitDelay => tween.data.calcDurationWithWaitDependencies();

	[EditorBrowsable(EditorBrowsableState.Never)]
	public TweenAwaiter GetAwaiter()
	{
		return new TweenAwaiter(this);
	}

	[NotNull]
	public IEnumerator ToYieldInstruction()
	{
		if (!isAlive || !TryManipulate())
		{
			return Array.Empty<object>().GetEnumerator();
		}
		if (tween.data.isInCoroutine)
		{
			Assert.LogErrorWithStackTrace("ToYieldInstruction can't be called on a tween that's already being waited for in a coroutine.", tween.id, tween.managedData.target);
			return Array.Empty<object>().GetEnumerator();
		}
		tween.data.isInCoroutine = true;
		return tween;
	}

	bool IEnumerator.MoveNext()
	{
		return isAlive;
	}

	void IEnumerator.Reset()
	{
		throw new NotSupportedException();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("SetCycles() was renamed to SetRemainingCycles().")]
	public void SetCycles(bool stopAtEndValue)
	{
		SetRemainingCycles(stopAtEndValue);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("SetCycles() was renamed to SetRemainingCycles().")]
	public void SetCycles(int cycles)
	{
		SetRemainingCycles(cycles);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("numMinExpected/numMaxExpected parameters are no longer supported.")]
	public static int StopAll([CanBeNull] object onTarget, int? numMinExpected, int? numMaxExpected = null)
	{
		return StopAll(onTarget);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("numMinExpected/numMaxExpected parameters are no longer supported.")]
	public static int CompleteAll([CanBeNull] object onTarget, int? numMinExpected, int? numMaxExpected = null)
	{
		return CompleteAll(onTarget);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("numMinExpected/numMaxExpected parameters are no longer supported.")]
	public static int SetPausedAll(bool isPaused, [CanBeNull] object onTarget, int? numMinExpected, int? numMaxExpected = null)
	{
		return SetPausedAll(isPaused, onTarget);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScale([NotNull] Transform target, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalScale(target, new TweenSettings<Vector3>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScale([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalScale(target, new TweenSettings<Vector3>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScale([NotNull] Transform target, Vector3 endValue, TweenSettings settings)
	{
		return LocalScale(target, new TweenSettings<Vector3>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScale([NotNull] Transform target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return LocalScale(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScale([NotNull] Transform target, TweenSettings<Vector3> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.Scale);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleX([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalScaleX(target, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleX([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalScaleX(target, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleX([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return LocalScaleX(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleX([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return LocalScaleX(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleX([NotNull] Transform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.ScaleX);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleY([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalScaleY(target, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleY([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalScaleY(target, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleY([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return LocalScaleY(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleY([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return LocalScaleY(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleY([NotNull] Transform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.ScaleY);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleZ([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalScaleZ(target, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleZ([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalScaleZ(target, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleZ([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return LocalScaleZ(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleZ([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return LocalScaleZ(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScaleZ([NotNull] Transform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.ScaleZ);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScale([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalScale(target, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScale([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalScale(target, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScale([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return LocalScale(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScale([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return LocalScale(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween LocalScale([NotNull] Transform target, TweenSettings<float> uniformScaleSettings)
	{
		return Scale(target, uniformScaleSettings);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween ShakeLocalScale([NotNull] Transform target, Vector3 strength, float duration, float frequency = 10f, bool enableFalloff = true, Ease easeBetweenShakes = Ease.Default, float asymmetryFactor = 0f, int cycles = 1, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return ShakeScale(target, new ShakeSettings(strength, duration, frequency, enableFalloff, easeBetweenShakes, asymmetryFactor, cycles, startDelay, endDelay, useUnscaledTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween ShakeLocalScale([NotNull] Transform target, ShakeSettings settings)
	{
		return ShakeScale(target, settings);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween PunchLocalScale([NotNull] Transform target, Vector3 strength, float duration, float frequency = 10f, bool enableFalloff = true, Ease easeBetweenShakes = Ease.Default, float asymmetryFactor = 0f, int cycles = 1, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return PunchScale(target, new ShakeSettings(strength, duration, frequency, enableFalloff, easeBetweenShakes, asymmetryFactor, cycles, startDelay, endDelay, useUnscaledTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'Scale' instead of 'LocalScale'.")]
	public static Tween PunchLocalScale([NotNull] Transform target, ShakeSettings settings)
	{
		return ShakeScale(target, settings.WithPunch());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween LightRange([NotNull] Light target, float endValue, TweenSettings settings)
	{
		return LightRange(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween LightRange([NotNull] Light target, float startValue, float endValue, TweenSettings settings)
	{
		return LightRange(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween LightRange([NotNull] Light target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LightRange(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LightRange([NotNull] Light target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LightRange(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LightRange([NotNull] Light target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.LightRange);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween LightShadowStrength([NotNull] Light target, float endValue, TweenSettings settings)
	{
		return LightShadowStrength(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween LightShadowStrength([NotNull] Light target, float startValue, float endValue, TweenSettings settings)
	{
		return LightShadowStrength(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween LightShadowStrength([NotNull] Light target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LightShadowStrength(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LightShadowStrength([NotNull] Light target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LightShadowStrength(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LightShadowStrength([NotNull] Light target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.LightShadowStrength);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween LightIntensity([NotNull] Light target, float endValue, TweenSettings settings)
	{
		return LightIntensity(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween LightIntensity([NotNull] Light target, float startValue, float endValue, TweenSettings settings)
	{
		return LightIntensity(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween LightIntensity([NotNull] Light target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LightIntensity(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LightIntensity([NotNull] Light target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LightIntensity(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LightIntensity([NotNull] Light target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.LightIntensity);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween LightColor([NotNull] Light target, Color endValue, TweenSettings settings)
	{
		return LightColor(target, new TweenSettings<Color>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween LightColor([NotNull] Light target, Color startValue, Color endValue, TweenSettings settings)
	{
		return LightColor(target, new TweenSettings<Color>(startValue, endValue, settings));
	}

	public static Tween LightColor([NotNull] Light target, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LightColor(target, new TweenSettings<Color>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LightColor([NotNull] Light target, Color startValue, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LightColor(target, new TweenSettings<Color>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LightColor([NotNull] Light target, TweenSettings<Color> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.LightColor);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraOrthographicSize([NotNull] Camera target, float endValue, TweenSettings settings)
	{
		return CameraOrthographicSize(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraOrthographicSize([NotNull] Camera target, float startValue, float endValue, TweenSettings settings)
	{
		return CameraOrthographicSize(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween CameraOrthographicSize([NotNull] Camera target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraOrthographicSize(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraOrthographicSize([NotNull] Camera target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraOrthographicSize(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraOrthographicSize([NotNull] Camera target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.CameraOrthographicSize);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraBackgroundColor([NotNull] Camera target, Color endValue, TweenSettings settings)
	{
		return CameraBackgroundColor(target, new TweenSettings<Color>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraBackgroundColor([NotNull] Camera target, Color startValue, Color endValue, TweenSettings settings)
	{
		return CameraBackgroundColor(target, new TweenSettings<Color>(startValue, endValue, settings));
	}

	public static Tween CameraBackgroundColor([NotNull] Camera target, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraBackgroundColor(target, new TweenSettings<Color>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraBackgroundColor([NotNull] Camera target, Color startValue, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraBackgroundColor(target, new TweenSettings<Color>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraBackgroundColor([NotNull] Camera target, TweenSettings<Color> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.CameraBackgroundColor);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraAspect([NotNull] Camera target, float endValue, TweenSettings settings)
	{
		return CameraAspect(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraAspect([NotNull] Camera target, float startValue, float endValue, TweenSettings settings)
	{
		return CameraAspect(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween CameraAspect([NotNull] Camera target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraAspect(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraAspect([NotNull] Camera target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraAspect(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraAspect([NotNull] Camera target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.CameraAspect);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraFarClipPlane([NotNull] Camera target, float endValue, TweenSettings settings)
	{
		return CameraFarClipPlane(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraFarClipPlane([NotNull] Camera target, float startValue, float endValue, TweenSettings settings)
	{
		return CameraFarClipPlane(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween CameraFarClipPlane([NotNull] Camera target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraFarClipPlane(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraFarClipPlane([NotNull] Camera target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraFarClipPlane(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraFarClipPlane([NotNull] Camera target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.CameraFarClipPlane);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraFieldOfView([NotNull] Camera target, float endValue, TweenSettings settings)
	{
		return CameraFieldOfView(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraFieldOfView([NotNull] Camera target, float startValue, float endValue, TweenSettings settings)
	{
		return CameraFieldOfView(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween CameraFieldOfView([NotNull] Camera target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraFieldOfView(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraFieldOfView([NotNull] Camera target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraFieldOfView(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraFieldOfView([NotNull] Camera target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.CameraFieldOfView);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraNearClipPlane([NotNull] Camera target, float endValue, TweenSettings settings)
	{
		return CameraNearClipPlane(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraNearClipPlane([NotNull] Camera target, float startValue, float endValue, TweenSettings settings)
	{
		return CameraNearClipPlane(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween CameraNearClipPlane([NotNull] Camera target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraNearClipPlane(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraNearClipPlane([NotNull] Camera target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraNearClipPlane(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraNearClipPlane([NotNull] Camera target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.CameraNearClipPlane);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraPixelRect([NotNull] Camera target, Rect endValue, TweenSettings settings)
	{
		return CameraPixelRect(target, new TweenSettings<Rect>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraPixelRect([NotNull] Camera target, Rect startValue, Rect endValue, TweenSettings settings)
	{
		return CameraPixelRect(target, new TweenSettings<Rect>(startValue, endValue, settings));
	}

	public static Tween CameraPixelRect([NotNull] Camera target, Rect endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraPixelRect(target, new TweenSettings<Rect>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraPixelRect([NotNull] Camera target, Rect startValue, Rect endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraPixelRect(target, new TweenSettings<Rect>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraPixelRect([NotNull] Camera target, TweenSettings<Rect> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.CameraPixelRect);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraRect([NotNull] Camera target, Rect endValue, TweenSettings settings)
	{
		return CameraRect(target, new TweenSettings<Rect>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween CameraRect([NotNull] Camera target, Rect startValue, Rect endValue, TweenSettings settings)
	{
		return CameraRect(target, new TweenSettings<Rect>(startValue, endValue, settings));
	}

	public static Tween CameraRect([NotNull] Camera target, Rect endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraRect(target, new TweenSettings<Rect>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraRect([NotNull] Camera target, Rect startValue, Rect endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return CameraRect(target, new TweenSettings<Rect>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween CameraRect([NotNull] Camera target, TweenSettings<Rect> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.CameraRect);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalRotation([NotNull] Transform target, Vector3 endValue, TweenSettings settings)
	{
		return LocalRotation(target, new TweenSettings<Vector3>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalRotation([NotNull] Transform target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return LocalRotation(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	public static Tween LocalRotation([NotNull] Transform target, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalRotation(target, new TweenSettings<Vector3>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LocalRotation([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalRotation(target, new TweenSettings<Vector3>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Scale([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return Scale(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Scale([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return Scale(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween Scale([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Scale(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Scale([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Scale(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Rotation([NotNull] Transform target, Vector3 endValue, TweenSettings settings)
	{
		return Rotation(target, new TweenSettings<Vector3>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Rotation([NotNull] Transform target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return Rotation(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	public static Tween Rotation([NotNull] Transform target, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Rotation(target, new TweenSettings<Vector3>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Rotation([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Rotation(target, new TweenSettings<Vector3>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Position([NotNull] Transform target, Vector3 endValue, TweenSettings settings)
	{
		return Position(target, new TweenSettings<Vector3>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Position([NotNull] Transform target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return Position(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	public static Tween Position([NotNull] Transform target, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Position(target, new TweenSettings<Vector3>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Position([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Position(target, new TweenSettings<Vector3>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Position([NotNull] Transform target, TweenSettings<Vector3> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.Position);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween PositionX([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return PositionX(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween PositionX([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return PositionX(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween PositionX([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return PositionX(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween PositionX([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return PositionX(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween PositionX([NotNull] Transform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.PositionX);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween PositionY([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return PositionY(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween PositionY([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return PositionY(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween PositionY([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return PositionY(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween PositionY([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return PositionY(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween PositionY([NotNull] Transform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.PositionY);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween PositionZ([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return PositionZ(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween PositionZ([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return PositionZ(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween PositionZ([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return PositionZ(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween PositionZ([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return PositionZ(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween PositionZ([NotNull] Transform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.PositionZ);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalPosition([NotNull] Transform target, Vector3 endValue, TweenSettings settings)
	{
		return LocalPosition(target, new TweenSettings<Vector3>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalPosition([NotNull] Transform target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return LocalPosition(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	public static Tween LocalPosition([NotNull] Transform target, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalPosition(target, new TweenSettings<Vector3>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LocalPosition([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalPosition(target, new TweenSettings<Vector3>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LocalPosition([NotNull] Transform target, TweenSettings<Vector3> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.LocalPosition);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalPositionX([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return LocalPositionX(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalPositionX([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return LocalPositionX(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween LocalPositionX([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalPositionX(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LocalPositionX([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalPositionX(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LocalPositionX([NotNull] Transform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.LocalPositionX);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalPositionY([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return LocalPositionY(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalPositionY([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return LocalPositionY(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween LocalPositionY([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalPositionY(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LocalPositionY([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalPositionY(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LocalPositionY([NotNull] Transform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.LocalPositionY);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalPositionZ([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return LocalPositionZ(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalPositionZ([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return LocalPositionZ(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween LocalPositionZ([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalPositionZ(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LocalPositionZ([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalPositionZ(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LocalPositionZ([NotNull] Transform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.LocalPositionZ);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Rotation([NotNull] Transform target, Quaternion endValue, TweenSettings settings)
	{
		return Rotation(target, new TweenSettings<Quaternion>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Rotation([NotNull] Transform target, Quaternion startValue, Quaternion endValue, TweenSettings settings)
	{
		return Rotation(target, new TweenSettings<Quaternion>(startValue, endValue, settings));
	}

	public static Tween Rotation([NotNull] Transform target, Quaternion endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Rotation(target, new TweenSettings<Quaternion>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Rotation([NotNull] Transform target, Quaternion startValue, Quaternion endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Rotation(target, new TweenSettings<Quaternion>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Rotation([NotNull] Transform target, TweenSettings<Quaternion> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.RotationQuaternion);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalRotation([NotNull] Transform target, Quaternion endValue, TweenSettings settings)
	{
		return LocalRotation(target, new TweenSettings<Quaternion>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalRotation([NotNull] Transform target, Quaternion startValue, Quaternion endValue, TweenSettings settings)
	{
		return LocalRotation(target, new TweenSettings<Quaternion>(startValue, endValue, settings));
	}

	public static Tween LocalRotation([NotNull] Transform target, Quaternion endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalRotation(target, new TweenSettings<Quaternion>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LocalRotation([NotNull] Transform target, Quaternion startValue, Quaternion endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalRotation(target, new TweenSettings<Quaternion>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween LocalRotation([NotNull] Transform target, TweenSettings<Quaternion> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.LocalRotationQuaternion);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Scale([NotNull] Transform target, Vector3 endValue, TweenSettings settings)
	{
		return Scale(target, new TweenSettings<Vector3>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Scale([NotNull] Transform target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return Scale(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	public static Tween Scale([NotNull] Transform target, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Scale(target, new TweenSettings<Vector3>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Scale([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Scale(target, new TweenSettings<Vector3>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Scale([NotNull] Transform target, TweenSettings<Vector3> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.Scale);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween ScaleX([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return ScaleX(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween ScaleX([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return ScaleX(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween ScaleX([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return ScaleX(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween ScaleX([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return ScaleX(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween ScaleX([NotNull] Transform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.ScaleX);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween ScaleY([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return ScaleY(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween ScaleY([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return ScaleY(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween ScaleY([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return ScaleY(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween ScaleY([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return ScaleY(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween ScaleY([NotNull] Transform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.ScaleY);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween ScaleZ([NotNull] Transform target, float endValue, TweenSettings settings)
	{
		return ScaleZ(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween ScaleZ([NotNull] Transform target, float startValue, float endValue, TweenSettings settings)
	{
		return ScaleZ(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween ScaleZ([NotNull] Transform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return ScaleZ(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween ScaleZ([NotNull] Transform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return ScaleZ(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween ScaleZ([NotNull] Transform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.ScaleZ);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Color([NotNull] SpriteRenderer target, Color endValue, TweenSettings settings)
	{
		return Color(target, new TweenSettings<Color>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Color([NotNull] SpriteRenderer target, Color startValue, Color endValue, TweenSettings settings)
	{
		return Color(target, new TweenSettings<Color>(startValue, endValue, settings));
	}

	public static Tween Color([NotNull] SpriteRenderer target, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Color(target, new TweenSettings<Color>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Color([NotNull] SpriteRenderer target, Color startValue, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Color(target, new TweenSettings<Color>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Color([NotNull] SpriteRenderer target, TweenSettings<Color> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.ColorSpriteRenderer);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Alpha([NotNull] SpriteRenderer target, float endValue, TweenSettings settings)
	{
		return Alpha(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Alpha([NotNull] SpriteRenderer target, float startValue, float endValue, TweenSettings settings)
	{
		return Alpha(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween Alpha([NotNull] SpriteRenderer target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Alpha(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Alpha([NotNull] SpriteRenderer target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Alpha(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Alpha([NotNull] SpriteRenderer target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.AlphaSpriteRenderer);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween TweenTimeScale([NotNull] Tween target, float endValue, TweenSettings settings)
	{
		return TweenTimeScale(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween TweenTimeScale([NotNull] Tween target, float startValue, float endValue, TweenSettings settings)
	{
		return TweenTimeScale(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween TweenTimeScale([NotNull] Tween target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return TweenTimeScale(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween TweenTimeScale([NotNull] Tween target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return TweenTimeScale(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween TweenTimeScale([NotNull] Sequence target, float endValue, TweenSettings settings)
	{
		return TweenTimeScale(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween TweenTimeScale([NotNull] Sequence target, float startValue, float endValue, TweenSettings settings)
	{
		return TweenTimeScale(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween TweenTimeScale([NotNull] Sequence target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return TweenTimeScale(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween TweenTimeScale([NotNull] Sequence target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return TweenTimeScale(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UISliderValue([NotNull] UnityEngine.UI.Slider target, float endValue, TweenSettings settings)
	{
		return UISliderValue(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UISliderValue([NotNull] UnityEngine.UI.Slider target, float startValue, float endValue, TweenSettings settings)
	{
		return UISliderValue(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UISliderValue([NotNull] UnityEngine.UI.Slider target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UISliderValue(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UISliderValue([NotNull] UnityEngine.UI.Slider target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UISliderValue(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UISliderValue([NotNull] UnityEngine.UI.Slider target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UISliderValue);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UINormalizedPosition([NotNull] ScrollRect target, Vector2 endValue, TweenSettings settings)
	{
		return UINormalizedPosition(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UINormalizedPosition([NotNull] ScrollRect target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return UINormalizedPosition(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween UINormalizedPosition([NotNull] ScrollRect target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UINormalizedPosition(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UINormalizedPosition([NotNull] ScrollRect target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UINormalizedPosition(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UINormalizedPosition([NotNull] ScrollRect target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UINormalizedPosition);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIHorizontalNormalizedPosition([NotNull] ScrollRect target, float endValue, TweenSettings settings)
	{
		return UIHorizontalNormalizedPosition(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIHorizontalNormalizedPosition([NotNull] ScrollRect target, float startValue, float endValue, TweenSettings settings)
	{
		return UIHorizontalNormalizedPosition(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIHorizontalNormalizedPosition([NotNull] ScrollRect target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIHorizontalNormalizedPosition(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIHorizontalNormalizedPosition([NotNull] ScrollRect target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIHorizontalNormalizedPosition(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIHorizontalNormalizedPosition([NotNull] ScrollRect target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIHorizontalNormalizedPosition);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIVerticalNormalizedPosition([NotNull] ScrollRect target, float endValue, TweenSettings settings)
	{
		return UIVerticalNormalizedPosition(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIVerticalNormalizedPosition([NotNull] ScrollRect target, float startValue, float endValue, TweenSettings settings)
	{
		return UIVerticalNormalizedPosition(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIVerticalNormalizedPosition([NotNull] ScrollRect target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIVerticalNormalizedPosition(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIVerticalNormalizedPosition([NotNull] ScrollRect target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIVerticalNormalizedPosition(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIVerticalNormalizedPosition([NotNull] ScrollRect target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIVerticalNormalizedPosition);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIPivotX([NotNull] RectTransform target, float endValue, TweenSettings settings)
	{
		return UIPivotX(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIPivotX([NotNull] RectTransform target, float startValue, float endValue, TweenSettings settings)
	{
		return UIPivotX(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIPivotX([NotNull] RectTransform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIPivotX(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIPivotX([NotNull] RectTransform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIPivotX(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIPivotX([NotNull] RectTransform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIPivotX);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIPivotY([NotNull] RectTransform target, float endValue, TweenSettings settings)
	{
		return UIPivotY(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIPivotY([NotNull] RectTransform target, float startValue, float endValue, TweenSettings settings)
	{
		return UIPivotY(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIPivotY([NotNull] RectTransform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIPivotY(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIPivotY([NotNull] RectTransform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIPivotY(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIPivotY([NotNull] RectTransform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIPivotY);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIPivot([NotNull] RectTransform target, Vector2 endValue, TweenSettings settings)
	{
		return UIPivot(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIPivot([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return UIPivot(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween UIPivot([NotNull] RectTransform target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIPivot(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIPivot([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIPivot(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIPivot([NotNull] RectTransform target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIPivot);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchorMax([NotNull] RectTransform target, Vector2 endValue, TweenSettings settings)
	{
		return UIAnchorMax(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchorMax([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return UIAnchorMax(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween UIAnchorMax([NotNull] RectTransform target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchorMax(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchorMax([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchorMax(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchorMax([NotNull] RectTransform target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIAnchorMax);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchorMin([NotNull] RectTransform target, Vector2 endValue, TweenSettings settings)
	{
		return UIAnchorMin(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchorMin([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return UIAnchorMin(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween UIAnchorMin([NotNull] RectTransform target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchorMin(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchorMin([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchorMin(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchorMin([NotNull] RectTransform target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIAnchorMin);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPosition3D([NotNull] RectTransform target, Vector3 endValue, TweenSettings settings)
	{
		return UIAnchoredPosition3D(target, new TweenSettings<Vector3>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPosition3D([NotNull] RectTransform target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return UIAnchoredPosition3D(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	public static Tween UIAnchoredPosition3D([NotNull] RectTransform target, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPosition3D(target, new TweenSettings<Vector3>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPosition3D([NotNull] RectTransform target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPosition3D(target, new TweenSettings<Vector3>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPosition3D([NotNull] RectTransform target, TweenSettings<Vector3> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIAnchoredPosition3D);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPosition3DX([NotNull] RectTransform target, float endValue, TweenSettings settings)
	{
		return UIAnchoredPosition3DX(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPosition3DX([NotNull] RectTransform target, float startValue, float endValue, TweenSettings settings)
	{
		return UIAnchoredPosition3DX(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIAnchoredPosition3DX([NotNull] RectTransform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPosition3DX(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPosition3DX([NotNull] RectTransform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPosition3DX(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPosition3DX([NotNull] RectTransform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIAnchoredPosition3DX);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPosition3DY([NotNull] RectTransform target, float endValue, TweenSettings settings)
	{
		return UIAnchoredPosition3DY(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPosition3DY([NotNull] RectTransform target, float startValue, float endValue, TweenSettings settings)
	{
		return UIAnchoredPosition3DY(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIAnchoredPosition3DY([NotNull] RectTransform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPosition3DY(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPosition3DY([NotNull] RectTransform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPosition3DY(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPosition3DY([NotNull] RectTransform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIAnchoredPosition3DY);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPosition3DZ([NotNull] RectTransform target, float endValue, TweenSettings settings)
	{
		return UIAnchoredPosition3DZ(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPosition3DZ([NotNull] RectTransform target, float startValue, float endValue, TweenSettings settings)
	{
		return UIAnchoredPosition3DZ(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIAnchoredPosition3DZ([NotNull] RectTransform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPosition3DZ(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPosition3DZ([NotNull] RectTransform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPosition3DZ(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPosition3DZ([NotNull] RectTransform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIAnchoredPosition3DZ);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIEffectDistance([NotNull] Shadow target, Vector2 endValue, TweenSettings settings)
	{
		return UIEffectDistance(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIEffectDistance([NotNull] Shadow target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return UIEffectDistance(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween UIEffectDistance([NotNull] Shadow target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIEffectDistance(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIEffectDistance([NotNull] Shadow target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIEffectDistance(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIEffectDistance([NotNull] Shadow target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIEffectDistance);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Alpha([NotNull] Shadow target, float endValue, TweenSettings settings)
	{
		return Alpha(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Alpha([NotNull] Shadow target, float startValue, float endValue, TweenSettings settings)
	{
		return Alpha(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween Alpha([NotNull] Shadow target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Alpha(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Alpha([NotNull] Shadow target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Alpha(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Alpha([NotNull] Shadow target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIAlphaShadow);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Color([NotNull] Shadow target, Color endValue, TweenSettings settings)
	{
		return Color(target, new TweenSettings<Color>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Color([NotNull] Shadow target, Color startValue, Color endValue, TweenSettings settings)
	{
		return Color(target, new TweenSettings<Color>(startValue, endValue, settings));
	}

	public static Tween Color([NotNull] Shadow target, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Color(target, new TweenSettings<Color>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Color([NotNull] Shadow target, Color startValue, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Color(target, new TweenSettings<Color>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Color([NotNull] Shadow target, TweenSettings<Color> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIColorShadow);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIPreferredSize([NotNull] LayoutElement target, Vector2 endValue, TweenSettings settings)
	{
		return UIPreferredSize(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIPreferredSize([NotNull] LayoutElement target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return UIPreferredSize(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween UIPreferredSize([NotNull] LayoutElement target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIPreferredSize(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIPreferredSize([NotNull] LayoutElement target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIPreferredSize(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIPreferredSize([NotNull] LayoutElement target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIPreferredSize);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIPreferredWidth([NotNull] LayoutElement target, float endValue, TweenSettings settings)
	{
		return UIPreferredWidth(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIPreferredWidth([NotNull] LayoutElement target, float startValue, float endValue, TweenSettings settings)
	{
		return UIPreferredWidth(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIPreferredWidth([NotNull] LayoutElement target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIPreferredWidth(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIPreferredWidth([NotNull] LayoutElement target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIPreferredWidth(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIPreferredWidth([NotNull] LayoutElement target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIPreferredWidth);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIPreferredHeight([NotNull] LayoutElement target, float endValue, TweenSettings settings)
	{
		return UIPreferredHeight(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIPreferredHeight([NotNull] LayoutElement target, float startValue, float endValue, TweenSettings settings)
	{
		return UIPreferredHeight(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIPreferredHeight([NotNull] LayoutElement target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIPreferredHeight(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIPreferredHeight([NotNull] LayoutElement target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIPreferredHeight(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIPreferredHeight([NotNull] LayoutElement target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIPreferredHeight);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIFlexibleSize([NotNull] LayoutElement target, Vector2 endValue, TweenSettings settings)
	{
		return UIFlexibleSize(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIFlexibleSize([NotNull] LayoutElement target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return UIFlexibleSize(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween UIFlexibleSize([NotNull] LayoutElement target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIFlexibleSize(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIFlexibleSize([NotNull] LayoutElement target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIFlexibleSize(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIFlexibleSize([NotNull] LayoutElement target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIFlexibleSize);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIFlexibleWidth([NotNull] LayoutElement target, float endValue, TweenSettings settings)
	{
		return UIFlexibleWidth(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIFlexibleWidth([NotNull] LayoutElement target, float startValue, float endValue, TweenSettings settings)
	{
		return UIFlexibleWidth(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIFlexibleWidth([NotNull] LayoutElement target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIFlexibleWidth(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIFlexibleWidth([NotNull] LayoutElement target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIFlexibleWidth(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIFlexibleWidth([NotNull] LayoutElement target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIFlexibleWidth);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIFlexibleHeight([NotNull] LayoutElement target, float endValue, TweenSettings settings)
	{
		return UIFlexibleHeight(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIFlexibleHeight([NotNull] LayoutElement target, float startValue, float endValue, TweenSettings settings)
	{
		return UIFlexibleHeight(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIFlexibleHeight([NotNull] LayoutElement target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIFlexibleHeight(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIFlexibleHeight([NotNull] LayoutElement target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIFlexibleHeight(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIFlexibleHeight([NotNull] LayoutElement target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIFlexibleHeight);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIMinSize([NotNull] LayoutElement target, Vector2 endValue, TweenSettings settings)
	{
		return UIMinSize(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIMinSize([NotNull] LayoutElement target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return UIMinSize(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween UIMinSize([NotNull] LayoutElement target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIMinSize(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIMinSize([NotNull] LayoutElement target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIMinSize(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIMinSize([NotNull] LayoutElement target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIMinSize);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIMinWidth([NotNull] LayoutElement target, float endValue, TweenSettings settings)
	{
		return UIMinWidth(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIMinWidth([NotNull] LayoutElement target, float startValue, float endValue, TweenSettings settings)
	{
		return UIMinWidth(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIMinWidth([NotNull] LayoutElement target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIMinWidth(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIMinWidth([NotNull] LayoutElement target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIMinWidth(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIMinWidth([NotNull] LayoutElement target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIMinWidth);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIMinHeight([NotNull] LayoutElement target, float endValue, TweenSettings settings)
	{
		return UIMinHeight(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIMinHeight([NotNull] LayoutElement target, float startValue, float endValue, TweenSettings settings)
	{
		return UIMinHeight(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIMinHeight([NotNull] LayoutElement target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIMinHeight(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIMinHeight([NotNull] LayoutElement target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIMinHeight(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIMinHeight([NotNull] LayoutElement target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIMinHeight);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Color([NotNull] Graphic target, Color endValue, TweenSettings settings)
	{
		return Color(target, new TweenSettings<Color>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Color([NotNull] Graphic target, Color startValue, Color endValue, TweenSettings settings)
	{
		return Color(target, new TweenSettings<Color>(startValue, endValue, settings));
	}

	public static Tween Color([NotNull] Graphic target, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Color(target, new TweenSettings<Color>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Color([NotNull] Graphic target, Color startValue, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Color(target, new TweenSettings<Color>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Color([NotNull] Graphic target, TweenSettings<Color> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIColorGraphic);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPosition([NotNull] RectTransform target, Vector2 endValue, TweenSettings settings)
	{
		return UIAnchoredPosition(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPosition([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return UIAnchoredPosition(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween UIAnchoredPosition([NotNull] RectTransform target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPosition(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPosition([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPosition(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPosition([NotNull] RectTransform target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIAnchoredPosition);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPositionX([NotNull] RectTransform target, float endValue, TweenSettings settings)
	{
		return UIAnchoredPositionX(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPositionX([NotNull] RectTransform target, float startValue, float endValue, TweenSettings settings)
	{
		return UIAnchoredPositionX(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIAnchoredPositionX([NotNull] RectTransform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPositionX(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPositionX([NotNull] RectTransform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPositionX(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPositionX([NotNull] RectTransform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIAnchoredPositionX);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPositionY([NotNull] RectTransform target, float endValue, TweenSettings settings)
	{
		return UIAnchoredPositionY(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIAnchoredPositionY([NotNull] RectTransform target, float startValue, float endValue, TweenSettings settings)
	{
		return UIAnchoredPositionY(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIAnchoredPositionY([NotNull] RectTransform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPositionY(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPositionY([NotNull] RectTransform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIAnchoredPositionY(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIAnchoredPositionY([NotNull] RectTransform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIAnchoredPositionY);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UISizeDelta([NotNull] RectTransform target, Vector2 endValue, TweenSettings settings)
	{
		return UISizeDelta(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UISizeDelta([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return UISizeDelta(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween UISizeDelta([NotNull] RectTransform target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UISizeDelta(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UISizeDelta([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UISizeDelta(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UISizeDelta([NotNull] RectTransform target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UISizeDelta);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Alpha([NotNull] CanvasGroup target, float endValue, TweenSettings settings)
	{
		return Alpha(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Alpha([NotNull] CanvasGroup target, float startValue, float endValue, TweenSettings settings)
	{
		return Alpha(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween Alpha([NotNull] CanvasGroup target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Alpha(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Alpha([NotNull] CanvasGroup target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Alpha(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Alpha([NotNull] CanvasGroup target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIAlphaCanvasGroup);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Alpha([NotNull] Graphic target, float endValue, TweenSettings settings)
	{
		return Alpha(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Alpha([NotNull] Graphic target, float startValue, float endValue, TweenSettings settings)
	{
		return Alpha(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween Alpha([NotNull] Graphic target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Alpha(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Alpha([NotNull] Graphic target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Alpha(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Alpha([NotNull] Graphic target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIAlphaGraphic);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIFillAmount([NotNull] UnityEngine.UI.Image target, float endValue, TweenSettings settings)
	{
		return UIFillAmount(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIFillAmount([NotNull] UnityEngine.UI.Image target, float startValue, float endValue, TweenSettings settings)
	{
		return UIFillAmount(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIFillAmount([NotNull] UnityEngine.UI.Image target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIFillAmount(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIFillAmount([NotNull] UnityEngine.UI.Image target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIFillAmount(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIFillAmount([NotNull] UnityEngine.UI.Image target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIFillAmount);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIOffsetMin([NotNull] RectTransform target, Vector2 endValue, TweenSettings settings)
	{
		return UIOffsetMin(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIOffsetMin([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return UIOffsetMin(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween UIOffsetMin([NotNull] RectTransform target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIOffsetMin(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIOffsetMin([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIOffsetMin(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIOffsetMin([NotNull] RectTransform target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIOffsetMin);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIOffsetMinX([NotNull] RectTransform target, float endValue, TweenSettings settings)
	{
		return UIOffsetMinX(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIOffsetMinX([NotNull] RectTransform target, float startValue, float endValue, TweenSettings settings)
	{
		return UIOffsetMinX(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIOffsetMinX([NotNull] RectTransform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIOffsetMinX(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIOffsetMinX([NotNull] RectTransform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIOffsetMinX(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIOffsetMinX([NotNull] RectTransform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIOffsetMinX);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIOffsetMinY([NotNull] RectTransform target, float endValue, TweenSettings settings)
	{
		return UIOffsetMinY(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIOffsetMinY([NotNull] RectTransform target, float startValue, float endValue, TweenSettings settings)
	{
		return UIOffsetMinY(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIOffsetMinY([NotNull] RectTransform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIOffsetMinY(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIOffsetMinY([NotNull] RectTransform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIOffsetMinY(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIOffsetMinY([NotNull] RectTransform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIOffsetMinY);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIOffsetMax([NotNull] RectTransform target, Vector2 endValue, TweenSettings settings)
	{
		return UIOffsetMax(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIOffsetMax([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return UIOffsetMax(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween UIOffsetMax([NotNull] RectTransform target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIOffsetMax(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIOffsetMax([NotNull] RectTransform target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIOffsetMax(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIOffsetMax([NotNull] RectTransform target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIOffsetMax);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIOffsetMaxX([NotNull] RectTransform target, float endValue, TweenSettings settings)
	{
		return UIOffsetMaxX(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIOffsetMaxX([NotNull] RectTransform target, float startValue, float endValue, TweenSettings settings)
	{
		return UIOffsetMaxX(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIOffsetMaxX([NotNull] RectTransform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIOffsetMaxX(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIOffsetMaxX([NotNull] RectTransform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIOffsetMaxX(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIOffsetMaxX([NotNull] RectTransform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIOffsetMaxX);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIOffsetMaxY([NotNull] RectTransform target, float endValue, TweenSettings settings)
	{
		return UIOffsetMaxY(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween UIOffsetMaxY([NotNull] RectTransform target, float startValue, float endValue, TweenSettings settings)
	{
		return UIOffsetMaxY(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween UIOffsetMaxY([NotNull] RectTransform target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIOffsetMaxY(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIOffsetMaxY([NotNull] RectTransform target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return UIOffsetMaxY(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween UIOffsetMaxY([NotNull] RectTransform target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.UIOffsetMaxY);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody target, Vector3 endValue, TweenSettings settings)
	{
		return RigidbodyMoveRotation(target, new TweenSettings<Vector3>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return RigidbodyMoveRotation(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody target, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RigidbodyMoveRotation(target, new TweenSettings<Vector3>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RigidbodyMoveRotation(target, new TweenSettings<Vector3>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween RigidbodyMovePosition([NotNull] Rigidbody target, Vector3 endValue, TweenSettings settings)
	{
		return RigidbodyMovePosition(target, new TweenSettings<Vector3>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween RigidbodyMovePosition([NotNull] Rigidbody target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return RigidbodyMovePosition(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	public static Tween RigidbodyMovePosition([NotNull] Rigidbody target, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RigidbodyMovePosition(target, new TweenSettings<Vector3>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween RigidbodyMovePosition([NotNull] Rigidbody target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RigidbodyMovePosition(target, new TweenSettings<Vector3>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween RigidbodyMovePosition([NotNull] Rigidbody target, TweenSettings<Vector3> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.RigidbodyMovePosition);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody target, Quaternion endValue, TweenSettings settings)
	{
		return RigidbodyMoveRotation(target, new TweenSettings<Quaternion>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody target, Quaternion startValue, Quaternion endValue, TweenSettings settings)
	{
		return RigidbodyMoveRotation(target, new TweenSettings<Quaternion>(startValue, endValue, settings));
	}

	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody target, Quaternion endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RigidbodyMoveRotation(target, new TweenSettings<Quaternion>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody target, Quaternion startValue, Quaternion endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RigidbodyMoveRotation(target, new TweenSettings<Quaternion>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody target, TweenSettings<Quaternion> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.RigidbodyMoveRotationQuaternion);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween RigidbodyMovePosition([NotNull] Rigidbody2D target, Vector2 endValue, TweenSettings settings)
	{
		return RigidbodyMovePosition(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween RigidbodyMovePosition([NotNull] Rigidbody2D target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return RigidbodyMovePosition(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween RigidbodyMovePosition([NotNull] Rigidbody2D target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RigidbodyMovePosition(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween RigidbodyMovePosition([NotNull] Rigidbody2D target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RigidbodyMovePosition(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween RigidbodyMovePosition([NotNull] Rigidbody2D target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.RigidbodyMovePosition2D);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody2D target, float endValue, TweenSettings settings)
	{
		return RigidbodyMoveRotation(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody2D target, float startValue, float endValue, TweenSettings settings)
	{
		return RigidbodyMoveRotation(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody2D target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RigidbodyMoveRotation(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody2D target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RigidbodyMoveRotation(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween RigidbodyMoveRotation([NotNull] Rigidbody2D target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.RigidbodyMoveRotation2D);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialColor([NotNull] Material target, Color endValue, TweenSettings settings)
	{
		return MaterialColor(target, new TweenSettings<Color>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialColor([NotNull] Material target, Color startValue, Color endValue, TweenSettings settings)
	{
		return MaterialColor(target, new TweenSettings<Color>(startValue, endValue, settings));
	}

	public static Tween MaterialColor([NotNull] Material target, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialColor(target, new TweenSettings<Color>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween MaterialColor([NotNull] Material target, Color startValue, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialColor(target, new TweenSettings<Color>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween MaterialColor([NotNull] Material target, TweenSettings<Color> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.MaterialColor);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialAlpha([NotNull] Material target, float endValue, TweenSettings settings)
	{
		return MaterialAlpha(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialAlpha([NotNull] Material target, float startValue, float endValue, TweenSettings settings)
	{
		return MaterialAlpha(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween MaterialAlpha([NotNull] Material target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialAlpha(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween MaterialAlpha([NotNull] Material target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialAlpha(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween MaterialAlpha([NotNull] Material target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.MaterialAlpha);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialMainTextureOffset([NotNull] Material target, Vector2 endValue, TweenSettings settings)
	{
		return MaterialMainTextureOffset(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialMainTextureOffset([NotNull] Material target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return MaterialMainTextureOffset(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween MaterialMainTextureOffset([NotNull] Material target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialMainTextureOffset(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween MaterialMainTextureOffset([NotNull] Material target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialMainTextureOffset(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween MaterialMainTextureOffset([NotNull] Material target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.MaterialMainTextureOffset);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialMainTextureScale([NotNull] Material target, Vector2 endValue, TweenSettings settings)
	{
		return MaterialMainTextureScale(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialMainTextureScale([NotNull] Material target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return MaterialMainTextureScale(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween MaterialMainTextureScale([NotNull] Material target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialMainTextureScale(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween MaterialMainTextureScale([NotNull] Material target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialMainTextureScale(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween MaterialMainTextureScale([NotNull] Material target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.MaterialMainTextureScale);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween AudioVolume([NotNull] AudioSource target, float endValue, TweenSettings settings)
	{
		return AudioVolume(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween AudioVolume([NotNull] AudioSource target, float startValue, float endValue, TweenSettings settings)
	{
		return AudioVolume(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween AudioVolume([NotNull] AudioSource target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return AudioVolume(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween AudioVolume([NotNull] AudioSource target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return AudioVolume(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween AudioVolume([NotNull] AudioSource target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.AudioVolume);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween AudioPitch([NotNull] AudioSource target, float endValue, TweenSettings settings)
	{
		return AudioPitch(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween AudioPitch([NotNull] AudioSource target, float startValue, float endValue, TweenSettings settings)
	{
		return AudioPitch(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween AudioPitch([NotNull] AudioSource target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return AudioPitch(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween AudioPitch([NotNull] AudioSource target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return AudioPitch(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween AudioPitch([NotNull] AudioSource target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.AudioPitch);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween AudioPanStereo([NotNull] AudioSource target, float endValue, TweenSettings settings)
	{
		return AudioPanStereo(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween AudioPanStereo([NotNull] AudioSource target, float startValue, float endValue, TweenSettings settings)
	{
		return AudioPanStereo(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween AudioPanStereo([NotNull] AudioSource target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return AudioPanStereo(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween AudioPanStereo([NotNull] AudioSource target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return AudioPanStereo(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween AudioPanStereo([NotNull] AudioSource target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.AudioPanStereo);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween VisualElementLayout([NotNull] VisualElement target, Rect endValue, TweenSettings settings)
	{
		return VisualElementLayout(target, new TweenSettings<Rect>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween VisualElementLayout([NotNull] VisualElement target, Rect startValue, Rect endValue, TweenSettings settings)
	{
		return VisualElementLayout(target, new TweenSettings<Rect>(startValue, endValue, settings));
	}

	public static Tween VisualElementLayout([NotNull] VisualElement target, Rect endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return VisualElementLayout(target, new TweenSettings<Rect>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween VisualElementLayout([NotNull] VisualElement target, Rect startValue, Rect endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return VisualElementLayout(target, new TweenSettings<Rect>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween VisualElementLayout([NotNull] VisualElement target, TweenSettings<Rect> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.VisualElementLayout);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Position([NotNull] ITransform target, Vector3 endValue, TweenSettings settings)
	{
		return Position(target, new TweenSettings<Vector3>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Position([NotNull] ITransform target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return Position(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	public static Tween Position([NotNull] ITransform target, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Position(target, new TweenSettings<Vector3>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Position([NotNull] ITransform target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Position(target, new TweenSettings<Vector3>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Position([NotNull] ITransform target, TweenSettings<Vector3> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.VisualElementPosition);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Rotation([NotNull] ITransform target, Quaternion endValue, TweenSettings settings)
	{
		return Rotation(target, new TweenSettings<Quaternion>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Rotation([NotNull] ITransform target, Quaternion startValue, Quaternion endValue, TweenSettings settings)
	{
		return Rotation(target, new TweenSettings<Quaternion>(startValue, endValue, settings));
	}

	public static Tween Rotation([NotNull] ITransform target, Quaternion endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Rotation(target, new TweenSettings<Quaternion>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Rotation([NotNull] ITransform target, Quaternion startValue, Quaternion endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Rotation(target, new TweenSettings<Quaternion>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Rotation([NotNull] ITransform target, TweenSettings<Quaternion> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.VisualElementRotationQuaternion);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Scale([NotNull] ITransform target, Vector3 endValue, TweenSettings settings)
	{
		return Scale(target, new TweenSettings<Vector3>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Scale([NotNull] ITransform target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return Scale(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	public static Tween Scale([NotNull] ITransform target, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Scale(target, new TweenSettings<Vector3>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Scale([NotNull] ITransform target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Scale(target, new TweenSettings<Vector3>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Scale([NotNull] ITransform target, TweenSettings<Vector3> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.VisualElementScale);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween VisualElementSize([NotNull] VisualElement target, Vector2 endValue, TweenSettings settings)
	{
		return VisualElementSize(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween VisualElementSize([NotNull] VisualElement target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return VisualElementSize(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween VisualElementSize([NotNull] VisualElement target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return VisualElementSize(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween VisualElementSize([NotNull] VisualElement target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return VisualElementSize(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween VisualElementSize([NotNull] VisualElement target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.VisualElementSize);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween VisualElementTopLeft([NotNull] VisualElement target, Vector2 endValue, TweenSettings settings)
	{
		return VisualElementTopLeft(target, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween VisualElementTopLeft([NotNull] VisualElement target, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return VisualElementTopLeft(target, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween VisualElementTopLeft([NotNull] VisualElement target, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return VisualElementTopLeft(target, new TweenSettings<Vector2>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween VisualElementTopLeft([NotNull] VisualElement target, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return VisualElementTopLeft(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween VisualElementTopLeft([NotNull] VisualElement target, TweenSettings<Vector2> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.VisualElementTopLeft);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween VisualElementColor([NotNull] VisualElement target, Color endValue, TweenSettings settings)
	{
		return VisualElementColor(target, new TweenSettings<Color>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween VisualElementColor([NotNull] VisualElement target, Color startValue, Color endValue, TweenSettings settings)
	{
		return VisualElementColor(target, new TweenSettings<Color>(startValue, endValue, settings));
	}

	public static Tween VisualElementColor([NotNull] VisualElement target, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return VisualElementColor(target, new TweenSettings<Color>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween VisualElementColor([NotNull] VisualElement target, Color startValue, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return VisualElementColor(target, new TweenSettings<Color>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween VisualElementColor([NotNull] VisualElement target, TweenSettings<Color> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.VisualElementColor);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Color([NotNull] VisualElement target, Color endValue, TweenSettings settings)
	{
		return Color(target, new TweenSettings<Color>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Color([NotNull] VisualElement target, Color startValue, Color endValue, TweenSettings settings)
	{
		return Color(target, new TweenSettings<Color>(startValue, endValue, settings));
	}

	public static Tween Color([NotNull] VisualElement target, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Color(target, new TweenSettings<Color>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Color([NotNull] VisualElement target, Color startValue, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Color(target, new TweenSettings<Color>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Color([NotNull] VisualElement target, TweenSettings<Color> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.VisualElementColor);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween VisualElementBackgroundColor([NotNull] VisualElement target, Color endValue, TweenSettings settings)
	{
		return VisualElementBackgroundColor(target, new TweenSettings<Color>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween VisualElementBackgroundColor([NotNull] VisualElement target, Color startValue, Color endValue, TweenSettings settings)
	{
		return VisualElementBackgroundColor(target, new TweenSettings<Color>(startValue, endValue, settings));
	}

	public static Tween VisualElementBackgroundColor([NotNull] VisualElement target, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return VisualElementBackgroundColor(target, new TweenSettings<Color>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween VisualElementBackgroundColor([NotNull] VisualElement target, Color startValue, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return VisualElementBackgroundColor(target, new TweenSettings<Color>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween VisualElementBackgroundColor([NotNull] VisualElement target, TweenSettings<Color> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.VisualElementBackgroundColor);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween VisualElementOpacity([NotNull] VisualElement target, float endValue, TweenSettings settings)
	{
		return VisualElementOpacity(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween VisualElementOpacity([NotNull] VisualElement target, float startValue, float endValue, TweenSettings settings)
	{
		return VisualElementOpacity(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween VisualElementOpacity([NotNull] VisualElement target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return VisualElementOpacity(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween VisualElementOpacity([NotNull] VisualElement target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return VisualElementOpacity(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween VisualElementOpacity([NotNull] VisualElement target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.VisualElementOpacity);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween Alpha([NotNull] VisualElement target, float endValue, TweenSettings settings)
	{
		return Alpha(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Alpha([NotNull] VisualElement target, float startValue, float endValue, TweenSettings settings)
	{
		return Alpha(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween Alpha([NotNull] VisualElement target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Alpha(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Alpha([NotNull] VisualElement target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Alpha(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween Alpha([NotNull] VisualElement target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.VisualElementOpacity);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween TextMaxVisibleCharacters([NotNull] TMP_Text target, int endValue, TweenSettings settings)
	{
		return TextMaxVisibleCharacters(target, new TweenSettings<int>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween TextMaxVisibleCharacters([NotNull] TMP_Text target, int startValue, int endValue, TweenSettings settings)
	{
		return TextMaxVisibleCharacters(target, new TweenSettings<int>(startValue, endValue, settings));
	}

	public static Tween TextMaxVisibleCharacters([NotNull] TMP_Text target, int endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return TextMaxVisibleCharacters(target, new TweenSettings<int>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween TextMaxVisibleCharacters([NotNull] TMP_Text target, int startValue, int endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return TextMaxVisibleCharacters(target, new TweenSettings<int>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween TextFontSize([NotNull] TMP_Text target, float endValue, TweenSettings settings)
	{
		return TextFontSize(target, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween TextFontSize([NotNull] TMP_Text target, float startValue, float endValue, TweenSettings settings)
	{
		return TextFontSize(target, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween TextFontSize([NotNull] TMP_Text target, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return TextFontSize(target, new TweenSettings<float>(endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween TextFontSize([NotNull] TMP_Text target, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return TextFontSize(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween TextFontSize([NotNull] TMP_Text target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.TextFontSize);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom(float startValue, float endValue, TweenSettings settings, [NotNull] Action<float> onValueChange)
	{
		return Custom(new TweenSettings<float>(startValue, endValue, settings), onValueChange);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom<T>([NotNull] T target, float startValue, float endValue, TweenSettings settings, [NotNull] Action<T, float> onValueChange) where T : class
	{
		return Custom_internal(target, new TweenSettings<float>(startValue, endValue, settings), onValueChange);
	}

	public static Tween Custom(float startValue, float endValue, float duration, [NotNull] Action<float> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Custom(new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, float startValue, float endValue, float duration, [NotNull] Action<T, float> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false) where T : class
	{
		return Custom_internal(target, new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, TweenSettings<float> settings, [NotNull] Action<T, float> onValueChange) where T : class
	{
		return Custom_internal(target, settings, onValueChange);
	}

	public static Tween Custom(TweenSettings<float> settings, [NotNull] Action<float> onValueChange)
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(PrimeTweenManager.dummyTarget, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomFloat, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			float obj = TweenData.FloatVal(startValue, easedInterpolationFactor, endValueOrDiff);
			Action<float> action = rt2.cold.customOnValueChange as Action<float>;
			try
			{
				action(obj);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween Custom_internal<T>([NotNull] T target, TweenSettings<float> settings, [NotNull] Action<T, float> onValueChange, bool isAdditive = false) where T : class
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		data.isAdditive = isAdditive;
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(target, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomFloat, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			bool isAdditive2 = d2.isAdditive;
			T arg = rt2.target as T;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			float arg2;
			if (isAdditive2)
			{
				float num = TweenData.FloatVal(startValue, easedInterpolationFactor, endValueOrDiff);
				arg2 = num.calcDelta(rt2.cold.prevVal);
				rt2.cold.prevVal.single = num;
			}
			else
			{
				arg2 = TweenData.FloatVal(startValue, easedInterpolationFactor, endValueOrDiff);
			}
			Action<T, float> action = rt2.cold.customOnValueChange as Action<T, float>;
			try
			{
				action(arg, arg2);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween animate(object target, ref TweenSettings<float> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween AnimateMaterial([CanBeNull] object target, long longParam, ref TweenSettings<float> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.longParam = longParam;
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom(Color startValue, Color endValue, TweenSettings settings, [NotNull] Action<Color> onValueChange)
	{
		return Custom(new TweenSettings<Color>(startValue, endValue, settings), onValueChange);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom<T>([NotNull] T target, Color startValue, Color endValue, TweenSettings settings, [NotNull] Action<T, Color> onValueChange) where T : class
	{
		return Custom_internal(target, new TweenSettings<Color>(startValue, endValue, settings), onValueChange);
	}

	public static Tween Custom(Color startValue, Color endValue, float duration, [NotNull] Action<Color> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Custom(new TweenSettings<Color>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, Color startValue, Color endValue, float duration, [NotNull] Action<T, Color> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false) where T : class
	{
		return Custom_internal(target, new TweenSettings<Color>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, TweenSettings<Color> settings, [NotNull] Action<T, Color> onValueChange) where T : class
	{
		return Custom_internal(target, settings, onValueChange);
	}

	public static Tween Custom(TweenSettings<Color> settings, [NotNull] Action<Color> onValueChange)
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(PrimeTweenManager.dummyTarget, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomColor, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			Color obj = TweenData.ColorVal(startValue, easedInterpolationFactor, endValueOrDiff);
			Action<Color> action = rt2.cold.customOnValueChange as Action<Color>;
			try
			{
				action(obj);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween Custom_internal<T>([NotNull] T target, TweenSettings<Color> settings, [NotNull] Action<T, Color> onValueChange, bool isAdditive = false) where T : class
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		data.isAdditive = isAdditive;
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(target, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomColor, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			bool isAdditive2 = d2.isAdditive;
			T arg = rt2.target as T;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			Color arg2;
			if (isAdditive2)
			{
				Color color = TweenData.ColorVal(startValue, easedInterpolationFactor, endValueOrDiff);
				arg2 = color.calcDelta(rt2.cold.prevVal);
				rt2.cold.prevVal.color = color;
			}
			else
			{
				arg2 = TweenData.ColorVal(startValue, easedInterpolationFactor, endValueOrDiff);
			}
			Action<T, Color> action = rt2.cold.customOnValueChange as Action<T, Color>;
			try
			{
				action(arg, arg2);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween animate(object target, ref TweenSettings<Color> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween AnimateMaterial([CanBeNull] object target, long longParam, ref TweenSettings<Color> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.longParam = longParam;
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom(Vector2 startValue, Vector2 endValue, TweenSettings settings, [NotNull] Action<Vector2> onValueChange)
	{
		return Custom(new TweenSettings<Vector2>(startValue, endValue, settings), onValueChange);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom<T>([NotNull] T target, Vector2 startValue, Vector2 endValue, TweenSettings settings, [NotNull] Action<T, Vector2> onValueChange) where T : class
	{
		return Custom_internal(target, new TweenSettings<Vector2>(startValue, endValue, settings), onValueChange);
	}

	public static Tween Custom(Vector2 startValue, Vector2 endValue, float duration, [NotNull] Action<Vector2> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Custom(new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, Vector2 startValue, Vector2 endValue, float duration, [NotNull] Action<T, Vector2> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false) where T : class
	{
		return Custom_internal(target, new TweenSettings<Vector2>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, TweenSettings<Vector2> settings, [NotNull] Action<T, Vector2> onValueChange) where T : class
	{
		return Custom_internal(target, settings, onValueChange);
	}

	public static Tween Custom(TweenSettings<Vector2> settings, [NotNull] Action<Vector2> onValueChange)
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(PrimeTweenManager.dummyTarget, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomVector2, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			Vector2 obj = TweenData.Vector2Val(startValue, easedInterpolationFactor, endValueOrDiff);
			Action<Vector2> action = rt2.cold.customOnValueChange as Action<Vector2>;
			try
			{
				action(obj);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween Custom_internal<T>([NotNull] T target, TweenSettings<Vector2> settings, [NotNull] Action<T, Vector2> onValueChange, bool isAdditive = false) where T : class
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		data.isAdditive = isAdditive;
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(target, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomVector2, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			bool isAdditive2 = d2.isAdditive;
			T arg = rt2.target as T;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			Vector2 arg2;
			if (isAdditive2)
			{
				Vector2 vector = TweenData.Vector2Val(startValue, easedInterpolationFactor, endValueOrDiff);
				arg2 = vector.calcDelta(rt2.cold.prevVal);
				rt2.cold.prevVal.vector2 = vector;
			}
			else
			{
				arg2 = TweenData.Vector2Val(startValue, easedInterpolationFactor, endValueOrDiff);
			}
			Action<T, Vector2> action = rt2.cold.customOnValueChange as Action<T, Vector2>;
			try
			{
				action(arg, arg2);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween animate(object target, ref TweenSettings<Vector2> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween AnimateMaterial([CanBeNull] object target, long longParam, ref TweenSettings<Vector2> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.longParam = longParam;
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom(Vector3 startValue, Vector3 endValue, TweenSettings settings, [NotNull] Action<Vector3> onValueChange)
	{
		return Custom(new TweenSettings<Vector3>(startValue, endValue, settings), onValueChange);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom<T>([NotNull] T target, Vector3 startValue, Vector3 endValue, TweenSettings settings, [NotNull] Action<T, Vector3> onValueChange) where T : class
	{
		return Custom_internal(target, new TweenSettings<Vector3>(startValue, endValue, settings), onValueChange);
	}

	public static Tween Custom(Vector3 startValue, Vector3 endValue, float duration, [NotNull] Action<Vector3> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Custom(new TweenSettings<Vector3>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, Vector3 startValue, Vector3 endValue, float duration, [NotNull] Action<T, Vector3> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false) where T : class
	{
		return Custom_internal(target, new TweenSettings<Vector3>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, TweenSettings<Vector3> settings, [NotNull] Action<T, Vector3> onValueChange) where T : class
	{
		return Custom_internal(target, settings, onValueChange);
	}

	public static Tween Custom(TweenSettings<Vector3> settings, [NotNull] Action<Vector3> onValueChange)
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(PrimeTweenManager.dummyTarget, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomVector3, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			Vector3 obj = TweenData.Vector3Val(startValue, easedInterpolationFactor, endValueOrDiff);
			Action<Vector3> action = rt2.cold.customOnValueChange as Action<Vector3>;
			try
			{
				action(obj);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween Custom_internal<T>([NotNull] T target, TweenSettings<Vector3> settings, [NotNull] Action<T, Vector3> onValueChange, bool isAdditive = false) where T : class
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		data.isAdditive = isAdditive;
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(target, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomVector3, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			bool isAdditive2 = d2.isAdditive;
			T arg = rt2.target as T;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			Vector3 arg2;
			if (isAdditive2)
			{
				Vector3 vector = TweenData.Vector3Val(startValue, easedInterpolationFactor, endValueOrDiff);
				arg2 = vector.calcDelta(rt2.cold.prevVal);
				rt2.cold.prevVal.vector3 = vector;
			}
			else
			{
				arg2 = TweenData.Vector3Val(startValue, easedInterpolationFactor, endValueOrDiff);
			}
			Action<T, Vector3> action = rt2.cold.customOnValueChange as Action<T, Vector3>;
			try
			{
				action(arg, arg2);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween animate(object target, ref TweenSettings<Vector3> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween AnimateMaterial([CanBeNull] object target, long longParam, ref TweenSettings<Vector3> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.longParam = longParam;
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom(Vector4 startValue, Vector4 endValue, TweenSettings settings, [NotNull] Action<Vector4> onValueChange)
	{
		return Custom(new TweenSettings<Vector4>(startValue, endValue, settings), onValueChange);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom<T>([NotNull] T target, Vector4 startValue, Vector4 endValue, TweenSettings settings, [NotNull] Action<T, Vector4> onValueChange) where T : class
	{
		return Custom_internal(target, new TweenSettings<Vector4>(startValue, endValue, settings), onValueChange);
	}

	public static Tween Custom(Vector4 startValue, Vector4 endValue, float duration, [NotNull] Action<Vector4> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Custom(new TweenSettings<Vector4>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, Vector4 startValue, Vector4 endValue, float duration, [NotNull] Action<T, Vector4> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false) where T : class
	{
		return Custom_internal(target, new TweenSettings<Vector4>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, TweenSettings<Vector4> settings, [NotNull] Action<T, Vector4> onValueChange) where T : class
	{
		return Custom_internal(target, settings, onValueChange);
	}

	public static Tween Custom(TweenSettings<Vector4> settings, [NotNull] Action<Vector4> onValueChange)
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(PrimeTweenManager.dummyTarget, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomVector4, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			Vector4 obj = TweenData.Vector4Val(startValue, easedInterpolationFactor, endValueOrDiff);
			Action<Vector4> action = rt2.cold.customOnValueChange as Action<Vector4>;
			try
			{
				action(obj);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween Custom_internal<T>([NotNull] T target, TweenSettings<Vector4> settings, [NotNull] Action<T, Vector4> onValueChange, bool isAdditive = false) where T : class
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		data.isAdditive = isAdditive;
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(target, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomVector4, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			bool isAdditive2 = d2.isAdditive;
			T arg = rt2.target as T;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			Vector4 arg2;
			if (isAdditive2)
			{
				Vector4 vector = TweenData.Vector4Val(startValue, easedInterpolationFactor, endValueOrDiff);
				arg2 = vector.calcDelta(rt2.cold.prevVal);
				rt2.cold.prevVal.vector4 = vector;
			}
			else
			{
				arg2 = TweenData.Vector4Val(startValue, easedInterpolationFactor, endValueOrDiff);
			}
			Action<T, Vector4> action = rt2.cold.customOnValueChange as Action<T, Vector4>;
			try
			{
				action(arg, arg2);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween animate(object target, ref TweenSettings<Vector4> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween AnimateMaterial([CanBeNull] object target, long longParam, ref TweenSettings<Vector4> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.longParam = longParam;
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom(Quaternion startValue, Quaternion endValue, TweenSettings settings, [NotNull] Action<Quaternion> onValueChange)
	{
		return Custom(new TweenSettings<Quaternion>(startValue, endValue, settings), onValueChange);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom<T>([NotNull] T target, Quaternion startValue, Quaternion endValue, TweenSettings settings, [NotNull] Action<T, Quaternion> onValueChange) where T : class
	{
		return Custom_internal(target, new TweenSettings<Quaternion>(startValue, endValue, settings), onValueChange);
	}

	public static Tween Custom(Quaternion startValue, Quaternion endValue, float duration, [NotNull] Action<Quaternion> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Custom(new TweenSettings<Quaternion>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, Quaternion startValue, Quaternion endValue, float duration, [NotNull] Action<T, Quaternion> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false) where T : class
	{
		return Custom_internal(target, new TweenSettings<Quaternion>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, TweenSettings<Quaternion> settings, [NotNull] Action<T, Quaternion> onValueChange) where T : class
	{
		return Custom_internal(target, settings, onValueChange);
	}

	public static Tween Custom(TweenSettings<Quaternion> settings, [NotNull] Action<Quaternion> onValueChange)
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(PrimeTweenManager.dummyTarget, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomQuaternion, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			Quaternion obj = TweenData.QuaternionVal(startValue, easedInterpolationFactor, endValueOrDiff);
			Action<Quaternion> action = rt2.cold.customOnValueChange as Action<Quaternion>;
			try
			{
				action(obj);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween Custom_internal<T>([NotNull] T target, TweenSettings<Quaternion> settings, [NotNull] Action<T, Quaternion> onValueChange, bool isAdditive = false) where T : class
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		data.isAdditive = isAdditive;
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(target, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomQuaternion, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			bool isAdditive2 = d2.isAdditive;
			T arg = rt2.target as T;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			Quaternion arg2;
			if (isAdditive2)
			{
				Quaternion quaternion = TweenData.QuaternionVal(startValue, easedInterpolationFactor, endValueOrDiff);
				arg2 = quaternion.calcDelta(rt2.cold.prevVal);
				rt2.cold.prevVal.quaternion = quaternion;
			}
			else
			{
				arg2 = TweenData.QuaternionVal(startValue, easedInterpolationFactor, endValueOrDiff);
			}
			Action<T, Quaternion> action = rt2.cold.customOnValueChange as Action<T, Quaternion>;
			try
			{
				action(arg, arg2);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween animate(object target, ref TweenSettings<Quaternion> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween AnimateMaterial([CanBeNull] object target, long longParam, ref TweenSettings<Quaternion> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.longParam = longParam;
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom(Rect startValue, Rect endValue, TweenSettings settings, [NotNull] Action<Rect> onValueChange)
	{
		return Custom(new TweenSettings<Rect>(startValue, endValue, settings), onValueChange);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween Custom<T>([NotNull] T target, Rect startValue, Rect endValue, TweenSettings settings, [NotNull] Action<T, Rect> onValueChange) where T : class
	{
		return Custom_internal(target, new TweenSettings<Rect>(startValue, endValue, settings), onValueChange);
	}

	public static Tween Custom(Rect startValue, Rect endValue, float duration, [NotNull] Action<Rect> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return Custom(new TweenSettings<Rect>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, Rect startValue, Rect endValue, float duration, [NotNull] Action<T, Rect> onValueChange, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false) where T : class
	{
		return Custom_internal(target, new TweenSettings<Rect>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime), onValueChange);
	}

	public static Tween Custom<T>([NotNull] T target, TweenSettings<Rect> settings, [NotNull] Action<T, Rect> onValueChange) where T : class
	{
		return Custom_internal(target, settings, onValueChange);
	}

	public static Tween Custom(TweenSettings<Rect> settings, [NotNull] Action<Rect> onValueChange)
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(PrimeTweenManager.dummyTarget, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomRect, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			Rect obj = TweenData.RectVal(startValue, easedInterpolationFactor, endValueOrDiff);
			Action<Rect> action = rt2.cold.customOnValueChange as Action<Rect>;
			try
			{
				action(obj);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween Custom_internal<T>([NotNull] T target, TweenSettings<Rect> settings, [NotNull] Action<T, Rect> onValueChange, bool isAdditive = false) where T : class
	{
		if (settings.startFromCurrent)
		{
			Debug.LogWarning("Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n");
		}
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		data.isAdditive = isAdditive;
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.customOnValueChange = onValueChange;
		coldData.Setup(target, ref settings.settings, _startFromCurrent: false, TweenAnimation.TweenType.CustomRect, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			if (d2.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return;
			}
			d2.isUpdating = true;
			TweenAnimation.ValueWrapper startValue = d2.startValue;
			float easedInterpolationFactor = d2.easedInterpolationFactor;
			bool isAdditive2 = d2.isAdditive;
			T arg = rt2.target as T;
			TweenAnimation.ValueWrapper endValueOrDiff = rt2.endValueOrDiff;
			Rect arg2;
			if (isAdditive2)
			{
				Rect rect = TweenData.RectVal(startValue, easedInterpolationFactor, endValueOrDiff);
				arg2 = rect.calcDelta(rt2.cold.prevVal);
				rt2.cold.prevVal.rect = rect;
			}
			else
			{
				arg2 = TweenData.RectVal(startValue, easedInterpolationFactor, endValueOrDiff);
			}
			Action<T, Rect> action = rt2.cold.customOnValueChange as Action<T, Rect>;
			try
			{
				action(arg, arg2);
			}
			finally
			{
				d2.isUpdating = false;
			}
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween animate(object target, ref TweenSettings<Rect> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	private static Tween AnimateMaterial([CanBeNull] object target, long longParam, ref TweenSettings<Rect> settings, TweenAnimation.TweenType _tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref UnmanagedTweenData data = ref coldData.data;
		ref TweenData managedData = ref coldData.managedData;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.longParam = longParam;
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, _tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	public static Tween PositionAtSpeed([NotNull] Transform target, Vector3 endValue, float averageSpeed, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return PositionAtSpeed(target, new TweenSettings<Vector3>(endValue, new TweenSettings(averageSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween PositionAtSpeed([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float averageSpeed, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return PositionAtSpeed(target, new TweenSettings<Vector3>(startValue, endValue, new TweenSettings(averageSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	private static Tween PositionAtSpeed([NotNull] Transform target, TweenSettings<Vector3> settings)
	{
		float num = settings.settings.duration;
		if (num <= 0f)
		{
			Debug.LogError(string.Format("Invalid speed provided to the Tween.{0}() method: {1}.", "PositionAtSpeed", num));
			return default(Tween);
		}
		if (settings.startFromCurrent)
		{
			settings.startFromCurrent = false;
			settings.startValue = target.position;
		}
		settings.settings.duration = Extensions.CalcDistance(settings.startValue, settings.endValue) / num;
		return Position(target, settings);
	}

	public static Tween LocalPositionAtSpeed([NotNull] Transform target, Vector3 endValue, float averageSpeed, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalPositionAtSpeed(target, new TweenSettings<Vector3>(endValue, new TweenSettings(averageSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween LocalPositionAtSpeed([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float averageSpeed, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalPositionAtSpeed(target, new TweenSettings<Vector3>(startValue, endValue, new TweenSettings(averageSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	private static Tween LocalPositionAtSpeed([NotNull] Transform target, TweenSettings<Vector3> settings)
	{
		float num = settings.settings.duration;
		if (num <= 0f)
		{
			Debug.LogError(string.Format("Invalid speed provided to the Tween.{0}() method: {1}.", "LocalPositionAtSpeed", num));
			return default(Tween);
		}
		if (settings.startFromCurrent)
		{
			settings.startFromCurrent = false;
			settings.startValue = target.localPosition;
		}
		settings.settings.duration = Extensions.CalcDistance(settings.startValue, settings.endValue) / num;
		return LocalPosition(target, settings);
	}

	public static Tween RotationAtSpeed([NotNull] Transform target, Quaternion endValue, float averageAngularSpeed, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RotationAtSpeed(target, new TweenSettings<Quaternion>(endValue, new TweenSettings(averageAngularSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween RotationAtSpeed([NotNull] Transform target, Quaternion startValue, Quaternion endValue, float averageAngularSpeed, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RotationAtSpeed(target, new TweenSettings<Quaternion>(startValue, endValue, new TweenSettings(averageAngularSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	private static Tween RotationAtSpeed([NotNull] Transform target, TweenSettings<Quaternion> settings)
	{
		float num = settings.settings.duration;
		if (num <= 0f)
		{
			Debug.LogError(string.Format("Invalid speed provided to the Tween.{0}() method: {1}.", "RotationAtSpeed", num));
			return default(Tween);
		}
		if (settings.startFromCurrent)
		{
			settings.startFromCurrent = false;
			settings.startValue = target.rotation;
		}
		settings.settings.duration = Extensions.CalcDistance(settings.startValue, settings.endValue) / num;
		return Rotation(target, settings);
	}

	public static Tween LocalRotationAtSpeed([NotNull] Transform target, Quaternion endValue, float averageAngularSpeed, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalRotationAtSpeed(target, new TweenSettings<Quaternion>(endValue, new TweenSettings(averageAngularSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween LocalRotationAtSpeed([NotNull] Transform target, Quaternion startValue, Quaternion endValue, float averageAngularSpeed, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalRotationAtSpeed(target, new TweenSettings<Quaternion>(startValue, endValue, new TweenSettings(averageAngularSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	private static Tween LocalRotationAtSpeed([NotNull] Transform target, TweenSettings<Quaternion> settings)
	{
		float num = settings.settings.duration;
		if (num <= 0f)
		{
			Debug.LogError(string.Format("Invalid speed provided to the Tween.{0}() method: {1}.", "LocalRotationAtSpeed", num));
			return default(Tween);
		}
		if (settings.startFromCurrent)
		{
			settings.startFromCurrent = false;
			settings.startValue = target.localRotation;
		}
		settings.settings.duration = Extensions.CalcDistance(settings.startValue, settings.endValue) / num;
		return LocalRotation(target, settings);
	}

	public static int GetTweensCount([CanBeNull] object onTarget = null)
	{
		PrimeTweenManager instance = PrimeTweenManager.Instance;
		if (onTarget == null && instance.updateDepth == 0)
		{
			return instance.tweensCount;
		}
		return PrimeTweenManager.ProcessAll(onTarget, (ColdData _) => true, allowToProcessTweensInsideSequence: true);
	}

	public static int StopAll([CanBeNull] object onTarget = null)
	{
		int result = PrimeTweenManager.ProcessAll(onTarget, delegate(ColdData tween)
		{
			ref UnmanagedTweenData data = ref tween.data;
			if (data.isInSequence)
			{
				if (data.IsMainSequenceRoot())
				{
					new Sequence(tween.sequence).Stop();
				}
			}
			else
			{
				tween.managedData.Kill(ref data);
			}
			return true;
		}, allowToProcessTweensInsideSequence: false);
		ForceUpdateManagerIfTargetIsNull(onTarget);
		return result;
	}

	public static int CompleteAll([CanBeNull] object onTarget = null)
	{
		PrimeTweenManager instance = PrimeTweenManager.Instance;
		if (instance.updateDepth != 0)
		{
			instance.completeAllRequested = true;
			instance.completeAllRequestedTarget = onTarget;
			if (onTarget != null)
			{
				return GetTweensCount(onTarget);
			}
			return instance.tweensCount;
		}
		instance.AddNewTweens(_UpdateType.Update);
		instance.AddNewTweens(_UpdateType.LateUpdate);
		instance.AddNewTweens(_UpdateType.FixedUpdate);
		int result = PrimeTweenManager.ProcessAll(onTarget, delegate(ColdData tween)
		{
			ref UnmanagedTweenData data = ref tween.data;
			if (data.isInSequence)
			{
				if (data.IsMainSequenceRoot())
				{
					new Sequence(tween.sequence).Complete();
				}
			}
			else
			{
				new Tween(tween).Complete();
			}
			return true;
		}, allowToProcessTweensInsideSequence: false);
		ForceUpdateManagerIfTargetIsNull(onTarget);
		return result;
	}

	private static void ForceUpdateManagerIfTargetIsNull([CanBeNull] object onTarget)
	{
		if (onTarget == null)
		{
			PrimeTweenManager instance = PrimeTweenManager.Instance;
			if (instance != null && instance.updateDepth == 0)
			{
				instance.UpdateTweens(_UpdateType.Update, 0f, 0f);
				instance.UpdateTweens(_UpdateType.LateUpdate, 0f, 0f);
				instance.UpdateTweens(_UpdateType.FixedUpdate, 0f, 0f);
			}
		}
	}

	public static int SetPausedAll(bool isPaused, [CanBeNull] object onTarget = null)
	{
		if (isPaused)
		{
			return PrimeTweenManager.ProcessAll(onTarget, (ColdData tween) => tween.data.trySetPause(isPaused: true), allowToProcessTweensInsideSequence: false);
		}
		return PrimeTweenManager.ProcessAll(onTarget, (ColdData tween) => tween.data.trySetPause(isPaused: false), allowToProcessTweensInsideSequence: false);
	}

	public static Tween Delay(float duration, [CanBeNull] Action onComplete = null, bool useUnscaledTime = false, bool? warnIfTargetDestroyed = null)
	{
		return DelayInternal(PrimeTweenManager.dummyTarget, duration, onComplete, useUnscaledTime, warnIfTargetDestroyed);
	}

	public static Tween Delay([NotNull] object target, float duration, [CanBeNull] Action onComplete = null, bool useUnscaledTime = false, bool? warnIfTargetDestroyed = null)
	{
		return DelayInternal(target, duration, onComplete, useUnscaledTime, warnIfTargetDestroyed);
	}

	private static Tween DelayInternal([CanBeNull] object target, float duration, [CanBeNull] Action onComplete, bool useUnscaledTime, bool? warnIfTargetDestroyed)
	{
		Tween? tween = delay_internal(target, duration, useUnscaledTime);
		if (onComplete != null)
		{
			tween?.tween.managedData.OnComplete(onComplete, warnIfTargetDestroyed);
		}
		return tween.GetValueOrDefault();
	}

	public static Tween Delay<T>([NotNull] T target, float duration, [NotNull] Action<T> onComplete, bool useUnscaledTime = false, bool? warnIfTargetDestroyed = null) where T : class
	{
		Tween? tween = delay_internal(target, duration, useUnscaledTime);
		if (!tween.HasValue)
		{
			return default(Tween);
		}
		Tween value = tween.Value;
		value.tween.managedData.OnComplete(target, onComplete, warnIfTargetDestroyed);
		return value;
	}

	private static Tween? delay_internal([CanBeNull] object target, float duration, bool useUnscaledTime)
	{
		PrimeTweenManager.CheckDuration(target, duration);
		return PrimeTweenManager.DelayWithoutDurationCheck(target, duration, useUnscaledTime);
	}

	public static Tween MaterialColor([NotNull] Material target, int propertyId, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialColor(target, propertyId, new TweenSettings<Color>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween MaterialColor([NotNull] Material target, int propertyId, Color startValue, Color endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialColor(target, propertyId, new TweenSettings<Color>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialColor([NotNull] Material target, int propertyId, Color endValue, TweenSettings settings)
	{
		return MaterialColor(target, propertyId, new TweenSettings<Color>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialColor([NotNull] Material target, int propertyId, Color startValue, Color endValue, TweenSettings settings)
	{
		return MaterialColor(target, propertyId, new TweenSettings<Color>(startValue, endValue, settings));
	}

	public static Tween MaterialColor([NotNull] Material target, int propertyId, TweenSettings<Color> settings)
	{
		return AnimateMaterial(target, propertyId, ref settings, TweenAnimation.TweenType.MaterialColorProperty);
	}

	public static Tween MaterialProperty([NotNull] Material target, int propertyId, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialProperty(target, propertyId, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween MaterialProperty([NotNull] Material target, int propertyId, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialProperty(target, propertyId, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialProperty([NotNull] Material target, int propertyId, float endValue, TweenSettings settings)
	{
		return MaterialProperty(target, propertyId, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialProperty([NotNull] Material target, int propertyId, float startValue, float endValue, TweenSettings settings)
	{
		return MaterialProperty(target, propertyId, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween MaterialProperty([NotNull] Material target, int propertyId, TweenSettings<float> settings)
	{
		return AnimateMaterial(target, propertyId, ref settings, TweenAnimation.TweenType.MaterialProperty);
	}

	public static Tween MaterialAlpha([NotNull] Material target, int propertyId, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialAlpha(target, propertyId, new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween MaterialAlpha([NotNull] Material target, int propertyId, float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialAlpha(target, propertyId, new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialAlpha([NotNull] Material target, int propertyId, float endValue, TweenSettings settings)
	{
		return MaterialAlpha(target, propertyId, new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialAlpha([NotNull] Material target, int propertyId, float startValue, float endValue, TweenSettings settings)
	{
		return MaterialAlpha(target, propertyId, new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween MaterialAlpha([NotNull] Material target, int propertyId, TweenSettings<float> settings)
	{
		return AnimateMaterial(target, propertyId, ref settings, TweenAnimation.TweenType.MaterialAlphaProperty);
	}

	public static Tween MaterialTextureOffset([NotNull] Material target, int propertyId, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialTextureOffset(target, propertyId, new TweenSettings<Vector2>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween MaterialTextureOffset([NotNull] Material target, int propertyId, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialTextureOffset(target, propertyId, new TweenSettings<Vector2>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialTextureOffset([NotNull] Material target, int propertyId, Vector2 endValue, TweenSettings settings)
	{
		return MaterialTextureOffset(target, propertyId, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialTextureOffset([NotNull] Material target, int propertyId, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return MaterialTextureOffset(target, propertyId, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween MaterialTextureOffset([NotNull] Material target, int propertyId, TweenSettings<Vector2> settings)
	{
		return AnimateMaterial(target, propertyId, ref settings, TweenAnimation.TweenType.MaterialTextureOffset);
	}

	public static Tween MaterialTextureScale([NotNull] Material target, int propertyId, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialTextureScale(target, propertyId, new TweenSettings<Vector2>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween MaterialTextureScale([NotNull] Material target, int propertyId, Vector2 startValue, Vector2 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialTextureScale(target, propertyId, new TweenSettings<Vector2>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialTextureScale([NotNull] Material target, int propertyId, Vector2 endValue, TweenSettings settings)
	{
		return MaterialTextureScale(target, propertyId, new TweenSettings<Vector2>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialTextureScale([NotNull] Material target, int propertyId, Vector2 startValue, Vector2 endValue, TweenSettings settings)
	{
		return MaterialTextureScale(target, propertyId, new TweenSettings<Vector2>(startValue, endValue, settings));
	}

	public static Tween MaterialTextureScale([NotNull] Material target, int propertyId, TweenSettings<Vector2> settings)
	{
		return AnimateMaterial(target, propertyId, ref settings, TweenAnimation.TweenType.MaterialTextureScale);
	}

	public static Tween MaterialProperty([NotNull] Material target, int propertyId, Vector4 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialProperty(target, propertyId, new TweenSettings<Vector4>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween MaterialProperty([NotNull] Material target, int propertyId, Vector4 startValue, Vector4 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return MaterialProperty(target, propertyId, new TweenSettings<Vector4>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialProperty([NotNull] Material target, int propertyId, Vector4 endValue, TweenSettings settings)
	{
		return MaterialProperty(target, propertyId, new TweenSettings<Vector4>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween MaterialProperty([NotNull] Material target, int propertyId, Vector4 startValue, Vector4 endValue, TweenSettings settings)
	{
		return MaterialProperty(target, propertyId, new TweenSettings<Vector4>(startValue, endValue, settings));
	}

	public static Tween MaterialProperty([NotNull] Material target, int propertyId, TweenSettings<Vector4> settings)
	{
		return AnimateMaterial(target, propertyId, ref settings, TweenAnimation.TweenType.MaterialPropertyVector4);
	}

	private static long PackPropertyIdsToLowHigh(string textureName)
	{
		int num = Shader.PropertyToID(textureName);
		if (!textureIdTo_ST.TryGetValue(num, out var value))
		{
			value = Shader.PropertyToID(textureName + "_ST");
			textureIdTo_ST.Add(num, value);
		}
		return ((long)value << 32) | (uint)num;
	}

	public static Tween EulerAngles([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return EulerAngles(target, new TweenSettings<Vector3>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween EulerAngles([NotNull] Transform target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return EulerAngles(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	public static Tween EulerAngles([NotNull] Transform target, TweenSettings<Vector3> settings)
	{
		ValidateEulerAnglesData(ref settings);
		return animate(target, ref settings, TweenAnimation.TweenType.EulerAngles);
	}

	public static Tween LocalEulerAngles([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalEulerAngles(target, new TweenSettings<Vector3>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween LocalEulerAngles([NotNull] Transform target, Vector3 startValue, Vector3 endValue, TweenSettings settings)
	{
		return LocalEulerAngles(target, new TweenSettings<Vector3>(startValue, endValue, settings));
	}

	public static Tween LocalEulerAngles([NotNull] Transform target, TweenSettings<Vector3> settings)
	{
		ValidateEulerAnglesData(ref settings);
		return animate(target, ref settings, TweenAnimation.TweenType.LocalEulerAngles);
	}

	private static void ValidateEulerAnglesData(ref TweenSettings<Vector3> settings)
	{
		if (settings.startFromCurrent)
		{
			settings.startFromCurrent = false;
			Debug.LogWarning("Animating euler angles from the current value may produce unexpected results because there is more than one way to represent the current rotation using Euler angles.\n'startFromCurrent' was ignored.\nMore info: https://docs.unity3d.com/ScriptReference/Transform-eulerAngles.html\n");
		}
	}

	public static Tween Scale([NotNull] Transform target, TweenSettings<float> uniformScaleSettings)
	{
		TweenSettings<Vector3> tweenSettings = new TweenSettings<Vector3>(uniformScaleSettings.startValue * Vector3.one, uniformScaleSettings.endValue * Vector3.one, uniformScaleSettings.settings);
		tweenSettings.startFromCurrent = uniformScaleSettings.startFromCurrent;
		TweenSettings<Vector3> settings = tweenSettings;
		return Scale(target, settings);
	}

	public static Tween Rotation([NotNull] Transform target, TweenSettings<Vector3> eulerAnglesSettings)
	{
		return Rotation(target, ToQuaternion(eulerAnglesSettings));
	}

	public static Tween LocalRotation([NotNull] Transform target, TweenSettings<Vector3> localEulerAnglesSettings)
	{
		return LocalRotation(target, ToQuaternion(localEulerAnglesSettings));
	}

	private static TweenSettings<Quaternion> ToQuaternion(TweenSettings<Vector3> s)
	{
		TweenSettings<Quaternion> result = new TweenSettings<Quaternion>(Quaternion.Euler(s.startValue), Quaternion.Euler(s.endValue), s.settings);
		result.startFromCurrent = s.startFromCurrent;
		return result;
	}

	public static Tween TextMaxVisibleCharacters([NotNull] TMP_Text target, TweenSettings<int> settings)
	{
		int characterCount = target.textInfo.characterCount;
		target.ForceMeshUpdate();
		if (characterCount != target.textInfo.characterCount)
		{
			Debug.LogWarning("Please call TMP_Text.ForceMeshUpdate() before animating maxVisibleCharacters.");
		}
		TweenSettings<float> settings2 = new TweenSettings<float>(settings.startValue, settings.endValue, settings.settings);
		return AnimateIntAsFloat(target, ref settings2, TweenAnimation.TweenType.TextMaxVisibleCharacters);
	}

	private static Tween AnimateIntAsFloat(object target, ref TweenSettings<float> settings, TweenAnimation.TweenType tweenType)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings.settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref settings.startValue);
		managedData.endValueOrDiff.CopyFrom(ref settings.endValue);
		coldData.Setup(target, ref settings.settings, settings.startFromCurrent, tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	public static Tween TextMaxVisibleCharactersNormalized([NotNull] TMP_Text target, TweenSettings<float> settings)
	{
		return animate(target, ref settings, TweenAnimation.TweenType.TextMaxVisibleCharactersNormalized);
	}

	public static Tween GlobalTimeScale(float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f)
	{
		return GlobalTimeScale(new TweenSettings<float>(endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime: true)));
	}

	public static Tween GlobalTimeScale(float startValue, float endValue, float duration, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f)
	{
		return GlobalTimeScale(new TweenSettings<float>(startValue, endValue, new TweenSettings(duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime: true)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(endValue, tweenSettings));\nTween.Position(transform, new (endValue, tweenSettings)); // shorter syntax")]
	public static Tween GlobalTimeScale(float endValue, TweenSettings settings)
	{
		return GlobalTimeScale(new TweenSettings<float>(endValue, settings));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use the overload with TweenSettings<T> instead.\n\nFor example, instead of:\nTween.Position(transform, startValue, endValue, tweenSettings);\n\nUse this:\nTween.Position(transform, new TweenSettings<Vector3>(startValue, endValue, tweenSettings));\nTween.Position(transform, new (startValue, endValue, tweenSettings)); // shorter syntax")]
	public static Tween GlobalTimeScale(float startValue, float endValue, TweenSettings settings)
	{
		return GlobalTimeScale(new TweenSettings<float>(startValue, endValue, settings));
	}

	public static Tween GlobalTimeScale(TweenSettings<float> settings)
	{
		clampTimescale(ref settings.startValue);
		clampTimescale(ref settings.endValue);
		if (!settings.settings.useUnscaledTime)
		{
			Debug.LogWarning("Setting useUnscaledTime to true to animate Time.timeScale correctly.");
			settings.settings.useUnscaledTime = true;
		}
		return animate(PrimeTweenManager.dummyTarget, ref settings, TweenAnimation.TweenType.GlobalTimeScale);
		static void clampTimescale(ref float value)
		{
			if (value < 0f)
			{
				Debug.LogError($"timeScale should be >= 0, but was {value}");
				value = 0f;
			}
		}
	}

	public static Tween TweenTimeScale(Tween tween, TweenSettings<float> settings)
	{
		return AnimateTimeScale(tween, settings, TweenAnimation.TweenType.TweenTimeScale);
	}

	private static Tween AnimateTimeScale(Tween tween, TweenSettings<float> settings, TweenAnimation.TweenType tweenType)
	{
		if (!tween.TryManipulate())
		{
			return default(Tween);
		}
		Tween result = animate(tween.tween, ref settings, tweenType);
		result.tween.longParam = tween.id;
		return result;
	}

	public static Tween TweenTimeScale(Sequence sequence, TweenSettings<float> settings)
	{
		return AnimateTimeScale(sequence.root, settings, TweenAnimation.TweenType.TweenTimeScaleSequence);
	}

	public static Tween RotationAtSpeed([NotNull] Transform target, Vector3 endValue, float averageAngularSpeed, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RotationAtSpeed(target, new TweenSettings<Vector3>(endValue, new TweenSettings(averageAngularSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween RotationAtSpeed([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float averageAngularSpeed, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return RotationAtSpeed(target, new TweenSettings<Vector3>(startValue, endValue, new TweenSettings(averageAngularSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	private static Tween RotationAtSpeed([NotNull] Transform target, TweenSettings<Vector3> settingsVector3)
	{
		TweenSettings<Quaternion> settings = ToQuaternion(settingsVector3);
		float num = settings.settings.duration;
		if (num <= 0f)
		{
			Debug.LogError(string.Format("Invalid speed provided to the Tween.{0}() method: {1}.", "RotationAtSpeed", num));
			return default(Tween);
		}
		if (settings.startFromCurrent)
		{
			settings.startFromCurrent = false;
			settings.startValue = target.rotation;
		}
		settings.settings.duration = Extensions.CalcDistance(settings.startValue, settings.endValue) / num;
		return Rotation(target, settings);
	}

	public static Tween LocalRotationAtSpeed([NotNull] Transform target, Vector3 endValue, float averageAngularSpeed, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalRotationAtSpeed(target, new TweenSettings<Vector3>(endValue, new TweenSettings(averageAngularSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween LocalRotationAtSpeed([NotNull] Transform target, Vector3 startValue, Vector3 endValue, float averageAngularSpeed, Easing ease = default(Easing), int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return LocalRotationAtSpeed(target, new TweenSettings<Vector3>(startValue, endValue, new TweenSettings(averageAngularSpeed, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime)));
	}

	private static Tween LocalRotationAtSpeed([NotNull] Transform target, TweenSettings<Vector3> settingsVector3)
	{
		TweenSettings<Quaternion> settings = ToQuaternion(settingsVector3);
		float num = settings.settings.duration;
		if (num <= 0f)
		{
			Debug.LogError(string.Format("Invalid speed provided to the Tween.{0}() method: {1}.", "LocalRotationAtSpeed", num));
			return default(Tween);
		}
		if (settings.startFromCurrent)
		{
			settings.startFromCurrent = false;
			settings.startValue = target.localRotation;
		}
		settings.settings.duration = Extensions.CalcDistance(settings.startValue, settings.endValue) / num;
		return LocalRotation(target, settings);
	}

	public static Tween RigidbodyMoveRotation(Rigidbody target, TweenSettings<Vector3> eulerAnglesSettings)
	{
		return RigidbodyMoveRotation(target, ToQuaternion(eulerAnglesSettings));
	}

	public static Sequence ShakeCamera([NotNull] Camera camera, float strengthFactor, float duration = 0.5f, float frequency = 10f, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		Transform transform = camera.transform;
		if (camera.orthographic)
		{
			float num = strengthFactor * camera.orthographicSize * 0.03f;
			return Sequence.Create().Group(ShakeLocalPosition(transform, new ShakeSettings(new Vector3(num, num), duration, frequency, enableFalloff: true, Ease.Default, 0f, 1, startDelay, endDelay, useUnscaledTime))).Group(ShakeLocalRotation(transform, new ShakeSettings(new Vector3(0f, 0f, strengthFactor * 0.6f), duration, frequency, enableFalloff: true, Ease.Default, 0f, 1, startDelay, endDelay, useUnscaledTime)));
		}
		return Sequence.Create().Group(ShakeLocalRotation(transform, new ShakeSettings(strengthFactor * Vector3.one, duration, frequency, enableFalloff: true, Ease.Default, 0f, 1, startDelay, endDelay, useUnscaledTime)));
	}

	public static Tween ShakeLocalPosition([NotNull] Transform target, Vector3 strength, float duration, float frequency = 10f, bool enableFalloff = true, Ease easeBetweenShakes = Ease.Default, float asymmetryFactor = 0f, int cycles = 1, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return ShakeLocalPosition(target, new ShakeSettings(strength, duration, frequency, enableFalloff, easeBetweenShakes, asymmetryFactor, cycles, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween ShakeLocalPosition([NotNull] Transform target, ShakeSettings settings)
	{
		return ShakeTransform(TweenAnimation.TweenType.ShakeLocalPosition, target, settings);
	}

	public static Tween PunchLocalPosition([NotNull] Transform target, Vector3 strength, float duration, float frequency = 10f, bool enableFalloff = true, Ease easeBetweenShakes = Ease.Default, float asymmetryFactor = 0f, int cycles = 1, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return PunchLocalPosition(target, new ShakeSettings(strength, duration, frequency, enableFalloff, easeBetweenShakes, asymmetryFactor, cycles, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween PunchLocalPosition([NotNull] Transform target, ShakeSettings settings)
	{
		return ShakeLocalPosition(target, settings.WithPunch());
	}

	public static Tween ShakeLocalRotation([NotNull] Transform target, Vector3 strength, float duration, float frequency = 10f, bool enableFalloff = true, Ease easeBetweenShakes = Ease.Default, float asymmetryFactor = 0f, int cycles = 1, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return ShakeLocalRotation(target, new ShakeSettings(strength, duration, frequency, enableFalloff, easeBetweenShakes, asymmetryFactor, cycles, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween ShakeLocalRotation([NotNull] Transform target, ShakeSettings settings)
	{
		return ShakeTransform(TweenAnimation.TweenType.ShakeLocalRotation, target, settings);
	}

	public static Tween PunchLocalRotation([NotNull] Transform target, Vector3 strength, float duration, float frequency = 10f, bool enableFalloff = true, Ease easeBetweenShakes = Ease.Default, float asymmetryFactor = 0f, int cycles = 1, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return PunchLocalRotation(target, new ShakeSettings(strength, duration, frequency, enableFalloff, easeBetweenShakes, asymmetryFactor, cycles, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween PunchLocalRotation([NotNull] Transform target, ShakeSettings settings)
	{
		return ShakeLocalRotation(target, settings.WithPunch());
	}

	public static Tween ShakeScale([NotNull] Transform target, Vector3 strength, float duration, float frequency = 10f, bool enableFalloff = true, Ease easeBetweenShakes = Ease.Default, float asymmetryFactor = 0f, int cycles = 1, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return ShakeScale(target, new ShakeSettings(strength, duration, frequency, enableFalloff, easeBetweenShakes, asymmetryFactor, cycles, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween ShakeScale([NotNull] Transform target, ShakeSettings settings)
	{
		return ShakeTransform(TweenAnimation.TweenType.ShakeScale, target, settings);
	}

	public static Tween PunchScale([NotNull] Transform target, Vector3 strength, float duration, float frequency = 10f, bool enableFalloff = true, Ease easeBetweenShakes = Ease.Default, float asymmetryFactor = 0f, int cycles = 1, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
	{
		return PunchScale(target, new ShakeSettings(strength, duration, frequency, enableFalloff, easeBetweenShakes, asymmetryFactor, cycles, startDelay, endDelay, useUnscaledTime));
	}

	public static Tween PunchScale([NotNull] Transform target, ShakeSettings settings)
	{
		return ShakeScale(target, settings.WithPunch());
	}

	private static Tween ShakeTransform(TweenAnimation.TweenType tweenType, [NotNull] Transform target, ShakeSettings settings)
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		prepareShakeData(settings, ref managedData, ref data, target);
		TweenSettings _settings = settings.tweenSettings;
		coldData.Setup(target, ref _settings, _startFromCurrent: true, tweenType, ref managedData, ref data);
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	public static Tween ShakeCustom<T>([NotNull] T target, Vector3 startValue, ShakeSettings settings, [NotNull] Action<T, Vector3> onValueChange) where T : class
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Tween);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		data.startValue.CopyFrom(ref startValue);
		prepareShakeData(settings, ref managedData, ref data, target);
		coldData.customOnValueChange = onValueChange;
		TweenSettings _settings = settings.tweenSettings;
		coldData.Setup(target, ref _settings, _startFromCurrent: false, TweenAnimation.TweenType.ShakeCustom, ref managedData, ref data);
		coldData.onValueChange = delegate(ref TweenData rt2, ref UnmanagedTweenData d2)
		{
			(rt2.cold.customOnValueChange as Action<T, Vector3>)(arg2: d2.startValue.vector3 + getShakeVal(ref rt2, ref d2), arg1: rt2.target as T);
		};
		return PrimeTweenManager.Animate(ref managedData, ref data);
	}

	public static Tween PunchCustom<T>([NotNull] T target, Vector3 startValue, ShakeSettings settings, [NotNull] Action<T, Vector3> onValueChange) where T : class
	{
		return ShakeCustom(target, startValue, settings.WithPunch(), onValueChange);
	}

	private static void prepareShakeData(ShakeSettings settings, ref TweenData rt, ref UnmanagedTweenData d, object target)
	{
		rt.endValueOrDiff.Reset();
		rt.cold.shakeData.Setup(settings, ref rt, ref d, target);
	}

	internal static Vector3 getShakeVal(ref TweenData rt, ref UnmanagedTweenData d)
	{
		float num = calcFadeInOutFactor(ref rt, ref d);
		return rt.shakeData.getNextVal(ref rt, ref d) * num;
	}

	private static float calcFadeInOutFactor(ref TweenData tween, ref UnmanagedTweenData d)
	{
		float animationDuration = d.animationDuration;
		float num = d.easedInterpolationFactor * animationDuration;
		if (animationDuration == 0f)
		{
			return 0f;
		}
		float num2 = animationDuration * 0.5f;
		float num3 = 1f / tween.cold.shakeData.frequency;
		if (num3 > num2)
		{
			num3 = num2;
		}
		float num4 = num3 * 0.5f;
		if (num < num4)
		{
			return Mathf.InverseLerp(0f, num4, num);
		}
		float num5 = animationDuration - num3;
		if (num > num5)
		{
			return Mathf.InverseLerp(animationDuration, num5, num);
		}
		return 1f;
	}

	internal Tween([NotNull] ColdData tween)
	{
		id = tween.id;
		this.tween = tween;
	}

	private void SetElapsedTime(float value)
	{
		if (!TryManipulate())
		{
			return;
		}
		if (value < 0f || float.IsNaN(value))
		{
			Debug.LogError($"Invalid elapsedTime value: {value}, tween: {ToString()}");
			return;
		}
		float num = duration;
		if (value > num)
		{
			value = num;
		}
		int num2 = cyclesDone;
		if (num2 == cyclesTotal)
		{
			num2--;
		}
		SetElapsedTimeTotal(value + num * (float)num2);
	}

	[NotNull]
	public override string ToString()
	{
		if (isAlive && tween.hasData)
		{
			return tween.managedData.GetDescription();
		}
		return $"DEAD / id {id}";
	}

	private void SetElapsedTimeTotal(float value)
	{
		if (!TryManipulate())
		{
			return;
		}
		if (value < 0f || float.IsNaN(value) || (cyclesTotal == -1 && value >= float.MaxValue))
		{
			Debug.LogError($"Invalid elapsedTimeTotal value: {value}, tween: {ToString()}");
			return;
		}
		ref TweenData managedData = ref tween.managedData;
		ref UnmanagedTweenData data = ref tween.data;
		managedData.SetElapsedTimeTotal(value, earlyExitSequenceIfPaused: false, ref data);
		if (data.isAlive)
		{
			float num = durationTotal;
			if (value > num)
			{
				data.elapsedTimeTotal = num;
			}
		}
	}

	private float GetProgressFromState()
	{
		if ((tween.data.flags & Flags.StateAfter) == 0)
		{
			return 0f;
		}
		return 1f;
	}

	public void Stop()
	{
		if (isAlive && TryManipulate(checkRecursive: false))
		{
			tween.managedData.Kill(ref tween.data);
		}
	}

	public void Complete()
	{
		if (isAlive && TryManipulate(checkRecursive: false))
		{
			tween.managedData.ForceComplete(ref tween.data);
		}
	}

	internal bool TryManipulate(bool checkRecursive = true)
	{
		if (!ValidateIsAlive())
		{
			return false;
		}
		ref UnmanagedTweenData data = ref tween.data;
		if (!data.canManipulate())
		{
			tween.managedData.LogErrorWithStackTrace("It's not allowed to manipulate 'nested' animations, please use the parent Sequence instead.\nWhen an animation is added to another sequence, it becomes 'nested', and manipulating it directly is no longer allowed.\nUse Stop()/Complete()/isPaused/timeScale/elapsedTime/etc. of the parent animation instead.\n");
			return false;
		}
		if (data.isInSequence)
		{
			if (checkRecursive)
			{
				Sequence.SequenceChildrenEnumerator enumerator = new Sequence(tween).GetAllTweens().GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.data.isUpdating)
					{
						Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
						return false;
					}
				}
			}
			else
			{
				Sequence.SequenceChildrenEnumerator enumerator = new Sequence(tween).GetAllTweens().GetEnumerator();
				while (enumerator.MoveNext())
				{
					enumerator.Current.data.isUpdating = false;
				}
			}
		}
		else if (checkRecursive)
		{
			if (data.isUpdating)
			{
				Debug.LogError("Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().");
				return false;
			}
		}
		else
		{
			data.isUpdating = false;
		}
		return true;
	}

	public void SetRemainingCycles(bool stopAtEndValue)
	{
		if (TryManipulate())
		{
			ref UnmanagedTweenData data = ref tween.data;
			if (data.cycleMode == CycleMode.Restart || data.cycleMode == CycleMode.Incremental)
			{
				Debug.LogWarning("SetRemainingCycles(bool stopAtEndValue) is meant to be used with CycleMode.Yoyo or Rewind. Please consider using the overload that accepts int instead.");
			}
			bool flag = data.getCyclesDone() % 2 == 0 == stopAtEndValue;
			if (tween.data.isSequenceInverted)
			{
				flag = !flag;
			}
			SetRemainingCycles(flag ? 1 : 2);
		}
	}

	public void SetRemainingCycles(int cycles)
	{
		if (!TryManipulate())
		{
			return;
		}
		ref UnmanagedTweenData data = ref tween.data;
		if (data.tweenType == TweenAnimation.TweenType.Delay && tween.managedData.HasOnComplete)
		{
			Debug.LogError("Applying cycles to Delay will not repeat the OnComplete() callback, but instead will increase the Delay duration.\nOnComplete() is called only once when ALL tween cycles complete. To repeat the OnComplete() callback, please use the Sequence.Create(cycles: numCycles) and put the tween inside a Sequence.\nMore info: https://discussions.unity.com/t/926420/101\n");
		}
		if (cycles == -1)
		{
			if (data.timeScale > 0f)
			{
				data.cyclesTotal = -1;
			}
			else
			{
				Debug.LogError("'SetRemainingCycles()' doesn't work with negative 'timeScale' and infinite(-1) 'cycles'.");
			}
			return;
		}
		TweenSettings.setCyclesTo1If0(ref cycles);
		if (data.timeScale > 0f)
		{
			data.cyclesTotal = data.getCyclesDone() + cycles;
			return;
		}
		int num = cycles - 1;
		data.elapsedTimeTotal = (float)num * data.cycleDuration + elapsedTime;
		data.cyclesDone = num;
		if (data.cyclesTotal < num)
		{
			data.cyclesTotal = num + 1;
		}
	}

	public Tween OnComplete([CanBeNull] Action onComplete, bool? warnIfTargetDestroyed = null)
	{
		if (ValidateIsAlive())
		{
			tween.managedData.OnComplete(onComplete, warnIfTargetDestroyed);
		}
		return this;
	}

	public Tween OnComplete<T>([NotNull] T target, [CanBeNull] Action<T> onComplete, bool? warnIfTargetDestroyed = null) where T : class
	{
		if (ValidateIsAlive())
		{
			tween.managedData.OnComplete(target, onComplete, warnIfTargetDestroyed);
		}
		return this;
	}

	public Sequence Group(Tween _tween)
	{
		if (!TryManipulate())
		{
			return default(Sequence);
		}
		return Sequence.Create(this).Group(_tween);
	}

	public Sequence Chain(Tween _tween)
	{
		if (!TryManipulate())
		{
			return default(Sequence);
		}
		return Sequence.Create(this).Chain(_tween);
	}

	public Sequence Group(Sequence sequence)
	{
		if (!TryManipulate())
		{
			return default(Sequence);
		}
		return Sequence.Create(this).Group(sequence);
	}

	public Sequence Chain(Sequence sequence)
	{
		if (!TryManipulate())
		{
			return default(Sequence);
		}
		return Sequence.Create(this).Chain(sequence);
	}

	internal bool ValidateIsAlive()
	{
		if (!IsCreated)
		{
			if (!PrimeTweenManager.Instance.isDestroyed)
			{
				Debug.LogError("Animation is not created. Please check the 'isAlive' property before calling this API.\n- Use 'Sequence.Create()' to start a Sequence.\n- Use static 'Tween.' methods to start a Tween.\n");
			}
		}
		else if (!isAlive)
		{
			Assert.LogErrorWithStackTrace("Animation is not alive. Please check the 'isAlive' property before calling this API.\n", id, null);
		}
		return isAlive;
	}

	public Tween OnUpdate<T>(T target, Action<T, Tween> onUpdate) where T : class
	{
		if (ValidateIsAlive())
		{
			tween.managedData.SetOnUpdate(target, onUpdate);
		}
		return this;
	}

	public override int GetHashCode()
	{
		return id.GetHashCode();
	}

	public bool Equals(Tween other)
	{
		if (isAlive && other.isAlive)
		{
			return id == other.id;
		}
		return false;
	}

	[Obsolete("This method was renamed to 'ResetOnCompletion'.", true)]
	private Tween ResetOnComplete()
	{
		return ResetOnCompletion();
	}

	internal Tween ResetOnCompletion()
	{
		if (ValidateIsAlive())
		{
			tween.data.resetOnComplete = true;
		}
		return this;
	}
}
