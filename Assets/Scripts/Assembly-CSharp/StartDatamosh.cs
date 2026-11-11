using UnityEngine;

public class StartDatamosh : MonoBehaviour
{
	private Datamosh datamosh;

	private void Start()
	{
		datamosh = Camera.main.GetComponent<Datamosh>();
	}

	public void SetActive(bool active)
	{
		datamosh.enabled = active;
	}

	public void StopUpdate(float duration = 1f)
	{
		datamosh.noUpdateTime = Time.time + duration;
	}

	public void FadeOut()
	{
		datamosh.FadeOut = true;
	}
}
