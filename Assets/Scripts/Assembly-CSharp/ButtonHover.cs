using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public GameObject _arrow;

	private void OnEnable()
	{
		_arrow.SetActive(value: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_arrow.SetActive(value: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_arrow.SetActive(value: false);
	}
}
