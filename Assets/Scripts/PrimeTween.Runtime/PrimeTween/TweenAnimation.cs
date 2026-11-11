using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PrimeTween;

internal class TweenAnimation
{
	public enum TweenType : byte
	{
		Disabled = 0,
		AlphaSpriteRenderer = 1,
		AudioPanStereo = 2,
		AudioPitch = 3,
		AudioVolume = 4,
		Callback = 5,
		CameraAspect = 6,
		CameraBackgroundColor = 7,
		CameraFarClipPlane = 8,
		CameraFieldOfView = 9,
		CameraNearClipPlane = 10,
		CameraOrthographicSize = 11,
		CameraPixelRect = 12,
		CameraRect = 13,
		ColorSpriteRenderer = 14,
		CustomColor = 15,
		CustomFloat = 17,
		CustomQuaternion = 18,
		CustomRect = 19,
		CustomVector2 = 20,
		CustomVector3 = 21,
		CustomVector4 = 22,
		Delay = 23,
		EulerAngles = 24,
		GlobalTimeScale = 25,
		LightColor = 26,
		LightIntensity = 27,
		LightRange = 28,
		LightShadowStrength = 29,
		LocalEulerAngles = 30,
		LocalPosition = 31,
		LocalPositionX = 32,
		LocalPositionY = 33,
		LocalPositionZ = 34,
		LocalRotation = 35,
		LocalRotationQuaternion = 36,
		MainSequence = 37,
		MaterialAlpha = 38,
		MaterialAlphaProperty = 39,
		MaterialColor = 40,
		MaterialColorProperty = 41,
		MaterialMainTextureOffset = 42,
		MaterialMainTextureScale = 43,
		MaterialProperty = 44,
		MaterialPropertyVector4 = 45,
		MaterialTextureOffset = 46,
		MaterialTextureScale = 47,
		NestedSequence = 48,
		Position = 49,
		PositionX = 50,
		PositionY = 51,
		PositionZ = 52,
		RigidbodyMovePosition = 53,
		RigidbodyMovePosition2D = 54,
		RigidbodyMoveRotation = 55,
		RigidbodyMoveRotation2D = 56,
		RigidbodyMoveRotationQuaternion = 57,
		Rotation = 58,
		RotationQuaternion = 59,
		Scale = 60,
		ScaleUniform = 61,
		ScaleX = 62,
		ScaleY = 63,
		ScaleZ = 64,
		ShakeCamera = 65,
		ShakeCustom = 66,
		ShakeLocalPosition = 67,
		ShakeLocalRotation = 68,
		ShakeScale = 69,
		TextFontSize = 70,
		TextMaxVisibleCharacters = 71,
		TweenAwaiter = 73,
		TweenTimeScale = 74,
		TweenTimeScaleSequence = 75,
		UIAlphaCanvasGroup = 76,
		UIAlphaGraphic = 77,
		UIAlphaShadow = 78,
		UIAnchoredPosition = 79,
		UIAnchoredPosition3D = 80,
		UIAnchoredPosition3DX = 81,
		UIAnchoredPosition3DY = 82,
		UIAnchoredPosition3DZ = 83,
		UIAnchoredPositionX = 84,
		UIAnchoredPositionY = 85,
		UIAnchorMax = 86,
		UIAnchorMin = 87,
		UIColorGraphic = 88,
		UIColorShadow = 89,
		UIEffectDistance = 90,
		UIFillAmount = 91,
		UIFlexibleHeight = 92,
		UIFlexibleSize = 93,
		UIFlexibleWidth = 94,
		UIHorizontalNormalizedPosition = 95,
		UIMinHeight = 96,
		UIMinSize = 97,
		UIMinWidth = 98,
		UINormalizedPosition = 99,
		UIOffsetMax = 100,
		UIOffsetMaxX = 101,
		UIOffsetMaxY = 102,
		UIOffsetMin = 103,
		UIOffsetMinX = 104,
		UIOffsetMinY = 105,
		UIPivot = 106,
		UIPivotX = 107,
		UIPivotY = 108,
		UIPreferredHeight = 109,
		UIPreferredSize = 110,
		UIPreferredWidth = 111,
		UISizeDelta = 112,
		UISliderValue = 113,
		UIVerticalNormalizedPosition = 114,
		VisualElementBackgroundColor = 115,
		VisualElementColor = 116,
		VisualElementLayout = 117,
		VisualElementOpacity = 118,
		VisualElementPosition = 119,
		VisualElementRotationQuaternion = 120,
		VisualElementScale = 121,
		VisualElementSize = 122,
		VisualElementTopLeft = 123,
		TextMaxVisibleCharactersNormalized = 130
	}

