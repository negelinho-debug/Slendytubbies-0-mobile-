using System;
using UnityEngine;

namespace HostGame;

[Serializable]
public class InputData
{
	public Vector3 MoveVectorOS;

	public Vector2 CameraVector;

	public float DesiredSpeed;

	public InputMoveFlags MoveFlags;

	public bool WantMove => MoveVectorOS.sqrMagnitude > 0.00012207031f;

	public bool WantJump
	{
		get
		{
			return (MoveFlags & InputMoveFlags.WantJump) != 0;
		}
		set
		{
			MoveFlags = (value ? (MoveFlags | InputMoveFlags.WantJump) : (MoveFlags & ~InputMoveFlags.WantJump));
		}
	}

	public Vector3 MoveVectorWS(Transform target)
	{
		return target.forward * MoveVectorOS.z + target.right * MoveVectorOS.x;
	}
}
