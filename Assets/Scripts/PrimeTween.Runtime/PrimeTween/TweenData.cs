using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace PrimeTween;

[StructLayout(LayoutKind.Explicit, Size = 32)]
internal struct TweenData : IEquatable<TweenData>
{
	[FieldOffset(0)]
	internal TweenAnimation.ValueWrapper endValueOrDiff;

	[FieldOffset(16)]
	[CanBeNull]
	internal object target;

	[FieldOffset(24)]
	internal ColdData cold;

	internal const float negativeElapsedTime = -1000f;

	private static readonly StringBuilder _sb = new StringBuilder();

	internal const int iniCyclesDone = -1;

	internal ref ColdData sequence => ref cold.sequence;

	internal ref ShakeData shakeData => ref cold.shakeData;

	internal ref long id => ref cold.id;

	internal int intParam => (int)cold.longParam;

	internal bool HasOnComplete => cold.onComplete != null;

	internal bool UpdateAndCheckIfRunning(float dt, ref UnmanagedTweenData d)
	{
		if (!d.isAlive)
		{
			return d.isInSequence;
		}
		if (!d.isPaused)
		{
			return SetElapsedTimeTotal(d.elapsedTimeTotal + dt * d.timeScale, earlyExitSequenceIfPaused: true, ref d);
		}
		if (IsUnityTargetDestroyed())
		{
			EmergencyStop(isTargetDestroyed: true, ref d);
			return false;
		}
		return d.isAlive;
	}

	internal bool SetElapsedTimeTotal(float newElapsedTimeTotal, bool earlyExitSequenceIfPaused, ref UnmanagedTweenData d)
	{
		if (d.isUpdatedInJob)
		{
			d.isUpdatedInJob = false;
			bool isDone = d.isDone;
			if (d.isValueChanged && !ReportOnValueChange(ref d))
			{
				return false;
			}
			if (isDone && d.isAlive)
			{
				if (!d.isPaused)
				{
					Kill(ref d);
				}
				ReportOnComplete(ref d);
				return false;
			}
			return true;
		}
		if (d.isInSequence)
		{
			if (d.IsMainSequenceRoot())
			{
				UpdateSequence(newElapsedTimeTotal, isRestart: false, earlyExitSequenceIfPaused, allowSkipChildrenUpdate: true, invertEase: false, ref d);
			}
		}
		else
		{
			UpdateAndSetElapsedTimeTotal(newElapsedTimeTotal, out var cyclesDiff, invertEase: false, ref d);
			if (!d.stoppedEmergently && d.isAlive && d.IsDone(cyclesDiff))
			{
				if (!d.isPaused)
				{
					Kill(ref d);
				}
				ReportOnComplete(ref d);
			}
		}
		return d.isAlive;
	}

