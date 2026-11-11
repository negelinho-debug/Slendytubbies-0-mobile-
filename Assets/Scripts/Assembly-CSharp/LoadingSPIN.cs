using UnityEngine;

public class LoadingSPIN : MonoBehaviour
{
	private void Update()
	{
		base.transform.Rotate(0f, 0f, -180f * Time.deltaTime);
	}
}
