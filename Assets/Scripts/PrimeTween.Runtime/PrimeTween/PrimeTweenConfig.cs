using System;
using System.ComponentModel;
using JetBrains.Annotations;
using UnityEngine;

namespace PrimeTween;

[PublicAPI]
public static class PrimeTweenConfig
{
	internal const bool defaultUseUnscaledTimeForShakes = false;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("this setting is replaced with the 'warnIfTargetDestroyed' parameter that you can pass to the tween.OnComplete(), Tween.Delay() and Sequence.ChainDelay() methods.")]
	public static bool warnDestroyedTweenHasOnComplete
	{
		set
		{
		}
	}

	internal static PrimeTweenManager Instance => PrimeTweenManager.Instance;

	public static Ease defaultEase
	{
		get
		{
			return Instance.defaultEase;
		}
		set
		{
			if (value == Ease.Custom || value == Ease.Default)
			{
				Debug.LogError("defaultEase can't be Ease.Custom or Ease.Default.");
			}
			else
			{
				Instance.defaultEase = value;
			}
		}
	}

	public static UpdateType defaultUpdateType
	{
		get
		{
			return new UpdateType(Instance.defaultUpdateType);
		}
		set
		{
			if (value == UpdateType.Default)
			{
				Debug.LogError("defaultUpdateType can't be Default.");
			}
			else
			{
				Instance.defaultUpdateType = value.enumValue;
			}
		}
	}

	public static bool warnTweenOnDisabledTarget
	{
		internal get
		{
			return Instance.warnTweenOnDisabledTarget;
		}
		set
		{
			Instance.warnTweenOnDisabledTarget = value;
		}
	}

	public static bool warnZeroDuration
	{
		internal get
		{
			return Instance.warnZeroDuration;
		}
		set
		{
			Instance.warnZeroDuration = value;
		}
	}

	public static bool warnStructBoxingAllocationInCoroutine
	{
		set
		{
			Instance.warnStructBoxingAllocationInCoroutine = value;
		}
	}

	public static bool validateCustomCurves
	{
		set
		{
			Instance.validateCustomCurves = value;
		}
	}

	public static bool warnBenchmarkWithAsserts
	{
		set
		{
			Instance.warnBenchmarkWithAsserts = value;
		}
	}

	public static bool warnEndValueEqualsCurrent
	{
		set
		{
			Instance.warnEndValueEqualsCurrent = value;
		}
	}

	public static bool warnIfTargetDestroyed
	{
		set
		{
			Instance.warnIfTargetDestroyed = value;
		}
	}

	public static void SetTweensCapacity(int capacity)
	{
		if (PrimeTweenManager.HasInstance && !PrimeTweenManager.Instance.isDestroyed)
		{
			PrimeTweenManager.Instance.SetTweensCapacity(capacity);
		}
		else
		{
			PrimeTweenManager.customInitialCapacity = capacity;
		}
	}

	internal static void ManualInitialize()
	{
		if (!PrimeTweenManager.HasInstance)
		{
			PrimeTweenManager.CreateInstanceAndDontDestroy();
		}
		else
		{
			Debug.LogError("'PrimeTweenManager' is already created. Use this method only to create 'PrimeTweenManager' before 'RuntimeInitializeLoadType.BeforeSceneLoad'.");
		}
	}
}
