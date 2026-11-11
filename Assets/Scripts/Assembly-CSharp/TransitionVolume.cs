using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TransitionVolume : MonoBehaviour
{
	public static TransitionVolume Instance;

	public PostProcessVolume volume { get; private set; }

	private void Awake()
	{
		if ((bool)Instance && this != Instance)
		{
			Debug.LogWarning("Main camera already exists, disabling " + this, this);
			base.gameObject.SetActive(value: false);
		}
		else
		{
			Instance = this;
			volume = GetComponent<PostProcessVolume>();
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}
}
