using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace PrimeTween;

public readonly struct Sequence : IEnumerator, IEquatable<Sequence>
{
	internal struct SequenceDirectEnumerator
	{
		private readonly Sequence sequence;

		private ColdData current;

		private readonly bool isEmpty;

		private readonly bool isForward;

		private bool isStarted;

		public readonly ColdData Current => current;

		internal SequenceDirectEnumerator(Sequence s, bool isForward)
		{
			sequence = s;
			this.isForward = isForward;
			isStarted = false;
			isEmpty = IsSequenceEmpty(s);
			if (isEmpty)
			{
				current = null;
				return;
			}
			current = sequence.root.tween.next;
			if (isForward)
			{
				return;
			}
			while (true)
			{
				ColdData nextSibling = current.nextSibling;
				if (nextSibling != null)
				{
					current = nextSibling;
					continue;
				}
				break;
			}
		}

		private static bool IsSequenceEmpty(Sequence s)
		{
			return s.root.tween.intParam == -43;
		}

		public readonly SequenceDirectEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			if (isEmpty)
			{
				return false;
			}
			if (!isStarted)
			{
				isStarted = true;
				return true;
			}
			current = (isForward ? current.nextSibling : current.prevSibling);
			return current != null;
		}
	}

	internal struct SequenceChildrenEnumerator
	{
		private readonly Sequence sequence;

		private ColdData current;

		private bool isStarted;

		public readonly ColdData Current => current;

		internal SequenceChildrenEnumerator(Sequence s)
		{
			sequence = s;
			current = null;
			isStarted = false;
		}

		public readonly SequenceChildrenEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			if (!isStarted)
			{
				current = sequence.root.tween;
				isStarted = true;
				return true;
			}
			current = current.next;
			return current != null;
		}
	}

	public enum SequenceCycleMode : byte
	{
		[Tooltip("Restarts the animation from the beginning.")]
		Restart = 0,
		[Tooltip("Preserves easing of animations when 'TweenAnimation' is moving backward. Useful for having the same motion on the backward cycle.")]
		YoyoChildren = 4,
		[Tooltip("Animates forth and back, like a yoyo. Easing is the same on the backward cycle. Use 'YoyoChildren' to preserve easing of animations on the backward cycle.")]
		Yoyo = 1,
		[Tooltip("Rewinds the animation as if time was reversed. Easing is reversed on the backward cycle.")]
		Rewind = 3
	}

	private const string chainAndInsertCallbackMessage = "The behavior of ChainCallback() and InsertCallback() methods was fixed in version 1.2.0. Use their obsolete counterparts only if you need to preserve the old incorrect behaviour in the existing project.\nMore info: https://github.com/KyryloKuzyk/PrimeTween/discussions/112\n\n";

	internal readonly Tween root;

	private const int emptySequenceTag = -43;

	object IEnumerator.Current => null;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'isAlive' (lower case 'i') instead.")]
	public bool IsAlive => isAlive;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("please use 'isPaused' (lower case 'i') instead.")]
	public bool IsPaused => isPaused;

	internal bool IsCreated => root.IsCreated;

	private long id => root.id;

	public bool isAlive => root.isAlive;

	public float elapsedTime
	{
		get
		{
			return root.elapsedTime;
		}
		set
		{
			root.elapsedTime = value;
		}
	}

	public int cyclesTotal => root.cyclesTotal;

	public int cyclesDone => root.cyclesDone;

	public float duration
	{
		get
		{
			return root.duration;
		}
		private set
		{
			ColdData tween = root.tween;
			ref TweenData managedData = ref tween.managedData;
			ref UnmanagedTweenData data = ref tween.data;
			data.animationDuration = value;
			TweenData.CalculateCycleDuration(0f, ref data);
			managedData.endValueOrDiff = value.ToContainer();
			TweenData.CacheDiff(ref data, ref managedData);
		}
	}

	public float elapsedTimeTotal
	{
		get
		{
			return root.elapsedTimeTotal;
		}
		set
		{
			root.elapsedTimeTotal = value;
		}
	}

	public float durationTotal => root.durationTotal;

	public float progress
	{
		get
		{
			return root.progress;
		}
		set
		{
			root.progress = value;
		}
	}

	public float progressTotal
	{
		get
		{
			return root.progressTotal;
		}
		set
		{
			root.progressTotal = value;
		}
	}

	public bool isPaused
	{
		get
		{
			return root.isPaused;
		}
		set
		{
			root.isPaused = value;
		}
	}

	public float timeScale
	{
		get
		{
			return root.timeScale;
		}
		set
		{
			root.timeScale = value;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public Tween.TweenAwaiter GetAwaiter()
	{
		return new Tween.TweenAwaiter(root);
	}

	[NotNull]
	public IEnumerator ToYieldInstruction()
	{
		return root.ToYieldInstruction();
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
	[Obsolete("please use Sequence.Create(cycles: numCycles) instead. Or use SetRemainingCycles() to modify the number of remaining cycles when the Sequence is already running.")]
	public Sequence SetCycles(int cycles)
	{
		SetRemainingCycles(cycles);
		return this;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The behavior of ChainCallback() and InsertCallback() methods was fixed in version 1.2.0. Use their obsolete counterparts only if you need to preserve the old incorrect behaviour in the existing project.\nMore info: https://github.com/KyryloKuzyk/PrimeTween/discussions/112\n\n")]
	public Sequence ChainCallbackObsolete([NotNull] Action callback, bool? warnIfTargetDestroyed = null)
	{
		if (TryManipulate())
		{
			InsertCallbackObsolete(duration, callback, warnIfTargetDestroyed);
		}
		return this;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The behavior of ChainCallback() and InsertCallback() methods was fixed in version 1.2.0. Use their obsolete counterparts only if you need to preserve the old incorrect behaviour in the existing project.\nMore info: https://github.com/KyryloKuzyk/PrimeTween/discussions/112\n\n")]
	public Sequence InsertCallbackObsolete(float atTime, Action callback, bool? warnIfTargetDestroyed = null)
	{
		if (!TryManipulate())
		{
			return this;
		}
		Tween? tween = PrimeTweenManager.DelayWithoutDurationCheck(PrimeTweenManager.dummyTarget, atTime, useUnscaledTime: false);
		tween.Value.tween.managedData.OnComplete(callback, warnIfTargetDestroyed);
		return Insert(0f, tween.Value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The behavior of ChainCallback() and InsertCallback() methods was fixed in version 1.2.0. Use their obsolete counterparts only if you need to preserve the old incorrect behaviour in the existing project.\nMore info: https://github.com/KyryloKuzyk/PrimeTween/discussions/112\n\n")]
	public Sequence ChainCallbackObsolete<T>([NotNull] T target, [NotNull] Action<T> callback, bool? warnIfTargetDestroyed = null) where T : class
	{
		if (TryManipulate())
		{
			InsertCallbackObsolete(duration, target, callback, warnIfTargetDestroyed);
		}
		return this;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The behavior of ChainCallback() and InsertCallback() methods was fixed in version 1.2.0. Use their obsolete counterparts only if you need to preserve the old incorrect behaviour in the existing project.\nMore info: https://github.com/KyryloKuzyk/PrimeTween/discussions/112\n\n")]
	public Sequence InsertCallbackObsolete<T>(float atTime, [NotNull] T target, Action<T> callback, bool? warnIfTargetDestroyed = null) where T : class
	{
		if (!TryManipulate())
		{
			return this;
		}
		Tween? tween = PrimeTweenManager.DelayWithoutDurationCheck(target, atTime, useUnscaledTime: false);
		if (!tween.HasValue)
		{
			return this;
		}
		tween.Value.tween.managedData.OnComplete(target, callback, warnIfTargetDestroyed);
		return Insert(0f, tween.Value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("use 'Sequence.SequenceCycleMode' instead.")]
	public static Sequence Create(int cycles, CycleMode cycleMode, Ease sequenceEase = Ease.Linear, bool useUnscaledTime = false, UpdateType updateType = default(UpdateType))
	{
		return Create(cycles, (SequenceCycleMode)cycleMode, sequenceEase, useUnscaledTime, updateType);
	}

	private bool TryManipulate(bool checkRecursive = true)
	{
		return root.TryManipulate(checkRecursive);
	}

	private bool ValidateCanManipulateSequence()
	{
		if (!TryManipulate())
		{
			return false;
		}
		if (root.elapsedTimeTotal != 0f)
		{
			UnityEngine.Debug.LogError("Animation has already been started, it's not allowed to manipulate it anymore.");
			return false;
		}
		return true;
	}

	public static Sequence Create(int cycles = 1, SequenceCycleMode cycleMode = SequenceCycleMode.Restart, Ease sequenceEase = Ease.Linear, bool useUnscaledTime = false, UpdateType updateType = default(UpdateType))
	{
		if (PrimeTweenManager.Instance.isDestroyed)
		{
			return default(Sequence);
		}
		ColdData coldData = PrimeTweenManager.FetchTween(updateType.enumValue);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		if (cycleMode == (SequenceCycleMode)2)
		{
			UnityEngine.Debug.LogError("Sequence doesn't support CycleMode.Incremental. Parameter sequenceEase is applied to the sequence's timeline, and incrementing the timeline doesn't make sense. For the same reason, sequenceEase is clamped to [0:1] range.");
			cycleMode = SequenceCycleMode.Restart;
		}
		if (sequenceEase == Ease.Custom)
		{
			UnityEngine.Debug.LogError("Sequence doesn't support Ease.Custom.");
			sequenceEase = Ease.Linear;
		}
		if (sequenceEase == Ease.Default)
		{
			sequenceEase = Ease.Linear;
		}
		TweenSettings _settings = new TweenSettings(0f, sequenceEase, cycles, (CycleMode)cycleMode, 0f, 0f, useUnscaledTime, updateType);
		coldData.intParam = -43;
		coldData.Setup(PrimeTweenManager.dummyTarget, ref _settings, _startFromCurrent: false, TweenAnimation.TweenType.MainSequence, ref managedData, ref data);
		return new Sequence(PrimeTweenManager.AddTween(ref managedData, ref data));
	}

	public static Sequence Create(Tween firstTween)
	{
		return Create().Group(firstTween);
	}

	internal Sequence(ColdData cold)
	{
		root = new Tween(cold);
	}

	private Sequence(Tween rootTween)
	{
		root = rootTween;
		SetSequence(rootTween);
	}

	public Sequence Group(Tween tween)
	{
		if (TryManipulate())
		{
			Insert(GetLastInSelfOrRoot().data.waitDelay, tween);
		}
		return this;
	}

	private void AddLinkedReference(Tween tween)
	{
		ColdData coldData;
		if (root.tween.next != null)
		{
			coldData = GetLast();
			ColdData lastInSelfOrRoot = GetLastInSelfOrRoot();
			lastInSelfOrRoot.nextSibling = tween.tween;
			tween.tween.prevSibling = lastInSelfOrRoot;
		}
		else
		{
			coldData = root.tween;
		}
		coldData.next = tween.tween;
		tween.tween.prev = coldData;
		root.tween.intParam = 0;
	}

	private ColdData GetLast()
	{
		ColdData result = null;
		SequenceChildrenEnumerator enumerator = GetAllTweens().GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = enumerator.Current;
		}
		return result;
	}

	public Sequence Chain(Tween tween)
	{
		if (TryManipulate())
		{
			Insert(duration, tween);
		}
		return this;
	}

	public Sequence Insert(float atTime, Tween tween)
	{
		if (!ValidateCanAdd(tween))
		{
			return this;
		}
		if (tween.tween.sequence != null)
		{
			UnityEngine.Debug.LogError("An animation can be added to a sequence only once and can only belong to one sequence. Tween: " + tween.tween.managedData.GetDescription());
			return this;
		}
		SetSequence(tween);
		Insert_internal(atTime, tween);
		return this;
	}

	private void Insert_internal(float atTime, Tween other)
	{
		if (atTime < 0f)
		{
			UnityEngine.Debug.LogError($"Inserting at negative time ({atTime}) is not allowed.");
			atTime = 0f;
		}
		other.tween.data.waitDelay = atTime;
		duration = Mathf.Max(duration, other.durationWithWaitDelay);
		AddLinkedReference(other);
	}

	public Sequence ChainCallback([NotNull] Action callback, bool? warnIfTargetDestroyed = null)
	{
		if (TryManipulate())
		{
			InsertCallback(duration, callback, warnIfTargetDestroyed);
		}
		return this;
	}

	public Sequence InsertCallback(float atTime, Action callback, bool? warnIfTargetDestroyed = null)
	{
		if (!TryManipulate())
		{
			return this;
		}
		Tween? tween = PrimeTweenManager.DelayWithoutDurationCheck(PrimeTweenManager.dummyTarget, 0f, useUnscaledTime: false);
		tween.Value.tween.managedData.OnComplete(callback, warnIfTargetDestroyed);
		return Insert(atTime, tween.Value);
	}

	public Sequence ChainCallback<T>([NotNull] T target, [NotNull] Action<T> callback, bool? warnIfTargetDestroyed = null) where T : class
	{
		if (TryManipulate())
		{
			InsertCallback(duration, target, callback, warnIfTargetDestroyed);
		}
		return this;
	}

	public Sequence InsertCallback<T>(float atTime, [NotNull] T target, Action<T> callback, bool? warnIfTargetDestroyed = null) where T : class
	{
		if (!TryManipulate())
		{
			return this;
		}
		Tween? tween = CreateCallback(target, callback, warnIfTargetDestroyed);
		if (!tween.HasValue)
		{
			return this;
		}
		return Insert(atTime, tween.Value);
	}

	internal static Tween? CreateCallback<T>([NotNull] T target, Action<T> callback, bool? warnIfTargetDestroyed = null) where T : class
	{
		Tween? tween = PrimeTweenManager.DelayWithoutDurationCheck(target, 0f, useUnscaledTime: false);
		if (!tween.HasValue)
		{
			return null;
		}
		tween.Value.tween.managedData.OnComplete(target, callback, warnIfTargetDestroyed);
		return tween.Value;
	}

	public Sequence ChainDelay(float duration)
	{
		return Chain(Tween.Delay(duration));
	}

	private ColdData GetLastInSelfOrRoot()
	{
		ColdData result = root.tween;
		SequenceDirectEnumerator enumerator = GetSelfChildren().GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = enumerator.Current;
		}
		return result;
	}

	private void SetSequence(Tween handle)
	{
		ColdData tween = handle.tween;
		tween.sequence = root.tween;
		tween.data.isInSequence = true;
	}

	private bool ValidateCanAdd(Tween other)
	{
		if (!ValidateCanManipulateSequence())
		{
			return false;
		}
		if (!other.isAlive)
		{
			UnityEngine.Debug.LogError("It's not allowed to add 'dead' tweens to a sequence.");
			return false;
		}
		UnmanagedTweenData data = other.tween.data;
		if (data.cyclesTotal == -1)
		{
			UnityEngine.Debug.LogError("It's not allowed to have infinite tweens (cycles == -1) in a sequence. If you want the sequence to repeat forever, SetRemainingCycles(-1) on the parent sequence instead.");
			return false;
		}
		UnmanagedTweenData data2 = root.tween.data;
		if (data.isPaused && data.isPaused != data2.isPaused)
		{
			warnIgnoredChildrenSetting("isPaused", data2.isPaused, data.isPaused);
		}
		if (data.timeScale != 1f && data.timeScale != data2.timeScale)
		{
			warnIgnoredChildrenSetting("timeScale", data2.timeScale, data.timeScale);
		}
		if (data.useUnscaledTime && data.useUnscaledTime != data2.useUnscaledTime)
		{
			warnIgnoredChildrenSetting("useUnscaledTime", data2.useUnscaledTime, data.useUnscaledTime);
		}
		if (data.updateType != PrimeTweenManager.Instance.defaultUpdateType && data.updateType != data2.updateType)
		{
			warnIgnoredChildrenSetting("updateType", data2.updateType, data.updateType);
		}
		return true;
		static void warnIgnoredChildrenSetting(string settingName, object sequenceSetting, object childSetting)
		{
			UnityEngine.Debug.LogError($"'{settingName}' was ignored after adding child animation to the Sequence (Sequence has '{sequenceSetting}', but the child had '{childSetting}').\n" + "Parent Sequence controls '" + settingName + "' of all its children animations. To prevent this error:\n- Use the default value of '" + settingName + "' in child animation.\n- OR use the same '" + settingName + "' in child animation.\n");
		}
	}

	public void Stop()
	{
		if (isAlive && TryManipulate(checkRecursive: false))
		{
			ReleaseTweens();
		}
	}

	public void Complete()
	{
		if (isAlive && TryManipulate(checkRecursive: false))
		{
			if (cyclesTotal == -1 || root.tween.data.cycleMode == CycleMode.Restart)
			{
				SetRemainingCycles(1);
			}
			else
			{
				int num = cyclesTotal - cyclesDone;
				SetRemainingCycles((num % 2 == 1) ? 1 : 2);
			}
			root.isPaused = false;
			root.tween.managedData.UpdateSequence((timeScale > 0f) ? float.MaxValue : (-1000f), isRestart: false, earlyExitSequenceIfPaused: true, allowSkipChildrenUpdate: false, invertEase: false, ref root.tween.data);
		}
	}

	internal void EmergencyStop()
	{
		ReleaseTweens(delegate(TweenData t)
		{
			t.WarnOnCompleteIgnored(isTargetDestroyed: false);
		});
	}

	internal void ReleaseTweens([CanBeNull] Action<TweenData> beforeKill = null)
	{
		SequenceChildrenEnumerator allTweens = GetAllTweens();
		allTweens.MoveNext();
		ColdData coldData = allTweens.Current;
		while (true)
		{
			ColdData coldData2 = (allTweens.MoveNext() ? allTweens.Current : null);
			ColdData coldData3 = coldData;
			ref TweenData managedData = ref coldData3.managedData;
			ref UnmanagedTweenData data = ref coldData3.data;
			beforeKill?.Invoke(managedData);
			managedData.Kill(ref data);
			ReleaseTween(ref managedData);
			if (coldData2 != null)
			{
				coldData = coldData2;
				continue;
			}
			break;
		}
	}

	private static void ReleaseTween(ref TweenData tween)
	{
		tween.cold.next = null;
		tween.cold.prev = null;
		tween.cold.prevSibling = null;
		tween.cold.nextSibling = null;
		tween.cold.sequence = null;
		ref UnmanagedTweenData data = ref tween.cold.data;
		data.isInSequence = false;
		if (data.IsSequenceRoot())
		{
			data.tweenType = TweenAnimation.TweenType.Disabled;
		}
	}

	internal SequenceChildrenEnumerator GetAllChildren()
	{
		SequenceChildrenEnumerator allTweens = GetAllTweens();
		allTweens.MoveNext();
		return allTweens;
	}

	public void SetRemainingCycles(bool stopAtEndValue)
	{
		root.SetRemainingCycles(stopAtEndValue);
	}

	public void SetRemainingCycles(int cycles)
	{
		root.SetRemainingCycles(cycles);
	}

	internal SequenceDirectEnumerator GetSelfChildren(bool isForward = true)
	{
		return new SequenceDirectEnumerator(this, isForward);
	}

	internal SequenceChildrenEnumerator GetAllTweens()
	{
		return new SequenceChildrenEnumerator(this);
	}

	public override string ToString()
	{
		return root.ToString();
	}

	public Sequence Chain(Sequence sequence)
	{
		if (TryManipulate())
		{
			Insert(duration, sequence);
		}
		return this;
	}

	public Sequence Group(Sequence sequence)
	{
		if (TryManipulate())
		{
			Insert(GetLastInSelfOrRoot().data.waitDelay, sequence);
		}
		return this;
	}

	public Sequence Insert(float atTime, Sequence sequence)
	{
		if (!ValidateCanAdd(sequence.root))
		{
			return this;
		}
		if (sequence.root.tween.data.tweenType != TweenAnimation.TweenType.MainSequence)
		{
			UnityEngine.Debug.LogError("An animation can be added to a sequence only once and can only belong to one sequence.");
			return this;
		}
		sequence.root.tween.data.tweenType = TweenAnimation.TweenType.NestedSequence;
		Insert_internal(atTime, sequence.root);
		return this;
	}

	[Conditional("SAFETY_CHECKS")]
	private void ValidateSequenceEnumerator()
	{
		List<TweenData> list = new List<TweenData> { root.tween.managedData };
		SequenceChildrenEnumerator enumerator = GetAllTweens().GetEnumerator();
		while (enumerator.MoveNext())
		{
			ColdData current = enumerator.Current;
			if (current.data.IsSequenceRoot())
			{
				SequenceDirectEnumerator enumerator2 = new Sequence(current.sequence).GetSelfChildren().GetEnumerator();
				while (enumerator2.MoveNext())
				{
					ColdData current2 = enumerator2.Current;
					list.Add(current2.managedData);
				}
			}
		}
		if (list.Count != list.Select((TweenData _) => _.id).Distinct().Count())
		{
			UnityEngine.Debug.LogError(string.Format("{0}, duplicates in ValidateSequenceEnumerator():\n{1}", root.id, string.Join("\n", list)));
		}
	}

	public Sequence OnComplete(Action onComplete, bool? warnIfTargetDestroyed = null)
	{
		root.OnComplete(onComplete, warnIfTargetDestroyed);
		return this;
	}

	public Sequence OnComplete<T>(T target, Action<T> onComplete, bool? warnIfTargetDestroyed = null) where T : class
	{
		root.OnComplete(target, onComplete, warnIfTargetDestroyed);
		return this;
	}

	public override int GetHashCode()
	{
		return root.GetHashCode();
	}

	public bool Equals(Sequence other)
	{
		return root.Equals(other.root);
	}

	[Obsolete("This method was renamed to 'ResetOnCompletion'.", true)]
	private Sequence ResetOnComplete()
	{
		return ResetOnCompletion();
	}

	private Sequence ResetOnCompletion()
	{
		root.ResetOnCompletion();
		return this;
	}
}
