using UnityEngine;

public class EnableFog : MonoBehaviour
{
	public void SwitchFog(bool enable)
	{
		RenderSettings.fog = enable;
	}
}
