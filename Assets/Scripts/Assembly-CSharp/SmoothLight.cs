using PrimeTween;
using UnityEngine;

public class SmoothLight : MonoBehaviour
{
	public float _startIntensity;

	public float _endIntensity = 1f;

	public float _lerpDuration = 0.2f;

	public bool _onStart;

	private Light lightComp;

	private void Start()
	{
		lightComp = GetComponent<Light>();
		lightComp.intensity = _startIntensity;
		if (_onStart)
		{
			LerpIntensity();
		}
	}

	public void LerpIntensity()
	{
		Tween.LightIntensity(lightComp, _endIntensity, _lerpDuration);
	}
}
