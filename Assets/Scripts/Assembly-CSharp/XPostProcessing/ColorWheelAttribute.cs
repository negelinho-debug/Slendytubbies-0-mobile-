using System;

namespace XPostProcessing;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ColorWheelAttribute : Attribute
{
	public enum Mode
	{
		None,
		Lift,
		Gamma,
		Gain,
		Contrast
	}

	public readonly Mode mode;

	public ColorWheelAttribute(Mode mode)
	{
		this.mode = mode;
	}
}
