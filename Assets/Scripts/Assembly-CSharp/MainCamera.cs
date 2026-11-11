using UnityEngine;

public class MainCamera : MonoBehaviour
{
	public static MainCamera instance;

	private void Awake()
	{
		if ((bool)instance && this != instance)
		{
			Debug.LogWarning("Main camera already exists, disabling " + this, this);
			base.gameObject.SetActive(value: false);
		}
		else
		{
			Camera.main.depthTextureMode |= DepthTextureMode.MotionVectors;
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	private void Update()
	{
	}
}