	[Serializable]
	[StructLayout(LayoutKind.Explicit)]
	public struct ValueWrapper
	{
		[FieldOffset(0)]
		[SerializeField]
		internal float x;

		[FieldOffset(4)]
		[SerializeField]
		internal float y;

		[FieldOffset(8)]
		[SerializeField]
		internal float z;

		[FieldOffset(12)]
		[SerializeField]
		internal float w;

		[FieldOffset(0)]
		[NonSerialized]
		public float single;

		[FieldOffset(0)]
		[NonSerialized]
		public Color color;

		[FieldOffset(0)]
		[NonSerialized]
		public Vector2 vector2;

		[FieldOffset(0)]
		[NonSerialized]
		public Vector3 vector3;

		[FieldOffset(0)]
		[NonSerialized]
		public Vector4 vector4;

		[FieldOffset(0)]
		[NonSerialized]
		public Quaternion quaternion;

		[FieldOffset(0)]
		[NonSerialized]
		public Rect rect;

		[FieldOffset(0)]
		[NonSerialized]
		internal double DoubleVal;

		internal float this[int i]
		{
			get
			{
				return i switch
				{
					0 => x, 
					1 => y, 
					2 => z, 
					3 => w, 
					_ => throw new IndexOutOfRangeException(), 
				};
			}
			set
			{
				switch (i)
				{
				case 0:
					x = value;
					break;
				case 1:
					y = value;
					break;
				case 2:
					z = value;
					break;
				case 3:
					w = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		internal void CopyFrom(ref float val)
		{
			x = val;
			y = 0f;
			z = 0f;
			w = 0f;
		}

		internal void CopyFrom(ref Color val)
		{
			color = val;
		}

		internal void CopyFrom(ref Vector2 val)
		{
			vector2 = val;
			z = 0f;
			w = 0f;
		}

		internal void CopyFrom(ref Vector3 val)
		{
			vector3 = val;
			w = 0f;
		}

		internal void CopyFrom(ref Vector4 val)
		{
			vector4 = val;
		}

		internal void CopyFrom(ref Rect val)
		{
			rect = val;
		}

		internal void CopyFrom(ref Quaternion val)
		{
			quaternion = val;
		}

		internal void CopyFrom(ref double val)
		{
			DoubleVal = val;
			z = 0f;
			w = 0f;
		}

		internal void Reset()
		{
			x = (y = (z = (w = 0f)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static float QuaternionAngle(ValueWrapper a, ValueWrapper b)
		{
			float num = Mathf.Min(Mathf.Abs(QuaternionDot(a, b)), 1f);
			if (!QuaternionIsEqualUsingDot(num))
			{
				return (float)((double)Mathf.Acos(num) * 2.0 * 57.295780181884766);
			}
			return 0f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool QuaternionIsEqualUsingDot(float dot)
		{
			return (double)dot > 0.9999989867210388;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float QuaternionDot(ValueWrapper a, ValueWrapper b)
		{
			return (float)((double)a.x * (double)b.x + (double)a.y * (double)b.y + (double)a.z * (double)b.z + (double)a.w * (double)b.w);
		}

		internal void QuaternionNormalize()
		{
			if (Mathf.Approximately(w, 0f))
			{
				w = 1f;
			}
			float f = Vector4Dot(this, this);
			float num = 1f / Mathf.Sqrt(f);
			x *= num;
			y *= num;
			z *= num;
			w *= num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal float Vector4Magnitude()
		{
			return Mathf.Sqrt(Vector4Dot(this, this));
		}

		private static float Vector4Dot(ValueWrapper a, ValueWrapper b)
		{
			return (float)((double)a.x * (double)b.x + (double)a.y * (double)b.y + (double)a.z * (double)b.z + (double)a.w * (double)b.w);
		}

		public override string ToString()
		{
			return vector4.ToString();
		}

		internal bool IsDefault()
		{
			if (x == 0f && y == 0f && z == 0f)
			{
				return w == 0f;
			}
			return false;
		}
	}
}
