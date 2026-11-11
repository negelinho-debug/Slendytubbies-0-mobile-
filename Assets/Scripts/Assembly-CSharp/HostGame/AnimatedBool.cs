using System;
using Unity.Mathematics;
using UnityEngine;

namespace HostGame;

[Serializable]
public struct AnimatedBool
{
	[SerializeField]
	private float speed;

	[SerializeField]
	private float value;

	private bool target;

	public float Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public bool BoolValue => value.RoughlyEquals(1f);

	public bool Target => target;

	public AnimatedBool(bool value, float speed = 1f)
	{
		this.speed = speed;
		this.value = (value ? 1 : 0);
		target = value;
	}

	public void SetTarget(bool value)
	{
		target = value;
	}

	public void SetSpeed(float val)
	{
		speed = val;
	}

	public float Update(float deltaTime)
	{
		value += deltaTime * (float)(Target ? 1 : (-1)) * speed;
		value = math.clamp(value, 0f, 1f);
		return value;
	}

	public static implicit operator bool(in AnimatedBool b)
	{
		return b.BoolValue;
	}

	public static implicit operator AnimatedBool(bool f)
	{
		return new AnimatedBool(f);
	}
}
