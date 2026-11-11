using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace FMODUnityResonance;

public static class FmodResonanceAudio
{
	private struct RoomProperties
	{
		public float PositionX;

		public float PositionY;

		public float PositionZ;

		public float RotationX;

		public float RotationY;

		public float RotationZ;

		public float RotationW;

		public float DimensionsX;

		public float DimensionsY;

		public float DimensionsZ;

		public FmodResonanceAudioRoom.SurfaceMaterial MaterialLeft;

		public FmodResonanceAudioRoom.SurfaceMaterial MaterialRight;

		public FmodResonanceAudioRoom.SurfaceMaterial MaterialBottom;

		public FmodResonanceAudioRoom.SurfaceMaterial MaterialTop;

		public FmodResonanceAudioRoom.SurfaceMaterial MaterialFront;

		public FmodResonanceAudioRoom.SurfaceMaterial MaterialBack;

		public float ReflectionScalar;

		public float ReverbGain;

		public float ReverbTime;

		public float ReverbBrightness;
	}

	public const float MaxGainDb = 24f;

	public const float MinGainDb = -24f;

	public const float MaxReverbBrightness = 1f;

	public const float MinReverbBrightness = -1f;

	public const float MaxReverbTime = 3f;

	public const float MaxReflectivity = 2f;

	private static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));

	private static readonly string listenerPluginName = "Resonance Audio Listener";

	private static readonly int roomPropertiesSize = Marshal.SizeOf<RoomProperties>();

	private static readonly int roomPropertiesIndex = 1;

	private static Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

	private static List<FmodResonanceAudioRoom> enabledRooms = new List<FmodResonanceAudioRoom>();

	private static VECTOR listenerPositionFmod = default(VECTOR);

	private static DSP listenerPlugin;

	private static DSP ListenerPlugin
	{
		get
		{
			if (!listenerPlugin.hasHandle())
			{
				listenerPlugin = Initialize();
			}
			return listenerPlugin;
		}
	}

	public static void UpdateAudioRoom(FmodResonanceAudioRoom room, bool roomEnabled)
	{
		if (roomEnabled)
		{
			if (!enabledRooms.Contains(room))
			{
				enabledRooms.Add(room);
			}
		}
		else
		{
			enabledRooms.Remove(room);
		}
		if (enabledRooms.Count > 0)
		{
			RoomProperties roomProperties = GetRoomProperties(enabledRooms[enabledRooms.Count - 1]);
			IntPtr intPtr = Marshal.AllocHGlobal(roomPropertiesSize);
			Marshal.StructureToPtr(roomProperties, intPtr, fDeleteOld: false);
			ListenerPlugin.setParameterData(roomPropertiesIndex, GetBytes(intPtr, roomPropertiesSize));
			Marshal.FreeHGlobal(intPtr);
		}
		else
		{
			ListenerPlugin.setParameterData(roomPropertiesIndex, GetBytes(IntPtr.Zero, 0));
		}
	}

	public static bool IsListenerInsideRoom(FmodResonanceAudioRoom room)
	{
		RuntimeManager.CoreSystem.get3DListenerAttributes(0, out listenerPositionFmod, out var vel, out vel, out vel);
		Vector3 vector = new Vector3(listenerPositionFmod.x, listenerPositionFmod.y, listenerPositionFmod.z) - room.transform.position;
		Quaternion quaternion = Quaternion.Inverse(room.transform.rotation);
		bounds.size = Vector3.Scale(room.transform.lossyScale, room.Size);
		return bounds.Contains(quaternion * vector);
	}

	private static float ConvertAmplitudeFromDb(float db)
	{
		return Mathf.Pow(10f, 0.05f * db);
	}

	private static void ConvertAudioTransformFromUnity(ref Vector3 position, ref Quaternion rotation)
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, rotation, Vector3.one);
		matrix4x = flipZ * matrix4x * flipZ;
		position = matrix4x.GetColumn(3);
		rotation = Quaternion.LookRotation((Vector3)matrix4x.GetColumn(2), (Vector3)matrix4x.GetColumn(1));
	}

	private static byte[] GetBytes(IntPtr ptr, int length)
	{
		if (ptr != IntPtr.Zero)
		{
			byte[] array = new byte[length];
			Marshal.Copy(ptr, array, 0, length);
			return array;
		}
		return new byte[1];
	}

	private static RoomProperties GetRoomProperties(FmodResonanceAudioRoom room)
	{
		Vector3 position = room.transform.position;
		Quaternion rotation = room.transform.rotation;
		Vector3 vector = Vector3.Scale(room.transform.lossyScale, room.Size);
		ConvertAudioTransformFromUnity(ref position, ref rotation);
		RoomProperties result = default(RoomProperties);
		result.PositionX = position.x;
		result.PositionY = position.y;
		result.PositionZ = position.z;
		result.RotationX = rotation.x;
		result.RotationY = rotation.y;
		result.RotationZ = rotation.z;
		result.RotationW = rotation.w;
		result.DimensionsX = vector.x;
		result.DimensionsY = vector.y;
		result.DimensionsZ = vector.z;
		result.MaterialLeft = room.LeftWall;
		result.MaterialRight = room.RightWall;
		result.MaterialBottom = room.Floor;
		result.MaterialTop = room.Ceiling;
		result.MaterialFront = room.FrontWall;
		result.MaterialBack = room.BackWall;
		result.ReverbGain = ConvertAmplitudeFromDb(room.ReverbGainDb);
		result.ReverbTime = room.ReverbTime;
		result.ReverbBrightness = room.ReverbBrightness;
		result.ReflectionScalar = room.Reflectivity;
		return result;
	}

	private static DSP Initialize()
	{
		int count = 0;
		DSP dsp = default(DSP);
		Bank[] array = null;
		RuntimeManager.StudioSystem.getBankCount(out count);
		RuntimeManager.StudioSystem.getBankList(out array);
		for (int i = 0; i < count; i++)
		{
			int count2 = 0;
			Bus[] array2 = null;
			array[i].getBusCount(out count2);
			array[i].getBusList(out array2);
			for (int j = 0; j < count2; j++)
			{
				string path = null;
				array2[j].getPath(out path);
				RuntimeManager.StudioSystem.getBus(path, out array2[j]);
				array2[j].lockChannelGroup();
				RuntimeManager.StudioSystem.flushCommands();
				array2[j].getChannelGroup(out var group);
				if (group.hasHandle())
				{
					int numdsps = 0;
					group.getNumDSPs(out numdsps);
					for (int k = 0; k < numdsps; k++)
					{
						group.getDSP(k, out dsp);
						int channels = 0;
						uint version = 0u;
						dsp.getInfo(out var name, out version, out channels, out channels, out channels);
						if (name.ToString().Equals(listenerPluginName) && dsp.hasHandle())
						{
							return dsp;
						}
					}
				}
				array2[j].unlockChannelGroup();
			}
		}
		RuntimeUtils.DebugLogError(listenerPluginName + " not found in the FMOD project.");
		return dsp;
	}
}