	internal void UpdateSequence(float _elapsedTimeTotal, bool isRestart, bool earlyExitSequenceIfPaused, bool allowSkipChildrenUpdate, bool invertEase, ref UnmanagedTweenData d)
	{
		bool flag = (d.clampCyclesDone(d.cyclesDone) % 2 != 0) ^ d.isSequenceInverted;
		float num = FloatVal(d.startValue, d.easedInterpolationFactor, endValueOrDiff);
		bool invertEase2 = (d.cyclesTotal == 1 && flag && d.cycleMode == CycleMode.Rewind) ^ invertEase;
		if (!UpdateAndSetElapsedTimeTotal(_elapsedTimeTotal, out var cyclesDiff, invertEase2, ref d) && allowSkipChildrenUpdate)
		{
			return;
		}
		if (d.cycleMode == (CycleMode)4 && flag)
		{
			invertEase = !invertEase;
		}
		bool flag2 = isRestart && cyclesDiff < 0;
		Sequence.SequenceDirectEnumerator enumerator;
		if (cyclesDiff != 0 && !flag2)
		{
			if (isRestart)
			{
				cyclesDiff = 1;
			}
			int num2 = Mathf.Abs(cyclesDiff);
			d.cyclesDone -= cyclesDiff;
			int num3 = ((cyclesDiff > 0) ? 1 : (-1));
			float t = ((num3 > 0) ? 1f : 0f);
			for (int i = 0; i < num2; i++)
			{
				if (d.cyclesDone == d.cyclesTotal || d.cyclesDone == -1)
				{
					d.cyclesDone += num3;
					continue;
				}
				bool flag3 = (CalcEasedT(t, d.cyclesDone, invertEase: false, ref d) > 0.5f) ^ d.isSequenceInverted;
				float encompassingElapsedTime = (flag3 ? float.MaxValue : (-1000f));
				enumerator = GetSequenceSelfChildren(flag3).GetEnumerator();
				while (enumerator.MoveNext())
				{
					ColdData current = enumerator.Current;
					current.managedData.UpdateSequenceChild(encompassingElapsedTime, isRestart, invertEase, ref current.data);
					if (isEarlyExitAfterChildUpdate(ref d))
					{
						return;
					}
				}
				d.cyclesDone += num3;
				if (d.cycleMode != CycleMode.Restart || d.cyclesDone == d.cyclesTotal || d.cyclesDone == -1)
				{
					continue;
				}
				float num4 = ((!flag3) ? float.MaxValue : (-1000f));
				num = num4;
				enumerator = GetSequenceSelfChildren(!flag3).GetEnumerator();
				while (enumerator.MoveNext())
				{
					ColdData current2 = enumerator.Current;
					current2.managedData.UpdateSequenceChild(num4, isRestart: true, invertEase, ref current2.data);
					if (isEarlyExitAfterChildUpdate(ref d))
					{
						return;
					}
				}
			}
			if (d.IsDone(cyclesDiff))
			{
				if (d.resetOnComplete && d.IsMainSequenceRoot())
				{
					ResetSequence(sequence);
				}
				if (d.IsMainSequenceRoot() && !d.isPaused)
				{
					new Sequence(sequence).ReleaseTweens();
				}
				ReportOnComplete(ref d, canResetOnComplete: false);
				return;
			}
		}
		float num5 = Mathf.Clamp(FloatVal(d.startValue, d.easedInterpolationFactor, endValueOrDiff), 0f, d.cycleDuration);
		bool isForward = num5 > num;
		enumerator = GetSequenceSelfChildren(isForward).GetEnumerator();
		while (enumerator.MoveNext())
		{
			ColdData current3 = enumerator.Current;
			current3.managedData.UpdateSequenceChild(num5, isRestart, invertEase, ref current3.data);
			if (isEarlyExitAfterChildUpdate(ref d))
			{
				break;
			}
		}
		bool isEarlyExitAfterChildUpdate(ref UnmanagedTweenData reference)
		{
			if (!reference.isAlive)
			{
				return true;
			}
			if (earlyExitSequenceIfPaused)
			{
				return reference.isPaused;
			}
			return false;
		}
	}

	internal static void ResetSequence(ColdData seq)
	{
		ref UnmanagedTweenData data = ref seq.data;
		Sequence.SequenceDirectEnumerator enumerator = new Sequence(seq).GetSelfChildren(isForward: false).GetEnumerator();
		while (enumerator.MoveNext())
		{
			ColdData current = enumerator.Current;
			ref UnmanagedTweenData data2 = ref current.data;
			if (data2.IsSequenceRoot())
			{
				ResetSequence(current);
				continue;
			}
			data2.SetFlag(Flags.StateBefore, value: false);
			current.managedData.UpdateAndSetElapsedTimeTotal(-1000f, out var _, invertEase: false, ref data2);
			if (data.isAlive)
			{
				continue;
			}
			break;
		}
	}

	private Sequence.SequenceDirectEnumerator GetSequenceSelfChildren(bool isForward)
	{
		return new Sequence(sequence).GetSelfChildren(isForward);
	}

