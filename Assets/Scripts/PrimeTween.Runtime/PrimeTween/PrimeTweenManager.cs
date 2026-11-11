using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace PrimeTween;

[AddComponentMenu("")]
internal sealed class PrimeTweenManager : MonoBehaviour
{
	[Serializable]
	[UsedImplicitly]
	internal struct TweenInspectorData
	{
		public string debugDescription;

		public UnityEngine.Object unityTarget;

		public bool isPaused;

		public float elapsedTimeTotal;

		public float easedInterpolationFactor;

		public ValueContainerStartEnd startEndValue;

		public TweenSettings settings;

		public int cycledDone;
	}

	internal static readonly System.Random random = new System.Random();

	internal static PrimeTweenManager Instance;

	internal static int customInitialCapacity = -1;

	internal TweenArray tweensUpdate;

	internal TweenArray tweensLateUpdate;

	internal TweenArray tweensFixedUpdate;

	internal TweenArray newTweensUpdate;

	internal TweenArray newTweensLateUpdate;

	internal TweenArray newTweensFixedUpdate;

	internal TweenArray[] allTweenArrays;

	[NonSerialized]
	internal List<ColdData> pool;

	internal Dictionary<(Transform, TweenAnimation.TweenType), (TweenAnimation.ValueWrapper startValue, int count)> shakes;

	[NonSerialized]
	internal long lastId = 1L;

	internal Ease defaultEase = Ease.OutQuad;

	internal _UpdateType defaultUpdateType = _UpdateType.Update;

	internal const Ease defaultShakeEase = Ease.OutQuad;

	internal bool warnTweenOnDisabledTarget = true;

	internal bool warnZeroDuration = true;

	internal bool warnStructBoxingAllocationInCoroutine = true;

	internal bool warnBenchmarkWithAsserts = true;

	internal bool validateCustomCurves = true;

	internal bool warnEndValueEqualsCurrent = true;

	internal bool warnIfTargetDestroyed = true;

	internal int updateDepth;

	internal static readonly object dummyTarget = new object();

	internal bool completeAllRequested;

	internal object completeAllRequestedTarget;

	private readonly int _hashCode;

	internal MaterialPropertyBlock materialPropertyBlockForSetter;

	internal MaterialPropertyBlock materialPropertyBlockForGetter;

	private const string manualInstanceCreationIsNotAllowedMessage = "Please don't create the PrimeTweenManager instance manually.";

	internal static bool logCantManipulateError = true;

	internal bool isDestroyed { get; private set; }

	internal static bool HasInstance => Instance != null;

	internal int currentPoolCapacity { get; private set; }

	internal int maxSimultaneousTweensCount { get; private set; }

