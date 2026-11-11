using System.Runtime.CompilerServices;
using UnityEngine;

namespace HostGame;

public static class VectorUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Reorient(this in Vector3 vec, in Vector3 up, in Vector3 normal)
	{
		return Vector3.Cross(Vector3.Cross(up, vec), normal);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Reorient(this in Vector3 vec, in Vector3 normal)
	{
		return Vector3.Cross(Vector3.Cross(Vector3.up, vec), normal);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 ToV3(this Vector2 vec)
	{
		return new Vector3(vec.x, 0f, vec.y);
	}

	public static Vector3 WithY(this Vector3 v, float y)
	{
		return new Vector3(v.x, y, v.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ClampThis(this ref float f, float min, float max)
	{
		f = Mathf.Clamp(f, min, max);
	}
}
