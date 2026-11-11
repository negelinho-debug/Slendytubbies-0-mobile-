using System;
using UnityEngine;

public readonly struct TimerStamp : IEquatable<TimerStamp>
{
	public readonly double timerEnd;

	[SafeProperty]
	public bool RanOut => Time.timeAsDouble > timerEnd;

	[SafeProperty]
	public double TimeLeft => Mathf.Max(0f, (float)(timerEnd - Time.timeAsDouble));

	public static TimerStamp Invalid => new TimerStamp(double.NegativeInfinity);

	public static TimerStamp Infinity => new TimerStamp(double.PositiveInfinity);

	public bool IsInfinite => double.IsPositiveInfinity(timerEnd);

	public bool IsValid => !double.IsNegativeInfinity(timerEnd);

	public TimerStamp(double timerEnd)
	{
		this.timerEnd = timerEnd;
	}

	public static TimerStamp FromSeconds(float seconds)
	{
		return new TimerStamp(Time.timeAsDouble + (double)seconds);
	}

	public static TimerStamp FromSeconds(double seconds)
	{
		return new TimerStamp(Time.timeAsDouble + seconds);
	}

	public bool Equals(TimerStamp other)
	{
		return timerEnd.Equals(other.timerEnd);
	}

	public override bool Equals(object obj)
	{
		if (obj is TimerStamp other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return timerEnd.GetHashCode();
	}

	public static bool operator ==(TimerStamp s0, TimerStamp s1)
	{
		return s0.timerEnd.Equals(s1.timerEnd);
	}

	public static bool operator !=(TimerStamp s0, TimerStamp s1)
	{
		return !(s0 == s1);
	}
}
