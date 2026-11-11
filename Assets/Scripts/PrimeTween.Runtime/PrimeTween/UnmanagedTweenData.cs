using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace PrimeTween;

[StructLayout(LayoutKind.Explicit, Size = 64)]
internal struct UnmanagedTweenData
{
	[FieldOffset(0)]
	internal Flags flags;

	[FieldOffset(4)]
	internal float elapsedTimeTotal;

	[FieldOffset(8)]
	internal float timeScale;

	[FieldOffset(12)]
	internal int cyclesDone;

	[FieldOffset(16)]
	internal int cyclesTotal;

	[FieldOffset(20)]
	internal float waitDelay;

	[FieldOffset(24)]
	internal float cycleDuration;

	[FieldOffset(28)]
	internal float startDelay;

	[FieldOffset(32)]
	internal float animationDuration;

	[FieldOffset(36)]
	internal Ease ease;

	[FieldOffset(37)]
	internal CycleMode cycleMode;

	[FieldOffset(38)]
	internal TweenAnimation.TweenType tweenType;

	[FieldOffset(39)]
	internal _UpdateType updateType;

	[FieldOffset(40)]
	internal float easedInterpolationFactor;

	[FieldOffset(48)]
	internal TweenAnimation.ValueWrapper startValue;

	internal long id
	{
		get
		{
			return -1L;
		}
		set
		{
		}
	}

