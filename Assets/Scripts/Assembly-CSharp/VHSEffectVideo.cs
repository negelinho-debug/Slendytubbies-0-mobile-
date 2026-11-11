using UnityEngine;
using UnityEngine.Video;

public class VHSEffectVideo : MonoBehaviour
{
	public static VHSEffectVideo instance;

	public VideoPlayer _video;

	private void Awake()
	{
		instance = this;
	}
}
