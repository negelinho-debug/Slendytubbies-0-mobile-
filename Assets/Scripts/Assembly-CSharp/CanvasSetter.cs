using UnityEngine;

public class CanvasSetter : MonoBehaviour
{
	private void Start()
	{
		Canvas component = GetComponent<Canvas>();
		component.renderMode = RenderMode.ScreenSpaceCamera;
		component.worldCamera = Camera.main;
		component.planeDistance = 0.2f;
	}
}