	private void UpdateSequenceChild(float encompassingElapsedTime, bool isRestart, bool invertEase, ref UnmanagedTweenData d)
	{
		if (d.IsSequenceRoot())
		{
			UpdateSequence(encompassingElapsedTime, isRestart, earlyExitSequenceIfPaused: true, allowSkipChildrenUpdate: true, invertEase, ref d);
			return;
		}
		UpdateAndSetElapsedTimeTotal(encompassingElapsedTime, out var cyclesDiff, invertEase, ref d);
		if (!d.stoppedEmergently && d.isAlive && d.IsDone(cyclesDiff))
		{
			ReportOnComplete(ref d);
		}
	}

	internal bool UpdateAndSetElapsedTimeTotal(float newElapsedTimeTotal, out int cyclesDiff, bool invertEase, ref UnmanagedTweenData d)
	{
		int cyclesDone = d.cyclesDone;
		float t = d.UpdateData(newElapsedTimeTotal);
		cyclesDiff = d.cyclesDone - cyclesDone;
		if (d.isValueChanged)
		{
			d.easedInterpolationFactor = CalcEasedT(t, d.cyclesDone, invertEase, ref d);
			TryCacheDiff(ref d);
			ReportOnValueChange(ref d);
			return true;
		}
		return false;
	}

	[Conditional("PRIME_TWEEN_SAFETY_CHECKS")]
	internal void print(string msg)
	{
	}

	internal void Reset(ref UnmanagedTweenData d)
	{
		if (cold.shakeData.isAlive)
		{
			cold.shakeData.Reset(target, d.tweenType);
		}
		target = null;
		cold.customEase = null;
		cold.customOnValueChange = null;
		cold.onValueChange = null;
		cold.onComplete = null;
		cold.onCompleteCallback = null;
		cold.onCompleteTarget = null;
		clearOnUpdate(ref d);
		d = default(UnmanagedTweenData);
		id = -1L;
	}

	internal void OnComplete([CanBeNull] Action _onComplete, bool? warnIfTargetDestroyed)
	{
		if (_onComplete == null)
		{
			return;
		}
		ValidateOnCompleteAssignment();
		cold.data.warnIgnoredOnCompleteIfTargetDestroyed = warnIfTargetDestroyed ?? PrimeTweenManager.Instance.warnIfTargetDestroyed;
		cold.onCompleteCallback = _onComplete;
		cold.onComplete = delegate(TweenData tween)
		{
			Action action = tween.cold.onCompleteCallback as Action;
			try
			{
				action();
			}
			catch (Exception e)
			{
				tween.HandleOnCompleteException(e);
			}
		};
	}

	internal void OnComplete<T>([CanBeNull] T _target, [CanBeNull] Action<T> _onComplete, bool? warnIfTargetDestroyed) where T : class
	{
		if (_target == null || IsDestroyedUnityObject(_target))
		{
			UnityEngine.Debug.LogError("_target is null or has been destroyed. Tween's OnComplete callback was ignored.");
		}
		else
		{
			if (_onComplete == null)
			{
				return;
			}
			ValidateOnCompleteAssignment();
			cold.data.warnIgnoredOnCompleteIfTargetDestroyed = warnIfTargetDestroyed ?? PrimeTweenManager.Instance.warnIfTargetDestroyed;
			cold.onCompleteTarget = _target;
			cold.onCompleteCallback = _onComplete;
			cold.onComplete = delegate(TweenData tween)
			{
				Action<T> action = tween.cold.onCompleteCallback as Action<T>;
				T obj = tween.cold.onCompleteTarget as T;
				if (IsDestroyedUnityObject(obj))
				{
					tween.WarnOnCompleteIgnored(isTargetDestroyed: true);
					return;
				}
				try
				{
					action(obj);
				}
				catch (Exception e)
				{
					tween.HandleOnCompleteException(e);
				}
			};
		}
	}

	private void HandleOnCompleteException(Exception e)
	{
		LogErrorWithStackTrace("Tween's onComplete callback raised exception, tween: " + GetDescription());
		UnityEngine.Debug.LogException(e, target as UnityEngine.Object);
	}

