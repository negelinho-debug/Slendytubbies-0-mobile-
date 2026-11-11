using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace PrimeTween;

internal static class Utils
{
	private readonly struct MaterialPropertyBlockSetterScope : IDisposable
	{
		private readonly Renderer renderer;

		internal readonly MaterialPropertyBlock block;

		public MaterialPropertyBlockSetterScope(object target)
		{
			renderer = target as Renderer;
			block = PrimeTweenManager.Instance.materialPropertyBlockForSetter;
			renderer.GetPropertyBlock(block);
		}

		void IDisposable.Dispose()
		{
			renderer.SetPropertyBlock(block);
		}
	}

	private static bool UNITY_2021_1_OR_NEWER => true;

	internal static bool CanHaveCycles(TweenAnimation.TweenType tweenType)
	{
		if (tweenType == TweenAnimation.TweenType.Disabled || tweenType == TweenAnimation.TweenType.Callback || tweenType == TweenAnimation.TweenType.Delay)
		{
			return false;
		}
		return true;
	}

	internal static bool IsShake(TweenAnimation.TweenType tweenType)
	{
		if (tweenType - 65 <= TweenAnimation.TweenType.AudioVolume)
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsMaterialAnimation(TweenAnimation.TweenType tweenType)
	{
		if (tweenType - 38 <= TweenAnimation.TweenType.CameraFieldOfView)
		{
			return true;
		}
		return false;
	}

	private static Vector4 ST(Vector2 scale, Vector2 offset)
	{
		return new Vector4(scale.x, scale.y, offset.x, offset.y);
	}

	internal static void SetMaterialValue(TweenAnimation.TweenType tweenType, object t, long longParam, TweenAnimation.ValueWrapper value)
	{
		int nameID = (int)longParam;
		switch (tweenType)
		{
		case TweenAnimation.TweenType.MaterialColorProperty:
			(t as Material).SetColor(nameID, value.color);
			break;
		case TweenAnimation.TweenType.MaterialProperty:
			(t as Material).SetFloat(nameID, value.single);
			break;
		case TweenAnimation.TweenType.MaterialAlphaProperty:
		{
			Material material = t as Material;
			material.SetColor(nameID, material.GetColor(nameID).WithAlpha(value.single));
			break;
		}
		case TweenAnimation.TweenType.MaterialTextureOffset:
			(t as Material).SetTextureOffset(nameID, value.vector2);
			break;
		case TweenAnimation.TweenType.MaterialTextureScale:
			(t as Material).SetTextureScale(nameID, value.vector2);
			break;
		case TweenAnimation.TweenType.MaterialPropertyVector4:
			(t as Material).SetVector(nameID, value.vector4);
			break;
		case TweenAnimation.TweenType.MaterialColor:
			(t as Material).color = value.color;
			break;
		case TweenAnimation.TweenType.MaterialAlpha:
		{
			Material obj = t as Material;
			obj.color = obj.color.WithAlpha(value.single);
			break;
		}
		case TweenAnimation.TweenType.MaterialMainTextureOffset:
			(t as Material).mainTextureOffset = value.vector2;
			break;
		case TweenAnimation.TweenType.MaterialMainTextureScale:
			(t as Material).mainTextureScale = value.vector2;
			break;
		default:
			throw new Exception();
		}
	}

	internal static bool IsMaterialPropertyAnimation(TweenAnimation.TweenType tweenType)
	{
		switch (tweenType)
		{
		case TweenAnimation.TweenType.MaterialAlphaProperty:
		case TweenAnimation.TweenType.MaterialColorProperty:
		case TweenAnimation.TweenType.MaterialProperty:
		case TweenAnimation.TweenType.MaterialPropertyVector4:
		case TweenAnimation.TweenType.MaterialTextureOffset:
		case TweenAnimation.TweenType.MaterialTextureScale:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsValidMaterialProperty(TweenAnimation.TweenType tweenType, [NotNull] object target, int propId)
	{
		if (UNITY_2021_1_OR_NEWER)
		{
			switch (tweenType)
			{
			case TweenAnimation.TweenType.MaterialColorProperty:
				return (target as Material).HasColor(propId);
			case TweenAnimation.TweenType.MaterialProperty:
				return (target as Material).HasFloat(propId);
			case TweenAnimation.TweenType.MaterialAlphaProperty:
				return (target as Material).HasColor(propId);
			case TweenAnimation.TweenType.MaterialTextureOffset:
			case TweenAnimation.TweenType.MaterialTextureScale:
				return (target as Material).HasTexture(propId);
			case TweenAnimation.TweenType.MaterialPropertyVector4:
				return (target as Material).HasVector(propId);
			default:
				Debug.LogError($"Invalid tween type:{tweenType}, propId:{propId}");
				return false;
			}
		}
		return (target as Material).HasProperty(propId);
	}

	private static int Unpack_ST(long packed)
	{
		return (int)(packed >> 32);
	}

	internal static TweenAnimation.ValueWrapper GetAnimatedValue(object target, TweenAnimation.TweenType tweenType, long longParam)
	{
		TweenAnimation.ValueWrapper result;
		float single;
		switch (tweenType)
		{
		case TweenAnimation.TweenType.TextMaxVisibleCharacters:
			return new TweenAnimation.ValueWrapper
			{
				single = (target as TMP_Text).maxVisibleCharacters
			};
		case TweenAnimation.TweenType.TextMaxVisibleCharactersNormalized:
		{
			TMP_Text tMP_Text = target as TMP_Text;
			result = default(TweenAnimation.ValueWrapper);
			int? num = tMP_Text.textInfo?.characterCount;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				if (valueOrDefault > 0)
				{
					single = (float)tMP_Text.maxVisibleCharacters / (float)valueOrDefault;
					goto IL_028a;
				}
			}
			single = 0f;
			goto IL_028a;
		}
		case TweenAnimation.TweenType.GlobalTimeScale:
			return Time.timeScale.ToContainer();
		case TweenAnimation.TweenType.EulerAngles:
			return (target as Transform).eulerAngles.ToContainer();
		case TweenAnimation.TweenType.LocalEulerAngles:
			return (target as Transform).localEulerAngles.ToContainer();
		case TweenAnimation.TweenType.TweenTimeScale:
		case TweenAnimation.TweenType.TweenTimeScaleSequence:
		{
			ColdData coldData = target as ColdData;
			if (longParam != coldData.id || !coldData.data.isAlive)
			{
				return 1f.ToContainer();
			}
			return coldData.data.timeScale.ToContainer();
		}
		case TweenAnimation.TweenType.MaterialColorProperty:
			return (target as Material).GetColor((int)longParam).ToContainer();
		case TweenAnimation.TweenType.MaterialProperty:
			return (target as Material).GetFloat((int)longParam).ToContainer();
		case TweenAnimation.TweenType.MaterialAlphaProperty:
			return (target as Material).GetColor((int)longParam).a.ToContainer();
		case TweenAnimation.TweenType.MaterialTextureOffset:
			return (target as Material).GetTextureOffset((int)longParam).ToContainer();
		case TweenAnimation.TweenType.MaterialTextureScale:
			return (target as Material).GetTextureScale((int)longParam).ToContainer();
		case TweenAnimation.TweenType.MaterialPropertyVector4:
			return (target as Material).GetVector((int)longParam).ToContainer();
		case TweenAnimation.TweenType.ShakeLocalPosition:
			return (target as Transform).localPosition.ToContainer();
		case TweenAnimation.TweenType.ShakeLocalRotation:
			return (target as Transform).localRotation.ToContainer();
		case TweenAnimation.TweenType.ShakeScale:
			return (target as Transform).localScale.ToContainer();
		case TweenAnimation.TweenType.LightRange:
			return (target as Light).range.ToContainer();
		case TweenAnimation.TweenType.LightShadowStrength:
			return (target as Light).shadowStrength.ToContainer();
		case TweenAnimation.TweenType.LightIntensity:
			return (target as Light).intensity.ToContainer();
		case TweenAnimation.TweenType.LightColor:
			return (target as Light).color.ToContainer();
		case TweenAnimation.TweenType.CameraOrthographicSize:
			return (target as Camera).orthographicSize.ToContainer();
		case TweenAnimation.TweenType.CameraBackgroundColor:
			return (target as Camera).backgroundColor.ToContainer();
		case TweenAnimation.TweenType.CameraAspect:
			return (target as Camera).aspect.ToContainer();
		case TweenAnimation.TweenType.CameraFarClipPlane:
			return (target as Camera).farClipPlane.ToContainer();
		case TweenAnimation.TweenType.CameraFieldOfView:
			return (target as Camera).fieldOfView.ToContainer();
		case TweenAnimation.TweenType.CameraNearClipPlane:
			return (target as Camera).nearClipPlane.ToContainer();
		case TweenAnimation.TweenType.CameraPixelRect:
			return (target as Camera).pixelRect.ToContainer();
		case TweenAnimation.TweenType.CameraRect:
			return (target as Camera).rect.ToContainer();
		case TweenAnimation.TweenType.Position:
			return (target as Transform).position.ToContainer();
		case TweenAnimation.TweenType.PositionX:
			return (target as Transform).position.x.ToContainer();
		case TweenAnimation.TweenType.PositionY:
			return (target as Transform).position.y.ToContainer();
		case TweenAnimation.TweenType.PositionZ:
			return (target as Transform).position.z.ToContainer();
		case TweenAnimation.TweenType.LocalPosition:
			return (target as Transform).localPosition.ToContainer();
		case TweenAnimation.TweenType.LocalPositionX:
			return (target as Transform).localPosition.x.ToContainer();
		case TweenAnimation.TweenType.LocalPositionY:
			return (target as Transform).localPosition.y.ToContainer();
		case TweenAnimation.TweenType.LocalPositionZ:
			return (target as Transform).localPosition.z.ToContainer();
		case TweenAnimation.TweenType.RotationQuaternion:
			return (target as Transform).rotation.ToContainer();
		case TweenAnimation.TweenType.LocalRotationQuaternion:
			return (target as Transform).localRotation.ToContainer();
		case TweenAnimation.TweenType.Scale:
			return (target as Transform).localScale.ToContainer();
		case TweenAnimation.TweenType.ScaleX:
			return (target as Transform).localScale.x.ToContainer();
		case TweenAnimation.TweenType.ScaleY:
			return (target as Transform).localScale.y.ToContainer();
		case TweenAnimation.TweenType.ScaleZ:
			return (target as Transform).localScale.z.ToContainer();
		case TweenAnimation.TweenType.ColorSpriteRenderer:
			return (target as SpriteRenderer).color.ToContainer();
		case TweenAnimation.TweenType.AlphaSpriteRenderer:
			return (target as SpriteRenderer).color.a.ToContainer();
		case TweenAnimation.TweenType.UISliderValue:
			return (target as UnityEngine.UI.Slider).value.ToContainer();
		case TweenAnimation.TweenType.UINormalizedPosition:
			return (target as ScrollRect).GetNormalizedPosition().ToContainer();
		case TweenAnimation.TweenType.UIHorizontalNormalizedPosition:
			return (target as ScrollRect).horizontalNormalizedPosition.ToContainer();
		case TweenAnimation.TweenType.UIVerticalNormalizedPosition:
			return (target as ScrollRect).verticalNormalizedPosition.ToContainer();
		case TweenAnimation.TweenType.UIPivotX:
			return (target as RectTransform).pivot[0].ToContainer();
		case TweenAnimation.TweenType.UIPivotY:
			return (target as RectTransform).pivot[1].ToContainer();
		case TweenAnimation.TweenType.UIPivot:
			return (target as RectTransform).pivot.ToContainer();
		case TweenAnimation.TweenType.UIAnchorMax:
			return (target as RectTransform).anchorMax.ToContainer();
		case TweenAnimation.TweenType.UIAnchorMin:
			return (target as RectTransform).anchorMin.ToContainer();
		case TweenAnimation.TweenType.UIAnchoredPosition3D:
			return (target as RectTransform).anchoredPosition3D.ToContainer();
		case TweenAnimation.TweenType.UIAnchoredPosition3DX:
			return (target as RectTransform).anchoredPosition3D[0].ToContainer();
		case TweenAnimation.TweenType.UIAnchoredPosition3DY:
			return (target as RectTransform).anchoredPosition3D[1].ToContainer();
		case TweenAnimation.TweenType.UIAnchoredPosition3DZ:
			return (target as RectTransform).anchoredPosition3D[2].ToContainer();
		case TweenAnimation.TweenType.UIEffectDistance:
			return (target as Shadow).effectDistance.ToContainer();
		case TweenAnimation.TweenType.UIAlphaShadow:
			return (target as Shadow).effectColor.a.ToContainer();
		case TweenAnimation.TweenType.UIColorShadow:
			return (target as Shadow).effectColor.ToContainer();
		case TweenAnimation.TweenType.UIPreferredSize:
			return (target as LayoutElement).GetPreferredSize().ToContainer();
		case TweenAnimation.TweenType.UIPreferredWidth:
			return (target as LayoutElement).preferredWidth.ToContainer();
		case TweenAnimation.TweenType.UIPreferredHeight:
			return (target as LayoutElement).preferredHeight.ToContainer();
		case TweenAnimation.TweenType.UIFlexibleSize:
			return (target as LayoutElement).GetFlexibleSize().ToContainer();
		case TweenAnimation.TweenType.UIFlexibleWidth:
			return (target as LayoutElement).flexibleWidth.ToContainer();
		case TweenAnimation.TweenType.UIFlexibleHeight:
			return (target as LayoutElement).flexibleHeight.ToContainer();
		case TweenAnimation.TweenType.UIMinSize:
			return (target as LayoutElement).GetMinSize().ToContainer();
		case TweenAnimation.TweenType.UIMinWidth:
			return (target as LayoutElement).minWidth.ToContainer();
		case TweenAnimation.TweenType.UIMinHeight:
			return (target as LayoutElement).minHeight.ToContainer();
		case TweenAnimation.TweenType.UIColorGraphic:
			return (target as Graphic).color.ToContainer();
		case TweenAnimation.TweenType.UIAnchoredPosition:
			return (target as RectTransform).anchoredPosition.ToContainer();
		case TweenAnimation.TweenType.UIAnchoredPositionX:
			return (target as RectTransform).anchoredPosition.x.ToContainer();
		case TweenAnimation.TweenType.UIAnchoredPositionY:
			return (target as RectTransform).anchoredPosition.y.ToContainer();
		case TweenAnimation.TweenType.UISizeDelta:
			return (target as RectTransform).sizeDelta.ToContainer();
		case TweenAnimation.TweenType.UIAlphaCanvasGroup:
			return (target as CanvasGroup).alpha.ToContainer();
		case TweenAnimation.TweenType.UIAlphaGraphic:
			return (target as Graphic).color.a.ToContainer();
		case TweenAnimation.TweenType.UIFillAmount:
			return (target as UnityEngine.UI.Image).fillAmount.ToContainer();
		case TweenAnimation.TweenType.UIOffsetMin:
			return (target as RectTransform).offsetMin.ToContainer();
		case TweenAnimation.TweenType.UIOffsetMinX:
			return (target as RectTransform).offsetMin[0].ToContainer();
		case TweenAnimation.TweenType.UIOffsetMinY:
			return (target as RectTransform).offsetMin[1].ToContainer();
		case TweenAnimation.TweenType.UIOffsetMax:
			return (target as RectTransform).offsetMax.ToContainer();
		case TweenAnimation.TweenType.UIOffsetMaxX:
			return (target as RectTransform).offsetMax[0].ToContainer();
		case TweenAnimation.TweenType.UIOffsetMaxY:
			return (target as RectTransform).offsetMax[1].ToContainer();
		case TweenAnimation.TweenType.RigidbodyMovePosition:
			return (target as Rigidbody).position.ToContainer();
		case TweenAnimation.TweenType.RigidbodyMoveRotationQuaternion:
			return (target as Rigidbody).rotation.ToContainer();
		case TweenAnimation.TweenType.RigidbodyMovePosition2D:
			return (target as Rigidbody2D).position.ToContainer();
		case TweenAnimation.TweenType.RigidbodyMoveRotation2D:
			return (target as Rigidbody2D).rotation.ToContainer();
		case TweenAnimation.TweenType.MaterialColor:
			return (target as Material).color.ToContainer();
		case TweenAnimation.TweenType.MaterialAlpha:
			return (target as Material).color.a.ToContainer();
		case TweenAnimation.TweenType.MaterialMainTextureOffset:
			return (target as Material).mainTextureOffset.ToContainer();
		case TweenAnimation.TweenType.MaterialMainTextureScale:
			return (target as Material).mainTextureScale.ToContainer();
		case TweenAnimation.TweenType.AudioVolume:
			return (target as AudioSource).volume.ToContainer();
		case TweenAnimation.TweenType.AudioPitch:
			return (target as AudioSource).pitch.ToContainer();
		case TweenAnimation.TweenType.AudioPanStereo:
			return (target as AudioSource).panStereo.ToContainer();
		case TweenAnimation.TweenType.VisualElementLayout:
			return (target as VisualElement).GetResolvedStyleRect().ToContainer();
		case TweenAnimation.TweenType.VisualElementPosition:
			return (target as ITransform).position.ToContainer();
		case TweenAnimation.TweenType.VisualElementRotationQuaternion:
			return (target as ITransform).rotation.ToContainer();
		case TweenAnimation.TweenType.VisualElementScale:
			return (target as ITransform).scale.ToContainer();
		case TweenAnimation.TweenType.VisualElementSize:
			return (target as VisualElement).layout.size.ToContainer();
		case TweenAnimation.TweenType.VisualElementTopLeft:
			return (target as VisualElement).GetTopLeft().ToContainer();
		case TweenAnimation.TweenType.VisualElementColor:
			return (target as VisualElement).style.color.value.ToContainer();
		case TweenAnimation.TweenType.VisualElementBackgroundColor:
			return (target as VisualElement).style.backgroundColor.value.ToContainer();
		case TweenAnimation.TweenType.VisualElementOpacity:
			return (target as VisualElement).style.opacity.value.ToContainer();
		case TweenAnimation.TweenType.TextFontSize:
			return (target as TMP_Text).fontSize.ToContainer();
		default:
			{
				throw new Exception(tweenType.ToString());
			}
			IL_028a:
			result.single = single;
			return result;
		}
	}

	internal static bool SetAnimatedValue(ref TweenData rt, ref UnmanagedTweenData d)
	{
		TweenAnimation.TweenType tweenType = d.tweenType;
		TweenAnimation.ValueWrapper startValue = d.startValue;
		float t = d.easedInterpolationFactor;
		TweenAnimation.ValueWrapper delta = rt.endValueOrDiff;
		object target = rt.target;
		if (TweenData.IsDestroyedUnityObject(target))
		{
			rt.EmergencyStop(isTargetDestroyed: true, ref d);
			return false;
		}
		switch (tweenType)
		{
		case TweenAnimation.TweenType.TweenAwaiter:
			Tween.TweenAwaiter.UpdateTweenAwaiter(ref rt, ref d);
			break;
		case TweenAnimation.TweenType.TextMaxVisibleCharacters:
			(target as TMP_Text).maxVisibleCharacters = Mathf.RoundToInt(FloatVal());
			break;
		case TweenAnimation.TweenType.TextMaxVisibleCharactersNormalized:
		{
			TMP_Text tMP_Text = target as TMP_Text;
			tMP_Text.maxVisibleCharacters = Mathf.RoundToInt(Mathf.Lerp(0f, tMP_Text.textInfo?.characterCount ?? 0, FloatVal()));
			break;
		}
		case TweenAnimation.TweenType.GlobalTimeScale:
			Time.timeScale = FloatVal();
			break;
		case TweenAnimation.TweenType.EulerAngles:
			(target as Transform).eulerAngles = Vector3Val();
			break;
		case TweenAnimation.TweenType.LocalEulerAngles:
			(target as Transform).localEulerAngles = Vector3Val();
			break;
		case TweenAnimation.TweenType.TweenTimeScale:
		case TweenAnimation.TweenType.TweenTimeScaleSequence:
		{
			ColdData coldData = target as ColdData;
			if (rt.cold.longParam != coldData.id || !coldData.data.isAlive)
			{
				rt.EmergencyStop(isTargetDestroyed: false, ref d);
				return false;
			}
			coldData.data.timeScale = FloatVal();
			break;
		}
		case TweenAnimation.TweenType.ShakeLocalRotation:
			(target as Transform).localRotation = startValue.quaternion * Quaternion.Euler(Tween.getShakeVal(ref rt, ref d));
			break;
		case TweenAnimation.TweenType.ShakeScale:
			(target as Transform).localScale = startValue.vector3 + Tween.getShakeVal(ref rt, ref d);
			break;
		case TweenAnimation.TweenType.ShakeLocalPosition:
			(target as Transform).localPosition = startValue.vector3 + Tween.getShakeVal(ref rt, ref d);
			break;
		case TweenAnimation.TweenType.LightRange:
			(target as Light).range = FloatVal();
			break;
		case TweenAnimation.TweenType.LightShadowStrength:
			(target as Light).shadowStrength = FloatVal();
			break;
		case TweenAnimation.TweenType.LightIntensity:
			(target as Light).intensity = FloatVal();
			break;
		case TweenAnimation.TweenType.LightColor:
			(target as Light).color = ColorVal();
			break;
		case TweenAnimation.TweenType.CameraOrthographicSize:
			(target as Camera).orthographicSize = FloatVal();
			break;
		case TweenAnimation.TweenType.CameraBackgroundColor:
			(target as Camera).backgroundColor = ColorVal();
			break;
		case TweenAnimation.TweenType.CameraAspect:
			(target as Camera).aspect = FloatVal();
			break;
		case TweenAnimation.TweenType.CameraFarClipPlane:
			(target as Camera).farClipPlane = FloatVal();
			break;
		case TweenAnimation.TweenType.CameraFieldOfView:
			(target as Camera).fieldOfView = FloatVal();
			break;
		case TweenAnimation.TweenType.CameraNearClipPlane:
			(target as Camera).nearClipPlane = FloatVal();
			break;
		case TweenAnimation.TweenType.CameraPixelRect:
			(target as Camera).pixelRect = RectVal();
			break;
		case TweenAnimation.TweenType.CameraRect:
			(target as Camera).rect = RectVal();
			break;
		case TweenAnimation.TweenType.Position:
			(target as Transform).position = Vector3Val();
			break;
		case TweenAnimation.TweenType.PositionX:
		{
			Transform obj31 = target as Transform;
			obj31.position = Extensions.WithComponent(val: FloatVal(), v: obj31.position, index: 0);
			break;
		}
		case TweenAnimation.TweenType.PositionY:
		{
			Transform obj30 = target as Transform;
			obj30.position = Extensions.WithComponent(val: FloatVal(), v: obj30.position, index: 1);
			break;
		}
		case TweenAnimation.TweenType.PositionZ:
		{
			Transform obj29 = target as Transform;
			obj29.position = Extensions.WithComponent(val: FloatVal(), v: obj29.position, index: 2);
			break;
		}
		case TweenAnimation.TweenType.LocalPosition:
			(target as Transform).localPosition = Vector3Val();
			break;
		case TweenAnimation.TweenType.LocalPositionX:
		{
			Transform obj28 = target as Transform;
			obj28.localPosition = Extensions.WithComponent(val: FloatVal(), v: obj28.localPosition, index: 0);
			break;
		}
		case TweenAnimation.TweenType.LocalPositionY:
		{
			Transform obj27 = target as Transform;
			obj27.localPosition = Extensions.WithComponent(val: FloatVal(), v: obj27.localPosition, index: 1);
			break;
		}
		case TweenAnimation.TweenType.LocalPositionZ:
		{
			Transform obj26 = target as Transform;
			obj26.localPosition = Extensions.WithComponent(val: FloatVal(), v: obj26.localPosition, index: 2);
			break;
		}
		case TweenAnimation.TweenType.RotationQuaternion:
			(target as Transform).rotation = QuaternionVal();
			break;
		case TweenAnimation.TweenType.LocalRotationQuaternion:
			(target as Transform).localRotation = QuaternionVal();
			break;
		case TweenAnimation.TweenType.Scale:
			(target as Transform).localScale = Vector3Val();
			break;
		case TweenAnimation.TweenType.ScaleX:
		{
			Transform obj25 = target as Transform;
			obj25.localScale = Extensions.WithComponent(val: FloatVal(), v: obj25.localScale, index: 0);
			break;
		}
		case TweenAnimation.TweenType.ScaleY:
		{
			Transform obj24 = target as Transform;
			obj24.localScale = Extensions.WithComponent(val: FloatVal(), v: obj24.localScale, index: 1);
			break;
		}
		case TweenAnimation.TweenType.ScaleZ:
		{
			Transform obj23 = target as Transform;
			obj23.localScale = Extensions.WithComponent(val: FloatVal(), v: obj23.localScale, index: 2);
			break;
		}
		case TweenAnimation.TweenType.ColorSpriteRenderer:
			(target as SpriteRenderer).color = ColorVal();
			break;
		case TweenAnimation.TweenType.AlphaSpriteRenderer:
		{
			SpriteRenderer obj22 = target as SpriteRenderer;
			obj22.color = Extensions.WithAlpha(alpha: FloatVal(), c: obj22.color);
			break;
		}
		case TweenAnimation.TweenType.UISliderValue:
			(target as UnityEngine.UI.Slider).value = FloatVal();
			break;
		case TweenAnimation.TweenType.UINormalizedPosition:
		{
			ScrollRect target5 = target as ScrollRect;
			Vector2 vector5 = Vector2Val();
			target5.SetNormalizedPosition(vector5);
			break;
		}
		case TweenAnimation.TweenType.UIHorizontalNormalizedPosition:
			(target as ScrollRect).horizontalNormalizedPosition = FloatVal();
			break;
		case TweenAnimation.TweenType.UIVerticalNormalizedPosition:
			(target as ScrollRect).verticalNormalizedPosition = FloatVal();
			break;
		case TweenAnimation.TweenType.UIPivotX:
		{
			RectTransform obj21 = target as RectTransform;
			obj21.pivot = Extensions.WithComponent(val: FloatVal(), v: obj21.pivot, index: 0);
			break;
		}
		case TweenAnimation.TweenType.UIPivotY:
		{
			RectTransform obj20 = target as RectTransform;
			obj20.pivot = Extensions.WithComponent(val: FloatVal(), v: obj20.pivot, index: 1);
			break;
		}
		case TweenAnimation.TweenType.UIPivot:
			(target as RectTransform).pivot = Vector2Val();
			break;
		case TweenAnimation.TweenType.UIAnchorMax:
			(target as RectTransform).anchorMax = Vector2Val();
			break;
		case TweenAnimation.TweenType.UIAnchorMin:
			(target as RectTransform).anchorMin = Vector2Val();
			break;
		case TweenAnimation.TweenType.UIAnchoredPosition3D:
			(target as RectTransform).anchoredPosition3D = Vector3Val();
			break;
		case TweenAnimation.TweenType.UIAnchoredPosition3DX:
		{
			RectTransform obj19 = target as RectTransform;
			obj19.anchoredPosition3D = Extensions.WithComponent(val: FloatVal(), v: obj19.anchoredPosition3D, index: 0);
			break;
		}
		case TweenAnimation.TweenType.UIAnchoredPosition3DY:
		{
			RectTransform obj18 = target as RectTransform;
			obj18.anchoredPosition3D = Extensions.WithComponent(val: FloatVal(), v: obj18.anchoredPosition3D, index: 1);
			break;
		}
		case TweenAnimation.TweenType.UIAnchoredPosition3DZ:
		{
			RectTransform obj17 = target as RectTransform;
			obj17.anchoredPosition3D = Extensions.WithComponent(val: FloatVal(), v: obj17.anchoredPosition3D, index: 2);
			break;
		}
		case TweenAnimation.TweenType.UIEffectDistance:
			(target as Shadow).effectDistance = Vector2Val();
			break;
		case TweenAnimation.TweenType.UIAlphaShadow:
		{
			Shadow obj16 = target as Shadow;
			obj16.effectColor = Extensions.WithAlpha(alpha: FloatVal(), c: obj16.effectColor);
			break;
		}
		case TweenAnimation.TweenType.UIColorShadow:
			(target as Shadow).effectColor = ColorVal();
			break;
		case TweenAnimation.TweenType.UIPreferredSize:
		{
			LayoutElement target4 = target as LayoutElement;
			Vector2 vector4 = Vector2Val();
			target4.SetPreferredSize(vector4);
			break;
		}
		case TweenAnimation.TweenType.UIPreferredWidth:
			(target as LayoutElement).preferredWidth = FloatVal();
			break;
		case TweenAnimation.TweenType.UIPreferredHeight:
			(target as LayoutElement).preferredHeight = FloatVal();
			break;
		case TweenAnimation.TweenType.UIFlexibleSize:
		{
			LayoutElement target3 = target as LayoutElement;
			Vector2 vector3 = Vector2Val();
			target3.SetFlexibleSize(vector3);
			break;
		}
		case TweenAnimation.TweenType.UIFlexibleWidth:
			(target as LayoutElement).flexibleWidth = FloatVal();
			break;
		case TweenAnimation.TweenType.UIFlexibleHeight:
			(target as LayoutElement).flexibleHeight = FloatVal();
			break;
		case TweenAnimation.TweenType.UIMinSize:
		{
			LayoutElement target2 = target as LayoutElement;
			Vector2 vector2 = Vector2Val();
			target2.SetMinSize(vector2);
			break;
		}
		case TweenAnimation.TweenType.UIMinWidth:
			(target as LayoutElement).minWidth = FloatVal();
			break;
		case TweenAnimation.TweenType.UIMinHeight:
			(target as LayoutElement).minHeight = FloatVal();
			break;
		case TweenAnimation.TweenType.UIColorGraphic:
			(target as Graphic).color = ColorVal();
			break;
		case TweenAnimation.TweenType.UIAnchoredPosition:
			(target as RectTransform).anchoredPosition = Vector2Val();
			break;
		case TweenAnimation.TweenType.UIAnchoredPositionX:
		{
			RectTransform obj15 = target as RectTransform;
			obj15.anchoredPosition = Extensions.WithComponent(val: FloatVal(), v: obj15.anchoredPosition, index: 0);
			break;
		}
		case TweenAnimation.TweenType.UIAnchoredPositionY:
		{
			RectTransform obj14 = target as RectTransform;
			obj14.anchoredPosition = Extensions.WithComponent(val: FloatVal(), v: obj14.anchoredPosition, index: 1);
			break;
		}
		case TweenAnimation.TweenType.UISizeDelta:
			(target as RectTransform).sizeDelta = Vector2Val();
			break;
		case TweenAnimation.TweenType.UIAlphaCanvasGroup:
			(target as CanvasGroup).alpha = FloatVal();
			break;
		case TweenAnimation.TweenType.UIAlphaGraphic:
		{
			Graphic obj13 = target as Graphic;
			obj13.color = Extensions.WithAlpha(alpha: FloatVal(), c: obj13.color);
			break;
		}
		case TweenAnimation.TweenType.UIFillAmount:
			(target as UnityEngine.UI.Image).fillAmount = FloatVal();
			break;
		case TweenAnimation.TweenType.UIOffsetMin:
			(target as RectTransform).offsetMin = Vector2Val();
			break;
		case TweenAnimation.TweenType.UIOffsetMinX:
		{
			RectTransform obj12 = target as RectTransform;
			obj12.offsetMin = Extensions.WithComponent(val: FloatVal(), v: obj12.offsetMin, index: 0);
			break;
		}
		case TweenAnimation.TweenType.UIOffsetMinY:
		{
			RectTransform obj11 = target as RectTransform;
			obj11.offsetMin = Extensions.WithComponent(val: FloatVal(), v: obj11.offsetMin, index: 1);
			break;
		}
		case TweenAnimation.TweenType.UIOffsetMax:
			(target as RectTransform).offsetMax = Vector2Val();
			break;
		case TweenAnimation.TweenType.UIOffsetMaxX:
		{
			RectTransform obj10 = target as RectTransform;
			obj10.offsetMax = Extensions.WithComponent(val: FloatVal(), v: obj10.offsetMax, index: 0);
			break;
		}
		case TweenAnimation.TweenType.UIOffsetMaxY:
		{
			RectTransform obj9 = target as RectTransform;
			obj9.offsetMax = Extensions.WithComponent(val: FloatVal(), v: obj9.offsetMax, index: 1);
			break;
		}
		case TweenAnimation.TweenType.RigidbodyMovePosition:
		{
			Rigidbody obj8 = target as Rigidbody;
			Vector3 position2 = Vector3Val();
			obj8.MovePosition(position2);
			break;
		}
		case TweenAnimation.TweenType.RigidbodyMoveRotationQuaternion:
		{
			Rigidbody obj7 = target as Rigidbody;
			Quaternion rotation = QuaternionVal();
			obj7.MoveRotation(rotation);
			break;
		}
		case TweenAnimation.TweenType.RigidbodyMovePosition2D:
		{
			Rigidbody2D obj6 = target as Rigidbody2D;
			Vector2 position = Vector2Val();
			obj6.MovePosition(position);
			break;
		}
		case TweenAnimation.TweenType.RigidbodyMoveRotation2D:
		{
			Rigidbody2D obj5 = target as Rigidbody2D;
			float angle = FloatVal();
			obj5.MoveRotation(angle);
			break;
		}
		case TweenAnimation.TweenType.AudioVolume:
			(target as AudioSource).volume = FloatVal();
			break;
		case TweenAnimation.TweenType.AudioPitch:
			(target as AudioSource).pitch = FloatVal();
			break;
		case TweenAnimation.TweenType.AudioPanStereo:
			(target as AudioSource).panStereo = FloatVal();
			break;
		case TweenAnimation.TweenType.VisualElementLayout:
		{
			VisualElement e2 = target as VisualElement;
			Rect c2 = RectVal();
			e2.SetStyleRect(c2);
			break;
		}
		case TweenAnimation.TweenType.VisualElementPosition:
			(target as ITransform).position = Vector3Val();
			break;
		case TweenAnimation.TweenType.VisualElementRotationQuaternion:
			(target as ITransform).rotation = QuaternionVal();
			break;
		case TweenAnimation.TweenType.VisualElementScale:
			(target as ITransform).scale = Vector3Val();
			break;
		case TweenAnimation.TweenType.VisualElementSize:
		{
			VisualElement obj4 = target as VisualElement;
			Vector2 vector = Vector2Val();
			obj4.style.width = vector.x;
			obj4.style.height = vector.y;
			break;
		}
		case TweenAnimation.TweenType.VisualElementTopLeft:
		{
			VisualElement e = target as VisualElement;
			Vector2 c = Vector2Val();
			e.SetTopLeft(c);
			break;
		}
		case TweenAnimation.TweenType.VisualElementColor:
		{
			VisualElement obj3 = target as VisualElement;
			Color color2 = ColorVal();
			obj3.style.color = color2;
			break;
		}
		case TweenAnimation.TweenType.VisualElementBackgroundColor:
		{
			VisualElement obj2 = target as VisualElement;
			Color color = ColorVal();
			obj2.style.backgroundColor = color;
			break;
		}
		case TweenAnimation.TweenType.VisualElementOpacity:
		{
			VisualElement obj = target as VisualElement;
			float num = FloatVal();
			obj.style.opacity = num;
			break;
		}
		case TweenAnimation.TweenType.TextFontSize:
			(target as TMP_Text).fontSize = FloatVal();
			break;
		default:
			if (IsMaterialAnimation(tweenType))
			{
				SetMaterialValue(tweenType, target, rt.cold.longParam, Vector4Val().ToContainer());
			}
			else
			{
				rt.cold.onValueChange(ref rt, ref d);
			}
			break;
		case TweenAnimation.TweenType.Delay:
		case TweenAnimation.TweenType.MainSequence:
		case TweenAnimation.TweenType.NestedSequence:
			break;
		}
		return true;
		Color ColorVal()
		{
			return new Color(startValue.x + delta.x * t, startValue.y + delta.y * t, startValue.z + delta.z * t, startValue.w + delta.w * t);
		}
		float FloatVal()
		{
			return startValue.single + delta.single * t;
		}
		Quaternion QuaternionVal()
		{
			return Quaternion.SlerpUnclamped(startValue.quaternion, delta.quaternion, t);
		}
		Rect RectVal()
		{
			return new Rect(startValue.x + delta.x * t, startValue.y + delta.y * t, startValue.z + delta.z * t, startValue.w + delta.w * t);
		}
		Vector2 Vector2Val()
		{
			return new Vector2(startValue.x + delta.x * t, startValue.y + delta.y * t);
		}
		Vector3 Vector3Val()
		{
			return new Vector3(startValue.x + delta.x * t, startValue.y + delta.y * t, startValue.z + delta.z * t);
		}
		Vector4 Vector4Val()
		{
			return new Vector4(startValue.x + delta.x * t, startValue.y + delta.y * t, startValue.z + delta.z * t, startValue.w + delta.w * t);
		}
	}

	internal static (PropType, Type) TweenTypeToTweenData(TweenAnimation.TweenType tweenType)
	{
		return tweenType switch
		{
			TweenAnimation.TweenType.Disabled => (PropType.Float, null), 
			TweenAnimation.TweenType.LightRange => (PropType.Float, typeof(Light)), 
			TweenAnimation.TweenType.LightShadowStrength => (PropType.Float, typeof(Light)), 
			TweenAnimation.TweenType.LightIntensity => (PropType.Float, typeof(Light)), 
			TweenAnimation.TweenType.LightColor => (PropType.Color, typeof(Light)), 
			TweenAnimation.TweenType.CameraOrthographicSize => (PropType.Float, typeof(Camera)), 
			TweenAnimation.TweenType.CameraBackgroundColor => (PropType.Color, typeof(Camera)), 
			TweenAnimation.TweenType.CameraAspect => (PropType.Float, typeof(Camera)), 
			TweenAnimation.TweenType.CameraFarClipPlane => (PropType.Float, typeof(Camera)), 
			TweenAnimation.TweenType.CameraFieldOfView => (PropType.Float, typeof(Camera)), 
			TweenAnimation.TweenType.CameraNearClipPlane => (PropType.Float, typeof(Camera)), 
			TweenAnimation.TweenType.CameraPixelRect => (PropType.Rect, typeof(Camera)), 
			TweenAnimation.TweenType.CameraRect => (PropType.Rect, typeof(Camera)), 
			TweenAnimation.TweenType.LocalRotation => (PropType.Vector3, typeof(Transform)), 
			TweenAnimation.TweenType.ScaleUniform => (PropType.Float, typeof(Transform)), 
			TweenAnimation.TweenType.Rotation => (PropType.Vector3, typeof(Transform)), 
			TweenAnimation.TweenType.Position => (PropType.Vector3, typeof(Transform)), 
			TweenAnimation.TweenType.PositionX => (PropType.Float, typeof(Transform)), 
			TweenAnimation.TweenType.PositionY => (PropType.Float, typeof(Transform)), 
			TweenAnimation.TweenType.PositionZ => (PropType.Float, typeof(Transform)), 
			TweenAnimation.TweenType.LocalPosition => (PropType.Vector3, typeof(Transform)), 
			TweenAnimation.TweenType.LocalPositionX => (PropType.Float, typeof(Transform)), 
			TweenAnimation.TweenType.LocalPositionY => (PropType.Float, typeof(Transform)), 
			TweenAnimation.TweenType.LocalPositionZ => (PropType.Float, typeof(Transform)), 
			TweenAnimation.TweenType.RotationQuaternion => (PropType.Quaternion, typeof(Transform)), 
			TweenAnimation.TweenType.LocalRotationQuaternion => (PropType.Quaternion, typeof(Transform)), 
			TweenAnimation.TweenType.Scale => (PropType.Vector3, typeof(Transform)), 
			TweenAnimation.TweenType.ScaleX => (PropType.Float, typeof(Transform)), 
			TweenAnimation.TweenType.ScaleY => (PropType.Float, typeof(Transform)), 
			TweenAnimation.TweenType.ScaleZ => (PropType.Float, typeof(Transform)), 
			TweenAnimation.TweenType.ColorSpriteRenderer => (PropType.Color, typeof(SpriteRenderer)), 
			TweenAnimation.TweenType.AlphaSpriteRenderer => (PropType.Float, typeof(SpriteRenderer)), 
			TweenAnimation.TweenType.TweenTimeScale => (PropType.Float, typeof(Tween)), 
			TweenAnimation.TweenType.TweenTimeScaleSequence => (PropType.Float, typeof(Sequence)), 
			TweenAnimation.TweenType.UISliderValue => (PropType.Float, typeof(UnityEngine.UI.Slider)), 
			TweenAnimation.TweenType.UINormalizedPosition => (PropType.Vector2, typeof(ScrollRect)), 
			TweenAnimation.TweenType.UIHorizontalNormalizedPosition => (PropType.Float, typeof(ScrollRect)), 
			TweenAnimation.TweenType.UIVerticalNormalizedPosition => (PropType.Float, typeof(ScrollRect)), 
			TweenAnimation.TweenType.UIPivotX => (PropType.Float, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIPivotY => (PropType.Float, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIPivot => (PropType.Vector2, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIAnchorMax => (PropType.Vector2, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIAnchorMin => (PropType.Vector2, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIAnchoredPosition3D => (PropType.Vector3, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIAnchoredPosition3DX => (PropType.Float, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIAnchoredPosition3DY => (PropType.Float, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIAnchoredPosition3DZ => (PropType.Float, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIEffectDistance => (PropType.Vector2, typeof(Shadow)), 
			TweenAnimation.TweenType.UIAlphaShadow => (PropType.Float, typeof(Shadow)), 
			TweenAnimation.TweenType.UIColorShadow => (PropType.Color, typeof(Shadow)), 
			TweenAnimation.TweenType.UIPreferredSize => (PropType.Vector2, typeof(LayoutElement)), 
			TweenAnimation.TweenType.UIPreferredWidth => (PropType.Float, typeof(LayoutElement)), 
			TweenAnimation.TweenType.UIPreferredHeight => (PropType.Float, typeof(LayoutElement)), 
			TweenAnimation.TweenType.UIFlexibleSize => (PropType.Vector2, typeof(LayoutElement)), 
			TweenAnimation.TweenType.UIFlexibleWidth => (PropType.Float, typeof(LayoutElement)), 
			TweenAnimation.TweenType.UIFlexibleHeight => (PropType.Float, typeof(LayoutElement)), 
			TweenAnimation.TweenType.UIMinSize => (PropType.Vector2, typeof(LayoutElement)), 
			TweenAnimation.TweenType.UIMinWidth => (PropType.Float, typeof(LayoutElement)), 
			TweenAnimation.TweenType.UIMinHeight => (PropType.Float, typeof(LayoutElement)), 
			TweenAnimation.TweenType.UIColorGraphic => (PropType.Color, typeof(Graphic)), 
			TweenAnimation.TweenType.UIAnchoredPosition => (PropType.Vector2, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIAnchoredPositionX => (PropType.Float, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIAnchoredPositionY => (PropType.Float, typeof(RectTransform)), 
			TweenAnimation.TweenType.UISizeDelta => (PropType.Vector2, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIAlphaCanvasGroup => (PropType.Float, typeof(CanvasGroup)), 
			TweenAnimation.TweenType.UIAlphaGraphic => (PropType.Float, typeof(Graphic)), 
			TweenAnimation.TweenType.UIFillAmount => (PropType.Float, typeof(UnityEngine.UI.Image)), 
			TweenAnimation.TweenType.UIOffsetMin => (PropType.Vector2, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIOffsetMinX => (PropType.Float, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIOffsetMinY => (PropType.Float, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIOffsetMax => (PropType.Vector2, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIOffsetMaxX => (PropType.Float, typeof(RectTransform)), 
			TweenAnimation.TweenType.UIOffsetMaxY => (PropType.Float, typeof(RectTransform)), 
			TweenAnimation.TweenType.RigidbodyMoveRotation => (PropType.Vector3, typeof(Rigidbody)), 
			TweenAnimation.TweenType.RigidbodyMovePosition => (PropType.Vector3, typeof(Rigidbody)), 
			TweenAnimation.TweenType.RigidbodyMoveRotationQuaternion => (PropType.Quaternion, typeof(Rigidbody)), 
			TweenAnimation.TweenType.RigidbodyMovePosition2D => (PropType.Vector2, typeof(Rigidbody2D)), 
			TweenAnimation.TweenType.RigidbodyMoveRotation2D => (PropType.Float, typeof(Rigidbody2D)), 
			TweenAnimation.TweenType.MaterialColor => (PropType.Color, typeof(Material)), 
			TweenAnimation.TweenType.MaterialAlpha => (PropType.Float, typeof(Material)), 
			TweenAnimation.TweenType.MaterialMainTextureOffset => (PropType.Vector2, typeof(Material)), 
			TweenAnimation.TweenType.MaterialMainTextureScale => (PropType.Vector2, typeof(Material)), 
			TweenAnimation.TweenType.AudioVolume => (PropType.Float, typeof(AudioSource)), 
			TweenAnimation.TweenType.AudioPitch => (PropType.Float, typeof(AudioSource)), 
			TweenAnimation.TweenType.AudioPanStereo => (PropType.Float, typeof(AudioSource)), 
			TweenAnimation.TweenType.VisualElementLayout => (PropType.Rect, typeof(VisualElement)), 
			TweenAnimation.TweenType.VisualElementPosition => (PropType.Vector3, typeof(ITransform)), 
			TweenAnimation.TweenType.VisualElementRotationQuaternion => (PropType.Quaternion, typeof(ITransform)), 
			TweenAnimation.TweenType.VisualElementScale => (PropType.Vector3, typeof(ITransform)), 
			TweenAnimation.TweenType.VisualElementSize => (PropType.Vector2, typeof(VisualElement)), 
			TweenAnimation.TweenType.VisualElementTopLeft => (PropType.Vector2, typeof(VisualElement)), 
			TweenAnimation.TweenType.VisualElementColor => (PropType.Color, typeof(VisualElement)), 
			TweenAnimation.TweenType.VisualElementBackgroundColor => (PropType.Color, typeof(VisualElement)), 
			TweenAnimation.TweenType.VisualElementOpacity => (PropType.Float, typeof(VisualElement)), 
			TweenAnimation.TweenType.TextMaxVisibleCharacters => (PropType.Int, typeof(TMP_Text)), 
			TweenAnimation.TweenType.TextFontSize => (PropType.Float, typeof(TMP_Text)), 
			TweenAnimation.TweenType.Delay => (PropType.Float, null), 
			TweenAnimation.TweenType.Callback => (PropType.Float, null), 
			TweenAnimation.TweenType.ShakeLocalPosition => (PropType.Vector3, typeof(Transform)), 
			TweenAnimation.TweenType.ShakeLocalRotation => (PropType.Quaternion, typeof(Transform)), 
			TweenAnimation.TweenType.ShakeScale => (PropType.Vector3, typeof(Transform)), 
			TweenAnimation.TweenType.ShakeCustom => (PropType.Vector3, null), 
			TweenAnimation.TweenType.ShakeCamera => (PropType.Float, typeof(Camera)), 
			TweenAnimation.TweenType.CustomFloat => (PropType.Float, null), 
			TweenAnimation.TweenType.CustomColor => (PropType.Color, null), 
			TweenAnimation.TweenType.CustomVector2 => (PropType.Vector2, null), 
			TweenAnimation.TweenType.CustomVector3 => (PropType.Vector3, null), 
			TweenAnimation.TweenType.CustomVector4 => (PropType.Vector4, null), 
			TweenAnimation.TweenType.CustomQuaternion => (PropType.Quaternion, null), 
			TweenAnimation.TweenType.CustomRect => (PropType.Rect, null), 
			TweenAnimation.TweenType.MaterialColorProperty => (PropType.Color, typeof(Material)), 
			TweenAnimation.TweenType.MaterialProperty => (PropType.Float, typeof(Material)), 
			TweenAnimation.TweenType.MaterialAlphaProperty => (PropType.Float, typeof(Material)), 
			TweenAnimation.TweenType.MaterialTextureOffset => (PropType.Vector2, typeof(Material)), 
			TweenAnimation.TweenType.MaterialTextureScale => (PropType.Vector2, typeof(Material)), 
			TweenAnimation.TweenType.MaterialPropertyVector4 => (PropType.Vector4, typeof(Material)), 
			TweenAnimation.TweenType.EulerAngles => (PropType.Vector3, typeof(Transform)), 
			TweenAnimation.TweenType.LocalEulerAngles => (PropType.Vector3, typeof(Transform)), 
			TweenAnimation.TweenType.GlobalTimeScale => (PropType.Float, null), 
			TweenAnimation.TweenType.MainSequence => (PropType.Float, null), 
			TweenAnimation.TweenType.NestedSequence => (PropType.Float, null), 
			TweenAnimation.TweenType.TweenAwaiter => (PropType.Float, null), 
			TweenAnimation.TweenType.TextMaxVisibleCharactersNormalized => (PropType.Float, typeof(TMP_Text)), 
			_ => throw new Exception($"Unsupported tween type: {tweenType}. Please install necessary packages (TextMeshPro, UGUI, etc.) or use a newer version of Unity."), 
		};
	}
}
