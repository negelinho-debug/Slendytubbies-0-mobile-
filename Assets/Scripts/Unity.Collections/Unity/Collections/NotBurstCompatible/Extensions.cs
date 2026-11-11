using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace PrimeTween;

internal static class Extensions
{
	internal static float CalcDistance(Vector3 v1, Vector3 v2)
	{
		return Vector3.Distance(v1, v2);
	}

	internal static float CalcDistance(Quaternion q1, Quaternion q2)
	{
		return Quaternion.Angle(q1, q2);
	}

	internal static float calcDelta(this float val, TweenAnimation.ValueWrapper prevVal)
	{
		return val - prevVal.single;
	}

	internal static double calcDelta(this double val, TweenAnimation.ValueWrapper prevVal)
	{
		return val - prevVal.DoubleVal;
	}

	internal static Color calcDelta(this Color val, TweenAnimation.ValueWrapper prevVal)
	{
		return val - prevVal.color;
	}

	internal static Vector2 calcDelta(this Vector2 val, TweenAnimation.ValueWrapper prevVal)
	{
		return val - prevVal.vector2;
	}

	internal static Vector3 calcDelta(this Vector3 val, TweenAnimation.ValueWrapper prevVal)
	{
		return val - prevVal.vector3;
	}

	internal static Vector4 calcDelta(this Vector4 val, TweenAnimation.ValueWrapper prevVal)
	{
		return val - prevVal.vector4;
	}

	internal static Quaternion calcDelta(this Quaternion val, TweenAnimation.ValueWrapper prevVal)
	{
		return Quaternion.Inverse(prevVal.quaternion) * val;
	}

	internal static Rect calcDelta(this Rect val, TweenAnimation.ValueWrapper prevVal)
	{
		return new Rect(val.x - prevVal.x, val.y - prevVal.y, val.width - prevVal.z, val.height - prevVal.w);
	}

	internal static Color WithAlpha(this Color c, float alpha)
	{
		c.a = alpha;
		return c;
	}

	internal static TweenAnimation.ValueWrapper ToContainer(this float f)
	{
		return new TweenAnimation.ValueWrapper
		{
			single = f
		};
	}

	internal static TweenAnimation.ValueWrapper ToContainer(this Vector2 v)
	{
		return new TweenAnimation.ValueWrapper
		{
			vector2 = v
		};
	}

	internal static TweenAnimation.ValueWrapper ToContainer(this Vector3 v)
	{
		return new TweenAnimation.ValueWrapper
		{
			vector3 = v
		};
	}

	internal static TweenAnimation.ValueWrapper ToContainer(this Vector4 v)
	{
		return new TweenAnimation.ValueWrapper
		{
			vector4 = v
		};
	}

	internal static TweenAnimation.ValueWrapper XYToContainer(this Vector4 v)
	{
		return new TweenAnimation.ValueWrapper
		{
			vector2 = new Vector2(v.x, v.y)
		};
	}

	internal static TweenAnimation.ValueWrapper ZWToContainer(this Vector4 v)
	{
		return new TweenAnimation.ValueWrapper
		{
			vector2 = new Vector2(v.z, v.w)
		};
	}

	internal static TweenAnimation.ValueWrapper ToContainer(this Color c)
	{
		return new TweenAnimation.ValueWrapper
		{
			color = c
		};
	}

	internal static TweenAnimation.ValueWrapper ToContainer(this Quaternion q)
	{
		return new TweenAnimation.ValueWrapper
		{
			quaternion = q
		};
	}

	internal static TweenAnimation.ValueWrapper ToContainer(this Rect r)
	{
		return new TweenAnimation.ValueWrapper
		{
			rect = r
		};
	}

	internal static TweenAnimation.ValueWrapper ToContainer(this double d)
	{
		return new TweenAnimation.ValueWrapper
		{
			DoubleVal = d
		};
	}

	internal static Vector2 WithComponent(this Vector2 v, int index, float val)
	{
		v[index] = val;
		return v;
	}

	internal static Vector3 WithComponent(this Vector3 v, int index, float val)
	{
		v[index] = val;
		return v;
	}

	internal static Vector2 GetFlexibleSize(this LayoutElement target)
	{
		return new Vector2(target.flexibleWidth, target.flexibleHeight);
	}

	internal static void SetFlexibleSize(this LayoutElement target, Vector2 vector2)
	{
		target.flexibleWidth = vector2.x;
		target.flexibleHeight = vector2.y;
	}

	internal static Vector2 GetMinSize(this LayoutElement target)
	{
		return new Vector2(target.minWidth, target.minHeight);
	}

	internal static void SetMinSize(this LayoutElement target, Vector2 vector2)
	{
		target.minWidth = vector2.x;
		target.minHeight = vector2.y;
	}

	internal static Vector2 GetPreferredSize(this LayoutElement target)
	{
		return new Vector2(target.preferredWidth, target.preferredHeight);
	}

	internal static void SetPreferredSize(this LayoutElement target, Vector2 vector2)
	{
		target.preferredWidth = vector2.x;
		target.preferredHeight = vector2.y;
	}

	internal static Vector2 GetNormalizedPosition(this ScrollRect target)
	{
		return new Vector2(target.horizontalNormalizedPosition, target.verticalNormalizedPosition);
	}

	internal static void SetNormalizedPosition(this ScrollRect target, Vector2 vector2)
	{
		target.horizontalNormalizedPosition = vector2.x;
		target.verticalNormalizedPosition = vector2.y;
	}

	internal static Vector2 GetTopLeft(this VisualElement e)
	{
		IResolvedStyle resolvedStyle = e.resolvedStyle;
		return new Vector2(resolvedStyle.left, resolvedStyle.top);
	}

	internal static void SetTopLeft(this VisualElement e, Vector2 c)
	{
		IStyle style = e.style;
		style.left = c.x;
		style.top = c.y;
	}

	internal static Rect GetResolvedStyleRect(this VisualElement e)
	{
		IResolvedStyle resolvedStyle = e.resolvedStyle;
		return new Rect(resolvedStyle.left, resolvedStyle.top, resolvedStyle.width, resolvedStyle.height);
	}

	internal static void SetStyleRect(this VisualElement e, Rect c)
	{
		IStyle style = e.style;
		style.left = c.x;
		style.top = c.y;
		style.width = c.width;
		style.height = c.height;
	}

	private static bool TryGetPropertyBlock(object target, out MaterialPropertyBlock result)
	{
		Renderer renderer = target as Renderer;
		if (renderer.HasPropertyBlock())
		{
			result = PrimeTweenManager.Instance.materialPropertyBlockForGetter;
			renderer.GetPropertyBlock(result);
			return true;
		}
		result = null;
		return false;
	}

	internal static bool TryGetPropertyBlockColor(object target, int propId, out Color result)
	{
		if (TryGetPropertyBlock(target, out var result2) && result2.HasColor(propId))
		{
			result = result2.GetColor(propId);
			return true;
		}
		result = default(Color);
		return false;
	}

	internal static bool TryGetPropertyBlockVector(object target, int propId, out Vector4 result)
	{
		if (TryGetPropertyBlock(target, out var result2) && result2.HasVector(propId))
		{
			result = result2.GetVector(propId);
			return true;
		}
		result = default(Vector4);
		return false;
	}

	internal static bool TryGetPropertyBlockFloat(object target, int propId, out float result)
	{
		if (TryGetPropertyBlock(target, out var result2) && result2.HasFloat(propId))
		{
			result = result2.GetFloat(propId);
			return true;
		}
		result = 0f;
		return false;
	}
}
