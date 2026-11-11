using FMODUnity;
using UnityEngine;

public class HungerMeter : MonoBehaviour
{
	private float hunger;

	private int hungerPeriod;

	public EventReference _hungerSound;

	public float[] _hungerGrowTime;

	public float Hunger => hunger;

	private void Update()
	{
		float num = _hungerGrowTime[hungerPeriod];
		float num2 = 1f / num;
		AddHunger(num2 * Time.deltaTime);
	}

	public void AddHunger(float amount)
	{
		float num = hunger;
		hunger += amount;
		hunger = Mathf.Min(hunger, 1f);
		if (num < 1f && hunger >= 1f)
		{
			RuntimeManager.PlayOneShot(_hungerSound);
		}
	}

	public void NextHungerLevel()
	{
		hungerPeriod++;
		hungerPeriod = Mathf.Clamp(hungerPeriod, 0, _hungerGrowTime.Length);
		hunger = 0f;
	}
}
