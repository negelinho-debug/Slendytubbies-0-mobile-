using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;

namespace PrimeTween;

internal static class Assert
{
	private const string DUMMY = "_";

	internal static void LogErrorWithStackTrace(string msg, long id, [CanBeNull] object context)
	{
		UnityEngine.Debug.LogError(TryAddStackTrace(msg, id), context as Object);
	}

	internal static void LogWarningWithStackTrace(string msg, long id, [CanBeNull] object context)
	{
		UnityEngine.Debug.LogWarning(TryAddStackTrace(msg, id), context as Object);
	}

	[CanBeNull]
	[PublicAPI]
	private static string TryAddStackTrace([CanBeNull] string msg, long tweenId)
	{
		return msg;
	}

	[Conditional("_")]
	internal static void IsTrue(bool condition, long? tweenId = null, string msg = null)
	{
	}

	[Conditional("_")]
	internal static void AreEqual<T>(T expected, T actual, string msg = null)
	{
	}

	[Conditional("_")]
	internal static void AreNotEqual<T>(T expected, T actual, string msg = null)
	{
	}

	[Conditional("_")]
	internal static void IsFalse(bool condition, string msg = null)
	{
	}

	[ContractAnnotation("value:null => halt")]
	[Conditional("_")]
	internal static void IsNotNull<T>(T value, string msg = null) where T : class
	{
	}

	[Conditional("_")]
	internal static void IsNull<T>(T value, string msg = null) where T : class
	{
	}
}