	internal void LogErrorWithStackTrace(string msg)
	{
		Assert.LogErrorWithStackTrace(msg, cold.id, target);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsDestroyedUnityObject<T>(T obj) where T : class
	{
		if (obj is UnityEngine.Object obj2)
		{
			return obj2 == null;
		}
		return false;
	}

	private void ValidateOnCompleteAssignment()
	{
	}

	internal bool ReportOnValueChange(ref UnmanagedTweenData d)
	{
		bool hasOnUpdate = d.hasOnUpdate;
		try
		{
			if (!Utils.SetAnimatedValue(ref this, ref d))
			{
				return false;
			}
			if (hasOnUpdate && !d.stoppedEmergently && d.isAlive)
			{
				d.isUpdating = true;
				cold.onUpdate?.Invoke(this);
				d.isUpdating = false;
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception, target as UnityEngine.Object);
			Assert.LogWarningWithStackTrace("Tween was stopped because of exception in 'onValueChange', tween: " + GetDescription() + "\n", id, target);
			EmergencyStop(isTargetDestroyed: false, ref d);
			return false;
		}
		return true;
	}

	private void TryCacheDiff(ref UnmanagedTweenData d)
	{
		if (d.startFromCurrent)
		{
			d.startFromCurrent = false;
			if (!ShakeData.TryTakeStartValueFromOtherShake(ref this, ref d) && !IsUnityTargetDestroyed())
			{
				d.startValue = Utils.GetAnimatedValue(target, d.tweenType, cold.intParam);
			}
			if (d.startValue.vector4 == endValueOrDiff.vector4 && d.warnEndValueEqualsCurrent && !shakeData.isAlive)
			{
				Assert.LogWarningWithStackTrace($"Tween's 'endValue' equals to the current animated value: {d.startValue.vector4}, tween: {GetDescription()}.\n" + Constants.buildWarningCanBeDisabledMessage("warnEndValueEqualsCurrent") + "\n", id, target);
			}
			CacheDiff(ref d, ref this);
		}
	}

	private void ReportOnComplete(ref UnmanagedTweenData d, bool canResetOnComplete = true)
	{
		if (canResetOnComplete && d.resetOnComplete && !d.isInSequence)
		{
			UpdateAndSetElapsedTimeTotal(-1000f, out var _, invertEase: false, ref d);
		}
		cold.onComplete?.Invoke(this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool IsUnityTargetDestroyed()
	{
		return IsDestroyedUnityObject(target);
	}

	[NotNull]
	internal string GetDescription()
	{
		_sb.Clear();
		UnmanagedTweenData data = cold.data;
		if (!data.isAlive)
		{
			_sb.Append(" - ");
		}
		if (sequence != null)
		{
			ColdData coldData = sequence;
			while (true)
			{
				if (id != coldData.id)
				{
					_sb.Append(" · ");
				}
				ColdData prev = coldData.prev;
				if (prev == null)
				{
					break;
				}
				ColdData coldData2 = prev.sequence;
				if (coldData2 == null)
				{
					break;
				}
				coldData = coldData2;
			}
		}
		float animationDuration = data.animationDuration;
		bool flag = false;
		if (data.tweenType == TweenAnimation.TweenType.Delay)
		{
			if (animationDuration == 0f && cold.onComplete != null)
			{
				flag = true;
				_sb.Append("Callback");
			}
			else
			{
				_sb.Append("Delay");
			}
		}
		else if (data.tweenType == TweenAnimation.TweenType.MainSequence || data.tweenType == TweenAnimation.TweenType.NestedSequence)
		{
			_sb.Append("Sequence ");
		}
		else
		{
			_sb.Append(data.tweenType);
		}
		if (target != PrimeTweenManager.dummyTarget)
		{
			_sb.Append("  /  ");
			_sb.Append((target is UnityEngine.Object obj && obj != null) ? obj.name : target?.GetType().Name);
		}
		if (!flag)
		{
			_sb.Append("  /  ").AppendFormat("{0:0.0#}s", animationDuration);
		}
		return _sb.ToString();
	}

	internal static void CalculateCycleDuration(float endDelay, ref UnmanagedTweenData d)
	{
		d.cycleDuration = d.startDelay + d.animationDuration + endDelay;
	}

	internal static double DoubleVal(TweenAnimation.ValueWrapper startValue, float t, TweenAnimation.ValueWrapper delta)
	{
		return startValue.DoubleVal + delta.DoubleVal * (double)t;
	}

	internal static float FloatVal(TweenAnimation.ValueWrapper startValue, float t, TweenAnimation.ValueWrapper delta)
	{
		return startValue.single + delta.single * t;
	}

	internal static Color ColorVal(TweenAnimation.ValueWrapper startValue, float t, TweenAnimation.ValueWrapper delta)
	{
		return startValue.color + delta.color * t;
	}

	internal static Vector2 Vector2Val(TweenAnimation.ValueWrapper startValue, float t, TweenAnimation.ValueWrapper delta)
	{
		return startValue.vector2 + delta.vector2 * t;
	}

	internal static Vector3 Vector3Val(TweenAnimation.ValueWrapper startValue, float t, TweenAnimation.ValueWrapper delta)
	{
		return startValue.vector3 + delta.vector3 * t;
	}

	internal static Vector4 Vector4Val(TweenAnimation.ValueWrapper startValue, float t, TweenAnimation.ValueWrapper delta)
	{
		return startValue.vector4 + delta.vector4 * t;
	}

	internal static Rect RectVal(TweenAnimation.ValueWrapper startValue, float t, TweenAnimation.ValueWrapper delta)
	{
		return new Rect(startValue.x + delta.x * t, startValue.y + delta.y * t, startValue.z + delta.z * t, startValue.w + delta.w * t);
	}

	internal static Quaternion QuaternionVal(TweenAnimation.ValueWrapper startValue, float t, TweenAnimation.ValueWrapper delta)
	{
		return Quaternion.SlerpUnclamped(startValue.quaternion, delta.quaternion, t);
	}

	private float CalcEasedT(float t, int cyclesDone_, bool invertEase, ref UnmanagedTweenData d)
	{
		if (invertEase)
		{
			float num = CalcEasedTInternal(1f - t, cyclesDone_, ref d);
			if (!d.IsCustomEaseSameStartEndValues)
			{
				return 1f - num;
			}
			return num;
		}
		return CalcEasedTInternal(t, cyclesDone_, ref d);
	}

	private float CalcEasedTInternal(float t, int cyclesDone_, ref UnmanagedTweenData d)
	{
		switch (d.cycleMode)
		{
		case CycleMode.Restart:
			return Evaluate(t, ref d);
		case CycleMode.Incremental:
			return Evaluate(t, ref d) + (float)d.clampCyclesDone(cyclesDone_);
		case CycleMode.Yoyo:
		case (CycleMode)4:
			if (!d.IsForwardCycle(cyclesDone_))
			{
				return 1f - Evaluate(t, ref d);
			}
			return Evaluate(t, ref d);
		case CycleMode.Rewind:
			if (!d.IsForwardCycle(cyclesDone_))
			{
				return Evaluate(1f - t, ref d);
			}
			return Evaluate(t, ref d);
		default:
			throw new Exception();
		}
	}

	private float Evaluate(float t, ref UnmanagedTweenData d)
	{
		if (d.ease == Ease.Custom)
		{
			if (cold.parametricEase != ParametricEase.None)
			{
				return Easing.EvaluateParametricEase(t, ref this, ref d);
			}
			return cold.customEase.Evaluate(t);
		}
		return StandardEasing.Evaluate(t, d.ease);
	}

	internal static void CacheDiff(ref UnmanagedTweenData d, ref TweenData rt)
	{
		switch (d.propType)
		{
		case PropType.Quaternion:
			d.startValue.QuaternionNormalize();
			rt.endValueOrDiff.QuaternionNormalize();
			break;
		case PropType.Double:
			rt.endValueOrDiff.DoubleVal -= d.startValue.DoubleVal;
			rt.endValueOrDiff.z = 0f;
			rt.endValueOrDiff.w = 0f;
			break;
		default:
			rt.endValueOrDiff.vector4 -= d.startValue.vector4;
			break;
		}
	}

	internal void ForceComplete(ref UnmanagedTweenData d)
	{
		Kill(ref d);
		int num;
		if (d.timeScale > 0f)
		{
			num = d.cyclesTotal;
			if (num == -1)
			{
				num = (d.cyclesTotal = d.getCyclesDone() + 1);
			}
		}
		else
		{
			num = -1;
		}
		d.cyclesDone = num;
		d.easedInterpolationFactor = CalcEasedT(1f, num, invertEase: false, ref d);
		TryCacheDiff(ref d);
		ReportOnValueChange(ref d);
		if (!d.stoppedEmergently)
		{
			ReportOnComplete(ref d);
		}
	}

	internal void WarnOnCompleteIgnored(bool isTargetDestroyed)
	{
		if (HasOnComplete && cold.data.warnIgnoredOnCompleteIfTargetDestroyed)
		{
			cold.onComplete = null;
			string text = "Tween's OnComplete callback was ignored. Tween: " + GetDescription() + ".\n";
			if (isTargetDestroyed)
			{
				text += "\nIf you use tween.OnComplete(), Tween.Delay(), or sequence.ChainDelay() only for cosmetic purposes, you can turn off this error by passing 'warnIfTargetDestroyed: false' parameter to the method.\nMore info: https://github.com/KyryloKuzyk/PrimeTween/discussions/4\n\nNot recommended: it's also possible to disable this setting globally with 'PrimeTweenConfig.warnIfTargetDestroyed = false', but doing so will silent potential logic errors and might introduce subtle hard-to-debug issues to your project.\n";
			}
			Assert.LogErrorWithStackTrace(text, id, target ?? cold.onCompleteTarget);
		}
	}

	internal void EmergencyStop(bool isTargetDestroyed, ref UnmanagedTweenData d)
	{
		if (sequence != null)
		{
			ColdData coldData = sequence;
			while (true)
			{
				ColdData prev = coldData.prev;
				if (prev == null)
				{
					break;
				}
				ColdData coldData2 = prev.sequence;
				if (coldData2 == null)
				{
					break;
				}
				coldData = coldData2;
			}
			new Sequence(coldData).EmergencyStop();
		}
		else if (d.isAlive)
		{
			Kill(ref d);
		}
		d.stoppedEmergently = true;
		WarnOnCompleteIgnored(isTargetDestroyed);
	}

	internal void Kill(ref UnmanagedTweenData d)
	{
		d.isAlive = false;
	}

	internal void SetOnUpdate<T>(T _target, [NotNull] Action<T, Tween> _onUpdate) where T : class
	{
		cold.onUpdateTarget = _target;
		cold.onUpdateCallback = _onUpdate;
		cold.onUpdate = delegate(TweenData reusableTween)
		{
			reusableTween.invokeOnUpdate<T>();
		};
		cold.data.hasOnUpdate = true;
	}

	private void invokeOnUpdate<T>() where T : class
	{
		Action<T, Tween> action = cold.onUpdateCallback as Action<T, Tween>;
		T val = cold.onUpdateTarget as T;
		if (IsDestroyedUnityObject(val))
		{
			LogErrorWithStackTrace("OnUpdate() will not be called again because OnUpdate()'s target has been destroyed, tween: " + GetDescription());
			clearOnUpdate(ref cold.data);
			return;
		}
		try
		{
			action(val, new Tween(cold));
		}
		catch (Exception exception)
		{
			LogErrorWithStackTrace("OnUpdate() will not be called again because it thrown exception, tween: " + GetDescription());
			UnityEngine.Debug.LogException(exception, val as UnityEngine.Object);
			clearOnUpdate(ref cold.data);
		}
	}

	private void clearOnUpdate(ref UnmanagedTweenData d)
	{
		cold.onUpdateTarget = null;
		cold.onUpdateCallback = null;
		cold.onUpdate = null;
		d.hasOnUpdate = false;
	}

	public override string ToString()
	{
		return GetDescription();
	}

	public bool Equals(TweenData other)
	{
		return object.Equals(cold, other.cold);
	}

	public override bool Equals(object obj)
	{
		if (obj is TweenData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (cold == null)
		{
			return 0;
		}
		return cold.GetHashCode();
	}
}
