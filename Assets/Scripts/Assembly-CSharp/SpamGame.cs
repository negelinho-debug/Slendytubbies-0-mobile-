using UnityEngine;
using UnityEngine.Events;

public class SpamGame : MonoBehaviour
{
	[Range(0f, 1f)]
	public float _ratePerTap = 0.1f;

	[Range(0f, 1f)]
	public float _decayPerTick = 0.03f;

	[Space(10f)]
	public float _gameDuration = 10f;

	[Range(0f, 1f)]
	public float _minThreshold = 0.4f;

	public UnityEvent _onSuccess;

	public UnityEvent _onFailure;

	protected bool isTapping;

	protected float currentDecay;

	protected float currentMinThreshold;

	protected float currentRate;

	protected float gameTime;

	protected void Start()
	{
		gameTime = Time.time + _gameDuration;
		currentDecay = _decayPerTick;
		currentMinThreshold = _minThreshold;
	}

	protected void Update()
	{
		isTapping = InputManager.Instance.LMBTriggered() || InputManager.Instance.SpaceTriggered();
		if (isTapping)
		{
			currentRate += _ratePerTap;
			if (currentRate > 1f)
			{
				currentRate = 1f;
			}
		}
		if (currentRate > 0f)
		{
			currentRate -= currentDecay * Time.deltaTime;
		}
	}
}
