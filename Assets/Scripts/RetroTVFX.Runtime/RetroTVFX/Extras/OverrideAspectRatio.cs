using UnityEngine;

namespace RetroTVFX.Extras;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class OverrideAspectRatio : MonoBehaviour
{
	public bool OverrideAspect = true;

	public float AspectRatio = 1f;

	private Camera _cam;

	private void LateUpdate()
	{
	}

	private void OnPreCull()
	{
		if (_cam == null)
		{
			_cam = GetComponent<Camera>();
		}
		if (OverrideAspect)
		{
			_cam.aspect = AspectRatio;
		}
		else
		{
			_cam.ResetAspect();
		}
	}
}
