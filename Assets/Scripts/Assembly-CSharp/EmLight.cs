using PrimeTween;
using UnityEngine;

public class EmLight : MonoBehaviour
{
	public Light _lightComp;

	public Renderer _lightRenderer;

	[ColorUsage(false, true)]
	public Color _startColor;

	[ColorUsage(false, true)]
	public Color _endColor;

	public float _flickerTime;

	private void Start()
	{
		Tween.LightIntensity(_lightComp, 0f, _flickerTime, Ease.InExpo, -1, CycleMode.Rewind);
		Tween.Custom(_startColor, _endColor, _flickerTime, delegate(Color newVal)
		{
			_lightRenderer.material.SetColor("_EmissionColor", newVal);
		}, Ease.InExpo, -1, CycleMode.Rewind);
	}
}