	internal int tweensCount
	{
		get
		{
			int num = 0;
			TweenArray[] array = allTweenArrays;
			foreach (TweenArray tweenArray in array)
			{
				num += tweenArray.Count;
			}
			return num;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void BeforeSceneLoad()
	{
		if (!HasInstance)
		{
			CreateInstanceAndDontDestroy();
		}
	}

	internal static void CreateInstanceAndDontDestroy()
	{
		CreateInstance();
		UnityEngine.Object.DontDestroyOnLoad(Instance.gameObject);
	}

	private static GameObject CreateNewGameObject()
	{
		return new GameObject("PrimeTweenManager");
	}

	private static void CreateInstance()
	{
		PrimeTweenManager primeTweenManager = CreateNewGameObject().AddComponent<PrimeTweenManager>();
		primeTweenManager.Init((customInitialCapacity != -1) ? customInitialCapacity : 200);
		Instance = primeTweenManager;
	}

	private void Init(int capacity)
	{
		tweensUpdate = new TweenArray(capacity, "tweensUpdate");
		tweensLateUpdate = new TweenArray(capacity, "tweensLateUpdate");
		tweensFixedUpdate = new TweenArray(capacity, "tweensFixedUpdate");
		newTweensUpdate = new TweenArray(capacity, "newTweensUpdate");
		newTweensLateUpdate = new TweenArray(capacity, "newTweensLateUpdate");
		newTweensFixedUpdate = new TweenArray(capacity, "newTweensFixedUpdate");
		allTweenArrays = new TweenArray[6] { tweensUpdate, newTweensUpdate, tweensLateUpdate, newTweensLateUpdate, tweensFixedUpdate, newTweensFixedUpdate };
		pool = new List<ColdData>(capacity);
		for (int i = 0; i < capacity; i++)
		{
			pool.Add(new ColdData());
		}
		shakes = new Dictionary<(Transform, TweenAnimation.TweenType), (TweenAnimation.ValueWrapper, int)>(capacity);
		currentPoolCapacity = capacity;
		materialPropertyBlockForSetter = new MaterialPropertyBlock();
		materialPropertyBlockForGetter = new MaterialPropertyBlock();
	}

	private void Awake()
	{
	}

	private void OnDestroy()
	{
		Dispose();
	}

	private void Dispose()
	{
		if (!isDestroyed)
		{
			isDestroyed = true;
			TweenArray[] array = allTweenArrays;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Dispose();
			}
		}
	}

	[Conditional("_")]
	private static void DebugLifetimeStatic(string log)
	{
		int num = (HasInstance ? Instance._hashCode : (-1));
		UnityEngine.Debug.Log($"{num}: {log}");
	}

	[Conditional("_")]
	private void DebugLifetime(string log)
	{
		UnityEngine.Debug.Log($"{_hashCode}: {log}");
	}

	private void Start()
	{
	}

	internal void FixedUpdate()
	{
		UpdateTweens(_UpdateType.FixedUpdate);
	}

	internal void Update()
	{
		UpdateTweens(_UpdateType.Update);
	}

	private void UpdateTweenArray(TweenArray array, float deltaTime, float unscaledDeltaTime)
	{
		if (updateDepth != 0)
		{
			TweenArray[] array2 = allTweenArrays;
			foreach (TweenArray tweenArray in array2)
			{
				foreach (TweenArray.EnumeratorElement item in tweenArray)
				{
					ColdData cold = item.tween.cold;
					if (cold == null || !cold.data.isAlive)
					{
						continue;
					}
					Action<TweenData> onComplete = cold.onComplete;
					if (onComplete != null)
					{
						try
						{
							onComplete(cold.managedData);
						}
						catch (Exception exception)
						{
							UnityEngine.Debug.LogException(exception);
						}
					}
					cold.longParam = -1L;
					cold.id = -1L;
					cold.data.isAlive = false;
				}
				tweenArray.Clear();
			}
			shakes.Clear();
			updateDepth = 0;
			UnityEngine.Debug.LogError("PrimeTween recovered from an exception, all running animations have been stopped. Please reach out support describing the issue and providing error logs: https://github.com/KyryloKuzyk/PrimeTween?tab=readme-ov-file#support.");
			return;
		}
		updateDepth++;
		int count = array.Count;
		int num = 0;
		using (new TweenArray.Lock(array))
		{
			for (int j = 0; j < count; j++)
			{
				ref TweenData reference = ref array[j];
				ref UnmanagedTweenData dataAt = ref array.GetDataAt(j);
				int num2 = j - num;
				if (reference.UpdateAndCheckIfRunning(dataAt.useUnscaledTime ? unscaledDeltaTime : deltaTime, ref dataAt) || (dataAt.isInCoroutine && reference.cold.ResetCoroutineEnumerator()))
				{
					if (j != num2)
					{
						array.MoveAndClearOld(reference, j, num2);
					}
				}
				else
				{
					ReleaseTweenToPool(ref reference, ref dataAt);
					array[j] = default(TweenData);
					num++;
				}
			}
		}
		updateDepth--;
		if (num > 0)
		{
			array.TrimEndNulls(num);
		}
		ProcessRequestedCompleteAll();
	}

	private void ProcessRequestedCompleteAll()
	{
		if (completeAllRequested)
		{
			completeAllRequested = false;
			object onTarget = completeAllRequestedTarget;
			completeAllRequestedTarget = null;
			Tween.CompleteAll(onTarget);
		}
	}

	internal void LateUpdate()
	{
		UpdateTweens(_UpdateType.LateUpdate);
		ApplyStartValues(_UpdateType.Update);
		ApplyStartValues(_UpdateType.LateUpdate);
	}

	internal void ApplyStartValues(_UpdateType updateType)
	{
		switch (updateType)
		{
		case _UpdateType.Default:
			UnityEngine.Debug.LogError("Please provide non-default update type.");
			break;
		case _UpdateType.Update:
		case _UpdateType.LateUpdate:
		case _UpdateType.FixedUpdate:
		{
			TweenArray newTweensArray = GetNewTweensArray(updateType);
			TweenArray currentTweensArray = GetCurrentTweensArray(updateType);
			int count = currentTweensArray.Count;
			AddNewTweens(newTweensArray, currentTweensArray);
			using (new TweenArray.Lock(currentTweensArray))
			{
				updateDepth++;
				for (int i = count; i < currentTweensArray.Count; i++)
				{
					ref TweenData reference = ref currentTweensArray[i];
					ref UnmanagedTweenData dataAt = ref currentTweensArray.GetDataAt(i);
					if (dataAt.isAlive && !dataAt.startFromCurrent && dataAt.startDelay == 0f && !dataAt.isAdditive && dataAt.canManipulate() && dataAt.elapsedTimeTotal == 0f)
					{
						reference.SetElapsedTimeTotal(0f, earlyExitSequenceIfPaused: true, ref dataAt);
					}
				}
				updateDepth--;
			}
			AddNewTweens(newTweensArray, currentTweensArray);
			ProcessRequestedCompleteAll();
			break;
		}
		default:
			throw new Exception($"Invalid update type: {updateType}");
		}
	}

	internal void UpdateTweens(_UpdateType updateType, float? deltaTime = null, float? unscaledDeltaTime = null)
	{
		TweenArray currentTweensArray = GetCurrentTweensArray(updateType);
		switch (updateType)
		{
		case _UpdateType.Default:
			UnityEngine.Debug.LogError("Please provide non-default update type.");
			break;
		case _UpdateType.Update:
			AddNewTweens(updateType);
			UpdateTweenArray(currentTweensArray, deltaTime ?? Time.deltaTime, unscaledDeltaTime ?? Time.unscaledDeltaTime);
			break;
		case _UpdateType.LateUpdate:
			UpdateTweenArray(currentTweensArray, deltaTime ?? Time.deltaTime, unscaledDeltaTime ?? Time.unscaledDeltaTime);
			AddNewTweens(updateType);
			break;
		case _UpdateType.FixedUpdate:
			AddNewTweens(updateType);
			UpdateTweenArray(currentTweensArray, deltaTime ?? Time.fixedDeltaTime, unscaledDeltaTime ?? Time.fixedUnscaledDeltaTime);
			break;
		default:
			throw new Exception($"Invalid update type: {updateType}");
		}
	}

	internal void AddNewTweens(_UpdateType updateType)
	{
		AddNewTweens(GetNewTweensArray(updateType), GetCurrentTweensArray(updateType));
	}

	private static void AddNewTweens(TweenArray newTweens, TweenArray currentTweens)
	{
		if (newTweens.Count <= 0)
		{
			return;
		}
		foreach (TweenArray.EnumeratorElement newTween in newTweens)
		{
			UnmanagedTweenData data = newTween.data;
			TweenData tween = newTween.tween;
			ColdData cold = tween.cold;
			currentTweens.Add(cold);
			cold.managedData = tween;
			cold.data = data;
		}
		newTweens.Clear();
	}

	private void ReleaseTweenToPool(ref TweenData rt, ref UnmanagedTweenData d)
	{
		rt.Reset(ref d);
		ColdData cold = rt.cold;
		pool.Add(cold);
	}

	internal static Tween? DelayWithoutDurationCheck([CanBeNull] object target, float duration, bool useUnscaledTime)
	{
		TweenSettings _settings = new TweenSettings
		{
			duration = duration,
			ease = Ease.Linear,
			useUnscaledTime = useUnscaledTime
		};
		if (Instance.isDestroyed)
		{
			return null;
		}
		ColdData coldData = FetchTween(_settings._updateType);
		ref TweenData managedData = ref coldData.managedData;
		ref UnmanagedTweenData data = ref coldData.data;
		coldData.Setup(target, ref _settings, _startFromCurrent: false, TweenAnimation.TweenType.Delay, ref managedData, ref data);
		Tween value = AddTween(ref managedData, ref data);
		if (!value.IsCreated)
		{
			return null;
		}
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ColdData FetchTween(_UpdateType updateType)
	{
		return Instance.fetchTween_internal(updateType);
	}

	private ColdData fetchTween_internal(_UpdateType updateType)
	{
		ColdData coldData;
		if (pool.Count == 0)
		{
			coldData = new ColdData();
			if (tweensCount + 1 > currentPoolCapacity)
			{
				int num = ((currentPoolCapacity == 0) ? 4 : (currentPoolCapacity * 2));
				if (Application.isPlaying)
				{
					UnityEngine.Debug.LogWarning(string.Format("Tweens capacity has been increased from {0} to {1}. Please increase the capacity manually to prevent memory allocations at runtime by calling {2}.\n", currentPoolCapacity, num, "'PrimeTweenConfig.SetTweensCapacity(int capacity)'") + "To know the highest number of simultaneously running tweens, please observe the 'PrimeTweenManager/Max alive tweens' in Inspector.\n");
				}
				currentPoolCapacity = num;
			}
		}
		else
		{
			int index = pool.Count - 1;
			coldData = pool[index];
			pool.RemoveAt(index);
		}
		coldData.id = lastId;
		switch (updateType)
		{
		case _UpdateType.Default:
			updateType = Instance.defaultUpdateType;
			break;
		default:
			UnityEngine.Debug.LogError($"Invalid update type: {updateType}");
			updateType = _UpdateType.Update;
			break;
		case _UpdateType.Update:
		case _UpdateType.LateUpdate:
		case _UpdateType.FixedUpdate:
			break;
		}
		GetNewTweensArray(updateType).Add(coldData);
		coldData.data.updateType = updateType;
		return coldData;
	}

	private TweenArray GetCurrentTweensArray(_UpdateType updateType)
	{
		return updateType switch
		{
			_UpdateType.Update => tweensUpdate, 
			_UpdateType.LateUpdate => tweensLateUpdate, 
			_UpdateType.FixedUpdate => tweensFixedUpdate, 
			_ => throw new Exception(), 
		};
	}

	private TweenArray GetNewTweensArray(_UpdateType updateType)
	{
		return updateType switch
		{
			_UpdateType.Update => newTweensUpdate, 
			_UpdateType.LateUpdate => newTweensLateUpdate, 
			_UpdateType.FixedUpdate => newTweensFixedUpdate, 
			_ => throw new Exception(), 
		};
	}

	internal static Tween Animate(ref TweenData rt, ref UnmanagedTweenData d)
	{
		CheckDuration(rt.target, d.animationDuration);
		return AddTween(ref rt, ref d);
	}

	internal static void CheckDuration<T>([CanBeNull] T target, float duration) where T : class
	{
		if (Instance.warnZeroDuration && duration <= 0f)
		{
			UnityEngine.Debug.LogWarning(string.Format("Tween duration ({0}) <= 0. {1}", duration, Constants.buildWarningCanBeDisabledMessage("warnZeroDuration")), target as UnityEngine.Object);
		}
	}

	internal static Tween AddTween(ref TweenData tween, ref UnmanagedTweenData d)
	{
		PrimeTweenManager instance = Instance;
		Tween? tween2 = instance.TryAddTween(tween.cold, ref tween);
		if (tween2.HasValue)
		{
			return tween2.Value;
		}
		tween.Kill(ref d);
		instance.ReleaseTweenToPool(ref tween, ref d);
		tween.cold._tweenArray.RemoveLast(tween.cold);
		return default(Tween);
	}

	private Tween? TryAddTween(ColdData tween, ref TweenData rt)
	{
		if (rt.target == null || rt.IsUnityTargetDestroyed())
		{
			UnityEngine.Debug.LogError("Tween's target is null: " + rt.GetDescription() + ". This error can mean that:\n- The target reference is null.\n- UnityEngine.Object target reference is not populated in the Inspector.\n- UnityEngine.Object target has been destroyed.\nPlease ensure you're using a valid target.\n");
			return null;
		}
		if (warnTweenOnDisabledTarget && rt.target is Component component && !component.gameObject.activeInHierarchy)
		{
			UnityEngine.Debug.LogWarning("Tween is started on GameObject that is not active in hierarchy: " + component.name + ". " + Constants.buildWarningCanBeDisabledMessage("warnTweenOnDisabledTarget"), component);
		}
		lastId++;
		return new Tween(tween);
	}

	internal static int ProcessAll([CanBeNull] object onTarget, [NotNull] Predicate<ColdData> predicate, bool allowToProcessTweensInsideSequence)
	{
		return Instance.ProcessAllInternal(onTarget, predicate, allowToProcessTweensInsideSequence);
	}

	private int ProcessAllInternal([CanBeNull] object onTarget, [NotNull] Predicate<ColdData> predicate, bool allowToProcessTweensInsideSequence)
	{
		int num = 0;
		TweenArray[] array = allTweenArrays;
		foreach (TweenArray tweens in array)
		{
			updateDepth++;
			num += processInList(tweens);
			updateDepth--;
		}
		return num;
		int processInList(TweenArray tweenArray)
		{
			int num2 = 0;
			int num3 = 0;
			foreach (TweenArray.EnumeratorElement item in tweenArray)
			{
				ref TweenData tween = ref item.tween;
				ref UnmanagedTweenData data = ref item.data;
				if (tween.cold != null)
				{
					num3++;
					if (onTarget != null)
					{
						if (tween.target != onTarget)
						{
							continue;
						}
						if (!allowToProcessTweensInsideSequence && data.isInSequence)
						{
							if (logCantManipulateError)
							{
								tween.LogErrorWithStackTrace("It's not allowed to manipulate 'nested' animations, please use the parent Sequence instead.\nWhen an animation is added to another sequence, it becomes 'nested', and manipulating it directly is no longer allowed.\nUse Stop()/Complete()/isPaused/timeScale/elapsedTime/etc. of the parent animation instead.\n");
							}
							continue;
						}
					}
					if (data.isAlive && predicate(tween.cold))
					{
						num2++;
					}
				}
			}
			if (onTarget == null)
			{
				return num3;
			}
			return num2;
		}
	}

	internal void SetTweensCapacity(int capacity)
	{
		int num = tweensCount;
		if (capacity < num)
		{
			UnityEngine.Debug.LogError($"New capacity ({capacity}) should be greater than the number of currently running tweens ({num}).\n" + "You can use Tween.StopAll() to stop all running tweens.");
			return;
		}
		TweenArray[] array = allTweenArrays;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Capacity = capacity;
		}
		shakes.EnsureCapacity(capacity);
		ResizeAndSetCapacity(pool, capacity - num, capacity);
		currentPoolCapacity = capacity;
	}

	internal static void ResizeAndSetCapacity([NotNull] List<ColdData> list, int newCount, int newCapacity)
	{
		int count = list.Count;
		if (count > newCount)
		{
			int count2 = count - newCount;
			list.RemoveRange(newCount, count2);
			list.Capacity = newCapacity;
			return;
		}
		list.Capacity = newCapacity;
		if (newCount > count)
		{
			int num = newCount - count;
			for (int i = 0; i < num; i++)
			{
				list.Add(new ColdData());
			}
		}
	}

	[Conditional("UNITY_ASSERTIONS")]
	internal void WarnStructBoxingInCoroutineOnce(long id, [CanBeNull] ColdData tween)
	{
		if (warnStructBoxingAllocationInCoroutine)
		{
			warnStructBoxingAllocationInCoroutine = false;
			Assert.LogWarningWithStackTrace("Please use Tween/Sequence.ToYieldInstruction() when waiting for a Tween/Sequence in coroutines to prevent struct boxing.\n" + Constants.buildWarningCanBeDisabledMessage("warnStructBoxingAllocationInCoroutine") + "\n", id, tween?.managedData.target);
		}
	}
}
