using System.Collections.Generic;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
	public float _interactionDistance;

	public LayerMask _InteractionLayerMask;

	private List<IInteractable> interactables = new List<IInteractable>(8);

	private void Update()
	{
		if (Player.Instance.Controller.IsFreezed)
		{
			return;
		}
		Transform transform = Player.Instance.Camera.transform;
		if (!Physics.Raycast(new Ray(transform.position, transform.forward), out var hitInfo, _interactionDistance, _InteractionLayerMask))
		{
			return;
		}
		hitInfo.collider.GetComponents(interactables);
		if (interactables.Count <= 0)
		{
			return;
		}
		foreach (IInteractable interactable in interactables)
		{
			if (interactable.enabled && interactable.Active)
			{
				InteractionUI.Show(InteractionType.Normal);
				if (InputManager.Instance.LMBTriggered() || InputManager.Instance.TriggeredE())
				{
					interactable.Use();
				}
			}
		}
	}
}
