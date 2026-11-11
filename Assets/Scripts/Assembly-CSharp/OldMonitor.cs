using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(OldMonitorRenderer), PostProcessEvent.AfterStack, "JfranMora/OldMonitor", false)]
public class OldMonitor : PostProcessEffectSettings
{
	[Header("Monitor")]
	[Range(2f, 10f)]
	public FloatParameter monitorBend = new FloatParameter
	{
		value = 2f
	};

	[Header("Slowscan")]
	[Range(0.05f, 0.2f)]
	public FloatParameter slowscanWidth = new FloatParameter
	{
		value = 0.05f
	};

	public FloatParameter slowscanSpeed = new FloatParameter
	{
		value = 1f
	};

	[Range(-1f, 1f)]
	public FloatParameter slowscanDistortion = new FloatParameter
	{
		value = 0.5f
	};

	[Range(0f, 0.25f)]
	public FloatParameter slowscanIntensity = new FloatParameter
	{
		value = 0.1f
	};

	[Header("Scanlines")]
	public FloatParameter scanline = new FloatParameter
	{
		value = 600f
	};

	public FloatParameter scanlineSpeed = new FloatParameter
	{
		value = 10f
	};
}