	internal bool isPaused
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.IsPaused);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.IsPaused, value);
		}
	}

	internal bool isInSequence
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.IsInSequence);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.IsInSequence, value);
		}
	}

	internal bool useUnscaledTime
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.UseUnscaledTime);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.UseUnscaledTime, value);
		}
	}

	internal bool isValueChanged
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.IsValueChanged);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.IsValueChanged, value);
		}
	}

	internal bool isDone
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.IsDone);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.IsDone, value);
		}
	}

	internal bool startFromCurrent
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.StartFromCurrent);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.StartFromCurrent, value);
		}
	}

	internal bool isAlive
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.IsAlive);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.IsAlive, value);
		}
	}

	internal bool isUpdatedInJob
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.IsUpdatedInJob);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.IsUpdatedInJob, value);
		}
	}

	internal bool hasOnUpdate
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.HasOnUpdate);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.HasOnUpdate, value);
		}
	}

	internal bool isInCoroutine
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.IsInCoroutine);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.IsInCoroutine, value);
		}
	}

	internal bool resetOnComplete
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.ResetOnComplete);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.ResetOnComplete, value);
		}
	}

	internal bool stoppedEmergently
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.StoppedEmergently);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.StoppedEmergently, value);
		}
	}

	internal bool isAdditive
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.Additive);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.Additive, value);
		}
	}

	internal bool warnEndValueEqualsCurrent
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.WarnEndValueEqualsCurrent);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.WarnEndValueEqualsCurrent, value);
		}
	}

	internal bool isSequenceInverted
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.IsSequenceInverted);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.IsSequenceInverted, value);
		}
	}

	internal bool shakeSign
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.ShakeSign);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.ShakeSign, value);
		}
	}

	internal bool isPunch
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.ShakePunch);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.ShakePunch, value);
		}
	}

	internal bool isUpdating
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.IsUpdating);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.IsUpdating, value);
		}
	}

	internal bool warnIgnoredOnCompleteIfTargetDestroyed
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.WarnIgnoredOnCompleteIfTargetDestroyed);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.WarnIgnoredOnCompleteIfTargetDestroyed, value);
		}
	}

	internal bool IsCustomEaseSameStartEndValues
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetFlag(Flags.IsCustomEaseSameStartEndValues);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			SetFlag(Flags.IsCustomEaseSameStartEndValues, value);
		}
	}

	internal PropType propType => Utils.TweenTypeToTweenData(tweenType).Item1;

	private float calcTFromElapsedTimeTotal(float _elapsedTimeTotal, out Flags newState)
	{
		if (_elapsedTimeTotal == float.MaxValue)
		{
			cyclesDone = cyclesTotal;
			newState = Flags.StateAfter;
			return 1f;
		}
		_elapsedTimeTotal -= waitDelay;
		if (_elapsedTimeTotal < 0f)
		{
			cyclesDone = -1;
			newState = Flags.StateBefore;
			return 0f;
		}
		if (cycleDuration == 0f)
		{
			if (cyclesTotal == -1)
			{
				if (timeScale > 0f)
				{
					if (cyclesDone == -1)
					{
						cyclesDone = 1;
					}
					else
					{
						cyclesDone++;
					}
				}
				else if (timeScale != 0f)
				{
					cyclesDone--;
					if (cyclesDone == -1)
					{
						newState = Flags.StateBefore;
						return 0f;
					}
				}
				newState = Flags.StateRunning;
				return 1f;
			}
			if (_elapsedTimeTotal == 0f)
			{
				cyclesDone = -1;
				newState = Flags.StateBefore;
				return 0f;
			}
			cyclesDone = cyclesTotal;
			newState = Flags.StateAfter;
			return 1f;
		}
		cyclesDone = (int)(_elapsedTimeTotal / cycleDuration);
		if (cyclesTotal != -1 && cyclesDone > cyclesTotal)
		{
			cyclesDone = cyclesTotal;
		}
		if (cyclesTotal != -1 && cyclesDone == cyclesTotal)
		{
			newState = Flags.StateAfter;
			return 1f;
		}
		float num = _elapsedTimeTotal - cycleDuration * (float)cyclesDone - startDelay;
		if (num < 0f)
		{
			newState = Flags.StateBefore;
			return 0f;
		}
		if (animationDuration == 0f)
		{
			newState = Flags.StateAfter;
			return 1f;
		}
		float num2 = num / animationDuration;
		if (num2 > 1f)
		{
			newState = Flags.StateAfter;
			return 1f;
		}
		newState = Flags.StateRunning;
		return num2;
	}

	internal bool canManipulate()
	{
		if (isInSequence)
		{
			return tweenType == TweenAnimation.TweenType.MainSequence;
		}
		return true;
	}

	internal bool IsMainSequenceRoot()
	{
		return tweenType == TweenAnimation.TweenType.MainSequence;
	}

	internal bool IsSequenceRoot()
	{
		if (tweenType != TweenAnimation.TweenType.MainSequence)
		{
			return tweenType == TweenAnimation.TweenType.NestedSequence;
		}
		return true;
	}

	internal bool trySetPause(bool isPaused)
	{
		if (this.isPaused == isPaused)
		{
			return false;
		}
		this.isPaused = isPaused;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool GetFlag(Flags flag)
	{
		return (flags & flag) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void SetFlag(Flags flag, bool value)
	{
		if (value)
		{
			flags |= flag;
		}
		else
		{
			flags &= ~flag;
		}
	}

	internal bool IsDone(int cyclesDiff)
	{
		if (timeScale > 0f)
		{
			if (cyclesDiff > 0)
			{
				return cyclesDone == cyclesTotal;
			}
			return false;
		}
		if (cyclesDiff < 0)
		{
			return cyclesDone == -1;
		}
		return false;
	}

	internal bool CanUpdateData()
	{
		if (isAlive && !isInSequence && ease != Ease.Custom)
		{
			return !startFromCurrent;
		}
		return false;
	}

	internal float UpdateData(float newElapsedTimeTotal)
	{
		elapsedTimeTotal = newElapsedTimeTotal;
		int num = cyclesDone;
		Flags newState;
		float result = calcTFromElapsedTimeTotal(elapsedTimeTotal, out newState);
		int cyclesDiff = cyclesDone - num;
		isDone = IsDone(cyclesDiff);
		bool flag = newState == Flags.StateRunning || (flags & newState) == 0;
		isValueChanged = flag;
		if (isValueChanged)
		{
			flags &= ~(Flags.StateBefore | Flags.StateRunning | Flags.StateAfter);
			flags |= newState;
		}
		return result;
	}

	[Conditional("PRIME_TWEEN_SAFETY_CHECKS")]
	internal void print(string msg)
	{
		UnityEngine.Debug.Log($"[{Time.frameCount}] id:{id}  {msg}");
	}

	internal float calcEasedT(float t)
	{
		switch (cycleMode)
		{
		case CycleMode.Restart:
			return evaluate(t);
		case CycleMode.Incremental:
			return evaluate(t) + (float)clampCyclesDone(cyclesDone);
		case CycleMode.Yoyo:
		case (CycleMode)4:
			if (!IsForwardCycle(cyclesDone))
			{
				return 1f - evaluate(t);
			}
			return evaluate(t);
		case CycleMode.Rewind:
			if (!IsForwardCycle(cyclesDone))
			{
				return evaluate(1f - t);
			}
			return evaluate(t);
		default:
			throw new Exception();
		}
	}

	private float evaluate(float t)
	{
		return StandardEasing.Evaluate(t, ease);
	}

	internal int clampCyclesDone(int cyclesDone_)
	{
		if (cyclesDone_ == -1)
		{
			return 0;
		}
		if (cyclesDone_ == cyclesTotal)
		{
			return cyclesTotal - 1;
		}
		return cyclesDone_;
	}

	internal bool IsForwardCycle(int cycle)
	{
		return clampCyclesDone(cycle) % 2 == 0;
	}

	internal int getCyclesDone()
	{
		int num = cyclesDone;
		if (num == -1)
		{
			return 0;
		}
		return num;
	}

	internal float calcDurationWithWaitDependencies()
	{
		int num = cyclesTotal;
		return waitDelay + cycleDuration * (float)num;
	}

	internal unsafe bool IsDefault()
	{
		UnmanagedTweenData output = default(UnmanagedTweenData);
		return UnsafeUtility.MemCmp(UnsafeUtility.AddressOf(ref this), UnsafeUtility.AddressOf(ref output), UnsafeUtility.SizeOf<UnmanagedTweenData>()) == 0;
	}
}
